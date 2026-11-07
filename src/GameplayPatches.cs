using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using UnityEngine;
using RestVital = PajamaLlama.Fltsm.Rest;

namespace Puchatek.FlotsamPlus
{
    [HarmonyPatch(typeof(WorldMap), "get_MovementSpeed")]
    internal static class ShipSpeedPatch
    {
        private static void Postfix(ref float __result)
        {
            Plugin p = Plugin.Instance;
            if (p != null && p.ShipReady && Community.PlayerCommunity != null)
                __result = Tuning.Scale(__result, p.ShipSpeed.Value);
        }
    }

    [HarmonyPatch(typeof(WorldMap), "get_RotationSpeed")]
    internal static class ShipTurningPatch
    {
        private static void Postfix(ref float __result)
        {
            Plugin p = Plugin.Instance;
            if (p != null && p.ShipReady && Community.PlayerCommunity != null)
                __result = Tuning.Scale(__result, p.ShipTurning.Value);
        }
    }

    [HarmonyPatch]
    internal static class PathMovementPatch
    {
        private static readonly FieldInfo SpeedField = AccessTools.Field(typeof(WorldMap), "_movementSpeed");
        private static readonly FieldInfo TurnField = AccessTools.Field(typeof(WorldMap), "_rotationSpeed");
        private static readonly AccessTools.FieldRef<WorldMap, float> Speed =
            AccessTools.FieldRefAccess<WorldMap, float>("_movementSpeed");
        private static readonly AccessTools.FieldRef<WorldMap, float> Turn =
            AccessTools.FieldRefAccess<WorldMap, float>("_rotationSpeed");

        private static MethodBase TargetMethod()
        {
            return AccessTools.EnumeratorMoveNext(AccessTools.Method(typeof(WorldMap), "MoveToCoroutine"));
        }

        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            int speeds = 0, turns = 0;
            foreach (CodeInstruction instruction in instructions)
            {
                MethodInfo replacement = null;
                if (instruction.opcode == OpCodes.Ldfld && Equals(instruction.operand, SpeedField))
                {
                    replacement = AccessTools.Method(typeof(PathMovementPatch), nameof(PathSpeed));
                    ++speeds;
                }
                else if (instruction.opcode == OpCodes.Ldfld && Equals(instruction.operand, TurnField))
                {
                    replacement = AccessTools.Method(typeof(PathMovementPatch), nameof(PathTurning));
                    ++turns;
                }
                if (replacement == null) yield return instruction;
                else
                    yield return new CodeInstruction(instruction) { opcode = OpCodes.Call, operand = replacement };
            }
            if (speeds != 2 || turns != 1)
                throw new InvalidOperationException("Nieznana wersja ścieżki ruchu statku.");
        }

        // The path coroutine reads fields directly, bypassing the public getters.
        // Read vanilla fields without modifying the WorldMap or serialized assets.
        internal static float PathSpeed(WorldMap map)
        {
            float value = Speed(map);
            Plugin p = Plugin.Instance;
            return p != null && p.ShipReady ? Tuning.Scale(value, p.ShipSpeed.Value) : value;
        }

        internal static float PathTurning(WorldMap map)
        {
            float value = Turn(map);
            Plugin p = Plugin.Instance;
            return p != null && p.ShipReady ? Tuning.Scale(value, p.ShipTurning.Value) : value;
        }
    }

    internal static class TravelRules
    {
        internal static bool Free(Engine engine)
        {
            Plugin p = Plugin.Instance;
            return p != null && p.EnergyReady && p.FreeTravel.Value && Plugin.IsPlayerEngine(engine);
        }
    }

    [HarmonyPatch(typeof(Engine), nameof(Engine.ConsumeEnergy))]
    internal static class TravelEnergyPatch
    {
        private static bool Prefix(Engine __instance, ref bool __result)
        {
            if (!TravelRules.Free(__instance)) return true;
            __result = true;
            return false;
        }
    }

    [HarmonyPatch(typeof(Engine), nameof(Engine.ReturnEnergyCost))]
    internal static class TravelCostPatch
    {
        private static void Postfix(Engine __instance, ref float __result)
        {
            if (TravelRules.Free(__instance)) __result = 0f;
        }
    }

    [HarmonyPatch(typeof(Engine), nameof(Engine.ReturnEnergyRange), new[] { typeof(float) })]
    internal static class TravelRangePatch
    {
        private static bool Prefix(Engine __instance, ref float __result)
        {
            if (!TravelRules.Free(__instance)) return true;
            // Finite to keep range/UI calculations away from Infinity/NaN.
            __result = 1000000f;
            return false;
        }
    }

    [HarmonyPatch(typeof(Engine), nameof(Engine.ReturnMoveableDistance))]
    internal static class TravelCooldownPatch
    {
        private static bool Prefix(Engine __instance, float desiredDistance, ref float __result)
        {
            if (!TravelRules.Free(__instance)) return true;
            __result = desiredDistance;
            return false;
        }
    }

    [HarmonyPatch]
    internal static class InstantBuildingPatch
    {
        private static readonly FieldInfo InstantFlag = AccessTools.Field(typeof(BuildingDevTools), "InstantBuild");
        private static readonly AccessTools.FieldRef<ConstructionHandler, WeakReference<IConstructible>> Owner =
            AccessTools.FieldRefAccess<ConstructionHandler, WeakReference<IConstructible>>("_owner");

        private static IEnumerable<MethodBase> TargetMethods()
        {
            // Includes resource checks, boats, line constructions and upgrades.
            // Do not change the shared developer flag used by the older trainer.
            foreach (string name in new[] { "StartBuilding", "Upgrade", "TryToStartUpgrade", "StartUpgrade" })
                yield return AccessTools.DeclaredMethod(typeof(Buildable), name);
            yield return AccessTools.DeclaredMethod(typeof(ConstructionHandler), "StartBuilding");
            yield return AccessTools.DeclaredMethod(typeof(BoatCursorProperties), "UpdateCursor");
            // The 3-argument overload delegates to the 4-argument implementation.
            yield return AccessTools.GetDeclaredMethods(typeof(LineConstructionCursorProperties))
                .Single(m => m.Name == "PlaceLine" && m.GetParameters().Length == 4);
            yield return AccessTools.DeclaredMethod(typeof(ResourceManager), "AreCommunityResourcesAvailable");
        }

        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions,
            MethodBase __originalMethod)
        {
            int matched = 0;
            foreach (CodeInstruction instruction in instructions)
            {
                yield return instruction;
                if (instruction.opcode != OpCodes.Ldsfld || !Equals(instruction.operand, InstantFlag)) continue;
                ++matched;
                yield return new CodeInstruction(__originalMethod.IsStatic ? OpCodes.Ldnull : OpCodes.Ldarg_0);
                yield return new CodeInstruction(OpCodes.Call,
                    AccessTools.Method(typeof(InstantBuildingPatch), nameof(EffectiveFlag)));
            }
            if (matched == 0) throw new InvalidOperationException("Brak flagi budowania w metodzie.");
        }

        internal static bool EffectiveFlag(bool nativeFlag, object context)
        {
            if (nativeFlag) return true;
            Plugin p = Plugin.Instance;
            if (p == null || !p.BuildingReady || !p.InstantBuild.Value || Community.PlayerCommunity == null)
                return false;
            if (context is IConstructible constructible) return Plugin.IsPlayer(constructible.Community);
            if (context is ConstructionHandler handler)
            {
                WeakReference<IConstructible> owner = Owner(handler);
                return owner != null && owner.TryGetTarget(out IConstructible target) &&
                    Plugin.IsPlayer(target.Community);
            }
            // Cursors and AreCommunityResourcesAvailable operate on the player.
            return true;
        }
    }

    [HarmonyPatch(typeof(ResearchStation), nameof(ResearchStation.Research))]
    internal static class ResearchSpeedPatch
    {
        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            MethodInfo delta = AccessTools.PropertyGetter(typeof(Time), nameof(Time.deltaTime));
            int matched = 0;
            foreach (CodeInstruction instruction in instructions)
            {
                if (!instruction.Calls(delta)) { yield return instruction; continue; }
                ++matched;
                var load = new CodeInstruction(instruction) { opcode = OpCodes.Ldarg_0, operand = null };
                yield return load;
                yield return new CodeInstruction(OpCodes.Call,
                    AccessTools.Method(typeof(ResearchSpeedPatch), nameof(DeltaTime)));
            }
            if (matched != 1) throw new InvalidOperationException("Nieznana wersja obsługi badań.");
        }

        internal static float DeltaTime(ResearchStation station)
        {
            Plugin p = Plugin.Instance;
            float delta = Time.deltaTime;
            if (p != null && p.ResearchReady && station.Buildable != null &&
                Plugin.IsPlayer(station.Buildable.Community))
                return Tuning.Scale(delta, p.ResearchSpeed.Value, 100f);
            return delta;
        }
    }

    [HarmonyPatch(typeof(RestVital), "OnNighttimeStarted")]
    internal static class RestNightPatch
    {
        private static bool Prefix(RestVital __instance)
        {
            return !Plugin.SuppressRest(__instance);
        }
    }
}

