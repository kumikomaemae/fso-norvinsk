using System.Reflection;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.DI;
using SPTarkov.Server.Core.Helpers;
using SPTarkov.Server.Core.Models.Eft.Common.Tables;
using SPTarkov.Server.Core.Models.Spt.Config;
using SPTarkov.Server.Core.Models.Utils;
using SPTarkov.Server.Core.Routers;
using SPTarkov.Server.Core.Utils;

namespace FSO.NorvinskSection1.Server;

/// <summary>
/// Registers the FSO Section Manager, "Mae", as a custom trader.
/// Mirrors WTT-Artem's flow, SPT core only (WTT-CommonLib gets wired in for items + quests).
/// Reads db/trader/base.json + db/trader/assort.json, routes her avatar from
/// db/trader/res/&lt;id&gt;.jpg, registers her, wires locales, lists her on the flea.
/// </summary>
[Injectable(TypePriority = OnLoadOrder.PostDBModLoader + 2)]
public class FsoTrader(
    ISptLogger<FsoTrader> logger,
    ModHelper modHelper,
    ImageRouter imageRouter,
    TraderConfig traderConfig,
    RagfairConfig ragfairConfig,
    TimeUtil timeUtil,
    FsoTraderHelper traderHelper
) : IOnLoad
{
    /// <summary>Mae's trader id (MongoID). Also her res/ image filename + locale key prefix.</summary>
    public const string TraderId = "6a1ac8598933e3f023895bd3";

    private const string TraderFirstName = "Mae";

    /// <summary>Shown on her trader card. Kept matched to base.json "description". Final wording = writing phase.</summary>
    private const string TraderDescription =
        "FSO Section Manager. Runs the Office's regional contract for the LCCB \u2014 " +
        "investigation, cleanup, and whatever the contract demands. Works by comms and " +
        "couriers; you won't find her in the field. Coffee's hot, figuratively. Don't die.";

    public async Task OnLoad()
    {
        var pathToMod = modHelper.GetAbsolutePathToModFolder(Assembly.GetExecutingAssembly());

        // 1. Load Mae's trader definition.
        var traderBase = modHelper.GetJsonDataFromFile<TraderBase>(pathToMod, "db/trader/base.json")!;

        // 2. Route her avatar. base.json Avatar = "/files/trader/avatar/<id>.jpg";
        //    AddRoute strips ".jpg" and points the route at the real file on disk.
        var traderImagePath = System.IO.Path.Combine(pathToMod, $"db/trader/res/{TraderId}.jpg");
        imageRouter.AddRoute(traderBase.Avatar!.Replace(".jpg", ""), traderImagePath);

        // 3. Restock window (1h–2h).
        traderHelper.SetTraderUpdateTime(
            traderConfig, traderBase,
            timeUtil.GetHoursAsSeconds(1),
            timeUtil.GetHoursAsSeconds(2));

        // 4. List her offers on the flea.
        ragfairConfig.Traders.TryAdd(traderBase.Id, true);

        // 5. Register her (empty assort) + wire her locale strings.
        traderHelper.AddTraderWithEmptyAssortToDb(traderBase);
        traderHelper.AddTraderToLocales(traderBase, TraderFirstName, TraderDescription);

        // 6. Load + apply the real assort (empty in 3a; filled in 3b).
        var assort = modHelper.GetJsonDataFromFile<TraderAssort>(pathToMod, "db/trader/assort.json")!;
        traderHelper.OverwriteTraderAssort(traderBase.Id, assort);

        logger.Success("[FSO] Trader 'Mae' registered \u2014 the Office is open. Coffee's hot.");
        await Task.CompletedTask;
    }
}