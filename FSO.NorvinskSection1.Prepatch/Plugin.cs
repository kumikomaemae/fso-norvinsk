using BepInEx;

namespace FSO.NorvinskSection1.Prepatch
{
    /// <summary>
    /// Prepatcher entry point for FSO: Norvinsk Section 1.
    /// 
    /// This class declares the plugin metadata to BepInEx and ensures
    /// MoreBotsAPI's prepatcher loads first. The actual patching logic
    /// lives in WildSpawnTypePatch.cs.
    /// 
    /// Pattern modeled on RUAFComeHome.Prepatch.MoreBotsPrepatchExample (verified via dotPeek).
    /// </summary>
    [BepInPlugin(PluginGuid, PluginName, PluginVersion)]
    [BepInDependency("com.morebotsapiprepatch.tacticaltoaster", BepInDependency.DependencyFlags.HardDependency)]
    public class Plugin : BaseUnityPlugin
    {
        public const string PluginGuid = "com.fso.norvinsksection1.prepatch";
        public const string PluginName = "FSO: Norvinsk Section 1 Prepatch";
        public const string PluginVersion = "0.1.0";

        // Singleton reference - lets other code in this assembly access the plugin instance
        // (especially useful for logging from static methods like our patch class).
        public static Plugin Instance = null!;

        public void Awake() => Instance = this;
    }
}