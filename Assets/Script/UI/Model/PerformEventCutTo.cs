using Pb;
using System.Collections;
using UnityEngine;


/// <summary>
/// 剧本转场事件
/// </summary>
public class PerformEventCutTo: PerformEventBase
{
    public PerformEventCutTo(int id, GuildPerform cfg, PerformData data):base(id, cfg, data)
    {
    }

    public override IEnumerator Operation(object obj = null)
    {
        ChapterSceneManager._Instance.ChangeChapter(int.Parse(cfg.Num));
        yield return new WaitUntil(() => awaitEnd);
        Next();
    }
}
