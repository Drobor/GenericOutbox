using System.Diagnostics;

namespace EndToEndTests;

public static class Wait
{
    public static async Task<T> ForNotDefault<T>(int waitForMs, int timeoutMs, Func<Task<T>> func)
    {
        T result = default;

        var sw = Stopwatch.StartNew();

        for (; sw.ElapsedMilliseconds < waitForMs; await Task.Delay(timeoutMs))
        {
            result = await func();

            if (!EqualityComparer<T>.Default.Equals(result, default))
                return result;
        }

        return result;
    }
}