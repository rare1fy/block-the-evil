using Pb;
using System.Collections;
using UnityEngine;


/// <summary>
/// 剧本跳转战斗事件
/// </summary>
public class PerformEventFight : PerformEventBase
{
    public PerformEventFight(int id, GuildPerform cfg, PerformData data):base(id, cfg, data)
    {
    }

    public override IEnumerator Operation(object obj = null)
    {
        //GameManager.Instance.FightStart(int.Parse(cfg.Num));
        awaitEnd = true;
        yield return new WaitUntil(() => awaitEnd);
        Next();
    }
}
