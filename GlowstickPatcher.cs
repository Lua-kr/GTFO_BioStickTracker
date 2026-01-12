using GameData;
using HarmonyLib;
using UnityEngine;

namespace BioTrackerBeacon
{
    [HarmonyPatch(typeof(GlowstickInstance), nameof(GlowstickInstance.Setup))]
    [HarmonyPatch(new[] { typeof(ItemDataBlock) })]
    internal static class GlowstickPatcher
    {
        private static void Postfix(GlowstickInstance __instance, ItemDataBlock data)
        {
            if (__instance == null) return;

            int? cfgPid = BioStickTracker.CfgPersistentId.Value;
            if (cfgPid != null && cfgPid > 0)
            {
                int pid;
                try
                {
                    pid = (int)(data != null ? data.persistentID : 0);
                }
                catch
                {
                    if (BioStickTracker.CfgDebugLog.Value)
                        BioStickTracker.Logger?.LogWarning("[Glowstick] persistentID read failed; skipped.");
                    return;
                }

                if (pid != cfgPid)
                {
                    if (BioStickTracker.CfgDebugLog.Value)
                        BioStickTracker.Logger?.LogInfo($"[Glowstick] pid mismatch; pid={pid} cfg={cfgPid} skipped.");
                    return;
                }
            }

            GameObject go;
            try { go = __instance.gameObject; }
            catch { return; }

            if (go == null) return;

            var rt = go.GetComponent<GlowstickTagRuntime>();
            if (rt == null)
            {
                rt = go.AddComponent<GlowstickTagRuntime>();
                rt.Init(go.GetInstanceID());
                rt.Begin();

                if (BioStickTracker.CfgDebugLog.Value)
                    BioStickTracker.Logger?.LogInfo($"[Glowstick] runtime attached GameObjectId={go.GetInstanceID()}");
            }
        }
    }
}
