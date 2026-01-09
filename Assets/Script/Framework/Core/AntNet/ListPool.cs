using System.Collections.Generic;

/// <summary>
/// 从UGUI源码中挪过来的
/// </summary>
/// <typeparam name="T"></typeparam>
public static class ListPool<T>
{
    // Object pool to avoid allocations.
    private static readonly ObjectPoolEx<List<T>> s_ListPool = new ObjectPoolEx<List<T>>(null, l => l.Clear());

    public static List<T> Get()
    {
        return s_ListPool.Get();
    }

    public static void Release(List<T> toRelease)
    {
        s_ListPool.Release(toRelease);
    }

    public static void Dispose()
    {
        s_ListPool.Dispose();
    }
}

public static class HashSetPool<T>
{
    // Object pool to avoid allocations.
    private static readonly ObjectPoolEx<HashSet<T>> s_ListPool = new ObjectPoolEx<HashSet<T>>(null, l => l.Clear());

    public static HashSet<T> Get()
    {
        return s_ListPool.Get();
    }

    public static void Release(HashSet<T> toRelease)
    {
        s_ListPool.Release(toRelease);
    }

    public static void Dispose()
    {
        s_ListPool.Dispose();
    }
}

