using System.Collections.Generic;
using System;

/// <summary>
/// 状态机基类
/// </summary>
/// <typeparam name="T"></typeparam>
public class StateMachine<T> where T: IState
{
    private Dictionary<Type, T> States = new ();
    public T Current;

    public StateMachine()
    {
        var types = GetType().Assembly.GetTypes();
        foreach (var type in types)
        {
            if (type.IsSubclassOf(typeof(T)))
            {
                var state = (T)System.Activator.CreateInstance(type);
                state.Owner = this;
                States.Add(type, state);
            }
        }
        Current = null;
    }

    public Y Get<Y>() where Y : IState
    {
        return States[typeof(Y)] as Y;
    }

    public void Goto<Y>() where Y : IState
    {
        if (Current != null)
        {
            Current.OnExit();
            Current = null;
        }

        Current = States[typeof(Y)];
        Current.OnEnter();
    }

    public virtual void Update()
    {
        if (Current != null)
        {
            Current.OnUpdate();
        }
    }
}
