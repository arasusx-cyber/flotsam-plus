using System;
using Puchatek.FlotsamPlus;

internal static class PointGrantChecks
{
    internal static void Run(Action<bool, string> check)
    {
        check(PointGrant.TryAdd(0, 1, out int first) && first == 1,
            "A resident with no unspent points can receive a point");
        check(PointGrant.TryAdd(7, 100, out int maximum) && maximum == 107,
            "Maximum grant preserves the resident's previous unspent points");
        check(!PointGrant.TryAdd(7, 0, out int zero) && zero == 7 &&
            !PointGrant.TryAdd(7, -3, out _) && !PointGrant.TryAdd(7, 101, out _),
            "Invalid grants never consume or replace existing points");
        check(!PointGrant.TryAdd(int.MaxValue - 2, 3, out int overflow) &&
            overflow == int.MaxValue - 2,
            "Integer overflow cannot turn skill points negative");
        check(PointGrant.TryAdd(int.MaxValue - 100, 100, out int boundary) && boundary == int.MaxValue,
            "The last valid integer boundary remains exact");
        check(!PointGrant.TryAdd(-1, 5, out int corrupt) && corrupt == -1,
            "A corrupt negative counter is rejected without silently rewriting it");
    }
}
