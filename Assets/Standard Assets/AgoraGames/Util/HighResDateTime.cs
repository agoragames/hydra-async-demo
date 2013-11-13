using System;
using System.Diagnostics;

public class HiResDateTime
{
    private static DateTime _startTime;
    private static Stopwatch _stopWatch = null;
    private static TimeSpan _maxIdle = TimeSpan.FromSeconds(10);

    public static DateTime UtcNow
    {
        get
        {
            if ((_stopWatch == null))
            {
                Reset();
            }
            return _startTime.AddTicks(_stopWatch.Elapsed.Ticks);
        }
    }

    private static void Reset()
    {
        _startTime = DateTime.UtcNow;
        _stopWatch = Stopwatch.StartNew();
    }
}