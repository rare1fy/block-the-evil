using DG.Tweening;
using UnityEngine;


public class UIBubble : UIItemBase
{
    [BindNode] private CustomText _Txt_Desc;
    protected override void OnDestroy()
    {
        base.OnDestroy();
    }

    protected override void InitItem()
    {
    }

    public void SetOrderNotice()
    {
        var cfg = Config.GetConfig<Config_RandomwordHarry>().m_RandomwordHarryDic;
        var id = Random.Range(1, cfg.Count);
        var desc = cfg[id].Word;
        SetDesc(desc);
    }

    public void SetOrderComplete()
    {
        var cfg = Config.GetConfig<Config_RandomwordComplete>().m_RandomwordCompleteDic;
        var id = Random.Range(1, cfg.Count);
        var desc = cfg[id].Word;
        SetDesc(desc);
    }

    public void SetDesc(string desc)
    {
        gameObject.SetActive(true);
        _Txt_Desc.text = desc;
        DOVirtual.DelayedCall(2f, () =>
        {
            gameObject.SetActive(false);
        });
    }
}
