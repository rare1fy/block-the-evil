using Pb;
using System;
using System.Collections;
using UnityEngine;


/// <summary>
/// 剧本聊天事件
/// </summary>
public class PerformEventChat : PerformEventBase
{
    public PerformEventChat(int id, GuildPerform cfg, PerformData data):base(id, cfg, data)
    {
    }

    public override IEnumerator Operation(object obj = null)
    {
        UIManager.Instance.ShowUI("PerformWindow", param: cfg);
        yield return new WaitUntil(() => awaitEnd);
        Next();
    }
}
