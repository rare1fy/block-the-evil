using System;
using System.Runtime.InteropServices;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace WeChatWASM
{
    [System.Serializable]
    public class LoginCheckData
    {
        public string game;
        public string platform;
        public string uid;
        public string user_name;
        public int identify;
        public bool visitor;
        public string session;
        public string open_id;
        public string device_id;
        public string ad_server_id;
    }

    [System.Serializable]
    public class SDKInitData
    {
        public int code;
        public string msg;
        public string data;
    }

    [System.Serializable]
    public class SDKLoginCallBackInData
    {
        public string data;
    }

    [System.Serializable]
    public class SDKLoginCallBackData
    {
        public int code;
        public string msg;
        public SDKLoginCallBackInData data;
    }
    
    [System.Serializable]
    public class SDKLoginCallBackDataNoServer
    {
        public int code;
        public string msg;
        public LoginCheckData data;
    }

    [System.Serializable]
    public class SDKADData
    {
        public int data;
    }

    [System.Serializable]
    public class SDKShareData
    {
        public string title;
        public string imageUrl;
        public string imageUrlId;
        public string query;
    }
    
    [Serializable]
    public class RequestBaseData
    {
        public string func;          // 日志类型
        public string log_id;        // 日志唯一ID
        public string game;          // 游戏标识
        public string platform;      // 平台;
        public int platform_id;      // 专服ID
        public int server_id;        // 区服ID
        public string server_name;   // 区服名称
        public string timestamp;       // 时间戳
        public string account;       // 账号ID
        public string role_id;       // 角色ID
        public string role_name;     // 角色名称
        public int grade;            // 等级
        public string ip;            // IP
        public int check_id;         // 关卡ID
        public string check_name;    // 关卡名称
        public string version;       // 游戏版本号
    }

    enum SDKCode {
        Succeed = 1000,
    }

    public class ChuXinWeChatSDK : MonoBehaviour
    {
        [DllImport("__Internal")]
        private static extern void SDK_Init(string game, string apiSecretId, string apiSecretKey, bool sandbox, string gameVersion, string packageName);

        [DllImport("__Internal")]
        private static extern void SDK_Login();   
        
        [DllImport("__Internal")]
        private static extern void SDK_LoginNoServer();

        [DllImport("__Internal")]
        private static extern void SDK_ReportGateInfo(string serverId, string serverName, string roleId, string roleName, int roleLevel, string gateId, string gateName);

        [DllImport("__Internal")]
        private static extern void SDK_ShowRewardAd(string point_from, int gate_id);

        [DllImport("__Internal")]
        private static extern void SDK_SetShareQuery(string shareAppMessageQuery);

        [DllImport("__Internal")]
        private static extern void SDK_ShareAppMessage(string eventKey, int gate_id);

        [DllImport("__Internal")]
        private static extern void SDK_ShareTimeline();

        [DllImport("__Internal")]
        private static extern void SDK_AddToFavorites();

        [DllImport("__Internal")]
        private static extern void SDK_ReportLoginServer(string serverId, string serverName, string roleId, string roleName, int roleLevel, int vipLevel);

        [DllImport("__Internal")]
        private static extern void SDK_ReportActive();

        [DllImport("__Internal")]
        private static extern void SDK_SetUserInfo(string uid, string session, string open_id, string user_name);

        [DllImport("__Internal")]
        private static extern void SDK_ReportCreateRole(string serverId, string serverName, string roleId, string roleName, int roleLevel, string packageName);

        [DllImport("__Internal")]
        private static extern void SDK_GetIAAJumpInfo();

        [DllImport("__Internal")]
        private static extern void SDK_ReportIAAJump(int gameId, string game);
        
        [DllImport("__Internal")]
        private static extern void SDK_ReportCustom(string eventName, string eventType, string eventParams);

        private static ChuXinWeChatSDK instance;

        public static ChuXinWeChatSDK Instance
        {
            get
            {
                if (null == instance)
                {
                    GameObject go = new GameObject("ChuXinWeChatSDK");
                    GameObject.DontDestroyOnLoad(go);
                    instance = go.AddComponent<ChuXinWeChatSDK>();
                }
                return instance;
            }
        }

        private Action<bool> m_InitCallBack;
        // 初始化SDK（供Unity调用）
        public void InitSDK(string game, string apiSecretId, string apiSecretKey, bool sandbox, string gameVersion, string packageName, Action<bool> callBack)
        {
            m_InitCallBack = callBack;

            if (Application.platform == RuntimePlatform.WebGLPlayer)
            {
                SDK_Init(game, apiSecretId, apiSecretKey, sandbox, gameVersion, packageName);
            }
            else
            {
                m_InitCallBack?.Invoke(true);

                m_InitCallBack = null;
            }
        }

        // SDK初始化回调（由JS调用）
        public void OnSDKInit(string resultJson)
        {
            var result = JsonUtility.FromJson<SDKInitData>(resultJson);
            Debug.Log("SDK初始化结果: " + result.msg);

            m_InitCallBack?.Invoke(result.code == (int)SDKCode.Succeed);

            m_InitCallBack = null;
        }

        private Action<bool, SDKLoginCallBackInData> m_LoginCallBack;
        
        // 登录SDK（供Unity调用）
        public void LoginSDK(Action<bool, SDKLoginCallBackInData> callBack)
        {
            m_LoginCallBack = callBack;

            if (Application.platform == RuntimePlatform.WebGLPlayer)
            {
                SDK_Login();
            }
            else
            {
                m_LoginCallBack?.Invoke(true, new SDKLoginCallBackInData());

                m_LoginCallBack = null;
            }
        }

        // SDK登录回调（由JS调用）
        public void OnSDKLogin(string resultJson)
        {
            var result = JsonUtility.FromJson<SDKLoginCallBackData>(resultJson);
            Debug.Log("SDK登录结果: " + result.msg);

            m_LoginCallBack?.Invoke(true, result.data);

            m_LoginCallBack = null;
        }
        
        
        private Action<bool, LoginCheckData> m_LoginNoServerCallBack;
        public void LoginSDKNoServer(Action<bool, LoginCheckData> callBack)
        {
            m_LoginNoServerCallBack = callBack;
            if (Application.platform == RuntimePlatform.WebGLPlayer)
            {
                SDK_LoginNoServer();
            }
            else
            {
                m_LoginNoServerCallBack?.Invoke(true, new LoginCheckData());
                m_LoginNoServerCallBack = null;
            }
        }
        
        public void OnSDKLoginNoServer(string resultJson)
        {
            var result = JsonUtility.FromJson<SDKLoginCallBackDataNoServer>(resultJson);
            Debug.Log("SDK登录结果: " + result.code);

            m_LoginNoServerCallBack?.Invoke(true, result.data);
            m_LoginNoServerCallBack = null;
        }
        

        // 关卡信息上报（供Unity调用）
        public void ReportGateInfoSDK(string serverId, string serverName, string roleId, string roleName, int roleLevel, string gateId, string gateName)
        {
            if (Application.platform == RuntimePlatform.WebGLPlayer)
            {
                SDK_ReportGateInfo(serverId, serverName, roleId, roleName, roleLevel, gateId, gateName);
            }
        }

        private Action<bool> m_ADCallBack;
        // 展示广告（供Unity调用）
        public void ShowRewardAdSDK(Action<bool> callBack, string point_from, int gate_id)
        {
            m_ADCallBack = callBack;

            if (Application.platform == RuntimePlatform.WebGLPlayer)
            {
                SDK_ShowRewardAd(point_from, gate_id);
            }
            else
            {
                m_ADCallBack?.Invoke(true);

                m_ADCallBack = null;
            }
        }

        // SDK广告回调（由JS调用）
        public void OnSDKShowRewardAd(string resultJson)
        {
            var result = JsonUtility.FromJson<SDKADData>(resultJson);
            Debug.Log("广告回调: " + result.data);

            m_ADCallBack?.Invoke(result.data == 1);

            m_ADCallBack = null;
        }

        // 设置转发参数（供Unity调用）
        public void ShareAppMessageSDK(string shareAppMessageQuery)
        {
            if (Application.platform == RuntimePlatform.WebGLPlayer)
            {
                SDK_SetShareQuery(shareAppMessageQuery);
            }
        }

        private Action<bool> m_ShareCallBack;
        // 主动转发（供Unity调用）
        public void ShareAppMessageSDK(Action<bool> callBack, string eventKey, int gate_id)
        {
            m_ShareCallBack = callBack;

            if (Application.platform == RuntimePlatform.WebGLPlayer)
            {
                SDK_ShareAppMessage(eventKey, gate_id);
            }
            else
            {
                m_ShareCallBack?.Invoke(true);

                m_ShareCallBack = null;
            }
        }

        // SDK主动转发回调（由JS调用）
        public void OnShareAppMessageSDK(string resultJson)
        {
            var result = JsonUtility.FromJson<SDKShareData>(resultJson);
            Debug.Log(string.Format("SDK主动分享结果: {0}, {1}", result.title, result.query));

            m_ShareCallBack?.Invoke(true);

            m_ShareCallBack = null;
        }

        private Action<bool> m_OnShareTimelineCallBack;
        // 设置分享监听（供Unity调用）
        public void ShareTimelineSDK(Action<bool> callBack)
        {
            m_OnShareTimelineCallBack = callBack;

            if (Application.platform == RuntimePlatform.WebGLPlayer)
            {
                SDK_ShareTimeline();
            }
            else
            {
                m_OnShareTimelineCallBack?.Invoke(true);

                m_OnShareTimelineCallBack = null;
            }
        }

        // SDK分享监听回调（由JS调用）
        public void OnShareTimelineSDK(string resultJson)
        {
            var result = JsonUtility.FromJson<SDKShareData>(resultJson);
            Debug.Log(string.Format("SDK分享结果: {0}, {1}", result.title, result.query));

            m_OnShareTimelineCallBack?.Invoke(true);

            m_OnShareTimelineCallBack = null;
        }

        private Action<bool> m_OnAddToFavoritesBack;
        // 设置收藏监听（供Unity调用）
        public void AddToFavoritesSDK(Action<bool> callBack)
        {
            m_OnAddToFavoritesBack = callBack;

            if (Application.platform == RuntimePlatform.WebGLPlayer)
            {
                SDK_AddToFavorites();
            }
            else
            {
                m_OnAddToFavoritesBack?.Invoke(true);

                m_OnAddToFavoritesBack = null;
            }
        }

        // SDK收藏监听回调（由JS调用）
        public void OnAddToFavoritesSDK(string resultJson)
        {
            var result = JsonUtility.FromJson<SDKShareData>(resultJson);
            Debug.Log(string.Format("SDK收藏结果: {0}, {1}", result.title, result.query));

            m_OnAddToFavoritesBack?.Invoke(true);

            m_OnAddToFavoritesBack = null;
        }

        // 进入服务器上报（供Unity调用）
        public void ReportLoginServerSDK(string serverId, string serverName, string roleId, string roleName, int roleLevel, int vipLevel)
        {
            if (Application.platform == RuntimePlatform.WebGLPlayer)
            {
                SDK_ReportLoginServer(serverId, serverName, roleId, roleName, roleLevel, vipLevel);
            }
        }
        
        // 设置用户信息API（供Unity调用）
        public void SetUserInfoSDK(string uid, string session, string open_id, string user_name)
        {
            if (Application.platform == RuntimePlatform.WebGLPlayer)
            {
                SDK_SetUserInfo(uid, session, open_id, user_name);
            }
        }

        // 激活上报API（供Unity调用）
        public void ReportActiveSDK()
        {
            if (Application.platform == RuntimePlatform.WebGLPlayer)
            {
                SDK_ReportActive();
            }
        }

        // 角色创建上报API（供Unity调用）
        public void ReportCreateRoleSDK(string serverId, string serverName, string roleId, string roleName, int roleLevel, string packageName)
        {
            if (Application.platform == RuntimePlatform.WebGLPlayer)
            {
                SDK_ReportCreateRole(serverId, serverName, roleId, roleName, roleLevel, packageName);
            }
        }

        // 设置用户信息API（供Unity调用）
        public void ReportIAAJumpSDK(int gameId, string game)
        {
            if (Application.platform == RuntimePlatform.WebGLPlayer)
            {
                SDK_ReportIAAJump(gameId, game);
            }
        }


        private Action<string> m_OnGetIAAJumpInfoBack;
        // 获取IAA导量广告跳转配置（供Unity调用）
        public void GetIAAJumpInfoSDK(Action<string> callBack)
        {
            m_OnGetIAAJumpInfoBack = callBack;
            if (Application.platform == RuntimePlatform.WebGLPlayer)
            {
                SDK_GetIAAJumpInfo();
            }
            else
            {
                m_OnGetIAAJumpInfoBack.Invoke("");
                m_OnGetIAAJumpInfoBack = null;
            }
        }

        // 获取IAA导量广告跳转配置API（供JS调用）
        public void OnGetIAAJumpInfoSDK(string resultJson)
        {
            m_OnGetIAAJumpInfoBack?.Invoke(resultJson);

            m_OnGetIAAJumpInfoBack = null;
        }
        
        //自定义事件上报
        public void ReportCustomEvent(string eventName, string eventType, string eventParamsJson)
        {
            Debug.LogError("上报自定义事件");
            if (Application.platform == RuntimePlatform.WebGLPlayer)
            {
                SDK_ReportCustom(eventName, eventType, eventParamsJson);
            }
        }


        public void OnReportCustomSDK(string resultJson)
        {
            var result = JsonUtility.FromJson<SDKInitData>(resultJson);
            Debug.LogError("上报自定义事件回调  code:"+result.code);
        }
    }
}