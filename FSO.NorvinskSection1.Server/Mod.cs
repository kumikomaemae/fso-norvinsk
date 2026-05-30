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
    public override SemanticVersioning.Version Version { get; init; } = new("0.4.0");
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
/// Phase 2h: registers FSO faction relationships.
/// Phase 3: registers FSO spawn rules across 5 maps with 9 squad composition templates.
///
/// FSO is allied to player PMCs (USEC + Bear) and Rogues (anti-BD alignment).
/// FSO is hostile to Scavs, Scav-faction bosses, and Cultists (active threats to civilians).
/// FSO has NO relationship to Goons (mutual ignore — bigger fish to fry).
/// FSO has NO warn behavior — they're professionals on the clock, they don't posture.
/// </summary>
[Injectable(TypePriority = OnLoadOrder.PostDBModLoader + 2)]
public class Mod(
    ISptLogger<Mod> logger,
    MoreBotsCustomBotTypeService customBotTypeService,
    MoreBotsCustomBotConfigService customBotConfigService,
    LoadoutService loadoutService,
    FactionService factionService,
    CustomLocationWaveService waveService,
    LocationConfig locationConfig
) : IOnLoad
{
    public const string ModName = "FSO: Norvinsk Section 1";
    public const string ModVersion = "0.4.0";
    public const string FactionName = "fso";

    private static readonly List<WildSpawnType> FsoBotTypes = new()
    {
        (WildSpawnType)708300, // fsofixerrookie
        (WildSpawnType)708301, // fsofixeroperative
        (WildSpawnType)708302, // fsofixerspecialist
        (WildSpawnType)708303, // fsofixerlead
        (WildSpawnType)708304, // fsofixerinnercircle
    };
    // --- Spawn tuning (FPS / density control) ---
    // Per-squad spawn chance (0-100). Lower = fewer FSO squads per raid = better FPS + more variety.
    // ~40 -> Customs(4 squads) averages ~9 Fixers; Streets(9) ~18; Lighthouse(2) ~5.
    public const int FsoSpawnChance = 40;
    // true  = FSO spawn ON TOP OF the normal bot cap (reliable presence, additive load).
    // false = FSO respect the map's bot cap (hard ceiling on total bots / best FPS, but FSO may not always appear).
    public const bool FsoIgnoreMaxBots = true;
    
    private const string BotRookie = "fsofixerrookie";
    private const string BotOperative = "fsofixeroperative";
    private const string BotSpecialist = "fsofixerspecialist";
    private const string BotLead = "fsofixerlead";
    private const string BotInnerCircle = "fsofixerinnercircle";

    private const string MapStreets = "tarkovstreets";
    private const string MapSandbox = "sandbox";
    private const string MapSandboxHigh = "sandbox_high";
    private const string MapCustoms = "bigmap";
    private const string MapShoreline = "shoreline";
    private const string MapLighthouse = "lighthouse";

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

    // --- Faction setup ---
    RegisterFsoFaction();
    WireFactionRelationships();

    // --- Spawn rules ---
    EnsureSpawnKeysExist();
    WireSpawnRules();
    waveService.ApplyWaveChangesToAllMaps();

    logger.Success($"[{ModName}] Section Manager Mae reporting. Coffee's hot. Standing by.");
    logger.Info($"[{ModName}] Loaded bot types: {string.Join(", ", customBotTypeService.LoadedBotTypes)}");
    logger.Info($"[{ModName}] Faction '{FactionName}' registered with {FsoBotTypes.Count} bot tiers.");
    logger.Info($"[{ModName}] Faction relationships wired: friendly with usec/bear/rogues, hostile to scavs/scavbosses/sectants.");
    logger.Info($"[{ModName}] Spawn rules wired across 5 maps with 9 squad composition templates.");
}

    private void RegisterFsoFaction()
    {
        var fsoFaction = new Faction { Name = FactionName, BotTypes = FsoBotTypes };
        factionService.Factions.Add(fsoFaction.Name, fsoFaction);
    }

    private void WireFactionRelationships()
    {
        factionService.AddFriendlyByFaction(FactionName, "usec");
        factionService.AddFriendlyByFaction(FactionName, "bear");
        factionService.AddFriendlyByFaction("usec", FactionName);
        factionService.AddFriendlyByFaction("bear", FactionName);

        factionService.AddFriendlyByFaction(FactionName, "rogues");
        factionService.AddFriendlyByFaction("rogues", FactionName);

        factionService.AddEnemyByFaction(FactionName, "scavs");
        factionService.AddEnemyByFaction(FactionName, "scavbosses");
        factionService.AddEnemyByFaction("scavs", FactionName);
        factionService.AddEnemyByFaction("scavbosses", FactionName);

        factionService.AddEnemyByFaction(FactionName, "sectants");
        factionService.AddEnemyByFaction("sectants", FactionName);
    }

     private void EnsureSpawnKeysExist()
    {
        locationConfig.CustomWaves ??= new CustomWaves();
        var bossWaves = locationConfig.CustomWaves.Boss;

        var fsoMaps = new[]
        {
            MapStreets,
            MapSandbox,
            MapSandboxHigh,
            MapCustoms,
            MapShoreline,
            MapLighthouse,
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

    private static CompositionBuilder CompositionBase(string zone, string sptIdSuffix, string compositionName)
    {
        return new CompositionBuilder(zone, $"fso_{compositionName}_{sptIdSuffix}");
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