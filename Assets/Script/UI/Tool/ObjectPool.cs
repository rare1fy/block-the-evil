using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectPool<T>
{
    List<T> m_pool;
    List<T> m_showList;

    public ObjectPool()
    {
        m_pool = new List<T>();
        m_showList = new List<T>();
    }

    public void AddItem(Action<T> action)
    {

    }

    public void RemoveItem(Action<T> action) 
    {
    
    }

    public void Clear() 
    {
        foreach (var item in m_showList) 
        {
        }
    }

    public void Destroy()
    {
        m_pool.Clear();
        m_showList.Clear();
    }
}
