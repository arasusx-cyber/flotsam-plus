using HarmonyLib;

namespace Puchatek.FlotsamPlus
{
    // The native LateUpdate recomputes total/available capacity and weight-tier
    // events from this return value. Never scale a cached value or an engine asset.
    [HarmonyPatch(typeof(Engine), "ReturnTownTugCapacity")]
    internal static class TownCapacityPatch
    {
        private static void Postfix(Engine __instance, ref float __result)
        {
            Plugin plugin = Plugin.Instance;
            if (plugin != null && plugin.CapacityReady && Plugin.IsPlayerEngine(__instance))
                __result = WeightCapacity.Calculate(__result, plugin.TownCapacity.Value,
                    plugin.NoWeightLimit.Value);
        }
    }

    // Engine.CanTug contains only the engine-presence and available-weight
    // checks. Keep the former; all resource, unlock and placement checks remain
    // in their native callers. Only the unlimited switch bypasses the comparison.
    [HarmonyPatch(typeof(Engine), nameof(Engine.CanTug))]
    internal static class UnlimitedTownWeightPatch
    {
        private static bool Prefix(Engine ____townheartEngine, ref bool __result)
        {
            Plugin plugin = Plugin.Instance;
            if (plugin == null || !plugin.CapacityReady || !plugin.NoWeightLimit.Value ||
                !Plugin.IsPlayerEngine(____townheartEngine)) return true;
            __result = true;
            return false;
        }
    }
}

