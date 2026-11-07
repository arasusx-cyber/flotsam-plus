using System;

namespace Puchatek.FlotsamPlus
{
    // One batch is advanced only by Update, never by OnGUI (which can run several
    // times per frame). A failed insertion ends the batch without counting it.
    internal sealed class SpawnBatch
    {
        public int Requested { get; }
        public int Added { get; private set; }
        public int Remaining => Requested - Added;
        public bool Finished { get; private set; }
        public bool Cancelled { get; private set; }
        public string Failure { get; private set; }

        public SpawnBatch(int requested)
        {
            if (requested < 1 || requested > 5000)
                throw new ArgumentOutOfRangeException(nameof(requested));
            Requested = requested;
        }

        public void Step(Func<bool> tryAddOne, int budget)
        {
            if (Finished) return;
            if (tryAddOne == null) throw new ArgumentNullException(nameof(tryAddOne));
            if (budget < 1) throw new ArgumentOutOfRangeException(nameof(budget));
            for (int i = 0; i < budget && !Finished; ++i)
            {
                try
                {
                    if (!tryAddOne())
                    {
                        Failure = "Brak wolnego miejsca albo odpowiedniego magazynu.";
                        Finished = true;
                        return;
                    }
                    ++Added;
                    Finished = Added == Requested;
                }
                catch (Exception error)
                {
                    Failure = "Dodawanie przerwane: " + error.Message;
                    Finished = true;
                    return;
                }
            }
        }

        public void Cancel(string reason)
        {
            if (Finished) return;
            Cancelled = true;
            Failure = reason;
            Finished = true;
        }
    }
}
