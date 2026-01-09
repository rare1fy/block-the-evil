using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using YooAsset;

public class UILogin : UIBase
{
    private InputField _inputAccount;
    private InputField _inputPassword;

    private GameObject _ObjLogin;

    public override void InitOnce()
    {
        AddListener(ui_listener_type.onClick, "_Btn_Login", OnLoginClick);

        _inputAccount = GetNodeByName<InputField>("_Int_Account");
        _inputPassword = GetNodeByName<InputField>("_Int_Password");

        _ObjLogin = GetNodeByName("_Obj_Login");
    }

    public override void OnOpen(object param = null)
    {
        base.OnOpen(param);

        _ObjLogin.SetActiveEx(true);

        _inputAccount.text = PlayerPrefs.GetString("curAccount");
        _inputPassword.text = PlayerPrefs.GetString("curPassword");
    }

    /// <summary>
    /// 点击登录
    /// </summary>
    private void OnLoginClick(GameObject go, PointerEventData eventData)
    {
        WebRequestManager.Instance.RequestLogin(CallbackLogin);
    }

    /// <summary>
    /// 登录回调
    /// </summary>
    /// <param name="code"></param>
    /// <param name="data"></param>
    private void CallbackLogin()
    {
        StopCoroutine(ChangeScene());
        StartCoroutine(ChangeScene());
    }

    /// <summary>
    /// 场景切换
    /// </summary>
    /// <returns></returns>
    private IEnumerator ChangeScene()
    {
        yield return null;
        // while (!GameManager.Instance.IsConfigLoad)
        //     yield return null;
        // UIManager.Instance.ShowUI("UILoading");
        // var sceneHandle = YooAssets.LoadSceneAsync("Scene_Main");
        // if (sceneHandle != null)
        // {
        //     sceneHandle.Completed += delegate (SceneHandle sceneHandle)
        //     {
        //         // 切换到主页面场景
        //         GameManager.Instance.Launch();
        //         UIManager.Instance.HideUI(this);
        //         UIManager.Instance.HideUI("UILoading");
        //     };
        // }
    }
}
