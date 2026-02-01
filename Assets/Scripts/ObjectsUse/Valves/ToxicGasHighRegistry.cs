using System.Collections.Generic;

public static class ToxicGasHighRegistry
{
    private static readonly HashSet<ToxicGasStateController> high = new HashSet<ToxicGasStateController>();

    public static int HighCount => high.Count;

    public static void Register(ToxicGasStateController gas)
    {
        if (gas == null) return;
        if (gas.IsHigh) high.Add(gas);
        else high.Remove(gas);
    }

    public static void Unregister(ToxicGasStateController gas)
    {
        if (gas == null) return;
        high.Remove(gas);
    }

    public static void ClearAll() => high.Clear();
}