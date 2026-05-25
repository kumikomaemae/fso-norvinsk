using System.Reflection;
using MoreBotsServer.Models;
using MoreBotsServer.Services;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.DI;
using SPTarkov.Server.Core.Models.Eft.Common;
using SPTarkov.Server.Core.Models.Enums;
using SPTarkov.Server.Core.Models.Spt.Mod;
using SPTarkov.Server.Core.Models.Utils;

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
    public override SemanticVersioning.Version Version { get; init; } = new("0.3.0");
    public override SemanticVersioning.Range SptVersion { get; init; } = new("~4.0.0");
    public override List<string>? Incompatibilities { get; init; }
    public override Dictionary<string, SemanticVersioning.Range>? ModDependencies { get; init; }
    public override string? Url { get; init; } = "";
    public override bool? IsBundleMod { get; init; }
    public override string License { get; init; } = "MIT";
}

/// <summary>
/// Server mod entry point. Phase 2h: registers FSO faction relationships.
/// 
/// FSO is allied to player PMCs (USEC + Bear), hostile to Scavs and Scav-faction bosses,
/// and deliberately has NO warn behavior toward anyone (in-character: they're professionals
/// on the clock, they don't posture, they just work).
/// </summary>
[Injectable(TypePriority = OnLoadOrder.PostDBModLoader + 1)]
public class Mod(
    ISptLogger<Mod> logger,
    MoreBotsCustomBotTypeService customBotTypeService,
    FactionService factionService) : IOnLoad
{
    public const string ModName = "FSO: Norvinsk Section 1";
    public const string ModVersion = "0.3.0";
    public const string FactionName = "fso";

    // FSO's five tier WildSpawnType enum values (registered via the prepatcher).
    private static readonly List<WildSpawnType> FsoBotTypes = new()
    {
        (WildSpawnType)708300,  // fsofixerrookie
        (WildSpawnType)708301,  // fsofixeroperative
        (WildSpawnType)708302,  // fsofixerspecialist
        (WildSpawnType)708303,  // fsofixerlead
        (WildSpawnType)708304,  // fsofixerinnercircle
    };

    public async Task OnLoad()
    {
        logger.Info($"[{ModName}] v{ModVersion} loading...");

        // Tell MoreBotsAPI about our enum-name mapping (prepatcher registered the enums client-side;
        // this teaches the server side which enum IDs belong to which bot names).
        customBotTypeService.AddCustomWildSpawnTypeNames(new Dictionary<int, string>
        {
            { 708300, "fsofixerrookie" },
            { 708301, "fsofixeroperative" },
            { 708302, "fsofixerspecialist" },
            { 708303, "fsofixerlead" },
            { 708304, "fsofixerinnercircle" }
        });

        // Load bot data JSONs from db/bots/types/ into the SPT bot database.
        await customBotTypeService.CreateCustomBotTypes(Assembly.GetExecutingAssembly());

        // Register FSO as a faction containing all five of our bot tiers.
        RegisterFsoFaction();

        // Wire faction relationships.
        WireFactionRelationships();

        logger.Success($"[{ModName}] Section Manager Mae reporting. Coffee's hot. Standing by.");
        logger.Info($"[{ModName}] Loaded bot types: {string.Join(", ", customBotTypeService.LoadedBotTypes)}");
        logger.Info($"[{ModName}] Faction '{FactionName}' registered with {FsoBotTypes.Count} bot tiers.");
        logger.Info($"[{ModName}] Faction relationships wired: friendly with usec/bear, hostile to scavs + scavbosses.");
    }

    /// <summary>
    /// Add our FSO faction to MoreBotsAPI's faction dictionary.
    /// FactionService doesn't expose an AddFaction() method, but its Factions dictionary
    /// is publicly accessible for direct insertion.
    /// </summary>
    private void RegisterFsoFaction()
    {
        var fsoFaction = new Faction
        {
            Name = FactionName,
            BotTypes = FsoBotTypes
        };
        factionService.Factions.Add(fsoFaction.Name, fsoFaction);
    }

    /// <summary>
    /// Configure FSO's relationships with vanilla EFT factions.
    /// 
    /// Friendly (both directions): usec, bear
    ///   - covers PMC player too — since players spawn as PMC, "friendly to usec/bear" includes us
    /// Enemy (both directions): scavs, scavbosses
    ///   - FSO is the "good guys" of the design doc; hostile to Tarkov's lawless factions
    /// Warn: NONE
    ///   - in-character: FSO are professionals on the clock, they don't posture
    ///   - mechanical: warn behavior can't distinguish player PMC from AI PMC, so warning
    ///     PMCs would also warn the player. Skipping warn entirely keeps FSO non-intrusive.
    /// </summary>
    private void WireFactionRelationships()
    {
        // FSO won't attack player PMCs
        factionService.AddFriendlyByFaction(FactionName, "usec");
        factionService.AddFriendlyByFaction(FactionName, "bear");
        // Player PMC bots won't attack FSO
        factionService.AddFriendlyByFaction("usec", FactionName);
        factionService.AddFriendlyByFaction("bear", FactionName);

        // FSO will attack Scavs (regular Scavs) and Scav bosses (Reshala, Killa, Gluhar, etc.)
        factionService.AddEnemyByFaction(FactionName, "scavs");
        factionService.AddEnemyByFaction(FactionName, "scavbosses");
        // Scavs and Scav bosses will attack FSO
        factionService.AddEnemyByFaction("scavs", FactionName);
        factionService.AddEnemyByFaction("scavbosses", FactionName);
    }
}