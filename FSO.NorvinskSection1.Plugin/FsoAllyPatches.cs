using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Comfort.Common;
using EFT;
using HarmonyLib;
using SPT.Reflection.Patching;
using UnityEngine;

namespace FSO.NorvinskSection1.Plugin   // <-- match your Plugin.cs namespace
{
    /// Shared state: which FSO bots/controllers/groups are friendly to humans.
    public static class FsoAllyState
    {
        public const int FsoRoleMin = 708300;   // your 5 Fixer WildSpawnTypes
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
            if (enemy == null || enemy.IsAI) return true;            // let FSO fight scav BOTS
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