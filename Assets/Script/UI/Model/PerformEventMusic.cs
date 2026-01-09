using Pb;
using System.Collections;
using UnityEngine;


/// <summary>
/// 剧本跳转战斗事件
/// </summary>
public class PerformEventMusic : PerformEventBase
{
    public PerformEventMusic(int id, GuildPerform cfg, PerformData data):base(id, cfg, data)
    {
    }

    public override IEnumerator Operation(object obj = null)
    {
        AudioManagerNew.Instance.FadeStopMusic(() =>
        {
            AudioManagerNew.Instance.PlayMusic(cfg.Num);
        });
        awaitEnd = true;
        yield return new WaitUntil(() => awaitEnd);
        Next();
    }
}
