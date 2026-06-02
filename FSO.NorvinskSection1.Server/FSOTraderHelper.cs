using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Eft.Common.Tables;
using SPTarkov.Server.Core.Models.Spt.Config;
using SPTarkov.Server.Core.Models.Utils;
using SPTarkov.Server.Core.Services;
using SPTarkov.Server.Core.Utils.Cloners;
using SPTarkov.Server.Core.Utils.Json;

namespace FSO.NorvinskSection1.Server;

/// <summary>
/// Trader-registration helper for the FSO Section Manager, "Mae".
/// Port of WTT-Artem's helper (the proven SPT 4.x custom-trader pattern), SPT core only.
/// </summary>
[Injectable]
public class FsoTraderHelper(
    ISptLogger<FsoTraderHelper> logger,
    ICloner cloner,
    DatabaseService databaseService)
{
    /// <summary>Registers Mae's stock-refresh window (min/max seconds) in the trader config.</summary>
    public void SetTraderUpdateTime(
        TraderConfig traderConfig,
        TraderBase baseJson,
        int refreshTimeSecondsMin,
        int refreshTimeSecondsMax)
    {
        var updateTime = new UpdateTime
        {
            TraderId = baseJson.Id,
            Seconds = new MinMax<int>(refreshTimeSecondsMin, refreshTimeSecondsMax)
        };
        traderConfig.UpdateTime.Add(updateTime);
    }

    /// <summary>Adds the trader to the DB with an empty assort + the three quest-assort buckets.</summary>
    public void AddTraderWithEmptyAssortToDb(TraderBase traderDetailsToAdd)
    {
        var traderAssort = new TraderAssort
        {
            Items = new List<Item>(),
            BarterScheme = new Dictionary<MongoId, List<List<BarterScheme>>>(),
            LoyalLevelItems = new Dictionary<MongoId, int>()
        };

        var trader = new Trader
        {
            Assort = traderAssort,
            Base = cloner.Clone<TraderBase>(traderDetailsToAdd)!,
            QuestAssort = new Dictionary<string, Dictionary<MongoId, MongoId>>
            {
                { "Started", new Dictionary<MongoId, MongoId>() },
                { "Success", new Dictionary<MongoId, MongoId>() },
                { "Fail",    new Dictionary<MongoId, MongoId>() }
            },
            Dialogue = new Dictionary<string, List<string>?>()
        };

        databaseService.GetTables().Traders.TryAdd(traderDetailsToAdd.Id, trader);
    }

    /// <summary>
    /// Adds Mae's display strings to every locale via the lazy-load transformer.
    /// Indexer assignment (not .Add) so a re-run can't throw on an existing key.
    /// </summary>
    public void AddTraderToLocales(TraderBase baseJson, string firstName, string description)
    {
        var globalLocales = databaseService.GetTables().Locales?.Global;
        if (globalLocales is null)
        {
            logger.Warning("[FSO] Global locales unavailable \u2014 skipping Mae's locale registration.");
            return;
        }

        MongoId traderId = baseJson.Id;
        var fullName = baseJson.Name ?? firstName;
        var nickName = baseJson.Nickname ?? firstName;
        var location = baseJson.Location ?? string.Empty;

        foreach (var (_, lazyLoadedLocale) in globalLocales)
        {
            if (lazyLoadedLocale is null)
            {
                continue;
            }

            lazyLoadedLocale.AddTransformer(localeData =>
            {
                localeData ??= new Dictionary<string, string>();   // null-guard so the analyzer stops fretting
                localeData[$"{traderId} FullName"]    = fullName;
                localeData[$"{traderId} FirstName"]   = firstName;
                localeData[$"{traderId} Nickname"]    = nickName;
                localeData[$"{traderId} Location"]    = location;
                localeData[$"{traderId} Description"] = description;
                return localeData;
            });
        }
    }

    /// <summary>Replaces Mae's assort with the freshly-loaded one.</summary>
    public void OverwriteTraderAssort(MongoId traderId, TraderAssort newAssort)
    {
        if (!databaseService.GetTables().Traders.TryGetValue(traderId, out var trader))
        {
            logger.Warning($"[FSO] Could not update assort for trader {traderId.ToString()} \u2014 not found on the server.");
            return;
        }
        trader.Assort = newAssort;
    }
}