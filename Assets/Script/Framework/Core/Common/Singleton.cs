using System;

public abstract class Singleton<T> where T : class, new()
{
    private static T m_instance;
    public static T instance
    {
        get
        {
            if (m_instance == null)
            {
                m_instance = Activator.CreateInstance<T>();
                if (m_instance != null)
                {
                    (m_instance as Singleton<T>).Init();
                }
            }
            return m_instance;
        }
    }

    public static void Release()
    {
        if (!ReferenceEquals(m_instance,null))
        {
            (m_instance as Singleton<T>).Dispose();
            m_instance = null;
        }
    }

    public virtual void Init()
    {

    }

    public abstract void Dispose();

}
