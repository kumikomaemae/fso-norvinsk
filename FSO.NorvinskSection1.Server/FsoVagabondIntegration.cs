using System;
using System.Linq;
using System.Threading.Tasks;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.DI;
using SPTarkov.Server.Core.Models.Utils;
using SPTarkov.Server.Core.Servers;
using Vagabond.Common.Api;

namespace FSO.NorvinskSection1.Server;

// +2 so we initialise AFTER Vagabond (which loads at +1) — its API has to exist before we call it.
[Injectable(TypePriority = OnLoadOrder.PostDBModLoader + 2)]
public sealed class FsoVagabondIntegration : IOnLoad
{
    private const string MaeTraderId = "6a1ac8598933e3f023895bd3";

    private readonly ISptLogger<FsoVagabondIntegration> _logger;
    private readonly SaveServer _saveServer;

    public FsoVagabondIntegration(ISptLogger<FsoVagabondIntegration> logger, SaveServer saveServer)
    {
        _logger = logger;
        _saveServer = saveServer;
    }

    public Task OnLoad()
    {
        // Hideout add moved to FsoGameStartHook (per-login), since Vagabond
        // session state doesn't exist at server-boot time.
        // if (IsVagabondEnabled()) { AddMaeToHideouts(); }
        return Task.CompletedTask;
    }

    // All Vagabond.Common references live in here so this method is only ever JIT-compiled
    // when Vagabond is actually present (otherwise it'd throw a TypeLoadException).
    private void AddMaeToHideouts()
    {
        var count = 0;

        foreach (var profile in _saveServer.GetProfiles())
        {
            var sessionId = profile.Key.ToString();

            // Skip any profile whose Vagabond state hasn't been created yet.
            if (Api.GetState(sessionId) == null)
            {
                continue;
            }

            Api.AddHideoutTraders(sessionId, [MaeTraderId]);
            count++;
        }

        _logger.Success($"[FSO] Mae added to hideout traders for {count} profile(s).");
    }

    private static bool IsVagabondEnabled() =>
        AppDomain.CurrentDomain.GetAssemblies()
            .Any(a => string.Equals(a.GetName().Name, "Vagabond.Common", StringComparison.Ordinal));
}