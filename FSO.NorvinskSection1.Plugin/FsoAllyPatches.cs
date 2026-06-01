using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Comfort.Common;
using EFT;
using HarmonyLib;
using SPT.Reflection.Patching;
using UnityEngine;

namespace FSO.NorvinskSection1.Plugin // <-- match your Plugin.cs namespace
{
    /// Shared state: which FSO bots/controllers/groups are friendly to humans.
    public static class FsoAllyState
    {
        public const int FsoRoleMin = 708300; // your 5 Fixer WildSpawnTypes
        public const int FsoRoleMax = 708304;

        public static readonly HashSet<BotOwner> FriendlyBots = new HashSet<BotOwner>();
        public static readonly HashSet<BotEnemiesController> FriendlyControllers = new HashSet<BotEnemiesController>();
        public static readonly HashSet<BotsGroup> FriendlyGroups = new HashSet<BotsGroup>();

        public static bool IsFsoBot(BotOwner bot)
        {
            try
            {
                int role = (int)bot.Profile.Info.Settings.Role;
                return role >= FsoRoleMin && role <= FsoRoleMax;
            }
            catch { return false; }
        }

        public static void MarkFriendly(BotOwner bot, BotsGroup group = null)
        {
            if (bot == null) return;
            FriendlyBots.Add(bot);
            var ctrl = bot.EnemiesController;
            if (ctrl != null) FriendlyControllers.Add(ctrl);
            var grp = group ?? bot.BotsGroup;
            if (grp != null) FriendlyGroups.Add(grp);
        }

        public static void Clear()
        {
            FriendlyBots.Clear();
            FriendlyControllers.Clear();
            FriendlyGroups.Clear();
        }
    }

    /// <summary>
    /// Registers FSO's bot tiers as active HUNTERS with MoreBotsAPI's HuntManager, so they
    /// roam the map and seek enemies instead of loitering at their spawn zones.
    ///
    /// HOW IT WORKS (from the decompiled HuntManager):
    /// - HuntManager keeps two registries: validHuntRoles (Dict<hunterRole, List<huntedAIRole>>)
    ///   and validPMCHunts (Dict<hunterRole, List<EPlayerSide>>).
    /// - A bot only ever gets a hunt component if its spawn Id_spawn contains "hunt" (handled
    ///   server-side: FSO spawns use the "fso_hunt_..." id prefix).
    /// - When such a bot spawns, HuntManager picks a target whose ROLE is in validHuntRoles[hunter]
    ///   (for AI) OR whose SIDE is in validPMCHunts[hunter] (for any player).
    ///
    /// PLAYER SAFETY: we ONLY populate validHuntRoles (AI-by-role). We deliberately do NOT call
    /// AddHuntSides — side-based hunting would make FSO actively path toward the USEC side, i.e.
    /// you and Damjan. By hunting only AI roles (and never adding USEC's role pmcUSEC), FSO can
    /// never hunt-path the player. Enemy BEAR/scavs/bosses/cultists/BD/RUAF are all AI roles.
    ///
    /// We do NOT call HuntManager.InitRaid() — MoreBotsAPI / BlackDiv / RUAF already call it
    /// (it's one shared singleton, and their hunts work). InitRaid subscribes OnBotCreated;
    /// calling it again would double-subscribe and process every bot twice.
    ///
    /// Reflection is used so the plugin needs NO compile-time reference to MoreBotsPlugin.dll —
    /// it can't break the build, and if the API ever changes it fails safe (logs + skips,
    /// hunting just won't arm; hostility/fighting is unaffected since that's server-side).
    /// </summary>
    public static class FsoHuntRegistrar
    {
        // FSO's 5 Fixer WildSpawnType values (the hunters).
        private static readonly int[] FsoHunterRoles = { 708300, 708301, 708302, 708303, 708304 };

        // Enemy AI roles FSO should actively HUNT (seek across the map).
        // Custom faction ints:
        private static readonly int[] CustomHuntedRoles =
        {
            848420, 848421, 848422, 848423, 848424, // Black Division
            848400, 848401, 848402, 848403, 848404, 848405, // RUAF
            848406, // Remnant
        };

        // Vanilla enemy roles, referenced by their EFT WildSpawnType enum NAMES.
        // NOTE: deliberately EXCLUDED for safety/consistency:
        //   - pmcUSEC  (that's you + Damjan -- never hunt USEC)
        //   - exUsec   (Rogues -- kept NEUTRAL per the faction wiring)
        //   - bossKnight / followerBigPipe / followerBirdEye (the Goons -- Rogue-aligned, neutral)
        private static readonly WildSpawnType[] VanillaHuntedRoles =
        {
            // Scavs
            WildSpawnType.assault, WildSpawnType.assaultGroup,
            WildSpawnType.cursedAssault, WildSpawnType.marksman,
            // Raiders (Q5 target) + hostile PMC side
            WildSpawnType.pmcBot, WildSpawnType.pmcBEAR,
            // Scav bosses + their followers
            WildSpawnType.bossKilla, WildSpawnType.bossTagilla, WildSpawnType.bossGluhar,
            WildSpawnType.bossKojaniy, WildSpawnType.bossSanitar, WildSpawnType.bossBully,
            WildSpawnType.followerBully, WildSpawnType.followerKojaniy,
            WildSpawnType.followerGluharAssault, WildSpawnType.followerGluharSecurity,
            WildSpawnType.followerGluharScout, WildSpawnType.followerGluharSnipe,
            WildSpawnType.followerSanitar, WildSpawnType.followerTagilla,
            // Cultists
            WildSpawnType.sectantWarrior, WildSpawnType.sectantPriest,
        };

        private static bool _registeredThisRaid;

        public static void Register()
        {
            try
            {
                // Resolve HuntManager.Instance (MonoBehaviourSingleton<HuntManager>) via reflection.
                var huntManagerType = AccessTools.TypeByName("MoreBotsAPI.Components.HuntManager");
                if (huntManagerType == null)
                {
                    Plugin.Log.LogWarning("(FSO) HuntManager type not found -- MoreBotsAPI missing? Hunting disabled (fighting still works).");
                    return;
                }

                var instance = ResolveSingletonInstance(huntManagerType);
                if (instance == null)
                {
                    Plugin.Log.LogWarning("(FSO) HuntManager.Instance was null at match start -- hunting not registered this raid.");
                    return;
                }

                // Build the hunter + hunted role lists as List<WildSpawnType>.
                var hunters = FsoHunterRoles.Select(i => (WildSpawnType)i).ToList();

                var hunted = new List<WildSpawnType>();
                hunted.AddRange(CustomHuntedRoles.Select(i => (WildSpawnType)i));
                hunted.AddRange(VanillaHuntedRoles);

                // Find the batch overload: AddHuntRoles(List<WildSpawnType> hunters, List<WildSpawnType> hunted)
                var listType = typeof(List<WildSpawnType>);
                var addHuntRoles = huntManagerType.GetMethod(
                    "AddHuntRoles",
                    BindingFlags.Public | BindingFlags.Instance,
                    null,
                    new[] { listType, listType },
                    null);

                if (addHuntRoles == null)
                {
                    Plugin.Log.LogWarning("(FSO) HuntManager.AddHuntRoles(List,List) not found -- API changed? Hunting not registered.");
                    return;
                }

                addHuntRoles.Invoke(instance, new object[] { hunters, hunted });

                // NOTE: intentionally NO AddHuntSides call (player-safety -- see class docs).

                _registeredThisRaid = true;
                Plugin.Log.LogInfo($"(FSO) Hunt roles registered: {hunters.Count} FSO tiers will hunt {hunted.Count} enemy roles (AI-only; player is safe).");
            }
            catch (Exception ex)
            {
                Plugin.Log.LogError($"(FSO) Hunt registration failed (non-fatal, fighting still works): {ex.Message}");
            }
        }

        // MonoBehaviourSingleton<T> exposes a static Instance. Comfort.Common's Singleton<T> uses
        // Instance too. Try the type's own Instance property/field first, then fall back.
        private static object ResolveSingletonInstance(Type huntManagerType)
        {
            // Most likely: a static "Instance" property on the type (inherited from the singleton base).
            var prop = huntManagerType.GetProperty("Instance", BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy);
            if (prop != null)
            {
                var val = prop.GetValue(null);
                if (val != null) return val;
            }

            // Fallback: a static "Instance" field.
            var field = huntManagerType.GetField("Instance", BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy);
            if (field != null)
            {
                var val = field.GetValue(null);
                if (val != null) return val;
            }

            // Last resort: find the live MonoBehaviour in the scene.
            var found = UnityEngine.Object.FindObjectOfType(huntManagerType);
            return found;
        }

        public static void ResetForNewRaid() => _registeredThisRaid = false;
    }

    /// Spin up the enforcer when a raid starts (skip hideout).
    internal class FsoMatchStartedPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
            => AccessTools.Method(typeof(GameWorld), nameof(GameWorld.OnGameStarted));

        [PatchPostfix]
        private static void Postfix(GameWorld __instance)
        {
            if (__instance is HideoutGameWorld) return;
            if (__instance.LocationId != null && __instance.LocationId.ToLower() == "hideout") return;

            FsoAllyState.Clear();
            if (__instance.gameObject.GetComponent<FsoAllyManager>() == null)
                __instance.gameObject.AddComponent<FsoAllyManager>();

            // Register FSO as active hunters for this raid (roam + seek enemies).
            FsoHuntRegistrar.ResetForNewRaid();
            FsoHuntRegistrar.Register();

            Plugin.Log.LogInfo("(FSO) Ally enforcer started.");
        }
    }

    /// Tag every FSO bot the instant it spawns, before its AI can pick an enemy.
    internal class FsoBotSpawnCatcher : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
            => AccessTools.Method(typeof(BotOwner), nameof(BotOwner.PreActivate));

        [PatchPostfix]
        private static void Postfix(BotOwner __instance, BotsGroup group)
        {
            if (FsoAllyState.IsFsoBot(__instance))
            {
                FsoAllyState.MarkFriendly(__instance, group);
                Plugin.Log.LogInfo($"(FSO) Fixer '{__instance.Profile?.Nickname}' marked friendly at spawn.");
            }
        }
    }

    /// Block an FSO bot's enemy controller from adding a HUMAN as an enemy.
    internal class FsoPreventEnemyController : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
            => AccessTools.Method(typeof(BotEnemiesController), nameof(BotEnemiesController.AddNew));

        [PatchPrefix]
        private static bool Prefix(BotEnemiesController __instance, IPlayer enemy)
        {
            if (enemy == null || enemy.IsAI) return true; // let FSO fight scav BOTS
            if (FsoAllyState.FriendlyControllers.Contains(__instance)) return false; // never a human
            return true;
        }
    }

    /// Block an FSO bot's group from adding a HUMAN as an enemy.
    internal class FsoPreventEnemyGroup : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
            => AccessTools.Method(typeof(BotsGroup), nameof(BotsGroup.AddEnemy));

        [PatchPrefix]
        private static bool Prefix(BotsGroup __instance, IPlayer person)
        {
            if (person == null || person.IsAI) return true;
            if (FsoAllyState.FriendlyGroups.Contains(__instance)) return false;
            return true;
        }
    }

    /// Backstop: scrub humans out of FSO enemy lists and register them as allies (assist behavior).
    public class FsoAllyManager : MonoBehaviour
    {
        private void Start() => InvokeRepeating(nameof(Enforce), 1f, 3f);

        private void Enforce()
        {
            var gw = Singleton<GameWorld>.Instance;
            if (gw == null) return;

            var humans = gw.AllAlivePlayersList.Where(p => p != null && !p.IsAI).ToList();
            if (humans.Count == 0) return;

            foreach (var p in gw.AllAlivePlayersList)
            {
                if (p == null || !p.IsAI) continue;
                var bot = p.AIData?.BotOwner;
                if (bot == null || bot.IsDead) continue;
                if (!FsoAllyState.IsFsoBot(bot)) continue;

                FsoAllyState.MarkFriendly(bot);

                foreach (var human in humans)
                {
                    try
                    {
                        var ec = bot.EnemiesController;
                        if (ec?.EnemyInfos != null && ec.EnemyInfos.ContainsKey(human))
                            ec.EnemyInfos.Remove(human);

                        var grp = bot.BotsGroup;
                        if (grp != null)
                        {
                            if (grp.Enemies != null && grp.Enemies.ContainsKey(human))
                                grp.RemoveEnemy(human, (EBotEnemyCause)1);
                            if (!grp.IsAlly(human))
                                grp.AddAlly(human);
                        }
                    }
                    catch (Exception ex) { Plugin.Log.LogError($"(FSO) enforce error: {ex.Message}"); }
                }
            }
        }

        private void OnDestroy()
        {
            CancelInvoke();
            FsoAllyState.Clear();
        }
    }
}