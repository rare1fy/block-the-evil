using DG.Tweening;
using Framework;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class MaskItem : UIItemBase
{
    [BindNode] private UIButtonExtension _Btn_Money1;
    [BindNode] private UIButtonExtension _Btn_Money2;
    [BindNode] private CustomText _Txt_Money1;
    [BindNode] private UIButtonExtension _Btn_AD1;
    [BindNode] private CustomText _Txt_Money2;
    [BindNode] private UIButtonExtension _Btn_AD2;
    [BindNode] private Image _Img_Mask;
    [BindNode] private Transform _Obj_Eff;
    [BindNode(true)] private GameObject _Obj_Vertical;
    [BindNode(true)] private GameObject _Obj_Across;

    private RectTransform _rect;
    private GameObject obj_eff;
    private int money;
    protected override void InitItem()
    {
        _rect = GetComponent<RectTransform>();
        AddListener(ui_listener_type.onClick, "_Btn_Money1", OnClickMoney);
        AddListener(ui_listener_type.onClick, "_Btn_AD1", OnClickAD);
        AddListener(ui_listener_type.onClick, "_Btn_Money2", OnClickMoney);
        AddListener(ui_listener_type.onClick, "_Btn_AD2", OnClickAD);
    }

    private void OnClickAD(GameObject arg1, PointerEventData arg2)
    {
        PlatformManager.Instance.ShowRewardedVideoAd(7, (isOk) =>
        {
            var levelModel = GameManager.Instance.CurFightControl.LevelController.Model;
            GameManager.Instance.LogManager.Log_AD(7, isOk);
            if (isOk)
            {
                RemoveMask();
            }
            else
            {
                UIManager.Instance.ShowPromptWindow("观看时间不足，无法获取奖励");

            }
        });
    }

    private void OnClickMoney(GameObject arg1, PointerEventData arg2)
    {
        int hasMoney = GameManager.Instance.GameBagControl.GetItemNumberById(GameBagModel.GOLD);
        if(hasMoney >= money)
        {
            var _fightController = GameManager.Instance.CurFightControl;
            _fightController.Model.AddMoney(-money);
            RemoveMask();
        }
        else
        {
            UIManager.Instance.ShowSecondConfirm("钞票不足，是否观看广告解锁", () =>
            {
              
                PlatformManager.Instance.ShowRewardedVideoAd(7,(isOk) =>
                {
                    GameManager.Instance.LogManager.Log_AD(7, isOk);
                    if (isOk)
                    {
                        RemoveMask();
                    }
                    else
                    {
                        UIManager.Instance.ShowPromptWindow("观看时间不足，无法获取奖励");

                    }
                });
            }, isAd: true);
        }
    }

    private MaskData _curMaskData;
    public void InitMaskItem(MaskData maskData)
    {
        _curMaskData = maskData;
        money = Config.GetConfig<Config_GdConstant>().GetConfigById(29).Num * (maskData.Length * maskData.Width);
        _Txt_Money1.text = Util.FormatNumber(money);
        _Txt_Money2.text = Util.FormatNumber(money);
        
        var posX = 76 * maskData.PosY;
        var posY = 76 * maskData.PosX;
        _rect.anchoredPosition = new Vector2(posX, -posY);
        var length = 76 * maskData.Length;
        var width = 76 * maskData.Width;
        _rect.sizeDelta = new Vector2(length, width);
        SetMaskImage(maskData.Length, maskData.Width);
        _Btn_Money1.gameObject.SetActiveEx(maskData.Type == 0);
        _Btn_AD1.gameObject.SetActiveEx(maskData.Type == 1);
        _Btn_Money2.gameObject.SetActiveEx(maskData.Type == 0);
        _Btn_AD2.gameObject.SetActiveEx(maskData.Type == 1);
    }

    private void SetMaskImage(int x, int y)
    {
        _Img_Mask.transform.localRotation = new Quaternion(0, 0, 0, 1);
        string name = "";
        string effName = "";
        if (x == 2 && y == 2)
        {
            name = "UI_zdn_xjz_zhezhao02";
            effName = "FX_C_sha_002";
        }
        else if((x == 1 && y == 2) || (y == 1 && x == 2))
        {
            name = "UI_zdn_xjz_zhezhao01";
            effName = "FX_C_sha_001";
        }
        else if ((x == 1 && y == 3) || (y == 1 && x == 3))
        {
            name = "UI_zdn_xjz_zhezhao03";
            effName = "FX_C_sha_003";
        }
        else if ((x == 1 && y == 4) || (y == 1 && x == 4))
        {
            name = "UI_zdn_xjz_zhezhao04";
            effName = "FX_C_sha_004";
        }

        if (x >= y)
        {
            _Img_Mask.transform.Rotate(0, 0, 0);
            ((RectTransform)_Img_Mask.transform).sizeDelta = new Vector2(76 * x, 76 * y);
        }
        else
        {
            _Img_Mask.transform.Rotate(0,0,-90);
            effName += "_R";
            ((RectTransform)_Img_Mask.transform).sizeDelta = new Vector2(76 * y, 76 * x);
        }

        ResourceManagerNew.instance.LoadAssetAsync<GameObject>(effName, o =>
        {
            obj_eff = Instantiate(o, _Obj_Eff);
            obj_eff.SetActiveEx(false);
        });

        _Obj_Vertical.SetActiveEx(x < y);
        _Obj_Across.SetActiveEx(x >= y);
        ResourceManagerNew.instance.LoadSpriteAsset(name, _Img_Mask);
    }

    void RemoveMask()
    {
        AudioManagerNew.Instance.PlayAudio("fight_item_sandremove.ogg");
        obj_eff?.SetActiveEx(true);
        _Img_Mask.gameObject.SetActiveEx(false);
        _Obj_Vertical.SetActiveEx(false);
        _Obj_Across.SetActiveEx(false);
        DOVirtual.DelayedCall(0.5f, () =>
        {
            for (int i = 0; i < _curMaskData.Width; i++)
            {
                for (int j = 0; j < _curMaskData.Length; j++)
                {
                    var pos = new Vector2Int(_curMaskData.PosX + i, _curMaskData.PosY + j);
                    GameManager.Instance.CurFightControl.Model.SetBlockDataByPos(pos, false, 0);
                }
            }
            GameManager.Instance.CurFightControl.Model.MasksData.Remove(_curMaskData);
            GameManager.Instance.CurFightControl.RefreshAllPuzzleItem();
            gameObject.SetActiveEx(false);
        });
    }
}