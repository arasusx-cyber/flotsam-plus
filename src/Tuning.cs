using System;

namespace Puchatek.FlotsamPlus
{
    internal sealed class ScalarOverride
    {
        private float _original, _applied;
        private bool _active;

        internal float Apply(float original, float value)
        {
            _original = original;
            _applied = value;
            _active = original != value;
            return value;
        }

        internal float Restore(float current)
        {
            bool ours = _active && current == _applied;
            _active = false;
            return ours ? _original : current;
        }
    }

    internal static class Tuning
    {
        internal static float Multiplier(float value, float maximum)
        {
            if (float.IsNaN(value) || float.IsInfinity(value)) return 1f;
            return Math.Max(1f, Math.Min(maximum, value));
        }

        internal static float Scale(float original, float multiplier, float maximum = 5f)
        {
            return original * Multiplier(multiplier, maximum);
        }

        internal static float Damping(float original, float acceleration, float braking,
            bool hasInput, bool forceMode)
        {
            if (hasInput)
                return forceMode ? original * Multiplier(acceleration, 5f) : original;
            float brake = Multiplier(braking, 5f);
            // Still brakes when vanilla damping is zero; exactly preserves ×1.
            return original + (brake - 1f) * Math.Max(1f, original);
        }
    }
}

