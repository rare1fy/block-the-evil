/// <summary>
/// 元组结构，非常适合函数结果需要返回多值情况
/// 支持嵌套
/// 
/// 使用时，注意不要new!!!!!!!!!!!!!!!
/// 
/// 为什么使用struct?
/// 避免GC,几乎只用于存储数据，没有对外接口需求，暂时不用class
/// 
/// Example:
/// Tuple<int, string> tuple1;
/// tuple1.value1 = 1;
/// tuple1.value2 = "kaka";
/// SDDebug.LogWarningFormat("val1:{0}, val2:{1}", tuple1.value1, tuple1.value2);
/// Tuple<int, Tuple<int, string>> tuple2;
/// tuple2.value1 = 1;
/// tuple2.value2 = tuple1;
/// SDDebug.LogWarningFormat("val1:{0}, val2.val1:{1}, val2.val2:{2}", tuple2.value1, tuple2.value2.value1, tuple2.value2.value2);
/// </summary>

public struct Tuple<T1>
{
    public T1 value1;

    public Tuple(T1 v1)
    {
        value1 = v1;
    }
}

public struct Tuple<T1, T2>
{
    public T1 value1;
    public T2 value2;

    public Tuple(T1 v1, T2 v2)
    {
        value1 = v1;
        value2 = v2;
    }
}

public struct Tuple<T1, T2, T3>
{
    public T1 value1;
    public T2 value2;
    public T3 value3;

    public Tuple(T1 v1, T2 v2, T3 v3)
    {
        value1 = v1;
        value2 = v2;
        value3 = v3;
    }
}

public struct Tuple<T1, T2, T3, T4>
{
    public T1 value1;
    public T2 value2;
    public T3 value3;
    public T4 value4;

    public Tuple(T1 v1, T2 v2, T3 v3, T4 v4)
    {
        value1 = v1;
        value2 = v2;
        value3 = v3;
        value4 = v4;
    }
}

public struct Tuple<T1, T2, T3, T4, T5>
{
    public T1 value1;
    public T2 value2;
    public T3 value3;
    public T4 value4;
    public T5 value5;

    public Tuple(T1 v1, T2 v2, T3 v3, T4 v4, T5 v5)
    {
        value1 = v1;
        value2 = v2;
        value3 = v3;
        value4 = v4;
        value5 = v5;
    }
}

#region Tuplec

public class Tuplec<T1>
{
    public T1 value1;

    public Tuplec()
    {
    }

    public Tuplec(T1 v1)
    {
        value1 = v1;
    }
}

public class Tuplec<T1, T2>
{
    public T1 value1;
    public T2 value2;

    public Tuplec()
    {
    }

    public Tuplec(T1 v1, T2 v2)
    {
        value1 = v1;
        value2 = v2;
    }
}

public class Tuplec<T1, T2, T3>
{
    public T1 value1;
    public T2 value2;
    public T3 value3;

    public Tuplec()
    {
    }

    public Tuplec(T1 v1, T2 v2, T3 v3)
    {
        value1 = v1;
        value2 = v2;
        value3 = v3;
    }
}

public class Tuplec<T1, T2, T3, T4>
{
    public T1 value1;
    public T2 value2;
    public T3 value3;
    public T4 value4;

    public Tuplec()
    {
    }

    public Tuplec(T1 v1, T2 v2, T3 v3, T4 v4)
    {
        value1 = v1;
        value2 = v2;
        value3 = v3;
        value4 = v4;
    }
}

public class Tuplec<T1, T2, T3, T4, T5>
{
    public T1 value1;
    public T2 value2;
    public T3 value3;
    public T4 value4;
    public T5 value5;

    public Tuplec()
    {
    }

    public Tuplec(T1 v1, T2 v2, T3 v3, T4 v4, T5 v5)
    {
        value1 = v1;
        value2 = v2;
        value3 = v3;
        value4 = v4;
        value5 = v5;
    }
}

#endregion