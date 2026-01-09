using Pb;
using System.Collections;
using UnityEngine;


/// <summary>
/// 剧本动画事件
/// </summary>
public class PerformEventAnimator : PerformEventBase
{
    public PerformEventAnimator(int id, GuildPerform cfg, PerformData data):base(id,cfg,data)
    {
    }
    
    
    public override IEnumerator Operation(object obj = null)
    {
        ChapterSceneManager._Instance.OnPlayAnimator(cfg.Num);
        AudioManagerNew.Instance.PlayAudio(cfg.Audio);
        yield return null;
        var time = ChapterSceneManager._Instance.AnimatorLength();
        yield return new WaitForSeconds(time);
        AudioManagerNew.Instance.StopAudio(cfg.Audio);
        Next();
    }
}
