using BepInEx;
using BepInEx.Logging;

namespace FSO.NorvinskSection1.Plugin
{
    /// <summary>
    /// Runtime BepInEx plugin for FSO: Norvinsk Section 1.
    /// Loads at game runtime (BepInEx/plugins/) and applies the Harmony patches
    /// that make Fixers allied to human players. The prepatcher (separate project,
    /// BepInEx/patchers/) still handles registering the custom WildSpawnType values.
    /// </summary>
    [BepInPlugin(PluginGuid, PluginName, PluginVersion)]
    public class Plugin : BaseUnityPlugin
    {
        public const string PluginGuid = "com.fso.norvinsksection1.plugin";
        public const string PluginName = "FSO: Norvinsk Section 1";
        public const string PluginVersion = "0.1.0";

        public static ManualLogSource Log = null!;

        private void Awake()
        {
            Log = Logger;

            new FsoMatchStartedPatch().Enable();
            new FsoBotSpawnCatcher().Enable();
            new FsoPreventEnemyController().Enable();
            new FsoPreventEnemyGroup().Enable();

            Log.LogWarning("FSO: Norvinsk Section 1 ally plugin loaded.");
        }
    }
}