using UnityEngine;

public static class ProfilerUtils
{
    public static long GetMonoHeapSize()
    {
#if ZEALOT_CLIENT
    #if !UNITY_IOS
                return UnityEngine.Profiling.Profiler.GetMonoHeapSizeLong();
    #else
                return 0;
    #endif
#else
        return 0;
#endif
    }

    public static long GetMonoUsedHeapSize()
    {
#if ZEALOT_CLIENT
    #if !UNITY_IOS
            return UnityEngine.Profiling.Profiler.GetMonoUsedSizeLong();
    #else
            return 0;
    #endif
#else
        return 0;
#endif
    }

    public static void LogIncrementSize(string title, ref long prevsize)
    {
#if ZEALOT_CLIENT
        long newsize = GetMonoUsedHeapSize();
        float ret = (newsize - prevsize) * 1.0f / (1024 * 1024);
        prevsize = newsize;
        Debug.LogFormat("{0} used {1} MByte", title, ret.ToString("N2"));
#endif
    }

    public static void LogString(string title)
    {
#if ZEALOT_CLIENT
        long size = GetMonoUsedHeapSize();
        string used = (size / 1000).ToString();
        string heap = (GetMonoHeapSize() / 1000).ToString();
        Debug.LogFormat("{0} HEAP: {1}KB, USED: {2}KB", title, heap, used);
#endif
    }
}