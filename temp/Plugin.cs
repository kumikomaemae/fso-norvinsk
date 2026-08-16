using System;
using System.IO;
using System.Linq;
using System.Reflection;
using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using UnityEngine;

namespace Soppo.Voice.Client;

/// <summary>
/// Soppo Voice Loader — injects the Soppo voice into EFT's voice cache.
///
/// Why: the client's voice loader (GClass899.LoadVoice) resolves voice names to bundle
/// keys via its own internal route, which doesn't know about mod voices ("No bundle found
/// for voice Soppo"). Rather than patch that route, this plugin loads soppo_voice.bundle
/// directly (from the file beside this DLL) and inserts the Voice asset into the loader's
/// public cache (Dictionary_0). TakeVoice then finds it like any vanilla voice.
///
/// The injection runs lazily before every TakeVoice/method_1 call, so it survives
/// UnloadVoices() clearing the cache between raids.
/// </summary>
[BepInPlugin("com.mae.soppo.voice", "Soppo Voice Loader", "1.0.0")]
public class Plugin : BaseUnityPlugin
{
    internal const string VoiceName = "Soppo";
    internal const string BundleFileName = "soppo_voice.bundle";

    internal static ManualLogSource Log;
    internal static AssetBundle LoadedBundle;
    internal static UnityEngine.Object VoiceAsset; // typed as Object; it's the game's Voice ScriptableObject

    private void Awake()
    {
        Log = Logger;
        new Harmony("com.mae.soppo.voice").PatchAll(Assembly.GetExecutingAssembly());
        Log.LogInfo("[Soppo Voice] armed — will inject on first voice request.");
    }

    /// <summary>Loads the bundle (once) and returns the Voice asset, or null with logging.</summary>
    internal static UnityEngine.Object EnsureVoiceLoaded()
    {
        if (VoiceAsset != null)
        {
            return VoiceAsset;
        }

        try
        {
            if (LoadedBundle == null)
            {
                var pluginDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                var bundlePath = Path.Combine(pluginDir!, BundleFileName);
                if (!File.Exists(bundlePath))
                {
                    Log.LogError($"[Soppo Voice] {BundleFileName} not found beside the plugin DLL ({bundlePath}). Copy it there.");
                    return null;
                }

                LoadedBundle = AssetBundle.LoadFromFile(bundlePath);
                if (LoadedBundle == null)
                {
                    Log.LogError("[Soppo Voice] AssetBundle.LoadFromFile returned null — bundle unreadable?");
                    return null;
                }
            }

            // Load everything (banks + clips resolve internally), then find the Voice asset.
            var all = LoadedBundle.LoadAllAssets();
            VoiceAsset = all.FirstOrDefault(a => a is global::Voice);
            if (VoiceAsset == null)
            {
                Log.LogError("[Soppo Voice] No Voice asset found inside the bundle.");
                return null;
            }

            Log.LogInfo($"[Soppo Voice] Voice asset loaded: '{((global::Voice)VoiceAsset).Name}' with {((global::Voice)VoiceAsset).Banks?.Length ?? 0} banks.");
            return VoiceAsset;
        }
        catch (Exception ex)
        {
            Log.LogError($"[Soppo Voice] load failed: {ex}");
            return null;
        }
    }

    /// <summary>Puts the Voice into the loader's cache if missing.</summary>
    internal static void InjectInto(GClass899 loader)
    {
        if (loader == null || loader.Dictionary_0 == null)
        {
            return;
        }
        if (loader.Dictionary_0.ContainsKey(VoiceName))
        {
            return;
        }

        var voice = EnsureVoiceLoaded() as global::Voice;
        if (voice == null)
        {
            return;
        }

        loader.Dictionary_0[VoiceName] = voice;
        Log.LogInfo("[Soppo Voice] injected into voice cache.");
    }
}

[HarmonyPatch(typeof(GClass899), nameof(GClass899.TakeVoice))]
internal static class TakeVoicePatch
{
    [HarmonyPrefix]
    private static void Prefix(GClass899 __instance, string voiceName)
    {
        if (string.Equals(voiceName, Plugin.VoiceName, StringComparison.OrdinalIgnoreCase))
        {
            Plugin.InjectInto(__instance);
        }
    }
}

[HarmonyPatch(typeof(GClass899), "method_1")]
internal static class Method1Patch
{
    [HarmonyPrefix]
    private static void Prefix(GClass899 __instance, string voiceName)
    {
        if (string.Equals(voiceName, Plugin.VoiceName, StringComparison.OrdinalIgnoreCase))
        {
            Plugin.InjectInto(__instance);
        }
    }
}
