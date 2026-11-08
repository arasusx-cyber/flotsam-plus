namespace Puchatek.FlotsamPlus
{
    internal static class PointGrant
    {
        internal const int Maximum = 100;

        internal static bool TryAdd(int current, int amount, out int result)
        {
            result = current;
            if (current < 0 || amount < 1 || amount > Maximum || current > int.MaxValue - amount)
                return false;
            result = current + amount;
            return true;
        }
    }
}
