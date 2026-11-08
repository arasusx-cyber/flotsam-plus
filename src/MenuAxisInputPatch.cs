using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;

namespace Puchatek.FlotsamPlus
{
    // Scrolling a menu must not also zoom the ocean underneath it. Camera zoom
    // reads Rewired directly, so patch that public getter as well as axis helpers.
    [HarmonyPatch]
    internal static class MenuAxisInputPatch
    {
        private static IEnumerable<MethodBase> TargetMethods()
        {
            foreach (string name in new[] { "GetAxis", "GetAxisRaw" })
                yield return AccessTools.Method(typeof(FlotsamInputManager), name, new[] { typeof(int) });
            foreach (string name in new[] { "GetCameraZoom", "GetCameraRotation" })
                yield return AccessTools.Method(typeof(FlotsamInputManager), name, System.Type.EmptyTypes);
        }

        private static bool Prefix(ref float __result)
        {
            if (!Plugin.BlockGameplayInput()) return true;
            __result = 0f;
            return false;
        }
    }
}
