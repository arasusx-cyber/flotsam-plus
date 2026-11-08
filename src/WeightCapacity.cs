using System;

namespace Puchatek.FlotsamPlus
{
    internal static class WeightCapacity
    {
        internal const float MaximumMultiplier = 100f;
        internal const float UnlimitedReserve = 1000000000f;

        internal static float Calculate(float nativeCapacity, float multiplier, bool unlimited)
        {
            // Do not disguise an invalid native value. Finite saturation avoids
            // Infinity/Infinity and NaN in the game's weight percentage and UI.
            if (float.IsNaN(nativeCapacity) || float.IsInfinity(nativeCapacity) ||
                nativeCapacity < 0f) return nativeCapacity;
            double scaled = (double)nativeCapacity *
                Tuning.Multiplier(multiplier, MaximumMultiplier);
            if (unlimited) scaled = Math.Max(scaled, UnlimitedReserve);
            return (float)Math.Min(scaled, (double)float.MaxValue);
        }
    }
}

