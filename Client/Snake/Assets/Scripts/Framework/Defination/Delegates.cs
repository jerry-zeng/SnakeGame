/*
*    常用委托方法的定义，主要是避免调用System下面的这些.
*/

namespace Framework
{
    public delegate void Callback();

    public delegate void Action();
    public delegate void Action<T1>(T1 t1);
    public delegate void Action<T1,T2>(T1 t1, T2 t2);
    public delegate void Action<T1,T2,T3>(T1 t1, T2 t2, T3 t3);
    public delegate void Action<T1,T2,T3,T4>(T1 t1, T2 t2, T3 t3, T4 t4);
    public delegate void Action<T1,T2,T3,T4,T5>(T1 t1, T2 t2, T3 t3, T4 t4, T5 t5);

    public delegate bool Predicate();
    public delegate bool Predicate<T1>(T1 t1);
    public delegate bool Predicate<T1,T2>(T1 t1, T2 t2);

    public delegate int Comparison<T1,T2>(T1 t1, T2 t2);
    public delegate int Comparison<T1,T2,T3>(T1 t1, T2 t2, T3 t3);

    public delegate void MonoUpdateEvent();
}
