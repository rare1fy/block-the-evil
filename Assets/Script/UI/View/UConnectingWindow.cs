using System;
using UnityEngine;
using UnityEngine.UI;

public class UConnectingWindow : UIBase
{
    private CustomText _txtConnectTimes;
    private GameObject _ImgLoad;
    public override void InitOnce()
    {
        _txtConnectTimes = GetNodeByName<CustomText>("_Txt_ConnectTimes");
        _ImgLoad = GetNodeByName("_Img_load");
        EventDispatchCenter.Instance.Registry(SDEvents.S2C_NETWORK_DCONNECT_TIMES, OnConnectShow);
    }

    /// <summary>
    /// 重连次数显示
    /// </summary>
    /// <param name="param"></param>
    private void OnConnectShow(object param)
    {
        var time = (int)param;
        _txtConnectTimes.text = LanguageManager.Instance.Format("[FID:743]",time);
    }

    public override void OnOpen(object param = null) 
    {
        base.OnOpen(param);
        _txtConnectTimes.text = LanguageManager.Instance.Format("[FID:743]", 1);
    }

    private int _tempAngle = 0;
    private void Update()
    {
        _tempAngle += 5;
        if (_tempAngle > 360)
        {
            _tempAngle -= 360;
        }
        _ImgLoad.transform.rotation = Quaternion.Euler(0, 0, _tempAngle);
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        EventDispatchCenter.Instance.UnRegistry(SDEvents.S2C_NETWORK_DCONNECT_TIMES, OnConnectShow);
    }
}
