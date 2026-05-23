using System.Reflection;
using MoreBotsServer.Services;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.DI;
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
    public override SemanticVersioning.Version Version { get; init; } = new("0.2.0");
    public override SemanticVersioning.Range SptVersion { get; init; } = new("~4.0.0");
    public override List<string>? Incompatibilities { get; init; }
    public override Dictionary<string, SemanticVersioning.Range>? ModDependencies { get; init; }
    public override string? Url { get; init; } = "";
    public override bool? IsBundleMod { get; init; }
    public override string License { get; init; } = "MIT";
}

/// <summary>
/// Server mod entry point. Phase 2: registers five FSO bot types into the SPT bot database
/// via MoreBotsAPI's CreateCustomBotTypes pipeline.
/// </summary>
[Injectable(TypePriority = OnLoadOrder.PostDBModLoader + 1)]
public class Mod(
    ISptLogger<Mod> logger,
    MoreBotsCustomBotTypeService customBotTypeService) : IOnLoad
{
    public const string ModName = "FSO: Norvinsk Section 1";
    public const string ModVersion = "0.2.0";

    public async Task OnLoad()
    {
        logger.Info($"[{ModName}] v{ModVersion} loading...");

        // Tell MoreBotsAPI about our enum-name mapping (prepatcher already registered the enums client-side;
        // this teaches the server side which enum IDs belong to which bot names).
        customBotTypeService.AddCustomWildSpawnTypeNames(new Dictionary<int, string>
        {
            { 708300, "fsofixerrookie" },
            { 708301, "fsofixeroperative" },
            { 708302, "fsofixerspecialist" },
            { 708303, "fsofixerlead" },
            { 708304, "fsofixerinnercircle" }
        });

        // Scan db/bots/types/ in our mod folder, deserialize each .json as a BotType,
        // register under the filename (lowercased) into databaseService.GetTables().Bots.Types.
        await customBotTypeService.CreateCustomBotTypes(Assembly.GetExecutingAssembly());

        logger.Success($"[{ModName}] Section Manager Mae reporting. Coffee's hot. Standing by.");
        logger.Info($"[{ModName}] Loaded bot types: {string.Join(", ", customBotTypeService.LoadedBotTypes)}");
    }
}