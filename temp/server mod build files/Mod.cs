using System.Reflection;
using System.Text.Json;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.DI;
using SPTarkov.Server.Core.Helpers;
using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Eft.Common;
using SPTarkov.Server.Core.Models.Eft.Common.Tables;
using SPTarkov.Server.Core.Models.Logging;
using SPTarkov.Server.Core.Models.Spt.Mod;
using SPTarkov.Server.Core.Models.Utils;
using SPTarkov.Server.Core.Services;

namespace Soppo.Character.Server;

public record ModMetadata : AbstractModMetadata
{
    public override string ModGuid { get; init; } = "com.mae.soppo.character";
    public override string Name { get; init; } = "Project Soppo — SOPMOD II";
    public override string Author { get; init; } = "Mae";
    public override List<string>? Contributors { get; init; } = ["Damjan"];
    public override SemanticVersioning.Version Version { get; init; } = new("0.2.2");
    public override SemanticVersioning.Range SptVersion { get; init; } = new("~4.0.0");
    public override List<string>? Incompatibilities { get; init; }
    public override Dictionary<string, SemanticVersioning.Range>? ModDependencies { get; init; }
    public override string? Url { get; init; } = "";
    public override bool? IsBundleMod { get; init; } = true;
    public override string License { get; init; } = "MIT";
}

/// <summary>
/// Project Soppo v0.2.2 — SOPMOD II (GFL2, Redline Ranger) as playable character customization.
///   HEAD + VOICE → character creation · BODY+HANDS / FEET → Ragman suites
///   v0.2.2: voice Prefab reverted to string (client contract); in-raid voice audio requires the client plugin (next step).
/// </summary>
[Injectable(TypePriority = OnLoadOrder.PostDBModLoader + 12)]
public class Mod(
    ISptLogger<Mod> logger,
    DatabaseService databaseService,
    ModHelper modHelper
) : IOnLoad
{
    public const string ModName = "Project Soppo";
    public const string VoiceName = "Soppo";

    private static readonly MongoId BodyId = new("d011000000000000000000b0");
    private static readonly MongoId FeetId = new("d011000000000000000000fe");
    private static readonly MongoId HandsId = new("d011000000000000000000aa");
    private static readonly MongoId HeadId = new("d011000000000000000000ea");
    private static readonly MongoId VoiceId = new("d011000000000000000000ce");
    private static readonly MongoId UpperSuiteId = new("d011000000000000000000c1");
    private static readonly MongoId LowerSuiteId = new("d011000000000000000000c2");
    private static readonly MongoId UpperOfferId = new("d011000000000000000000f1");
    private static readonly MongoId LowerOfferId = new("d011000000000000000000f2");

    private const string ParentBody = "5cc0868e14c02e000c6bea68";
    private const string ParentFeet = "5cc0869814c02e000a4cad94";
    private const string ParentHands = "5cc086a314c02e000c6bea69";
    private const string ParentHead = "5cc085e214c02e000c6bea67";
    private const string ParentVoice = "5fc100cf95572123ae738483";
    private const string ParentUpperSuite = "5cd944ca1388ce03a44dc2a4";
    private const string ParentLowerSuite = "5cd944d01388ce000a659df9";

    private const string RagmanId = "5ac3b934156ae10c4430e83c";
    private const string RoublesTpl = "5449016a4bdc2d6f028b456f";

    private const string KeyBody = "assets/content/characters/soppo/soppo_body.bundle";
    private const string KeyFeet = "assets/content/characters/soppo/soppo_leg.bundle";
    private const string KeyHands = "assets/content/characters/soppo/soppo_hands.bundle";
    private const string KeyHead = "assets/content/characters/soppo/soppo_head.bundle";
    private const string KeyVoice = "assets/content/audio/phrases/soppo_voice.bundle";

    private static readonly JsonSerializerOptions ConfigJsonOptions = new()
    {
        ReadCommentHandling = JsonCommentHandling.Skip,
        AllowTrailingCommas = true,
        PropertyNameCaseInsensitive = true
    };

    public Task OnLoad()
    {
        var config = LoadConfig();
        if (config is null || !config.Enabled)
        {
            logger.Warning($"[{ModName}] disabled or config missing — Soppo stays in the dorm.");
            return Task.CompletedTask;
        }

        var customization = databaseService.GetTemplates().Customization;
        var storage = databaseService.GetTemplates().CustomisationStorage;

        RegisterPart(customization, BodyId, "soppo_body", ParentBody, "Body", KeyBody, config.Sides,
            "SOPMOD II — Redline Ranger", "Soppo", "M4 SOPMOD II, Redline Ranger outfit. Griffin's finest, on loan to Norvinsk.");
        RegisterPart(customization, FeetId, "soppo_feet", ParentFeet, "Feet", KeyFeet, config.Sides,
            "SOPMOD II Legs — Redline Ranger", "Soppo legs", "Redline Ranger lower body.");
        RegisterPart(customization, HandsId, "soppo_hands", ParentHands, "Hands", KeyHands, config.Sides,
            "SOPMOD II Hands — Redline Ranger", "Soppo hands", "First-person Redline Ranger arms.");
        RegisterPart(customization, HeadId, "soppo_head", ParentHead, "Head", KeyHead, config.Sides,
            "SOPMOD II Head", "Soppo head", "SOPMOD II. Select at character creation.");

        RegisterSuite(customization, UpperSuiteId, "soppo_kit_upper_redline", ParentUpperSuite, config.Sides,
            "Redline Ranger (Upper)", body: BodyId, hands: HandsId, feet: null);
        RegisterSuite(customization, LowerSuiteId, "soppo_kit_lower_redline", ParentLowerSuite, config.Sides,
            "Redline Ranger (Lower)", body: null, hands: null, feet: FeetId);

        if (config.EnableVoice)
        {
            RegisterVoice(customization, config.Sides);
            storage.Add(new CustomisationStorage
            {
                Id = VoiceId,
                Source = CustomisationSource.DEFAULT,
                Type = CustomisationType.VOICE
            });
        }

        if (config.UnlockByDefault)
        {
            storage.Add(new CustomisationStorage { Id = UpperSuiteId, Source = CustomisationSource.DEFAULT, Type = CustomisationType.SUITE });
            storage.Add(new CustomisationStorage { Id = LowerSuiteId, Source = CustomisationSource.DEFAULT, Type = CustomisationType.SUITE });
        }

        if (config.AddRagmanOffers)
        {
            AddRagmanOffer(UpperOfferId, UpperSuiteId, config.PriceRoubles);
            AddRagmanOffer(LowerOfferId, LowerSuiteId, config.PriceRoubles);
        }

        AddLocales(config.EnableVoice);

        logger.LogWithColor(
            $"[{ModName}] SOPMOD II registered: head{(config.EnableVoice ? " + voice" : "")} at character creation, " +
            $"Redline Ranger suites at Ragman. Griffin thanks the Office for its hospitality.",
            LogTextColor.Magenta);
        return Task.CompletedTask;
    }

    // -----------------------------------------------------------------------------------

    private void RegisterPart(Dictionary<MongoId, CustomizationItem> customization, MongoId id, string name,
        string parent, string bodyPart, string bundleKey, List<string> sides,
        string dispName, string shortName, string description)
    {
        customization[id] = new CustomizationItem
        {
            Id = id,
            Name = name,
            Parent = new MongoId(parent),
            Type = "Item",
            Properties = new CustomizationProperties
            {
                Name = dispName,
                ShortName = shortName,
                Description = description,
                Side = sides,
                BodyPart = bodyPart,
                IntegratedArmorVest = false,
                AvailableAsDefault = true,
                ProfileVersions = [],
                Prefab = new Dictionary<string, string> { ["path"] = bundleKey, ["rcid"] = "" },
                WatchPrefab = new Prefab { Path = "", Rcid = "" },
                WatchPosition = new() { X = 0, Y = 0, Z = 0 },
                WatchRotation = new() { X = 0, Y = 0, Z = 0 }
            }
        };
        logger.Info($"[{ModName}] registered {bodyPart}: {dispName}");
    }

    private void RegisterSuite(Dictionary<MongoId, CustomizationItem> customization, MongoId id, string name,
        string parent, List<string> sides, string dispName, MongoId? body, MongoId? hands, MongoId? feet)
    {
        customization[id] = new CustomizationItem
        {
            Id = id,
            Name = name,
            Parent = new MongoId(parent),
            Type = "Item",
            Properties = new CustomizationProperties
            {
                Name = dispName,
                ShortName = dispName,
                Description = dispName,
                Side = sides,
                Body = body,
                Hands = hands,
                Feet = feet,
                AvailableAsDefault = true,
                ProfileVersions = []
            }
        };
        logger.Info($"[{ModName}] registered suite: {dispName}");
    }

    private void RegisterVoice(Dictionary<MongoId, CustomizationItem> customization, List<string> sides)
    {
        customization[VoiceId] = new CustomizationItem
        {
            Id = VoiceId,
            Name = VoiceName,
            Parent = new MongoId(ParentVoice),
            Type = "Item",
            Properties = new CustomizationProperties
            {
                Name = VoiceName,
                ShortName = VoiceName,
                Description = "SOPMOD II (JP). Griffin-issue vocal cords.",
                Side = sides,
                IsNotRandom = true,
                AvailableAsDefault = true,
                ProfileVersions = [],
                // Voice Prefab MUST be a string (client types it as string; object form crashes /client/customization parse)
                Prefab = VoiceName
            }
        };

        var custGlobals = databaseService.GetGlobals().Configuration.Customization;
        var voices = custGlobals.VoiceOptions?.ToList() ?? [];
        voices.Add(new CustomizationVoice { Voice = VoiceName, Side = sides, IsNotRandom = true });
        custGlobals.VoiceOptions = voices;

        logger.Info($"[{ModName}] registered voice: {VoiceName}");
    }

    private void AddLocales(bool voiceEnabled)
    {
        var entries = new List<(MongoId id, string name, string shortName, string desc)>
        {
            (BodyId, "SOPMOD II — Redline Ranger", "Soppo", "M4 SOPMOD II, Redline Ranger outfit."),
            (FeetId, "SOPMOD II Legs — Redline Ranger", "Soppo legs", "Redline Ranger lower body."),
            (HandsId, "SOPMOD II Hands — Redline Ranger", "Soppo hands", "First-person Redline Ranger arms."),
            (HeadId, "SOPMOD II Head", "Soppo head", "SOPMOD II."),
            (UpperSuiteId, "Redline Ranger (Upper)", "Redline Upper", "SOPMOD II Redline Ranger — top half."),
            (LowerSuiteId, "Redline Ranger (Lower)", "Redline Lower", "SOPMOD II Redline Ranger — bottom half.")
        };
        if (voiceEnabled)
        {
            entries.Add((VoiceId, "Soppo", "Soppo", "SOPMOD II (JP)."));
        }

        foreach (var (_, lazyLocale) in databaseService.GetLocales().Global)
        {
            lazyLocale.AddTransformer(localeData =>
            {
                if (localeData is null)
                {
                    return localeData;
                }
                foreach (var (id, name, shortName, desc) in entries)
                {
                    localeData[$"{id} Name"] = name;
                    localeData[$"{id} ShortName"] = shortName;
                    localeData[$"{id} Description"] = desc;
                }
                return localeData;
            });
        }
    }

    private void AddRagmanOffer(MongoId offerId, MongoId suiteId, double priceRoubles)
    {
        if (!databaseService.GetTraders().TryGetValue(new MongoId(RagmanId), out var ragman) || ragman is null)
        {
            logger.Error($"[{ModName}] Ragman not found — no clothing offers added.");
            return;
        }

        ragman.Suits ??= [];
        ragman.Suits.Add(new Suit
        {
            Id = offerId,
            Tid = new MongoId(RagmanId),
            SuiteId = suiteId,
            IsActive = true,
            IsHiddenInPVE = false,
            ExternalObtain = false,
            InternalObtain = true,
            Requirements = new SuitRequirements
            {
                LoyaltyLevel = 1,
                ProfileLevel = 1,
                Standing = 0,
                SkillRequirements = [],
                QuestRequirements = [],
                AchievementRequirements = [],
                RequiredTid = new MongoId(RagmanId),
                ItemRequirements = priceRoubles > 0
                    ? [new ItemRequirement { Count = priceRoubles, Tpl = new MongoId(RoublesTpl), OnlyFunctional = true, Type = "ItemRequirement" }]
                    : []
            }
        });
        logger.Info($"[{ModName}] Ragman clothing offer added for suite {suiteId}");
    }

    private SoppoConfig? LoadConfig()
    {
        try
        {
            var modPath = modHelper.GetAbsolutePathToModFolder(Assembly.GetExecutingAssembly());
            var raw = File.ReadAllText(System.IO.Path.Combine(modPath, "config.jsonc"));
            return JsonSerializer.Deserialize<SoppoConfig>(raw, ConfigJsonOptions);
        }
        catch (Exception ex)
        {
            logger.Error($"[{ModName}] Failed to load config.jsonc: {ex.Message}");
            return null;
        }
    }
}

public class SoppoConfig
{
    public bool Enabled { get; set; } = true;
    public bool UnlockByDefault { get; set; } = true;
    public bool AddRagmanOffers { get; set; } = true;
    public bool EnableVoice { get; set; } = true;
    public double PriceRoubles { get; set; } = 0;
    public List<string> Sides { get; set; } = ["Usec", "Bear"];
}
