using System;
using UnityEngine;
using UnityEngine.EventSystems;
using static UIManager;

public class UISetPopup : UIBase
{
    [BindNode(true)] GameObject _Btn_go;
    [BindNode(true)] GameObject _Btn_cancel;
    [BindNode(true)] GameObject _Btn_adv;
    [BindNode(true)] GameObject _Obj_Open;
    [BindNode(true)] GameObject _Obj_Close;
    [BindNode] UnityEngine.UI.Slider _Obj_Slider;
    [BindNode] CustomText _Txt_Stamina;
    [BindNode] CustomText _Txt_Stamina1;
    PlayerData PlayerData;
    PlayerModel PlayerModel;
    bool showBtn;
    object _param;
    public override void InitOnce()
    {
        AddListener(ui_listener_type.onClick, "_Btn_cancel", OnClickHome);
        AddListener(ui_listener_type.onClick, "_Btn_adv", OnClickAdv);
        AddListener(ui_listener_type.onClick, "_Btn_go", OnClickReset);
        AddListener(ui_listener_type.onClick, "_Btn_Shak", OnClickShak);
        AddListener(ui_listener_type.onClick, "_Btn_Close", base.OnBackClick);
        EventDispatchCenter.Instance.Registry(SDEvents.ACROSS_THE_DAY, OnAcrossDay);

        _Obj_Slider.onValueChanged.AddListener(OnSliderChanged);
        PlayerData = PlayerDataManager.instance.PlayerData;
        PlayerModel = GameManager.Instance.PlayerControl.PlayerModel;
    }

    private void OnAcrossDay(object obj)
    {
        OnOpen(_param);
    }

    public override void OnOpen(object param = null)
    {
        _param = param;
        showBtn = param == null ? true : (bool)param;
        if (!showBtn)
        {
            _Btn_adv.SetActiveEx(false);
            _Btn_go.SetActiveEx(false);
            _Btn_cancel.SetActiveEx(false);
            _Txt_Stamina.text = $"今日剩余:{PlayerModel.Stamina}/ {PlayerModel.MaxStamina}";
            _Txt_Stamina1.text = $"今日剩余:{PlayerModel.Stamina}/ {PlayerModel.MaxStamina}";
        }
        else
        {
            var isEndless = GameManager.Instance.CurFightControl.LevelController.Model.IsEndLess;

            _Btn_cancel.SetActiveEx(true);
            _Btn_adv.SetActiveEx(!PlayerModel.HasEnoughStaminaForFight() && !PlayerModel.CanClaimDailyStamina() && !isEndless);
            _Btn_go.SetActiveEx((PlayerModel.HasEnoughStaminaForFight() || PlayerModel.CanClaimDailyStamina()) && !isEndless);
            _Txt_Stamina.text = $"今日剩余:{PlayerModel.Stamina}/ {PlayerModel.MaxStamina}";
            _Txt_Stamina1.text = $"今日剩余:{PlayerModel.Stamina}/ {PlayerModel.MaxStamina}";
        }
        _Obj_Slider.value = PlayerData.VolumeBlend;
        _Obj_Open.SetActiveEx(PlayerData.BShake);
        _Obj_Close.SetActiveEx(!PlayerData.BShake);
    }

    public override void OnClose()
    {
        base.OnClose();
        PlayerData.VolumeBlend = _Obj_Slider.value;
        AudioManagerNew.Instance.ChangeVolumeBlend(_Obj_Slider.value);
        PlayerDataManager.instance.SettingSave();
    }

    protected override void OnDestroy()
    {
        EventDispatchCenter.Instance.UnRegistry(SDEvents.ACROSS_THE_DAY, OnAcrossDay);

        if (_Obj_Slider != null)
            _Obj_Slider.onValueChanged.RemoveListener(OnSliderChanged);
        base.OnDestroy();
    }

    float await = 0.1f;
    bool bSlider = false;

    private void FixedUpdate()
    {
        await -= Time.fixedDeltaTime;
        if (await < 0 && bSlider)
        {
            bSlider = false;
            PlayerData.VolumeBlend = _Obj_Slider.value;
            AudioManagerNew.Instance.ChangeVolumeBlend(_Obj_Slider.value);
            PlayerDataManager.instance.SettingSave();
        }
    }

    private void OnSliderChanged(float arg0)
    {
        await = 0.1f;
        bSlider = true;
    }


    private void OnClickReset(GameObject @object, PointerEventData data)
    {
        GameRestart();
    }
    
    private void OnClickAdv(GameObject @object, PointerEventData data)
    {
        GameRestart();
    }

    private void GameRestart()
    {
        var isEndless = GameManager.Instance.CurFightControl.LevelController.Model.IsEndLess;
        if (isEndless)
        {
            GameManager.Instance.CurFightControl.ClearData();
            UIManager.Instance.CloseAll(true);
            GameManager.Instance.FightStart(0, 2);
        }
        else
        {
            GameManager.Instance.PlayerControl.StartFightHasTips(() =>
            {
                GameManager.Instance.CurFightControl.FightEnd(false);
                GameManager.Instance.CurFightControl.ClearData();
            });
        }
    }
    
    
    private void OnClickShak(GameObject @object, PointerEventData data)
    {
        PlayerData.BShake = !PlayerData.BShake;
        _Obj_Open.SetActiveEx(PlayerData.BShake);
        _Obj_Close.SetActiveEx(!PlayerData.BShake);
        PlayerDataManager.instance.SettingSave();
    }

    private void OnClickHome(GameObject @object, PointerEventData data)
    {
        var txt = "";
        var isEndless = GameManager.Instance.CurFightControl.LevelController.Model.IsEndLess;
        txt = isEndless ? "是否放弃本局游戏，立刻结算得分？" : "是否退出该局游戏，游戏进度将不保存！！";

        UIManager.Instance.ShowSecondConfirm( txt, () =>
        {
            if (isEndless)
            {
                UIManager.Instance.HideUI(this);
                EventDispatchCenter.Instance.Dispatch(SDEvents.C2C_FIGHT_END);
            }
            else
            {
                GameManager.Instance.CurFightControl.FightEnd(false);
                GameManager.Instance.CurFightControl.ClearData();
                UIManager.Instance.HideUI(this);
                CutToScene(() =>
                {
                    UIManager.Instance.CloseAll(true);
                    UIManager.Instance.ShowUI("MainWindow");
                });
            }
        });
    }
}
