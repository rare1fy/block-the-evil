using System;


public class EventDispatchCenter
{
    static private EventDispatchCenter m_inst = null;
    public static EventDispatchCenter Instance
    {
        get
        {
            if (null == m_inst)
                m_inst = new EventDispatchCenter();
            return m_inst;
        }
    }

    private EventDispatchCenter() { }


    public void Registry(string iMsgId, Action<object> callback, bool doImd, bool isGlobal = false)
    {
        LTEventCenter.Regist(iMsgId, callback, !doImd, isGlobal);
    }

    public void Registry(string iMsgId, Action<object> callback)
    {
        Registry(iMsgId, callback, false);
    }

    public void UnRegistry(string iMsgId, Action<object> callback)
    {
        LTEventCenter.UnRegist(iMsgId, callback);
    }

    public void Dispatch(string iMsgId)
    {
        Dispatch(iMsgId, null);
    }

    /// <summary>
    /// 抛消息
    /// </summary>
    /// <param name="iMsgId"></param>
    /// <param name="obj"></param>
    /// <param name="ENABLE_THREAD">是否开启多线程模式</param>
    public void Dispatch(string iMsgId, object obj, bool ENABLE_THREAD = true)
    {
        LTEventCenter.SendNotify(iMsgId, obj, ENABLE_THREAD);
    }

    public void ClearAll(bool isClearAll)
    {
        LTEventCenter.ClearAll(isClearAll);
    }
}

