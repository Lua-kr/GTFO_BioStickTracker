using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using BepInEx.Unity.IL2CPP;
using HarmonyLib;
using Il2CppInterop.Runtime.Injection;

namespace BioTrackerBeacon
{
    [BepInPlugin("BioStickTracker", "BioStickTracker", "1.0.0")]
    public sealed class BioStickTracker : BasePlugin
    {
        internal static new ManualLogSource? Logger;

        internal static ConfigEntry<bool>? CfgDebugLog;
        internal static ConfigEntry<int>? CfgPersistentId;
        internal static ConfigEntry<float>? CfgTagRadius;
        internal static ConfigEntry<float>? CfgTickInterval;
        internal static ConfigEntry<bool>? CfgTagSleepers;
        internal static ConfigEntry<bool>? CfgTagShadows;

        public override void Load()
        {
            Logger = new ManualLogSource("BioStickTracker");

            CfgDebugLog = Config.Bind(
                "Debug",
                "Debug",
                false,
                "Enable debug logging."
            );

            CfgPersistentId = Config.Bind(
                "General",
                "PersistentId",
                0,
                "Only attach to glowsticks whose persistentID matches this value. Set 0 to work on all types of glowstick."
            );

            CfgTagRadius = Config.Bind(
                "General",
                "TagRadius",
                3.0f,
                "Scan radius around the glowstick (meters)."
            );

            CfgTickInterval = Config.Bind(
                "General",
                "TickInterval",
                0.20f,
                "Scan interval (seconds)."
            );

            CfgTagShadows = Config.Bind(
                "General",
                "TagShadows",
                false,
                "Tag Shadows?"
            );

            CfgTagSleepers = Config.Bind(
                "General",
                "TagSleepers",
                true,
                "Tag Sleepers?"
            );

            new Harmony("BioStickTracker").PatchAll();
            ClassInjector.RegisterTypeInIl2Cpp<GlowstickTagRuntime>();
        }
    }
}
