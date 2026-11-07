using System;
using Puchatek.FlotsamPlus;

internal static class Program
{
    private static int _checks;
    private static void Check(bool value, string name)
    {
        if (!value) throw new Exception("FAIL: " + name);
        ++_checks;
        Console.WriteLine("PASS: " + name);
    }

    private static void Main()
    {
        var full = new SpawnBatch(100);
        int inserted = 0;
        full.Step(() => { ++inserted; return true; }, 24);
        Check(full.Added == 24 && !full.Finished, "Per-frame budget limits actual insertions");
        while (!full.Finished) full.Step(() => { ++inserted; return true; }, 24);
        full.Step(() => { ++inserted; return true; }, 24);
        Check(full.Added == 100 && inserted == 100, "Completed batch never adds extra objects");

        var limited = new SpawnBatch(100);
        int available = 31;
        int calls = 0;
        while (!limited.Finished)
            limited.Step(() => { ++calls; if (available == 0) return false; --available; return true; }, 24);
        Check(limited.Added == 31 && calls == 32 && available == 0 && limited.Failure != null,
            "Full storage reports exact partial success without overfilling");

        var none = new SpawnBatch(10);
        none.Step(() => false, 24);
        Check(none.Finished && none.Added == 0, "Missing compatible storage inserts nothing");

        var cancel = new SpawnBatch(100);
        cancel.Step(() => true, 7);
        cancel.Cancel("Changed save");
        bool afterCancel = false;
        cancel.Step(() => { afterCancel = true; return true; }, 24);
        Check(cancel.Added == 7 && cancel.Cancelled && !afterCancel,
            "Cancelled or changed-save batch cannot continue in another town");

        var error = new SpawnBatch(10);
        int ok = 0;
        error.Step(() => { if (ok == 3) throw new InvalidOperationException("rejected"); ++ok; return true; }, 24);
        Check(error.Finished && error.Added == 3 && error.Failure.Contains("rejected"),
            "Native insertion error preserves exact completed count and stops");

        int rejected = 0;
        foreach (int n in new[] { -1, 0, 5001, int.MaxValue })
        {
            try { new SpawnBatch(n); }
            catch (ArgumentOutOfRangeException) { ++rejected; }
        }
        Check(rejected == 4, "Rejects invalid and excessive batch sizes");
        var maximum = new SpawnBatch(5000);
        while (!maximum.Finished) maximum.Step(() => true, 24);
        Check(maximum.Added == 5000, "Maximum batch completes exactly");
        TuningChecks.Run(Check);
        Console.WriteLine("Passed " + _checks + " checks.");
    }
}
