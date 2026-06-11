using System;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using Framework;
using Google.Protobuf.WellKnownTypes;
using Pb;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public partial class UIFightMain : UIBase
{
    /// <summary>
    /// 方块父物体
    /// </summary>
    [BindNode] private RectTransform _Obj_GridParent;
    [BindNode] private Canvas _Obj_GridCanvas;
    [BindNode] private RandomItem _Obj_RandomItem1; //随机
    [BindNode] private RandomItem _Obj_RandomItem2;
    [BindNode] private RandomItem _Obj_RandomItem3;
    [BindNode] [HideInInspector] public UIOrderPanel _Obj_OrderPanel;
    [BindNode] private RandomItemRay _Obj_DragRandomItem;  //拖拽用
    [BindNode] private Transform _Obj_Money;
    [BindNode] private CustomText _Txt_Money;
    [BindNode] private CustomText _Txt_People;
    [BindNode] private CustomText _Txt_Day;
    [BindNode] private CustomText _Txt_AddMoney;
    [BindNode] private Image _Img_Money;
    [BindNode] private Image _Img_Block;    //小方块预制  动画用
    [BindNode(true)] private GameObject _Obj_People;   
    [BindNode] private UIGameOver _Obj_GameOver;
    [BindNode] private Image _Img_Music;
    [BindNode(true)] private GameObject _Obj_FX_MusicalNotes;
    [BindNode(true)] private GameObject _Img_BrokenMusic;
    [BindNode] private RectTransform _Btn_Music;
    [BindNode(nodeName:"_Btn_Music")] private Animator musicAnimator;
    [BindNode] private CustomText _Txt_PropCount1;
    [BindNode] private CustomText _Txt_PropCount2;
    [BindNode] private CustomText _Txt_PropCount3;
    [BindNode(true)] private GameObject _Obj_RedPoint;
    [BindNode] private ComboReword _Obj_ComboReword;

    private readonly List<BlockItem> _blockItemList = new List<BlockItem>();
    private readonly List<RandomItem> _randomItemList = new List<RandomItem>();
    private FightController _fightController;

    public override void InitOnce()
    {
        base.InitOnce();
        _fightController = GameManager.Instance.CurFightControl;
        _intervalGuideTime = Config.GetConfig<Config_GdConstant>().GetConfigById(30).Num;
        _Obj_RandomItem1.BindBtn(BtnBeginDrag, BtnOnDrag, BtnEndDrag);
        _Obj_RandomItem2.BindBtn(BtnBeginDrag, BtnOnDrag, BtnEndDrag);
        _Obj_RandomItem3.BindBtn(BtnBeginDrag, BtnOnDrag, BtnEndDrag);
        _randomItemList.Add(_Obj_RandomItem1);
        _randomItemList.Add(_Obj_RandomItem2);
        _randomItemList.Add(_Obj_RandomItem3);
        
        InitPartialOnce();
        InitEffect();
        InitMask();
        AddListener(ui_listener_type.onClick, "_Btn_Music", OnOpenMusicWindow);
        AddListener(ui_listener_type.onClick, "_Btn_Exit", OnClickExit);
        EventDispatchCenter.Instance.Registry(SDEvents.C2C_REFRESH_MAIN_BLOCK, RefreshAllBlock);
        EventDispatchCenter.Instance.Registry(SDEvents.C2C_REFRESH_MAIN_All_ITEM, RefreshAllItem);
        EventDispatchCenter.Instance.Registry(SDEvents.C2C_UPDATE_COMBO_TIME, UpdateComboEffect);
        EventDispatchCenter.Instance.Registry(SDEvents.C2C_GAME_CONTINUE, GameContinueEffect);
        EventDispatchCenter.Instance.Registry(SDEvents.C2C_MUSIC_PROGRESS_REFRESH, RefreshMusicProgress);
        EventDispatchCenter.Instance.Registry(SDEvents.C2C_MUSIC_BLOCK_ANIM, PlayMusicBlockClearAnim);
        EventDispatchCenter.Instance.Registry(SDEvents.C2C_MUSIC_SELECT, RefreshMusicImg);
        EventDispatchCenter.Instance.Registry(SDEvents.BAG_UPDATA_ITEM, RefreshItem);
        EventDispatchCenter.Instance.Registry(SDEvents.C2C_FIGHT_END, FightLoseEffect);
        EventDispatchCenter.Instance.Registry(SDEvents.C2C_REFRESH_PUZZLE_ITEM, RefreshAllPuzzleItem);
        EventDispatchCenter.Instance.Registry(SDEvents.C2C_UPDATE_SCORE, SetEndlessRank);
    }

    private void OnOpenMusicWindow(GameObject o, PointerEventData e)
    {
        var closeCB = new Action(() =>
        {
            _Obj_FX_MusicalNotes.SetActiveEx(true);
        });
        UIManager.Instance.ShowUI("UIMusicWindow", obj =>
        {
            _Obj_FX_MusicalNotes.SetActiveEx(false);
        }, new Tuple<Action, bool>(closeCB, false));
    }

    private void OnClickExit(GameObject _, PointerEventData __)
    {
        UIManager.Instance.ShowUI("UISetPopup");
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        EventDispatchCenter.Instance.UnRegistry(SDEvents.C2C_REFRESH_MAIN_BLOCK, RefreshAllBlock);
        EventDispatchCenter.Instance.UnRegistry(SDEvents.C2C_REFRESH_MAIN_All_ITEM, RefreshAllItem);
        EventDispatchCenter.Instance.UnRegistry(SDEvents.C2C_UPDATE_COMBO_TIME, UpdateComboEffect);
        EventDispatchCenter.Instance.UnRegistry(SDEvents.C2C_GAME_CONTINUE, GameContinueEffect);
        EventDispatchCenter.Instance.UnRegistry(SDEvents.C2C_MUSIC_PROGRESS_REFRESH, RefreshMusicProgress);
        EventDispatchCenter.Instance.UnRegistry(SDEvents.C2C_MUSIC_BLOCK_ANIM, PlayMusicBlockClearAnim);
        EventDispatchCenter.Instance.UnRegistry(SDEvents.C2C_MUSIC_SELECT, RefreshMusicImg);
        EventDispatchCenter.Instance.UnRegistry(SDEvents.BAG_UPDATA_ITEM, RefreshItem);
        EventDispatchCenter.Instance.UnRegistry(SDEvents.C2C_FIGHT_END, FightLoseEffect);
        EventDispatchCenter.Instance.UnRegistry(SDEvents.C2C_REFRESH_PUZZLE_ITEM, RefreshAllPuzzleItem);
        EventDispatchCenter.Instance.UnRegistry(SDEvents.C2C_UPDATE_SCORE, ShowEndlessTips);
        EventDispatchCenter.Instance.UnRegistry(SDEvents.C2C_UPDATE_SCORE, SetEndlessRank);
    }

    #region 界面刷新
    /// <summary>
    /// 道具数量
    /// </summary>
    /// <param name="param"></param>
    private void RefreshItem(object param = null)
    {
        ItemConfig cfg = (ItemConfig)param;
        RefreshPropItem(param);
        if (cfg!= null && cfg.Id == GameBagModel.GOLD)
        {
            var addMoney = cfg.Number;
            var addTxt = Instantiate(_Txt_AddMoney, _Obj_Money);
            addTxt.gameObject.SetActiveEx(true);
            if (addMoney > 0)
            {
                addTxt.GetComponent<Animator>().Play("Add");
                //PlayMoneyFlyAnim((int)addMoney);
                addTxt.text = $"+{addMoney}";
                var endNum =  _fightController.Model.Money;
                var startNum = endNum - (int)addMoney;
                DOVirtual.Int(startNum, endNum, 1f, v =>
                {
                    _Txt_Money.text = Util.FormatNumber(v);
                }).onComplete = () =>
                {
                    Destroy(addTxt);
                };
            }
            else
            {
                AudioManagerNew.Instance.PlayAudio("UI_use_money.ogg");
                addTxt.GetComponent<Animator>().Play("Remove");
                addTxt.text = $"{addMoney}";
                var endNum = _fightController.Model.Money;
                var startNum = endNum - (int)addMoney;
                DOVirtual.Int(startNum, endNum, 1f, v =>
                {
                    _Txt_Money.text = Util.FormatNumber(v);
                }).onComplete = () =>
                {
                    Destroy(addTxt);
                };
            }
        }
    }

    private void RefreshMusicProgress(object param = null)
    {
        var hasUnLockable  = GameManager.Instance.MusicControl.HasUnlockable();
        if (hasUnLockable) 
        {
            if (_Obj_RedPoint.activeSelf == false)
            {
                AudioManagerNew.Instance.PlayAudio("fight_notice_new_song.ogg");
            }
            musicAnimator.Play("_Btn_Music_Loop_Loop");
        }
        else
        {
            musicAnimator.Play("_Btn_Music_Loop");
        }
        _Obj_RedPoint.SetActiveEx(hasUnLockable);
    }

    public void RefreshOrderNum()
    {
        var levelModel = _fightController.LevelController.Model;
        if (levelModel.LevelBaseData.Modle == 1)
        {
            _Txt_People.text = $"<color=#00FC3B>{levelModel.FinishOrderList.Count}</color>/{levelModel.LevelBaseData.Modletarget}";
        }
    }

    public void RefreshAllBlock(object param = null)
    {
        foreach (var blockItem in _blockItemList)
        {
            blockItem.RefreshItem();
        }
    }

    public void RefreshPuzzleOnShow()
    {
        foreach (var randomItem in _randomItemList)
        {
            randomItem.RefreshPuzzle();
        }
    }

    private void RefreshAllPuzzleItem(object param = null)
    {
        if (param is bool and true)
        {
            AudioManagerNew.Instance.PlayAudio("fight_block_refresh");
            for (var i = 0; i < _randomItemList.Count; i++)
            {
                _randomItemList[i].InitPuzzle(i);
            }
        }
        else
        {
            foreach (var randomItem in _randomItemList)
            {
                randomItem.RefreshPuzzle();
            }
        }
        
        SetUrgency();
    }

    private void RefreshAllItem(object param = null)
    {
        RefreshAllBlock();  
        RefreshAllPuzzleItem();
    }

    private void RefreshMusicImg(object param = null)
    {
        var id = GameManager.Instance.MusicControl.PickMusicId;
        if (id > 0)
        {
            _Btn_Music.gameObject.SetActiveEx(true);
            _Img_BrokenMusic.SetActiveEx(false);
            var cfg = Config.GetConfig<Config_MusicPlayer>().GetConfigById(id);
            ResourceManagerNew.instance.LoadSpriteAsset(cfg.Img, _Img_Music);
        }
    }
    #endregion
    
    public override void OnOpen(object param = null)
    {
        base.OnOpen(param);
        OpenProp();
        OnOpenWait();
        CheckChat();
        InitRemoves();
        InitBlockList();
        CreateAllMask();
        RefreshMusicProgress();
        RefreshMusicImg();
        _Btn_Music.gameObject.SetActiveEx(_fightController.LevelController.Model.LevelId != 1 || _fightController.LevelController.Model.IsEndLess);  //第一关不显示
        _Img_BrokenMusic.SetActiveEx(_fightController.LevelController.Model.LevelId == 1 && !_fightController.LevelController.Model.IsEndLess);
        RefreshItem();
        InitEndlessTips();
        _Obj_OrderPanel.InitPanel(this);
        _Txt_AddMoney.gameObject.SetActiveEx(false);
        _Obj_GuideGroup.SetActiveEx(false);
        
        var levelModel = _fightController.LevelController.Model;
        _Txt_Money.text = Util.FormatNumber(_fightController.Model.Money);
        _Txt_Day.text = levelModel.IsEndLess ? "大咖挑战" : $"第{levelModel.LevelId}天";  
        _Obj_People.SetActiveEx(levelModel.LevelBaseData.Modle == 1 && !levelModel.IsEndLess);
        if (levelModel.LevelBaseData.Modle == 1)
        {
            _Txt_People.text = $"<color=#00FC3B>{levelModel.FinishOrderList.Count}</color>/{levelModel.Target}";
        }
        
        GameManager.Instance.LogManager.Log_GameStart(levelModel.LevelId, levelModel.IsEndLess); //统计
    }

    private void FixedUpdate()
    {
        #region 测试用

        #if UNITY_EDITOR
        if (Input.GetKeyDown(KeyCode.L))
        {
            foreach (var puzzleData in _fightController.Model.RandomPuzzleList)
            {
                if (!puzzleData.bUsed)
                {
                    if (_fightController.CheckPuzzleCanPut(puzzleData)) //还有可以放置的拼图
                    {
                        return;
                    }
                }
            }
        }

        if (Input.GetKeyDown(KeyCode.K))
        {
            var puzzleId = _fightController.LevelController.GetPuzzleCanComplete();
            Debug.LogError($"触发elo    puzzleId:{puzzleId}");
        }
        #endif
        
        #endregion

        if(_fightController == null || _fightController.IsGameEnd ||! _fightController.fightStart)
            return;
        
        PlayGuide();
    }

    private void InitBlockList()
    {
        foreach (var data in _fightController.Model.MBlockList)
        {
            ResourceManagerNew.instance.LoadAssetAsync<GameObject>("BlockItem", obj =>
            {
                var blockItemObj = Instantiate(obj, _Obj_GridParent);
                var blockItem = blockItemObj.GetComponent<BlockItem>();
                blockItem.name = $"Block_{data.Pos.x}_{data.Pos.y}";
                blockItem.SetPos(data.Pos);
                _blockItemList.Add(blockItem);
            });
        }

        for (var i = 0; i <  _randomItemList.Count; i++)
        {
            _randomItemList[i].InitPuzzle(i);
        }
    }


    private Vector2 _startPos;
    private void BtnBeginDrag(GameObject go, PointerEventData eventData)
    {
        if(_Obj_GuideGroup.activeSelf)
        {
            PlayerPrefs.SetInt(Util.GUIDEKEY, 1);
            _Obj_GuideGroup.SetActiveEx(false);
        }
        
        _guideTime = 0f;
        _Obj_DragRandomItem.CloneItem(go.transform.parent.gameObject.GetComponent<RandomItem>());
        _Obj_DragRandomItem.transform.localScale = Vector3.one * 2f;
        AudioManagerNew.Instance.PlayAudio("fight_pickup");
        RectTransformUtility.ScreenPointToLocalPointInRectangle(transform as RectTransform, eventData.position,
            eventData.pressEventCamera, out Vector2 localPos);
        _Obj_DragRandomItem.transform.localPosition = localPos;
        _startPos = localPos;
        _Obj_DragRandomItem.CreateRayDray(_Obj_GridCanvas);
        
        go.GetComponentInParent<RandomItem>().BShowPanel(false);
    }

    private float _tempTime = 0f;
    private float _interval = 0.1f;
    private void BtnOnDrag(GameObject go, PointerEventData eventData)
    {
        if (_tempTime < _interval)  //降低一点频率
        {
            _tempTime += Time.deltaTime;
            return;
        }

        if (UIManager.Instance.GetTopUI() != this)
        {
            BtnEndDrag(go, eventData);
            return;
        }
        
        RectTransformUtility.ScreenPointToLocalPointInRectangle(transform as RectTransform, eventData.position,
            eventData.pressEventCamera, out Vector2 localPos);

        var displacement = (localPos - _startPos) * 1.3f;
        
        _Obj_DragRandomItem.transform.localPosition = displacement + _startPos + new Vector2( 0, 180f);
        
        go.GetComponentInParent<RandomItem>().BShowPanel(false);
        if (_Obj_DragRandomItem.CheckRay())
        {
            var complete = _fightController.GetDragTempBlockCompletedData(_Obj_DragRandomItem._targetData);
            PlayHoldEffect(complete);
        }
        else
        {
            RefreshAllBlock();//不满足条件就不显示了
            _tempCols.Clear();
            _tempRows.Clear();
            foreach (var holdEffectObj in _holdEffectList)
            {
                holdEffectObj.SetActiveEx(false);
            }
            EventDispatchCenter.Instance.Dispatch(SDEvents.C2C_BLOCK_HOLD_EFFECT, null, false);
            AudioManagerNew.Instance.StopAudio("fight_kill_preview");
        }
    }
    
    private void BtnEndDrag(GameObject go, PointerEventData eventData)
    {
        AudioManagerNew.Instance.PlayAudio("fight_putdown");
        foreach (var holdEffectObj in _holdEffectList) //关闭所有特效
        {
            holdEffectObj.SetActiveEx(false);
        }
        EventDispatchCenter.Instance.Dispatch(SDEvents.C2C_BLOCK_HOLD_EFFECT, null, false);
        AudioManagerNew.Instance.StopAudio("fight_kill_preview");
        
        if (_Obj_DragRandomItem.CheckRay())
        {
            StartCoroutine(_fightController.
                OnPuzzlePutDown(this,  _Obj_DragRandomItem.Index, _Obj_DragRandomItem._targetData.RayPosColorList));
            
            _Obj_DragRandomItem.transform.localPosition = new Vector3(-2000, 0, 0);
        }
        else
        {
            ShowWait(0.2f);
            RefreshAllBlock();
            var targetPos = go.transform.parent.position;
            if (_Obj_DragRandomItem != null)
            {
                _Obj_DragRandomItem.transform.DOScale(1f, 0.2f);
                _Obj_DragRandomItem.transform.DOMove(targetPos, 0.2f).OnComplete(() =>
                {
                    go.GetComponentInParent<RandomItem>().BShowPanel(true);
                    _Obj_DragRandomItem.transform.localPosition = new Vector3(-2000, 0, 0);
                });
            }
            
            ResourceManagerNew.instance.LoadAssetAsync<GameObject>("FX_C1_CannotPlace1", obj =>
            {
                AudioManagerNew.Instance.PlayAudio("fight_cannot_place");
                var fx = Instantiate(obj, _Obj_Pool.transform);
                fx.transform.localPosition = Vector3.zero;
            });
            
        }
    }
    
    #region 特效相关
    
    [BindNode] private RectTransform _Obj_Pool; //特效父节点
    [BindNode] private RectTransform _Obj_MoneyAnimParent;
    [BindNode] private RectTransform _Obj_BlockAnimParent;

    private List<GameObject> _holdEffectList = new List<GameObject>();
    private List<GameObject> _clearEffectList = new List<GameObject>();
    private List<int> _tempCols= new List<int>();
    private List<int> _tempRows = new List<int>();
    /// <summary>
    /// 拖拽时特效
    /// </summary>
    private void PlayHoldEffect(CompletedLines clearData)
    {
        var cols = clearData.completedCols;
        var rows = clearData.completedRows;
        
        if (_tempCols.SequenceEqual(cols) && _tempRows.SequenceEqual(rows))
        {
            //完全相同就不执行了
            return;
        }
        _tempCols = cols;
        _tempRows = rows;
        foreach (var holdEffectObj in _holdEffectList)
        {
            holdEffectObj.SetActiveEx(false);
        }
        EventDispatchCenter.Instance.Dispatch(SDEvents.C2C_BLOCK_HOLD_EFFECT, null, false);
        
        if (cols.Count > 0)
        {
            foreach (var col in cols)
            {
                var effect = GetHoldEffect();
                if(effect != null)
                    PlayHoldEffect(effect, col, true);
                else
                {
                    ResourceManagerNew.instance.LoadAssetAsync<GameObject>("FX_C_TBC_Line_Loop001", eff =>
                    {
                        var effObj = Instantiate(eff, _Obj_Pool);
                        _holdEffectList.Add(effObj);
                        PlayHoldEffect(effObj, col, true);
                    });
                }
            }
        }
        
        if (rows.Count > 0)
        {
            foreach (var row in rows)
            {
                var effect = GetHoldEffect();
                if(effect != null)
                    PlayHoldEffect(effect, row, false);
                else
                {
                    ResourceManagerNew.instance.LoadAssetAsync<GameObject>("FX_C_TBC_Line_Loop001", eff =>
                    {
                        var effObj = Instantiate(eff, _Obj_Pool);
                        _holdEffectList.Add(effObj);
                        PlayHoldEffect(effObj, row, false);
                    });
                }
            }
        }

        if (cols.Count + rows.Count >= 1)
        {
            AudioManagerNew.Instance.PlayAudio("fight_kill_preview");
            PlatformManager.Instance.ShortVibration(1);
        }
        else
        {
            AudioManagerNew.Instance.StopAudio("fight_kill_preview");
        }
        
        foreach (var blockItemData in clearData.triggerCounts)
        {
            if (blockItemData.Value > 0)
            {
                EventDispatchCenter.Instance.Dispatch(SDEvents.C2C_BLOCK_HOLD_EFFECT, blockItemData.Key);
            }
        }
    }

    /// <summary>
    /// 拖拽特效
    /// </summary>
    /// <param name="effObj">特效obj</param>
    /// <param name="pos">位置</param>
    /// <param name="isCol">是不是行</param>
    private void PlayHoldEffect(GameObject effObj, int pos, bool isCol)
    {
        effObj.SetActiveEx(true);
        var targetPos = 76 * pos - 266;  //位置公式 调整gridLayout时要改
        Vector2 rectPos;
        if (isCol)
        {
            rectPos = new Vector2(0, -targetPos);
            effObj.transform.localPosition = rectPos;
            effObj.transform.localRotation = Quaternion.Euler(0, 0, 90);
        }
        else
        {
            rectPos = new Vector2(targetPos, 0);
            effObj.transform.localPosition = rectPos;
            effObj.transform.localRotation = Quaternion.identity;
        }
        
        // var fxData = new Tuple<int, bool> {value1 = pos, value2 = isCol };
        // EventDispatchCenter.Instance.Dispatch(SDEvents.C2C_BLOCK_HOLD_EFFECT, fxData);
    }

    private GameObject GetHoldEffect()
    {
        foreach (var holdEffObj in _holdEffectList)
        {
            if (!holdEffObj.activeSelf)
            {
                return holdEffObj;
            }
        }
        return null;
    }
    
    /// <summary>
    /// 播放消除特效
    /// </summary>
    /// <param name="param"></param>
    public void PlayClearEffect(object param)
    {
        var clearBlocks = (List<Vector2Int>)param;
        foreach (var blockPos in clearBlocks)
        {
            EventDispatchCenter.Instance.Dispatch(SDEvents.C2C_BLOCK_CLEAR_EFFECT, blockPos);
        }
        AudioManagerNew.Instance.PlayAudio("fight_kill_block");
    }

    /// <summary>
    /// 播放消除特效
    /// </summary>
    /// <param name="blocks"></param>
    public void PlayClearEffect(List<Vector2Int> blocks)
    {
        foreach (var blockPos in blocks)
        {
            EventDispatchCenter.Instance.Dispatch(SDEvents.C2C_BLOCK_CLEAR_EFFECT, blockPos);
        }
        AudioManagerNew.Instance.PlayAudio("fight_kill_block");
    }

    /// <summary>
    /// 连消 多消动画
    /// </summary>
    public void PlayComboEffect(FighteffectBase clearConf, FighteffectBase comboConf)
    {
        ShowComboEff();
        //ShowClearGlow(clearConf);
        if (clearConf.Num > 1)  //多消  1行以上
        {
            ResourceManagerNew.instance.LoadAssetAsync<GameObject>(clearConf.Img, obj =>
            {
                var fxObj = Instantiate(obj, transform);
                fxObj.transform.localPosition = Vector3.zero;
                fxObj.transform.localScale = Vector3.one;
                AudioManagerNew.Instance.PlayAudio(clearConf.Audio);
                _Obj_ComboReword.ShowComboReword(clearConf.Id);
                if (!GameManager.Instance.CurFightControl.LevelController.Model.IsEndLess)
                {
                    GameManager.Instance.CurFightControl.Model.AddMoney(clearConf.Reward);
                }
            });
            
            ResourceManagerNew.instance.LoadAssetAsync<GameObject>("FX_C_TBC_Frame_Loop", obj =>
            {
                AudioManagerNew.Instance.PlayAudio("fight_kill_block");
                var fxObj = Instantiate(obj, _Obj_Pool);
                DOVirtual.DelayedCall(0.2f, () => { Destroy(fxObj); });
            });
        } 
        
        if (comboConf != null && comboConf.Num > 1)
        {
            var func = new TweenCallback(() =>
            {
                ResourceManagerNew.instance.LoadAssetAsync<GameObject>(comboConf.Img, obj =>
                {
                    var fxObj = Instantiate(obj, transform);
                    fxObj.transform.localPosition = Vector3.zero;
                    fxObj.transform.localScale = Vector3.one;
                    AudioManagerNew.Instance.PlayAudio(comboConf.Audio);
                    if (!GameManager.Instance.CurFightControl.LevelController.Model.IsEndLess)
                    {
                        GameManager.Instance.CurFightControl.Model.AddMoney(comboConf.Reward);
                    }
                });
            });
            
            if (clearConf.Num > 1)  //只有多消才会出特效  就延迟一下
            {
                DOVirtual.DelayedCall(0.35f, func);
            }
            else
            {
                func.Invoke();
            }
        }

        if (clearConf.Num + comboConf?.Num > 1 && PlayerDataManager.instance.PlayerData.BShake)
        {
            UIManager.Instance.CameraShake();
            PlatformManager.Instance.ShortVibration(3);
        }
    }

    /// <summary>
    /// 播放方块消除飞入特效
    /// </summary>
    public void PlayBlockClearFlyAnim(BlockData blockData, RectTransform end, Action finishAction)   
    {
        var x = 266 - 76 * blockData.Pos.x;
        var y = -266 + 76 * blockData.Pos.y;
        var pos = new Vector2(y, x);
        var colorStr=  Config.GetConfig<Config_BlockColor>().GetColorImg(blockData.ColorType, true);
        ResourceManagerNew.instance.LoadAssetAsync<Sprite>(colorStr, sprite =>
        {
            var flyObj = Instantiate(_Img_Block, _Obj_Pool);
            flyObj.sprite = sprite;
            flyObj.transform.localScale = Vector3.one;
            flyObj.transform.localPosition = pos;
            var sequence = DOTween.Sequence();
            
            if (BlockData.IsLegacyItemColor(blockData.ColorType))
            {
                var t1 = flyObj.transform.DOMove(flyObj.transform.position + new Vector3(0f, -0.5f, 0f), 0.5f);
                var t2 = flyObj.transform.DOJump(end.position, 1.3f,1, 0.4f).SetEase(Ease.OutQuad);
                sequence.Append(t1);
                sequence.Append(t2);
            }
            else
            {
                var t1 = flyObj.transform.DOMove(end.position, 0.5f).SetEase(Ease.OutQuad);
                sequence.Append(t1);
            }
            
            sequence.onComplete = () =>
            {
                Destroy(flyObj);
                //AudioManagerNew.Instance.PlayAudio("notice_order_progressadd");
                finishAction?.Invoke();
            };
        });
    }

    private void PlayMusicBlockClearAnim(object param)
    {
        if (param is List<BlockData> blockDataList)
        {
            var end = _Btn_Music;
            foreach (var bd in blockDataList)
            {
                PlayBlockClearFlyAnim(bd, end, null);
            }
        }
    }

    //钱的飞行动画
    private void PlayMoneyFlyAnim(int addMoney)
    {
        var addTxt = Instantiate(_Txt_AddMoney, _Obj_Money);
        addTxt.text = $"+{addMoney}";
        addTxt.gameObject.SetActiveEx(true);
        
        var flyAnim = new MultiFlyAnimation();
        flyAnim.DoMultiFlyAnimation(_Img_Money, _Obj_MoneyAnimParent,
            _Txt_Money.rectTransform, 15, 1f,
            new Vector4(100, -100, 100, -100), 0.5f,0.3f, 0.01f);
        
        var endNum =   _fightController.Model.Money;
        var startNum = endNum - addMoney;
        flyAnim.OnOneFlyFinish = () =>
        {
            DOVirtual.Int(startNum, endNum, 0.5f, v =>
            {
                _Txt_Money.text = Util.FormatNumber(v);
            });
        };
        
        flyAnim.OnAllFlyFinish = () =>
        {
            Destroy(addTxt);
        };
    }
    
    /// <summary>
    /// 战斗失败的特效
    /// </summary>
    private void FightLoseEffect(object param)
    {
        _Obj_GameOver.gameObject.SetActiveEx(true);
        _Obj_GameOver.PlayOverAnim();
        AudioManagerNew.Instance.PlayAudio("fight_partylosezhuanchang.ogg");
        DOVirtual.DelayedCall(0.75f, () =>
        {
            if (_fightController.LevelController.Model.IsEndLess)
            {
                UIManager.Instance.ShowUI("UIFightEndlessEnd");
            }
            else
            {
                UIManager.Instance.ShowUI("UIFightEnd", null, false);
            }
        });
    }

    private void GameContinueEffect(object param)
    {
        _Obj_GameOver.gameObject.SetActiveEx(true);
        _Obj_GameOver.PlayReStartAnim();
    }

    /// <summary>
    /// 是否所有未使用的方块都可以放下
    /// </summary>
    private bool canPut = true;
    /// <summary>
    /// 设置拼图变灰提示
    /// </summary>
    public void SetUrgency()
    {
        var tmp = true;
        foreach (var item in _randomItemList)
        {
            tmp = tmp && item.GetCanPut();
        }
        if (tmp != canPut)
        {
            if (!tmp)
            {
                AudioManagerNew.Instance.PlayAudio("fight_notice_item.ogg");
                SetPropItemAnim("PropOnce");
                ResourceManagerNew.instance.LoadAssetAsync<GameObject>("FX_C1_WatchOutBarbiQ", obj =>
                {
                    var o = Instantiate(obj, _Obj_Pool);
                    DOVirtual.DelayedCall(2f, () =>
                    {
                        Destroy(o);
                    });
                });
            }
            else
            {
                SetPropItemAnim("PropIdel");
            }
        }
        canPut = tmp;
    }

    #endregion

    #region 连消计时


    private GameObject comboEff;
    private RawImage comboImg;
    
    private void UpdateComboEffect(object param)
    {
        var fightCtrl = GameManager.Instance.CurFightControl;
        if (fightCtrl.ComboTime <= 0)
        {
            if (comboEff != null)
            {
                comboEff.SetActiveEx(false);
            }
            fightCtrl.Model.ClearCombo();
        }
        else if(comboImg != null)
        {
            var survival = fightCtrl.ComboTime / fightCtrl.Model.GetComboTime();
            comboImg.color = survival > 0.5 ? new Color(2 * (1 - survival), 1,0,1): new Color(1, survival * 2, 0,1);
        }
    }

    private void ShowComboEff()
    {
        if (comboEff== null)
        {
            ResourceManagerNew.instance.LoadAssetAsync<GameObject>("FX_C_Frame_WhiteLoop", (obj) =>
            {
                comboEff = Instantiate(obj, _Obj_Pool);
                comboEff.SetActiveEx(true);
                comboImg = comboEff.GetComponentInChildren<RawImage>();
                comboImg.color = Color.green;
            });
        }
        else
        {
            comboEff.SetActiveEx(true);
            comboImg.color = Color.green;
        }
    }

    private GameObject clearGlow;

    private void ShowClearGlow(FighteffectBase cfg)
    {
        if (cfg.Num <= 1 || cfg.Type == 2)
        {
            return;
        }
        if (clearGlow == null)
        {
            ResourceManagerNew.instance.LoadAssetAsync<GameObject>("FX_C_Glow_Ani", o =>
            {
                clearGlow = Instantiate(o, transform);
                clearGlow.SetActiveEx(true);
                var glow = clearGlow.GetOrAddComponent<ComboGlowColor>();
                glow.SetColor(cfg);
                DOVirtual.DelayedCall(0.4f, () =>
                {
                    clearGlow.SetActiveEx(false);
                });
            });
        }
        else
        {
            clearGlow.SetActiveEx(true);
            var glow = clearGlow.GetOrAddComponent<ComboGlowColor>();
            glow.SetColor(cfg);
            DOVirtual.DelayedCall(0.4f, () =>
            {
                clearGlow.SetActiveEx(false);
            });
        }
    }

    #endregion
    
    #region 新手引导

    [BindNode(true)] private GameObject _Obj_GuideGroup;
    [BindNode(true)] private GameObject _Obj_Guide;
    
    private float _intervalGuideTime = 5f;
    private float _guideTime = 0f;
    
    private void PlayGuide()
    {
        if (_fightController.ChatController.Model.bInChat)
            return;
        
        if(_Obj_GuideGroup.activeSelf)
            return;
        
        if(_fightController.LevelController.Model.LevelBaseData.Id > 5 || _fightController.LevelController.Model.IsEndLess)
            return;

        if (PlayerPrefs.GetInt(Util.GUIDEKEY, 0) == 0)
        {
            _guideTime = 100;
        }
        
        if (_guideTime < _intervalGuideTime)
        {
            _guideTime += Time.deltaTime;
            return;
        }

        _guideTime = 0f;
        var pos = Vector3.zero;
        var il = new List<int>() { 1,  0,  2 };   //需要先检测中间的
        foreach (var i in il)
        {
            var randomItem = _randomItemList[i];
            var puzzleData = GameManager.Instance.CurFightControl.Model.GetPuzzleData(randomItem.Index);
            if (!puzzleData.bUsed)
            {
                pos = randomItem.transform.position;
                break;
            }
        }

        if (pos == Vector3.zero)
        {
            Debug.LogError("手牌区为空");
            return;
        }

        _Obj_GuideGroup.SetActiveEx(true);
        _Obj_Guide.transform.position = pos;
    }
    
    

    #endregion

}
