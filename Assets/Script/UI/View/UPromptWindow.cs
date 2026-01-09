using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class UPromptWindow : UIBase
{
    [BindNode]
    private Transform _Obj_Layer;

    private Transform _pool;

    private PromptItem _prefabGo;

    [Header("同时最大存在数量")]
    public int maxNumber = 5;

    public override void InitOnce()
    {
        _pool = GetNodeByName<Transform>("_Obj_Pool");
        _prefabGo = GetNodeByName<PromptItem>("_Obj_PromptItem");
    }

    public override void OnOpen(object param = null)
    {
        base.OnOpen(param);
        var tuple = (Tuple<int, int, int, string, bool>)param;

        var type = tuple.value1;
        _pool.gameObject.SetActiveEx(type == 1);
        _prefabGo.gameObject.SetActiveEx(type == 1);
        if (type == 1)
        {
            if (tuple.value5)
            {
                //AudioManagerNew.Instance.PlayAudio("UI_Toast");
            }
            CreatePromptItem(tuple.value4);
        }
    }

    private void CreatePromptItem(string content)
    {
        if (content == null)
        {
            return;
        }
        if (_Obj_Layer.childCount >= maxNumber)
        {
            return;
        }
        GameObject item = null;
        if (_pool.childCount > 0)
        {
            item = _pool.GetChild(0).gameObject;
        }
        else
        {
            item = Instantiate(_prefabGo.gameObject);
        }
        item.gameObject.SetActiveEx(true);
        var itemCtrl = item.GetComponent<PromptItem>();
        var child =  item.transform.GetChild(0);
        itemCtrl.SetDesc(content);
        LayoutRebuilder.ForceRebuildLayoutImmediate(child as RectTransform);
        item.SetParentEx(_Obj_Layer);
        var rTrans = item.transform as RectTransform;
        if (rTrans)
        {
            rTrans.offsetMin = Vector2.zero;
            rTrans.offsetMax = Vector2.zero;
            rTrans.localEulerAngles = Vector3.zero;
            rTrans.localScale = Vector3.one;
            rTrans.anchoredPosition3D = Vector3.zero;
        }
        var tweener = rTrans.transform.DOLocalMoveY(500, 1f, false);
        tweener.SetEase(Ease.OutExpo);
        tweener.onComplete = delegate {
            item.SetParentEx(_pool);
            item.gameObject.SetActiveEx(false);
        };
    }

    private void Update()
    {
    }
}
