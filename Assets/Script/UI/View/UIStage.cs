using System;
using System.Collections.Generic;

public class UIStage : UIItemBase
{
    [BindNode] LoopListView2 _Obj_StageList;

    public readonly int NpcItemCount = 16; 

    StageControl ctrl;
    int Count = 0;
    int addNum = 0;

    protected override void InitItem()
    {
        _Obj_StageList.InitListViewNoFill(InitList);
        ctrl = GameManager.Instance.StageControl;
        EventDispatchCenter.Instance.Registry(SDEvents.STAGE_NUM_REFRESH, SetUI);
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        EventDispatchCenter.Instance.UnRegistry(SDEvents.STAGE_NUM_REFRESH, SetUI);

    }

    public void SetUI(object o = null)
    {
        if (o != null && o is int num) 
        {
            addNum = num;
        }

        Count = (int)Math.Ceiling(ctrl.joined.Count / (double)NpcItemCount) + 2;
        Count = Count > 3 ? Count : 3;
        _Obj_StageList.ResetListView();
        _Obj_StageList.FillSuperList(Count > 3 ? Count : 3);
    }

    private LoopListViewItem2 InitList(LoopListView2 list, int idx)
    {
        LoopListViewItem2 item;
        if (idx == 0)
        {
            item = list.NewListViewItem("UIStageStart");
            var ui = item.GetComponent<UIStageItemStart>();
            ui.SetUI(this, addNum);
        }
        else if (idx == Count - 1)
        {
            item = list.NewListViewItem("UIStageEnd");
        }
        else
        {
            List<int> jumpData ;
            if (ctrl.joined.Count == 0)
            {
                jumpData = new List<int>();
            }
            else if(idx * NpcItemCount > ctrl.joined.Count)
            {
                jumpData = ctrl.joined.GetRange((idx - 1) * NpcItemCount, ctrl.joined.Count - (idx - 1) * NpcItemCount);
            }
            else
            {
                jumpData = ctrl.joined.GetRange((idx - 1) * NpcItemCount, NpcItemCount);
            }
            item = list.NewListViewItem($"UIStageLoop");
            var ui = item.GetComponent<UIStageItemLoop>();
            ui.SetItemUI(jumpData);
        }
        return item;
    }
}
