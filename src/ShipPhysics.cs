using System;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using UnityEngine;

namespace Puchatek.FlotsamPlus
{
    internal static class ShipPhysics
    {
        // Unity 6 renamed these properties. Resolve both names at runtime so the
        // build can still use the same Unity reference set as version 1.0.0.
        private static readonly PropertyInfo Linear = typeof(Rigidbody2D).GetProperty("linearDamping") ??
            typeof(Rigidbody2D).GetProperty("drag");
        private static readonly PropertyInfo Angular = typeof(Rigidbody2D).GetProperty("angularDamping") ??
            typeof(Rigidbody2D).GetProperty("angularDrag");
        private static readonly Dictionary<WorldMapTownheartPhycis2D, State> States =
            new Dictionary<WorldMapTownheartPhycis2D, State>();

        private sealed class State
        {
            internal Rigidbody2D Body;
            internal readonly ScalarOverride LinearOverride = new ScalarOverride();
            internal readonly ScalarOverride AngularOverride = new ScalarOverride();

            internal void Restore()
            {
                if (Body == null) return;
                // Respect another mod's subsequent changes rather than overwriting them.
                float linear = (float)Linear.GetValue(Body, null);
                float angular = (float)Angular.GetValue(Body, null);
                float restoredLinear = LinearOverride.Restore(linear);
                float restoredAngular = AngularOverride.Restore(angular);
                if (restoredLinear != linear) Linear.SetValue(Body, restoredLinear, null);
                if (restoredAngular != angular) Angular.SetValue(Body, restoredAngular, null);
            }
        }

        internal static void Validate()
        {
            if (Linear == null || Angular == null || !Linear.CanWrite || !Angular.CanWrite)
                throw new NotSupportedException("Brak obsługiwanych parametrów hamowania.");
        }

        internal static void Before(WorldMapTownheartPhycis2D physics)
        {
            if (States.TryGetValue(physics, out State state)) state.Restore();
        }

        internal static void After(WorldMapTownheartPhycis2D physics, Rigidbody2D body, Engine engine,
            WorldMapTownheartPhycis2D.Modes mode)
        {
            Plugin p = Plugin.Instance;
            if (p == null || !p.PhysicsReady || body == null || !Plugin.IsPlayerEngine(engine)) return;
            float acceleration = Tuning.Multiplier(p.ShipAcceleration.Value, 5f);
            float braking = Tuning.Multiplier(p.ShipBraking.Value, 5f);
            if (acceleration == 1f && braking == 1f) return;
            Validate();
            if (!States.TryGetValue(physics, out State state))
            {
                state = new State { Body = body };
                States.Add(physics, state);
            }
            float linear = (float)Linear.GetValue(body, null);
            float angular = (float)Angular.GetValue(body, null);
            float appliedLinear = Tuning.Damping(linear, acceleration, braking,
                physics.ProcessedInput, mode == WorldMapTownheartPhycis2D.Modes.Force);
            float appliedAngular = physics.ProcessedInput ? angular :
                Tuning.Damping(angular, 1f, braking, false, false);
            state.LinearOverride.Apply(linear, appliedLinear);
            state.AngularOverride.Apply(angular, appliedAngular);
            if (appliedLinear != linear) Linear.SetValue(body, appliedLinear, null);
            if (appliedAngular != angular) Angular.SetValue(body, appliedAngular, null);
        }

        internal static void RestoreAll()
        {
            foreach (State state in States.Values)
                if (state.Body != null) state.Restore();
            States.Clear();
        }
    }

    [HarmonyPatch(typeof(WorldMapTownheartPhycis2D), "FixedUpdate")]
    internal static class ShipPhysicsPatch
    {
        private static bool Prepare() { ShipPhysics.Validate(); return true; }

        private static void Prefix(WorldMapTownheartPhycis2D __instance)
        {
            ShipPhysics.Before(__instance);
        }

        private static void Postfix(WorldMapTownheartPhycis2D __instance, Rigidbody2D ____rigidbody,
            Engine ____engine, WorldMapTownheartPhycis2D.Modes ____mode)
        {
            ShipPhysics.After(__instance, ____rigidbody, ____engine, ____mode);
        }
    }

    [HarmonyPatch(typeof(WorldMapTownheartPhycis2D), "AddForceAndTorque")]
    internal static class ShipForcePatch
    {
        private static void Prefix(Engine ____engine, ref float movement)
        {
            Plugin p = Plugin.Instance;
            if (p != null && p.PhysicsReady && Plugin.IsPlayerEngine(____engine))
                movement = Tuning.Scale(movement, p.ShipAcceleration.Value);
        }
    }
}

