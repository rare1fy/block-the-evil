using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class BlockFinishEffect : MonoBehaviour
{
    public GameObject _Obj_Money;
    public GameObject _Obj_Score;
    public TextMeshProUGUI _Txt_Money;
    public TextMeshProUGUI _Txt_Score;
    
    private void InitAll()
    {
        _Obj_Money.SetActiveEx(false);
        _Obj_Score.SetActiveEx(false);
    }

    public void ShowMoney(int count)
    {
        _Obj_Money.SetActiveEx(true);
        _Obj_Score.SetActiveEx(false);
        _Txt_Money.SetText("x{0}", count);
    }
    
    public void ShowScore(int count)
    {
        _Obj_Money.SetActiveEx(false);
        _Obj_Score.SetActiveEx(true);
        _Txt_Score.text = count.ToString();
    }

}
