using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.DI;
using SPTarkov.Server.Core.Models.Enums;
using SPTarkov.Server.Core.Models.Spt.Mod;
using SPTarkov.Server.Core.Models.Utils;

namespace FSO.NorvinskSection1.Server;

/// <summary>
/// Mod metadata. SPT 4.x discovers and registers our mod via this record at boot.
/// Required even for the simplest mod — without it, the loader doesn't know we exist.
/// </summary>
public record ModMetadata : AbstractModMetadata
{
    public override string ModGuid { get; init; } = "com.mae.fso.norvinsksection1";
    public override string Name { get; init; } = "FSO: Norvinsk Section 1";
    public override string Author { get; init; } = "Mae";
    public override List<string>? Contributors { get; init; }
    public override SemanticVersioning.Version Version { get; init; } = new("0.1.0");
    public override SemanticVersioning.Range SptVersion { get; init; } = new("~4.0.0");
    public override List<string>? Incompatibilities { get; init; }
    public override Dictionary<string, SemanticVersioning.Range>? ModDependencies { get; init; }
    public override string? Url { get; init; } = "";
    public override bool? IsBundleMod { get; init; }
    public override string License { get; init; } = "MIT";
}

/// <summary>
/// Server mod entry point.
/// 
/// TypePriority = PostDBModLoader + 1 means: load AFTER SPT finishes setting up its
/// in-memory database. We need this because future phases (bots, traders, quests)
/// will read and write DB entries — they can't run before the DB exists.
/// </summary>
[Injectable(TypePriority = OnLoadOrder.PostDBModLoader + 1)]
public class Mod(ISptLogger<Mod> logger) : IOnLoad
{
    public const string ModName = "FSO: Norvinsk Section 1";
    public const string ModVersion = "0.1.0";

    public Task OnLoad()
    {
        logger.Info($"[{ModName}] v{ModVersion} loading...");
        logger.Info($"[{ModName}] Section Manager Mae reporting. Coffee's hot. Standing by.");
        logger.Info($"[{ModName}] Mod loaded successfully.");
        return Task.CompletedTask;
    }
}