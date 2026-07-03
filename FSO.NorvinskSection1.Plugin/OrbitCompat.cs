using System;
using System.Reflection;
using BepInEx.Bootstrap;
using BepInEx.Logging;

namespace FSO.NorvinskSection1.Plugin
{
    /// <summary>
    /// ORBIT compatibility — FSO declares sovereignty over its own bots.
    ///
    /// ORBIT's OrbitBrainLayer registers on the standard PMC brain names and, by its own
    /// admission, hijacks any custom faction whose bots use BaseBrain="PMC" by default.
    /// Our fixers run PMC brains, but their behavior belongs to their own custom layers
    /// (hunt / escort) — an ally who accepts a loot-and-extract mission mid-quest is a bug
    /// with emotional stakes.
    ///
    /// ORBIT exposes a public opt-out for exactly this: OrbitBrainLayer.AddExcludedRoleSubstring().
    /// We call it via reflection so FSO carries no hard reference to ORBIT and behaves
    /// identically whether or not ORBIT is installed. Uses the same mechanism as ORBIT's own
    /// built-in toggles for RUAF / BlackDivision / UNTAR — we're just adding ourselves to a
    /// list the author already maintains for factions he knows about.
    /// </summary>
    internal static class OrbitCompat
    {
        public const string OrbitGuid = "com.chazut.orbit";

        // Matches all five fsofixer* roles plus any future fso* types in one stroke.
        // ORBIT's match is case-insensitive substring on the WildSpawnType name, and no
        // vanilla or known mod role contains "fso".
        private const string RoleSubstring = "fso";

        public static void Apply(ManualLogSource log)
        {
            try
            {
                if (!Chainloader.PluginInfos.ContainsKey(OrbitGuid))
                {
                    return; // ORBIT not installed — nothing to do, nothing to log.
                }

                var layerType = Type.GetType("Orbit.Brain.OrbitBrainLayer, ORBIT", throwOnError: false);
                var method = layerType?.GetMethod(
                    "AddExcludedRoleSubstring",
                    BindingFlags.Public | BindingFlags.Static);

                if (method is null)
                {
                    log.LogWarning(
                        "[FSO] ORBIT detected, but OrbitBrainLayer.AddExcludedRoleSubstring wasn't found — " +
                        "ORBIT's internals may have changed. FSO bots may get hijacked until compat is updated.");
                    return;
                }

                method.Invoke(null, new object[] { RoleSubstring });
                log.LogInfo(
                    $"[FSO] ORBIT detected — role substring '{RoleSubstring}' excluded from takeover. " +
                    "Fixers keep their own brains. No freelance extractions.");
            }
            catch (Exception ex)
            {
                // Compat must never take the faction down with it.
                log.LogWarning($"[FSO] ORBIT compat failed non-fatally: {ex.Message}");
            }
        }
    }
}
