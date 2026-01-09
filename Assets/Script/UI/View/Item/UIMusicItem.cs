using Framework;
using Pb;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UIMusicItem : UIItemBase
{
    [BindNode] private Image _Img_Music;
    [BindNode] private Image _Img_Small;
    [BindNode] private UIButtonExtension _Btn_Click;
    
    [BindNode(true)] private GameObject _Img_Select;
    [BindNode(true)] private GameObject _Img_Mask;
    [BindNode] private Image _Img_Consume;
    [BindNode] private CustomText _Txt_ItemCount;
    [BindNode(true)] private GameObject _Obj_Red;
    private GameObject Obj_VFX;

    protected override void InitItem()
    {
        AddListener(ui_listener_type.onClick, "_Btn_Click", OnClick);
        EventDispatchCenter.Instance.Registry(SDEvents.C2C_MUSIC_SELECT, RefreshSelect);
        EventDispatchCenter.Instance.Registry(SDEvents.C2C_MUSIC_PROGRESS_REFRESH, RefreshSelect);
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        EventDispatchCenter.Instance.UnRegistry(SDEvents.C2C_MUSIC_SELECT, RefreshSelect);
        EventDispatchCenter.Instance.UnRegistry(SDEvents.C2C_MUSIC_PROGRESS_REFRESH, RefreshSelect);

    }

    private MusicPlayer _musicCfg;
    private bool isMainOpen;
    public void RefreshItem(MusicPlayer musicPlayerCfg, bool isMainOpen)
    {
        this.isMainOpen = isMainOpen;
        _musicCfg = musicPlayerCfg;
        ResourceManagerNew.instance.LoadSpriteAsset(musicPlayerCfg.Img, _Img_Music);
        ResourceManagerNew.instance.LoadSpriteAsset(musicPlayerCfg.Img, _Img_Small);
        RefreshSelect();
    }
    
    private void RefreshSelect(object param = null)
    {
        if (_musicCfg != null)
        {
            var ctrl = GameManager.Instance.MusicControl;
            var curMusic = ctrl.PickMusicId;
            if (isMainOpen)
            {
                curMusic = ctrl.MainMusicId;
            }
            _Img_Select.gameObject.SetActiveEx(curMusic == _musicCfg.Id);
            //if (Obj_VFX == null && curMusic == _musicCfg.Id)
            //{
            //    ResourceManagerNew.instance.LoadAssetAsync<GameObject>("FX_MusicalNotes", o =>
            //    {
            //        Obj_VFX = Instantiate(o, _Img_Select.transform);
            //    });
            //}
            _Obj_Red.SetActiveEx(ctrl.CheckCanUnLockMusic(_musicCfg.Id));
            var unlock = ctrl.CheckUnLock(_musicCfg.Id);
            _Img_Mask.gameObject.SetActiveEx(!unlock);
            var consume = _musicCfg.Consume.Split("#");
            var blockColorCfg = Config.GetConfig<Config_BlockColor>().GetConfigById(int.Parse(consume[0]));
            ResourceManagerNew.instance.LoadSpriteAsset(blockColorCfg.Img, _Img_Consume);
            _Txt_ItemCount.text = ctrl.GetConsumeText(_musicCfg.Id);
        }
    }

    private void OnClick(GameObject o , PointerEventData eventData)
    {
        var ctrl = GameManager.Instance.MusicControl;
        if (ctrl.CheckUnLock(_musicCfg.Id)) 
        {
            if (ctrl.isCd)
            {
                UIManager.Instance.ShowPromptWindow("切歌太快啦！休息下吧~");
                return;
            }
            ctrl.SelectBGM(_musicCfg.Id,false, isMainOpen);
        }
        else if(ctrl.CheckCanUnLockMusic(_musicCfg.Id))
        {
            var consume = _musicCfg.Consume.Split("#");
            int consumeId = int.Parse(consume[0]);
            int consumeNum = int.Parse(consume[1]);
            var blockCfg = Config.GetConfig<Config_BlockColor>().GetConfigById(consumeId);
            UIManager.Instance.ShowSecondConfirm($"是否消耗{blockCfg.Desc}X{consumeNum}进行解锁", () =>
            {
                ctrl.UnLockMusic(_musicCfg.Id, isMainOpen);
            });
        }
        else
        {
            UIManager.Instance.ShowPromptWindow("资源不足！！");
        }
    }
    
}
