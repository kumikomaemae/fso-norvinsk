using System;
using System.Reflection;
using MoreBotsServer.Models;
using MoreBotsServer.Services;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.DI;
using SPTarkov.Server.Core.Models.Eft.Common;
using SPTarkov.Server.Core.Models.Enums;
using SPTarkov.Server.Core.Models.Spt.Config;
using SPTarkov.Server.Core.Models.Spt.Mod;
using SPTarkov.Server.Core.Models.Utils;
using SPTarkov.Server.Core.Servers;
using SPTarkov.Server.Core.Services;
using SPTarkov.Server.Core.Utils.Json;
using WTTServerCommonLib.Services;

namespace FSO.NorvinskSection1.Server;

/// <summary>
/// Mod metadata. SPT 4.x discovers and registers our mod via this record at boot.
/// </summary>
public record ModMetadata : AbstractModMetadata
{
    public override string ModGuid { get; init; } = "com.mae.fso.norvinsksection1";
    public override string Name { get; init; } = "FSO: Norvinsk Section 1";
    public override string Author { get; init; } = "Mae";
    public override List<string>? Contributors { get; init; }
    public override SemanticVersioning.Version Version { get; init; } = new("0.5.1");
    public override SemanticVersioning.Range SptVersion { get; init; } = new("~4.0.0");
    public override List<string>? Incompatibilities { get; init; }
    public override Dictionary<string, SemanticVersioning.Range>? ModDependencies { get; init; } = new()
    {
        { "com.morebotsapi.tacticaltoaster", new SemanticVersioning.Range(">=1.0.0") }
    };
    public override string? Url { get; init; } = "";
    public override bool? IsBundleMod { get; init; }
    public override string License { get; init; } = "MIT";
}

/// <summary>
/// Server mod entry point.
///
/// - Registers the 5 FSO Fixer bot tiers (types + configs) via MoreBotsAPI.
/// - Wires FSO faction relationships (see WireFactionRelationships).
/// - Registers FSO spawn rules across 6 maps with reusable squad composition templates.
///   All FSO spawn ids carry the "hunt" keyword so MoreBotsAPI's HuntManager arms them as
///   active hunters (the client plugin registers which roles they hunt) — they roam + seek
///   enemies instead of loitering at spawn.
/// - Q5 finale (Labs): spawns FSO Inner Circle + Black Division (BD's own Labs spawn is
///   gate/exfil-triggered and Vagabond removes those triggers, so we spawn BD ourselves).
///
/// ROE: FSO are friendly to the player's USEC side (you + Damjan) and to themselves; NEUTRAL
/// to Rogues/Goons (bigger fish to fry); HOSTILE to everything else — BEAR, Scavs, Raiders,
/// Cultists, all scav bosses, and the Black Division / RUAF / Remnant factions. FSO have no
/// warn behavior — they're professionals on the clock, they don't posture.
/// </summary>
// Load order: we run at TypePriority 400008 (a safe late slot in the post-DB phase — for
// reference, BlackDiv runs at 400007 and PostDBModLoader is ~400000). Running late is safe:
// all our DI services are constructed before any OnLoad, so nothing we do depends on running
// before another mod, and our spawn/config edits just need the DB loaded.
[Injectable(TypePriority = 400008)]
public class Mod(
    ISptLogger<Mod> logger,
    MoreBotsCustomBotTypeService customBotTypeService,
    MoreBotsCustomBotConfigService customBotConfigService,
    LoadoutService loadoutService,
    FactionService factionService,
    CustomLocationWaveService waveService,
    ConfigServer configServer,
    WTTCustomItemServiceExtended wttItemService,
    WTTCustomAchievementService wttAchievementService
) : IOnLoad
{
    public const string ModName = "FSO: Norvinsk Section 1";
    public const string ModVersion = "0.5.1";
    public const string FactionName = "fso";

    // config objects can't be constructor-injected on 4.0.13 — pull from ConfigServer
    private readonly LocationConfig _locationConfig = configServer.GetConfig<LocationConfig>();

    private static readonly List<WildSpawnType> FsoBotTypes = new()
    {
        (WildSpawnType)708300, // fsofixerrookie
        (WildSpawnType)708301, // fsofixeroperative
        (WildSpawnType)708302, // fsofixerspecialist
        (WildSpawnType)708303, // fsofixerlead
        (WildSpawnType)708304, // fsofixerinnercircle
    };

    // Black Division / RUAF / Remnant custom WildSpawnType values (from their
    // WildSpawnTypeExtensions). These mods register their own MoreBotsAPI FACTIONS, so our
    // AddFactionIfMissing calls below simply skip them (and our hostility wiring resolves
    // against their existing factions). These int lists are kept only as a fallback grouping
    // in case those mods aren't loaded — harmless when they are.
    private static readonly List<WildSpawnType> BlackDivBotTypes = new()
    {
        (WildSpawnType)848420, (WildSpawnType)848421, (WildSpawnType)848422,
        (WildSpawnType)848423, (WildSpawnType)848424,
    };
    private static readonly List<WildSpawnType> RuafBotTypes = new()
    {
        (WildSpawnType)848400, (WildSpawnType)848401, (WildSpawnType)848402,
        (WildSpawnType)848403, (WildSpawnType)848404, (WildSpawnType)848405,
    };
    private static readonly List<WildSpawnType> RemnantBotTypes = new()
    {
        (WildSpawnType)848406,
    };

    // --- Spawn tuning (FPS / density control) ---
    // Per-squad spawn chance (0-100). Lower = fewer FSO squads per raid = better FPS + more variety.
    // ~40 -> Customs(4 squads) averages ~9 Fixers; Streets(9) ~18; Lighthouse(2) ~5.
    public const int FsoSpawnChance = 40;

    // true = FSO spawn ON TOP OF the normal bot cap (reliable presence, additive load).
    // false = FSO respect the map's bot cap (hard ceiling on total bots / best FPS, but FSO may not always appear).
    public const bool FsoIgnoreMaxBots = true;

    private const string BotRookie = "fsofixerrookie";
    private const string BotOperative = "fsofixeroperative";
    private const string BotSpecialist = "fsofixerspecialist";
    private const string BotLead = "fsofixerlead";
    private const string BotInnerCircle = "fsofixerinnercircle";

    // Black Division trooper role. Casing matches BD's own SpawnController (BossName =
    // "blackDivAssault") — the spawn system accepts this exact camelCase. We spawn BD on
    // Labs ourselves because Vagabond removes the Labs exfils, and BD's own Labs spawn is
    // gate/exfil-TRIGGERED — those triggers don't exist under Vagabond, so BD's native Labs
    // spawn never fires. (BD's HUNT spawns on other maps work fine; we don't touch those.)
    private const string BotBlackDivAssault = "blackDivAssault";

    private const string MapStreets = "tarkovstreets";
    private const string MapSandbox = "sandbox";
    private const string MapSandboxHigh = "sandbox_high";
    private const string MapCustoms = "bigmap";
    private const string MapShoreline = "shoreline";
    private const string MapLighthouse = "lighthouse";
    private const string MapLaboratory = "laboratory"; // Q5 finale

    public async Task OnLoad()
    {
        logger.Info($"[{ModName}] v{ModVersion} loading...");
        var asm = Assembly.GetExecutingAssembly();

        // Map custom WildSpawnType int values to role names so faction
        // relationship lookups can resolve our custom bots.
        customBotTypeService.AddCustomWildSpawnTypeNames(new Dictionary<int, string>
        {
            { 708300, BotRookie },
            { 708301, BotOperative },
            { 708302, BotSpecialist },
            { 708303, BotLead },
            { 708304, BotInnerCircle },
        });

        // --- Bot registration (replaces moreBotsApi.LoadBots) ---
        // 1. Register the 5 custom bot types from db/bots/types/*.json
        await customBotTypeService.CreateCustomBotTypes(asm);
        // 2. Register per-tier configs from db/bots/config/*.jsonc, keyed by
        //    filename. THIS is what LoadBots was failing to do correctly.
        await customBotConfigService.LoadCustomBotConfigs(asm);
        // 3. Apply custom loadouts from db/bots/loadouts/*.json.
        //    No-op until loadout files exist, so safe to call now.
        await loadoutService.LoadLoadouts(asm);

        // --- Custom content (WTT-ServerCommonLib) ---
        // Register FSO's custom items from db/CustomItems/*.json (the Hero King's
        // Rebellious Wristwatch — the Q5 anniversary reward). The watch uses no mod
        // slots / calibers / secure filters, so the deferred processors aren't needed.
        // NOTE: WTT-ServerCommonLib logs its own success at Debug level (hidden at INFO),
        // so we add our own Info confirmations here for visibility in the server log.
        logger.Info($"[{ModName}] Loading custom items from db/CustomItems...");
        await wttItemService.CreateCustomItems(asm);
        // Register FSO's custom achievements from db/CustomAchievements/ (the "Savior"
        // achievement — its CustomizationDirect rewards grant the ForHumanity menu
        // environment + the for_humanity dogtag when the achievement is awarded on Q5).
        logger.Info($"[{ModName}] Loading custom achievements from db/CustomAchievements...");
        await wttAchievementService.CreateCustomAchievements(asm);
        logger.Success($"[{ModName}] Custom content loaded: the watch + the Savior achievement.");

        // --- Faction setup ---
        RegisterFsoFaction();
        WireFactionRelationships();

        // --- Spawn rules ---
        EnsureSpawnKeysExist();
        WireSpawnRules();
        waveService.ApplyWaveChangesToAllMaps();

        // --- Labs finale tuning ---
        // We spawn FSO Inner Circle + Black Division on Labs ourselves (see AddLabsSpawns) —
        // BD's own Labs spawn is gate-triggered and Vagabond removes those triggers, so it
        // never fires. We also raise the Labs bot cap so FSO + BD + raiders all fit. Labs is
        // fully indoor / low-load, and bot AI runs on the host's CPU, so this is safe.
        RaiseLabsBotCap();

        logger.Success($"[{ModName}] Section Manager Mae reporting. Coffee's hot. Standing by.");
        logger.Info($"[{ModName}] Loaded bot types: {string.Join(", ", customBotTypeService.LoadedBotTypes)}");
        logger.Info($"[{ModName}] Faction '{FactionName}' registered with {FsoBotTypes.Count} bot tiers.");
        logger.Info($"[{ModName}] Spawn rules wired across 6 maps (Streets, Ground Zero, Customs, Shoreline, Lighthouse, Labs).");
    }

    private void RegisterFsoFaction()
    {
        AddFactionIfMissing(FactionName, FsoBotTypes);

        // BD / RUAF / Remnant register their OWN MoreBotsAPI factions, so these calls normally
        // find the faction already present and skip (logged "already registered"). We still call
        // them as a fallback in case those mods aren't installed — then we register the grouping
        // ourselves so our hostility wiring against those names resolves.
        AddFactionIfMissing("blackdiv", BlackDivBotTypes);
        AddFactionIfMissing("ruaf", RuafBotTypes);
        AddFactionIfMissing("remnant", RemnantBotTypes);
    }

    // Register a faction only if the name isn't already taken (avoids Dictionary.Add throwing,
    // and respects any faction another mod registered first).
    private void AddFactionIfMissing(string name, List<WildSpawnType> botTypes)
    {
        if (factionService.Factions.ContainsKey(name))
        {
            logger.Info($"[{ModName}] Faction '{name}' already registered — leaving as-is.");
            return;
        }
        factionService.Factions.Add(name, new Faction { Name = name, BotTypes = botTypes });
    }

    // Raise the Labs bot cap a moderate amount so FSO Inner Circle + Black Division + raiders
    // all fit. Labs is fully indoor and low-load; bot AI runs on the host CPU, so this is safe.
    private void RaiseLabsBotCap()
    {
        try
        {
            var botConfig = configServer.GetConfig<BotConfig>();
            var caps = botConfig?.MaxBotCap;
            if (caps == null)
            {
                logger.Info($"[{ModName}] Labs cap-raise: MaxBotCap not available (skipped).");
                return;
            }

            const int LabsCapBump = 8;
            if (caps.TryGetValue(MapLaboratory, out var current))
            {
                caps[MapLaboratory] = current + LabsCapBump;
                logger.Success($"[{ModName}] Labs cap-raise: {current} -> {caps[MapLaboratory]} (+{LabsCapBump}).");
            }
            else
            {
                // No explicit Labs entry — set a sensible value outright.
                caps[MapLaboratory] = 20;
                logger.Success($"[{ModName}] Labs cap-raise: no existing entry, set to {caps[MapLaboratory]}.");
            }
        }
        catch (Exception e)
        {
            logger.Warning($"[{ModName}] Labs cap-raise skipped (non-fatal): {e.Message}");
        }
    }

    private static readonly List<string> FsoTypeNames = new()
    {
        "fsofixerrookie", "fsofixeroperative", "fsofixerspecialist",
        "fsofixerlead", "fsofixerinnercircle",
    };
    private static readonly List<string> BlackDivTypeNames = new()
    { "blackdivlead", "blackdivassault", "blackdivbreacher", "blackdivsupport" };
    private static readonly List<string> RuafTypeNames = new()
    { "ruafrifleman", "ruafriflemansenior", "ruafautorifleman", "ruafgrenadier", "ruafmarksman", "ruafmachinegunner" };
    private static readonly List<string> RemnantTypeNames = new() { "remnantrifleman" };

    private void WireFactionRelationships()
    {
        // FSO ROE: hostile to everything except player USEC (you/Damjan) and Rogues (neutral).
        // Pass FSO's TYPE-NAME STRINGS as arg1 (the direct-dictionary overload) — this is the
        // reliable path BlackDiv uses. Passing the custom faction name "fso" as arg1 routes
        // through a fragile custom-enum lookup that was silently failing (the hostility bug).

        // "savage" umbrella = scavs + ALL scav bosses + smugglers + bloodhounds + raiders
        // (GetAllBotTypes recurses subfactions — confirmed). One entry covers them all.
        string[] builtinEnemies = { "savage", "cultists", "infected", "bear" };

        // (A) FSO hostile TO enemies — type-string overload (RELIABLE).
        foreach (var e in builtinEnemies)
            factionService.AddEnemyByFaction(FsoTypeNames, e);
        factionService.AddEnemyByFaction(FsoTypeNames, "blackdiv");
        factionService.AddEnemyByFaction(FsoTypeNames, "ruaf");
        factionService.AddEnemyByFaction(FsoTypeNames, "remnant");

        // (B) Enemies hostile TO FSO. Built-ins resolve fine as arg1. For the custom factions,
        // pass THEIR type strings as arg1 (reliable) rather than the custom faction name.
        foreach (var e in builtinEnemies)
            factionService.AddEnemyByFaction(e, FactionName);
        factionService.AddEnemyByFaction(BlackDivTypeNames, FactionName);
        factionService.AddEnemyByFaction(RuafTypeNames, FactionName);
        factionService.AddEnemyByFaction(RemnantTypeNames, FactionName);

        // Allies: player USEC (you + Damjan) + FSO self (anti-infighting). Rogues omitted = neutral.
        factionService.AddFriendlyByFaction(FsoTypeNames, "usec");
        factionService.AddFriendlyByFaction(FsoTypeNames, FactionName);

        // Revenge: FSO avenge fallen FSO AND avenge the player (allies who watch your back).
        factionService.AddRevengeByFaction(FsoTypeNames, FactionName);
        factionService.AddRevengeByFaction(FsoTypeNames, "usec");

        logger.Info($"[{ModName}] Faction relationships wired (BD-pattern, type-string overload): FSO hostile to savage/cultists/infected/bear/blackdiv/ruaf/remnant, friendly self+usec, neutral rogues, revenge on FSO+player deaths.");
    }

    private void EnsureSpawnKeysExist()
    {
        _locationConfig.CustomWaves ??= new CustomWaves();
        var bossWaves = _locationConfig.CustomWaves.Boss;

        var fsoMaps = new[]
        {
            MapStreets,
            MapSandbox,
            MapSandboxHigh,
            MapCustoms,
            MapShoreline,
            MapLighthouse,
            MapLaboratory,
        };

        foreach (var map in fsoMaps)
        {
            if (!bossWaves.ContainsKey(map))
            {
                bossWaves[map] = new List<BossLocationSpawn>();
            }
        }
    }

    private void WireSpawnRules()
    {
        AddStreetsSpawns();
        AddSandboxSpawns();
        AddCustomsSpawns();
        AddShorelineSpawns();
        AddLighthouseSpawns();
        AddLabsSpawns();
        // NOTE: On the standard maps, Black Division spawns via BD's OWN mod (its spawner adds
        // BD + timed hunts there and works fine — we don't touch those). We ONLY spawn BD on
        // Labs ourselves (see AddLabsSpawns), because BD's Labs spawn is gate-triggered and
        // Vagabond removes those triggers.
    }

    private void AddStreetsSpawns()
    {
        var streetsZones = "ZoneCarShowroom,ZoneSnipeCarShowroom,ZoneClimova,ZoneMvd";
        var spawns = new[]
        {
            BuildPatrolAlpha(streetsZones, "streets_01"),
            BuildPatrolBravo(streetsZones, "streets_02"),
            BuildSweepPatrol(streetsZones, "streets_03"),
            BuildHeavyElement(streetsZones, "streets_04"),
            BuildAssaultTeam(streetsZones, "streets_05"),
            BuildCommandElement(streetsZones, "streets_06"),
            BuildEliteStrike(streetsZones, "streets_07"),
            BuildInnerCircleRecon(streetsZones, "streets_08"),
        };

        foreach (var s in spawns)
            waveService.AddBossWaveToMap(MapStreets, s);

        logger.Success($"[{ModName}] Streets: registered {spawns.Length} spawn rules.");
    }

    private void AddSandboxSpawns()
    {
        var sandboxSpawns = new (string map, BossLocationSpawn spawn)[]
        {
            (MapSandbox, BuildPatrolAlpha("", "sandbox_01")),
            (MapSandbox, BuildSweepPatrol("", "sandbox_02")),
            (MapSandbox, BuildHeavyElement("", "sandbox_03")),
            (MapSandbox, BuildEliteStrike("", "sandbox_04")),
            (MapSandbox, BuildInnerCircleRecon("", "sandbox_05")),
            (MapSandboxHigh, BuildPatrolAlpha("", "sandboxh_01")),
            (MapSandboxHigh, BuildSweepPatrol("", "sandboxh_02")),
            (MapSandboxHigh, BuildHeavyElement("", "sandboxh_03")),
            (MapSandboxHigh, BuildEliteStrike("", "sandboxh_04")),
            (MapSandboxHigh, BuildInnerCircleRecon("", "sandboxh_05")),
        };

        foreach (var (map, spawn) in sandboxSpawns)
            waveService.AddBossWaveToMap(map, spawn);

        logger.Success($"[{ModName}] Ground Zero (both tiers): registered {sandboxSpawns.Length} spawn rules.");
    }

    private void AddCustomsSpawns()
    {
        var customsZones = "ZoneDormitory,ZoneGasStation,ZoneScavBase";
        var spawns = new[]
        {
            BuildPatrolAlpha(customsZones, "customs_01"),
            BuildPatrolBravo(customsZones, "customs_02"),
            BuildLightPatrol(customsZones, "customs_03"),
            BuildSweepPatrol(customsZones, "customs_04"),
        };

        foreach (var s in spawns)
            waveService.AddBossWaveToMap(MapCustoms, s);

        logger.Success($"[{ModName}] Customs: registered {spawns.Length} spawn rules.");
    }

    private void AddShorelineSpawns()
    {
        var shorelineZones = "ZoneGreenHouses,ZonePort,ZoneSanatorium1,ZoneSanatorium2,ZoneSmuglers,ZoneMeteoStation";
        var spawns = new[]
        {
            BuildPatrolAlpha(shorelineZones, "shoreline_01"),
            BuildSweepPatrol(shorelineZones, "shoreline_02"),
            BuildHeavyElement(shorelineZones, "shoreline_03"),
            BuildAssaultTeam(shorelineZones, "shoreline_04"),
        };

        foreach (var s in spawns)
            waveService.AddBossWaveToMap(MapShoreline, s);

        logger.Success($"[{ModName}] Shoreline: registered {spawns.Length} spawn rules.");
    }

    private void AddLighthouseSpawns()
    {
        var lighthouseZones = "Zone_Island,Zone_TreatmentContainers,Zone_Chalet,Zone_Blockpost,Zone_RoofContainers";
        var spawns = new[]
        {
            BuildLightPatrol(lighthouseZones, "lighthouse_01"),
            BuildPatrolAlpha(lighthouseZones, "lighthouse_02"),
        };

        foreach (var s in spawns)
            waveService.AddBossWaveToMap(MapLighthouse, s);

        logger.Success($"[{ModName}] Lighthouse: registered {spawns.Length} spawn rules.");
    }

    // ============================================================
    // Q5 FINALE — the Labs battle.
    // FSO Inner Circle (Mae's task force) deploys to Labs, weapons-free, to help the player
    // fight through to the Golden Bough. We spawn BOTH sides here: FSO Inner Circle AND Black
    // Division — because BD's OWN Labs spawn is gate/exfil-triggered and Vagabond removes those
    // triggers, so BD never appears on Labs on its own. Inner Circle count is kept MODERATE so
    // the Labs bot cap has room for the BD spawn — the war is FSO + BD side by side.
    // Labs zones confirmed from BlackDivServer's SpawnController.
    // ============================================================
    private void AddLabsSpawns()
    {
        var labsZones = "BotZoneFloor2,BotZoneFloor1,BotZoneBasement";

        // --- FSO Inner Circle on Labs (Q5 finale) ---
        var fsoSpawns = new[]
        {
            BuildLabsInnerCircle(labsZones, "labs_ic_01"),
            BuildLabsInnerCircle(labsZones, "labs_ic_02"),
        };
        foreach (var s in fsoSpawns)
            waveService.AddBossWaveToMap(MapLaboratory, s);

        // --- Black Division on Labs (the Q5 enemy) ---
        // START spawn (Time = -1) so BD is present from raid start — see the method comment
        // and the AddLabsSpawns header for why we spawn BD ourselves on Labs.
        var bdSpawns = new[]
        {
            BuildLabsBlackDivision(labsZones, "labs_bd_01"),
            BuildLabsBlackDivision(labsZones, "labs_bd_02"),
        };
        foreach (var s in bdSpawns)
            waveService.AddBossWaveToMap(MapLaboratory, s);

        logger.Success($"[{ModName}] Labs: registered {fsoSpawns.Length} Inner Circle + {bdSpawns.Length} Black Division spawns (Q5 finale).");
    }

    // FSO-side Black Division squad for Labs. Uses BD's proven structure (comma-list escort
    // amounts = several BD of varying sizes), camelCase name matching BD's own spawn code,
    // 100% chance + IgnoreMaxBots so they reliably appear regardless of the Labs cap.
    // NOTE: the spawn id intentionally does NOT contain "hunt" — these are Black Division, not
    // FSO, and BD's hunt behavior is managed by BD's own mod, not ours.
    private BossLocationSpawn BuildLabsBlackDivision(string zone, string sptIdSuffix)
    {
        return new BossLocationSpawn
        {
            SptId = $"fso_labs_blackdiv_{sptIdSuffix}",
            BossName = BotBlackDivAssault,
            BossChance = 100,
            BossDifficulty = "normal",
            BossEscortType = BotBlackDivAssault,
            BossEscortAmount = "2,2,3,3,4",
            BossEscortDifficulty = "normal",
            BossZone = zone,
            Time = -1,
            Delay = 0,
            ForceSpawn = true,
            IgnoreMaxBots = true,
            IsBossPlayer = false,
            IsRandomTimeSpawn = false,
            ShowOnTarkovMap = false,
            ShowOnTarkovMapPvE = false,
            SpawnMode = new List<string> { "regular", "pve" },
            DependKarma = false,
            DependKarmaPVE = false,
            TriggerId = "",
            TriggerName = "",
            Supports = new List<BossSupport>(),
        };
    }

    private BossLocationSpawn BuildPatrolAlpha(string zone, string sptIdSuffix)
    {
        return CompositionBase(zone, sptIdSuffix, "patrol_alpha")
            .WithBoss(BotSpecialist)
            .WithPrimaryEscort(BotOperative, "3")
            .WithSupport(BotRookie, "2")
            .Build();
    }

    private BossLocationSpawn BuildPatrolBravo(string zone, string sptIdSuffix)
    {
        return CompositionBase(zone, sptIdSuffix, "patrol_bravo")
            .WithBoss(BotLead)
            .WithPrimaryEscort(BotOperative, "4")
            .WithSupport(BotRookie, "1")
            .Build();
    }

    private BossLocationSpawn BuildLightPatrol(string zone, string sptIdSuffix)
    {
        return CompositionBase(zone, sptIdSuffix, "light_patrol")
            .WithBoss(BotSpecialist)
            .WithPrimaryEscort(BotOperative, "2")
            .WithSupport(BotRookie, "1")
            .Build();
    }

    private BossLocationSpawn BuildSweepPatrol(string zone, string sptIdSuffix)
    {
        return CompositionBase(zone, sptIdSuffix, "sweep_patrol")
            .WithBoss(BotLead)
            .WithPrimaryEscort(BotOperative, "4")
            .WithSupport(BotSpecialist, "1")
            .Build();
    }

    private BossLocationSpawn BuildHeavyElement(string zone, string sptIdSuffix)
    {
        return CompositionBase(zone, sptIdSuffix, "heavy_element")
            .WithBoss(BotLead)
            .WithPrimaryEscort(BotOperative, "3")
            .WithSupport(BotSpecialist, "2")
            .Build();
    }

    private BossLocationSpawn BuildAssaultTeam(string zone, string sptIdSuffix)
    {
        return CompositionBase(zone, sptIdSuffix, "assault_team")
            .WithBoss(BotLead)
            .WithPrimaryEscort(BotOperative, "4")
            .WithSupport(BotSpecialist, "2")
            .Build();
    }

    private BossLocationSpawn BuildCommandElement(string zone, string sptIdSuffix)
    {
        return CompositionBase(zone, sptIdSuffix, "command_element")
            .WithBoss(BotLead)
            .WithPrimaryEscort(BotOperative, "2")
            .WithSupportPair(BotSpecialist, "2", BotLead, "1")
            .Build();
    }

    private BossLocationSpawn BuildEliteStrike(string zone, string sptIdSuffix)
    {
        return CompositionBase(zone, sptIdSuffix, "elite_strike")
            .WithBoss(BotInnerCircle)
            .WithPrimaryEscort(BotLead, "2")
            .WithSupportPair(BotSpecialist, "2", BotInnerCircle, "1")
            .Build();
    }

    private BossLocationSpawn BuildInnerCircleRecon(string zone, string sptIdSuffix)
    {
        return CompositionBase(zone, sptIdSuffix, "ic_recon")
            .WithBoss(BotInnerCircle)
            .WithPrimaryEscort(BotInnerCircle, "1")
            .WithSupport(BotLead, "1")
            .Build();
    }

    // FSO Inner Circle squad: boss + 2 escort + 2 support = 5 per squad. 2 squads = ~10 on Labs.
    // Kept moderate (was 6/squad) so the Labs bot cap has room for BD's own spawn.
    private BossLocationSpawn BuildLabsInnerCircle(string zone, string sptIdSuffix)
    {
        var spawn = CompositionBase(zone, sptIdSuffix, "labs_inner_circle")
            .WithBoss(BotInnerCircle)
            .WithPrimaryEscort(BotInnerCircle, "2")
            .WithSupport(BotInnerCircle, "2")
            .Build();
        spawn.BossChance = 100; // force 100% for the finale
        spawn.IgnoreMaxBots = true; // guarantee they appear
        return spawn;
    }

    // All FSO spawn ids carry the "hunt" keyword (fso_hunt_...) so MoreBotsAPI's HuntManager
    // arms these bots as active hunters — without "hunt" in Id_spawn, the hunt component is
    // never attached and they'd just loiter at their spawn zone.
    private static CompositionBuilder CompositionBase(string zone, string sptIdSuffix, string compositionName)
    {
        return new CompositionBuilder(zone, $"fso_hunt_{compositionName}_{sptIdSuffix}");
    }

    private sealed class CompositionBuilder
    {
        private readonly BossLocationSpawn _spawn;
        private readonly List<BossSupport> _supports = new();

        public CompositionBuilder(string zone, string sptId)
        {
            _spawn = new BossLocationSpawn
            {
                SptId = sptId,
                BossChance = Mod.FsoSpawnChance,
                BossDifficulty = "normal",
                BossEscortDifficulty = "normal",
                BossZone = zone,
                Time = -1,
                Delay = 0,
                ForceSpawn = false,
                IgnoreMaxBots = Mod.FsoIgnoreMaxBots,
                IsBossPlayer = false,
                IsRandomTimeSpawn = false,
                ShowOnTarkovMap = false,
                ShowOnTarkovMapPvE = false,
                SpawnMode = new List<string> { "regular", "pve" },
                DependKarma = false,
                DependKarmaPVE = false,
                TriggerId = "",
                TriggerName = "",
                Supports = _supports,
            };
        }

        public CompositionBuilder WithBoss(string botType)
        {
            _spawn.BossName = botType;
            return this;
        }

        public CompositionBuilder WithPrimaryEscort(string botType, string amount)
        {
            _spawn.BossEscortType = botType;
            _spawn.BossEscortAmount = amount;
            return this;
        }

        public CompositionBuilder WithSupport(string botType, string amount)
        {
            _supports.Add(new BossSupport
            {
                BossEscortType = botType,
                BossEscortAmount = amount,
                BossEscortDifficulty = new ListOrT<string>(new List<string> { "normal" }, null),
            });
            return this;
        }

        public CompositionBuilder WithSupportPair(string typeA, string amountA, string typeB, string amountB)
        {
            return WithSupport(typeA, amountA).WithSupport(typeB, amountB);
        }

        public BossLocationSpawn Build()
        {
            return _spawn;
        }
    }
}