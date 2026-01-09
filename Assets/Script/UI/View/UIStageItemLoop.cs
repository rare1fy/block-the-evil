using System.Collections.Generic;
using UnityEngine;

public class UIStageItemLoop : UIStageItemBase
{
    [BindNode] Transform _Obj_Node1;
    [BindNode] Transform _Obj_Node2;
    [BindNode] Transform _Obj_Node3;
    [BindNode] Transform _Obj_Node4;
    [BindNode] Transform _Obj_Node5;
    [BindNode] Transform _Obj_Node6;
    [BindNode] Transform _Obj_Node7;
    [BindNode] Transform _Obj_Node8;
    [BindNode] Transform _Obj_Node9;
    [BindNode] Transform _Obj_Node10;
    [BindNode] Transform _Obj_Node11;
    [BindNode] Transform _Obj_Node12;
    [BindNode] Transform _Obj_Node13;
    [BindNode] Transform _Obj_Node14;
    [BindNode] Transform _Obj_Node15;
    [BindNode] Transform _Obj_Node16;

    private List<Transform> Node = new List<Transform>();
    /// <summary>
    /// npcÄ£°å
    /// </summary>
    public GameObject NpcTemplate;
    protected override void InitItem()
    {
        Node = new List<Transform>() { _Obj_Node1, _Obj_Node2, _Obj_Node3, _Obj_Node4, _Obj_Node5, _Obj_Node6, _Obj_Node7, _Obj_Node8,
        _Obj_Node9, _Obj_Node10, _Obj_Node11, _Obj_Node12, _Obj_Node13, _Obj_Node14, _Obj_Node15, _Obj_Node16};
    }

    public void SetItemUI(List<int> npcIds)
    {
        for (int i = 0; i < Node.Count; i++)
        {
            Node[i].gameObject.SetActive(i < npcIds.Count);
            if (i >= npcIds.Count) continue;
            var cfg = Config.GetConfig<Config_NpcBase>().GetConfigById(npcIds[i]);
            var npc = Node[i].GetComponentInChildren<UIStageNpc>();
            if (npc == null)
            {
                var obj = Instantiate(NpcTemplate, Node[i]);
                npc = obj.GetComponent<UIStageNpc>();
            }
            npc.SetUI(cfg);
        }
    }
}
