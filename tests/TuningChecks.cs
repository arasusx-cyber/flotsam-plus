using System;
using Puchatek.FlotsamPlus;

internal static class TuningChecks
{
    internal static void Run(Action<bool, string> check)
    {
        check(Tuning.Scale(50f, 1f) == 50f && Tuning.Scale(-7f, 1f) == -7f,
            "Default multipliers preserve forward and reverse values");
        check(Tuning.Scale(50f, 3f) == 150f && Tuning.Scale(50f, 1f) == 50f,
            "Movement tuning uses the original value, without accumulating");
        check(Tuning.Scale(-20f, 4f) == -80f, "Reverse movement preserves direction");
        check(Tuning.Multiplier(float.NaN, 5f) == 1f &&
            Tuning.Multiplier(float.PositiveInfinity, 5f) == 1f &&
            Tuning.Multiplier(-100f, 5f) == 1f && Tuning.Multiplier(900f, 5f) == 5f,
            "Invalid config values cannot inject NaN or excessive forces");
        check(Tuning.Scale(.25f, 100f, 100f) == 25f && Tuning.Scale(.25f, 1000f, 100f) == 25f,
            "Research uses its own maximum of one hundred");
        check(Tuning.Damping(0f, 1f, 1f, false, false) == 0f &&
            Tuning.Damping(10f, 1f, 1f, true, true) == 10f,
            "Default damping is unchanged including zero-drag bodies");
        check(Tuning.Damping(0f, 1f, 5f, false, false) > 0f &&
            Tuning.Damping(2f, 1f, 5f, false, false) > 2f,
            "Braking works after input release, including zero vanilla drag");
        check(Tuning.Damping(2f, 1f, 5f, true, false) == 2f,
            "Braking does not slow active velocity or direct-position movement");
        check(Tuning.Scale(12f, 4f) / Tuning.Damping(3f, 4f, 1f, true, true) == 4f,
            "Force and drag acceleration tuning preserve their steady-state ratio");

        var owned = new ScalarOverride();
        check(owned.Apply(2f, 8f) == 8f && owned.Restore(8f) == 2f,
            "Reset restores the exact captured property");
        owned.Apply(2f, 8f);
        check(owned.Restore(13f) == 13f, "Reset preserves another mod's later override");
        check(owned.Restore(8f) == 8f, "A completed reset cannot restore a stale baseline");
        owned.Apply(4f, 12f);
        float original = owned.Restore(12f);
        owned.Apply(original, Tuning.Damping(original, 2f, 1f, true, true));
        check(owned.Restore(8f) == 4f, "Changing tuning recaptures the restored baseline");
    }
}
