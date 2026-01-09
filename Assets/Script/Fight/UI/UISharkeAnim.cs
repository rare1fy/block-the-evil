using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UISharkeAnim : MonoBehaviour
{
    public List<UISharkeFlyItem> _Obj_items;

    private Animator animator;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        foreach (var item in _Obj_items)
        {
            item.SetItem();
        }
    }

    public void Sharke()
    {
        GameManager.Instance.CurFightControl.UseShaker();
        foreach (var item in _Obj_items)
        {
            item.SetItem();
        }
    }

}
