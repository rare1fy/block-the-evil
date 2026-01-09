using System;
using UnityEngine;

/// <summary>
/// 时间器管理器
/// </summary>
public class TimerManager : Singleton<TimerManager>
{
    /// <summary>
    /// 定时器Id
    /// </summary>
    private static uint allocId = 1;

    /// <summary>
    /// 定时器列表
    /// </summary>
    TimerEntity header = new TimerEntity(allocId++);

    /// <summary>
    /// 定时器缓存列表
    /// </summary>
    TimerEntity cache = null;

    public override void Init()
    {
        allocId = 1;
    }

    public void FixedUpdate()
    {
        try
        {
            var entity = header;
            var next = entity.next;
            if (ReferenceEquals(next,null)) return;
            var nowTime = Time.unscaledTime;
            Action action = null;
            while (!ReferenceEquals(next,null))
            {
                if (next.nextTime < nowTime)
                {
                    action = next.action;
                    action?.Invoke();
                    if (next.times > 0)
                        next.times--;
                    if (next.once || next.times == 0)
                    {
                        entity.next = next.next;
                        pushEntity(next);
                        next = entity.next;
                        continue;
                    }
                    next.nextTime += next.intervalTime;
                }
                entity = entity.next;
                if (!ReferenceEquals(entity,null))
                    next = entity.next;
                else
                    next = null;
            }
        }
        catch (Exception e)
        {
            Debug.LogError("--------------------TimerHelper----FixedUpdate Error: " + e.ToString()
                + "   StackTrace: " + e.StackTrace);
        }
    }

    /// <summary>
    /// 添加定时器
    /// </summary>
    /// <param name="intervalTime">时间间隔</param>
    /// <param name="action">定时器回调</param>
    /// <param name="firstCall">初始调用</param>
    /// <param name="once">为True则只执行一次</param>
    /// <param name="times">调用次数，-1则一直调用</param>
    /// <param name="bUseRealTime">使用真实时间</param>
    /// <returns></returns>
    public uint AddTimer(float intervalTime, Action action, bool firstCall, bool once, 
        int times = -1, bool bUseRealTime = false)
    {
        var newEntity = createEntity();
        newEntity.action = action;
        newEntity.intervalTime = intervalTime;
        newEntity.once = once;
        newEntity.times = times;

        if (firstCall)
            newEntity.nextTime = bUseRealTime ? Time.realtimeSinceStartup : Time.unscaledTime;
        else
            newEntity.nextTime = (bUseRealTime ? Time.realtimeSinceStartup : Time.unscaledTime) + intervalTime;
        newEntity.next = header.next;
        header.next = newEntity;
        return newEntity.id;
    }

    /// <summary>
    /// 删除定时器
    /// </summary>
    /// <param name="id"></param>
    public void RemoveTimer(uint id)
    {
        var entity = header;
        var next = entity.next;
        while (!ReferenceEquals(next, null))
        {
            if (next.id == id)
            {
                entity.next = next.next;
                pushEntity(next);
                break;
            }
            entity = entity.next;
            next = entity.next;
        }
    }

    /// <summary>
    /// 删除定时器
    /// </summary>
    /// <param name="action"></param>
    public void RemoveTimer(Action action)
    {
        var entity = header;
        var next = entity.next;
        while (!ReferenceEquals(next,null))
        {
            if (ReferenceEquals(next.action, action))
            {
                entity.next = next.next;
                pushEntity(next);
                break;
            }
            entity = entity.next;
            next = entity.next;
        }
    }

    /// <summary>
    /// 创建实体
    /// </summary>
    /// <returns></returns>
    private TimerEntity createEntity()
    {
        TimerEntity entity = null;
        if (!ReferenceEquals(cache,null))
        {
            entity = cache;
            entity.id= (allocId++);
            cache = entity.next;
            entity.next = null;
            return entity;
        }
        return new TimerEntity((allocId++));
    }

    /// <summary>
    /// 实体回收
    /// </summary>
    /// <param name="entity"></param>
    private void pushEntity(TimerEntity entity)
    {
        entity.action = null;
        entity.times = -1;
        entity.id = 0;
        entity.next = cache;
        cache = entity;
    }

    public override void Dispose()
    {
        allocId = 1;
        header = null;
        cache = null;
    }

    /// <summary>
    /// 实体
    /// </summary>
    class TimerEntity
    {
        public TimerEntity(uint uid)
        {
            id = uid;
        }
        /* fields */
        /// <summary>
        /// TimerId
        /// </summary>
        public uint id = 0;
        /// <summary>
        /// 调用次数
        /// </summary>
        public int times;
        /// <summary>
        /// 定时器回调
        /// </summary>
        public Action action;
        /// <summary>
        /// 间隔时间
        /// </summary>
        public float intervalTime;
        /// <summary>
        /// 下一次时间
        /// </summary>
        public float nextTime;
        /// <summary>
        /// 是否只调用一次
        /// </summary>
        public bool once;
        /// <summary>
        /// 下一个实体 
        /// </summary>
        public TimerEntity next;
    }
}

