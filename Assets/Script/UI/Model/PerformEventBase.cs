using Pb;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Diagnostics;

/// <summary>
/// 剧本事件基类
/// </summary>
public abstract class PerformEventBase
{
    //事件id
    protected int id;
    public int Id 
    {  
        get { return id; }
        protected set { id = value; }
    }

    protected GuildPerform cfg;
    public GuildPerform Cfg
    {
        get { return cfg; }
        protected set { cfg = value; }
    }

    protected PerformData data;
    public PerformData Data
    {
        get { return data; }
        protected set { data = value; }
    }

    public bool awaitEnd = false;

    public PerformEventBase(int id, GuildPerform cfg, PerformData data)
    {
        this.id = id;
        this.cfg = cfg;
        this.data = data;
    }

    public virtual IEnumerator Operation(object obj = null)
    {
        yield return new WaitUntil(() => awaitEnd);
        Next();
    }


    /// <summary>
    /// 没有点击事件，自动进入下一句
    /// </summary>
    public virtual void Next()
    {
        data.Next();
    }
}
