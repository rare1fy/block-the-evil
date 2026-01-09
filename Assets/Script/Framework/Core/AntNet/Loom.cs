using UnityEngine;
using System.Collections.Generic;
using System;

namespace AntNet
{
    /// <summary>
    /// 多线程辅助类
    /// </summary>
    public class Loom : MonoBehaviour
    {
        private readonly static System.Object mLockObject = new object();
        static bool initialized;
        private static Loom _current;
        public static Loom Current
        {
            get
            {
                Initialize();
                return _current;
            }
        }

        void Awake()
        {
            _current = this;
            initialized = true;
        }

        public static void Initialize()
        {
            if (!initialized)
            {
                if (!Application.isPlaying)
                    return;
                initialized = true;
                var g = new GameObject("Loom");
                g.hideFlags = HideFlags.NotEditable;
                DontDestroyOnLoad(g);
                _current = g.AddComponent<Loom>();
            }
        }

        private List<QueueCallItem> mLitThreadAction = new List<QueueCallItem>(30);
        private List<QueueCallItem> mLitCurrentAction = new List<QueueCallItem>(30);

        public struct QueueCallItem
        {
            public Action<object> Action;
            public object Parameter;
        }

        public static void ClearQueueOnMainThread()
        {
            if (initialized && Current != null)
                Current.mLitThreadAction.Clear();
        }

        public static void QueueOnMainThread(Action<object> action, object parameter
            )
        {
            if ((System.Object)Current == null)
                return;
            lock (mLockObject)
            {
                Current.mLitThreadAction.Add(new QueueCallItem()
                {
                    Action = action,
                    Parameter = parameter,
                });
            }
        }

        void OnDisable()
        {
            if (_current == this)
            {
                _current = null;
                initialized = false;
            }
        }

        void Update()
        {
            lock (mLockObject)
            {
                mLitCurrentAction.Clear();
                var litTemp = mLitThreadAction;
                mLitThreadAction = mLitCurrentAction;
                mLitCurrentAction = litTemp;
            }
            for (int i = 0, iLength = mLitCurrentAction.Count; i < iLength; i++)
            {
                QueueCallItem a = mLitCurrentAction[i];
                try
                {
                    a.Action(a.Parameter);
                }
                catch (Exception e)
                {
                    if (a.Action != null)
                    {
                        Debug.LogError("Loom Call Exception " + a.Action.Method.Name);
                    }
                    else
                    {
                        Debug.LogError("Loom Call Exception Action == null");
                    }
                    Debug.LogError("Loom Error:" + e.ToString());
                }
            }
        }
    }
}

