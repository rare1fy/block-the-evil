using UnityEngine;

namespace Framework
{
    /// <summary>
    /// 单例模式
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class MonoSingleton<T> : MonoBehaviour where T : MonoBehaviour
    {
        protected static T _Instance;
        public static T Instance
        {
            get
            {
                if (_Instance == null)
                {
                    if ((_Instance = Object.FindObjectOfType<T>()) == null)
                    {
                        GameObject go = new GameObject(objName == null ? typeof(T).ToString() : objName);
                        _Instance = go.AddComponent<T>();
                        (_Instance as MonoSingleton<T>).createInit();
                    }
                    if (Application.isPlaying && isNotDestory)
                        Object.DontDestroyOnLoad(_Instance.gameObject);
                }
                return _Instance;
            }
        }

        protected static string objName = null;
        protected static bool isNotDestory = true;
        public virtual void createInit() { }
    }
}

