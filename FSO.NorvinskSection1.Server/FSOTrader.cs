using System.Reflection;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.DI;
using SPTarkov.Server.Core.Helpers;
using SPTarkov.Server.Core.Models.Eft.Common.Tables;
using SPTarkov.Server.Core.Models.Spt.Config;
using SPTarkov.Server.Core.Models.Utils;
using SPTarkov.Server.Core.Routers;
using SPTarkov.Server.Core.Servers; // ConfigServer lives here — re-added for the fix
using SPTarkov.Server.Core.Utils;

namespace FSO.NorvinskSection1.Server;

/// <summary>
/// Registers the FSO Section Manager, "Mae", as a custom trader.
/// SPT core for trader registration; WTT-CommonLib loads her quests.
/// </summary>
[Injectable(TypePriority = OnLoadOrder.PostDBModLoader + 2)]
public class FsoTrader(
    ISptLogger<FsoTrader> logger,
    ModHelper modHelper,
    ImageRouter imageRouter,
    ConfigServer configServer,
    TimeUtil timeUtil,
    FsoTraderHelper traderHelper,
    WTTServerCommonLib.WTTServerCommonLib wttCommon
) : IOnLoad
{
    public const string TraderId = "6a1ac8598933e3f023895bd3";

    private const string TraderFirstName = "Mae";

    private const string TraderDescription =
        "FSO Section Manager. Runs the Office's regional contract for the LCCB \u2014 " +
        "investigation, cleanup, and whatever the contract demands. Works by comms and " +
        "couriers; you won't find her in the field. Coffee's hot, figuratively. Don't die.";

    private readonly TraderConfig _traderConfig = configServer.GetConfig<TraderConfig>();
    private readonly RagfairConfig _ragfairConfig = configServer.GetConfig<RagfairConfig>();

    public async Task OnLoad()
    {
        var pathToMod = modHelper.GetAbsolutePathToModFolder(Assembly.GetExecutingAssembly());

        // 1. Load Mae's trader definition.
        var traderBase = modHelper.GetJsonDataFromFile<TraderBase>(pathToMod, "db/trader/base.json")!;

        // 2. Route her avatar.
        var traderImagePath = System.IO.Path.Combine(pathToMod, $"db/trader/res/{TraderId}.jpg");
        imageRouter.AddRoute(traderBase.Avatar!.Replace(".jpg", ""), traderImagePath);

        // 3. Restock window (1h–2h).
        traderHelper.SetTraderUpdateTime(
            _traderConfig, traderBase,
            timeUtil.GetHoursAsSeconds(1),
            timeUtil.GetHoursAsSeconds(2));

        // 4. List her offers on the flea.
        _ragfairConfig.Traders.TryAdd(traderBase.Id, true);

        // 5. Register her (empty assort) + wire her locale strings.
        traderHelper.AddTraderWithEmptyAssortToDb(traderBase);
        traderHelper.AddTraderToLocales(traderBase, TraderFirstName, TraderDescription);

        // 6. Load + apply the real assort.
        var assort = modHelper.GetJsonDataFromFile<TraderAssort>(pathToMod, "db/trader/assort.json")!;
        traderHelper.OverwriteTraderAssort(traderBase.Id, assort);

        // 7. Load Mae's custom quests from db/CustomQuests/<traderId>/ via WTT-CommonLib.
        await wttCommon.CustomQuestService.CreateCustomQuests(Assembly.GetExecutingAssembly());

        // 8. Register Mae's custom clothing from db/CustomClothing/ via WTT-CommonLib.
        //    Gives her a suits collection (stops the GetTraderSuits crash loop) and
        //    sells the FSO Fixer Suit, unlocked by completing Quest 1.
        await wttCommon.CustomClothingService.CreateCustomClothing(Assembly.GetExecutingAssembly());

        // 9. Register custom global locales (kill-feed faction name, etc.) from db/CustomLocales/
        await wttCommon.CustomLocaleService.CreateCustomLocales(Assembly.GetExecutingAssembly());

        logger.Success("[FSO] Trader 'Mae' registered \u2014 the Office is open. Coffee's hot.");
    }
}