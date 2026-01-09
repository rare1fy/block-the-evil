using System.Resources;
using UnityEngine.Experimental.GlobalIllumination;

public static class SDEvents
{
    #region 框架事件
    /// <summary>
    /// 补丁包初始化失败
    /// </summary>
    public static string S2C_INIT_FAILED = "S2C_INIT_FAILED";

    /// <summary>
    /// 补丁流程步骤改变
    /// </summary>
    public static string S2C_PATCH_STATES_CHANGE = "S2C_PATCH_STATES_CHANGE";

    /// <summary>
    /// 发现更新文件
    /// </summary>
    public static string S2C_FOUND_UPDATE_FILES = "S2C_FOUND_UPDATE_FILES";

    /// <summary>
    /// 下载进度更新
    /// </summary>
    public static string S2C_DOWN_LOAD_PROGRESS_UPDATE = "S2C_DOWN_LOAD_PROGRESS_UPDATE";

    /// <summary>
    /// 资源版本号更新失败
    /// </summary>
    public static string S2C_PACKAGE_VERSION_UPDATE_FAILED = "S2C_PACKAGE_VERSION_UPDATE_FAILED";

    /// <summary>
    /// 补丁清单更新失败
    /// </summary>
    public static string S2C_PATCH_MANIFEST_UPDATE_FAILED = "S2C_PATCH_MANIFEST_UPDATE_FAILED";

    /// <summary>
    /// 网络文件下载失败
    /// </summary>
    public static string S2C_WEB_FILE_DOWNLOAD_FAILED = "S2C_WEB_FILE_DOWNLOAD_FAILED";

    /// <summary>
    /// 用户尝试再次初始化资源包
    /// </summary>
    public static string S2C_USER_TRY_INITIALIZE = "S2C_USER_TRY_INITIALIZE";

    /// <summary>
    /// 用户开始下载网络文件
    /// </summary>
    public static string S2C_USER_BEGIN_DOWNLOAD_WEBFILES = "S2C_USER_BEGIN_DOWNLOAD_WEBFILES";

    /// <summary>
    /// 用户尝试再次更新静态版本
    /// </summary>
    public static string S2C_USER_TRY_UPDATE_PACKAGE_VERSION = "S2C_USER_TRY_UPDATE_PACKAGE_VERSION";

    /// <summary>
    /// 用户尝试再次更新补丁清单
    /// </summary>
    public static string S2C_USER_TRY_UPDATE_PATCH_MANIFEST = "S2C_USER_TRY_UPDATE_PATCH_MANIFEST";

    /// <summary>
    /// 用户尝试再次下载网络文件
    /// </summary>
    public static string S2C_USER_TRY_DOWNLOAD_WEBFILES = "S2C_USER_TRY_DOWNLOAD_WEBFILES";
    
    /// <summary>
    /// 网络断开
    /// </summary>
    public static string S2C_NETWORK_DISCONNECT = "S2C_NETWORK_DISCONNECT";

    /// <summary>
    /// 网络重连
    /// </summary>
    public static string S2C_NETWORK_RECONNECTION = "S2C_NETWORK_RECONNECTION";
    
    /// <summary>
    /// 网络重连次数
    /// </summary>
    public static string S2C_NETWORK_DCONNECT_TIMES = "S2C_NETWORK_DCONNECT_TIMES";
    
    #endregion

    #region 战斗内
    
    /// <summary>
    /// 更新格子临时显示
    /// </summary>
    public static string C2C_SHOW_TEMP_BLOCK = "C2C_SHOW_TEMP_BLOCK";
    
    /// <summary>
    /// 更新主界面格子
    /// </summary>
    public static string C2C_REFRESH_MAIN_BLOCK = "C2C_REFRESH_MAIN_BLOCK";
    
    /// <summary>
    /// 更新主界面所有的
    /// </summary>
    public static string C2C_REFRESH_MAIN_All_ITEM = "C2C_REFRESH_MAIN_All_BLOCK";
    
    /// <summary>
    /// 继续游戏 复活
    /// </summary>
    public static string C2C_GAME_CONTINUE = "C2C_GAME_CONTINUE";
    
    /// <summary>
    /// 拼图刷新
    /// </summary>
    public static string C2C_REFRESH_PUZZLE_ITEM = "C2C_REFRESH_PUZZLE_ITEM";
    
    /// <summary>
    /// 刷新订单
    /// </summary>
    public static string C2C_REFRESH_ORDER_ITEM = "C2C_REFRESH_ORDER_ITEM";

    /// <summary>
    /// 更新分数
    /// </summary>
    public static string C2C_UPDATE_SCORE = "C2C_UPDATE_SCORE";
    
    /// <summary>
    /// 放下特效
    /// </summary>
    public static string C2C_BLOCK_PUTDOWN_EFFECT = "C2C_BLOCK_PUTDOWN_EFFECT";
    
    /// <summary>
    /// 消除预览特效
    /// </summary>
    public static string C2C_BLOCK_HOLD_EFFECT = "C2C_BLOCK_HOLD_EFFECT";
    
    /// <summary>
    /// 消除特效
    /// </summary>
    public static string C2C_BLOCK_CLEAR_EFFECT = "C2C_BLOCK_CLEAR_EFFECT";
    
    /// <summary>
    /// 更新combo时间
    /// </summary>
    public static string C2C_UPDATE_COMBO_TIME = "C2C_REFRESH_COMBO_TIME";

    /// <summary>
    /// 刷新宝箱格子信息
    /// </summary>
    public static string BLOCK_BOX_REFRESH = "BLOCK_BOX_REFRESH";

    /// <summary>
    /// 订单完成的格子动画
    /// </summary>
    public static string C2C_ORDER_FINISH_WITH_BLOCK = "C2C_ORDER_FINISH_WITH_BLOCK";
    
    /// <summary>
    /// 音乐解锁资源刷新
    /// </summary>
    public static string C2C_MUSIC_PROGRESS_REFRESH = "C2C_MUSIC_PROGRESS_REFRESH";

    /// <summary>
    /// 音乐消除动画
    /// </summary>
    public static string C2C_MUSIC_BLOCK_ANIM = "C2C_MUSIC_BLOCK_ANIM";

    /// <summary>
    /// 音乐选择
    /// </summary>
    public static string C2C_MUSIC_SELECT = "C2C_MUSIC_SELECT";
    
    /// <summary>
    /// 使用道具
    /// </summary>
    public static string C2C_USE_PROP = "C2C_USE_PROP";

    /// <summary>
    /// 战斗流程结束
    /// </summary>
    public static string C2C_FIGHT_END = "C2C_FIGHT_END";
    
    /// <summary>
    /// 局内聊天下一步
    /// </summary>
    public static string C2C_FIGHT_CHAT_NEXT = "C2C_FIGHT_CHAT_NEXT";
    
    /// <summary>
    /// 局内经验提升
    /// </summary>
    public static string FIGHT_FEEL_EXP_UP = "FIGHT_FEEL_EXP_UP";

    #endregion

    #region 街区派对 外围玩法
    /// <summary>
    /// 开始建筑建造
    /// </summary>
    public static string BUILD_ARCHITECTURE = "BUILD_ARCHITECTURE";
    /// <summary>
    /// 领取建造进度奖励
    /// </summary>
    public static string BUILD_REWORD = "BUILD_REWORD";
    /// <summary>
    /// 刷新当前章节
    /// </summary>
    public static string CHANGE_CHAPTER = "CHANGE_CHAPTER";

    /// <summary>
    /// 关卡变更
    /// </summary>
    public static string CHANGE_LEAVL = "CHANGE_lEAVL";

    /// <summary>
    /// 订单变更
    /// </summary>
    public static string CHANGE_ORDER = "CHANGE_ORDER";
    /// <summary>
    /// 好感提升
    /// </summary>
    public static string CHAGE_NPC_FEEL = "CHAGE_FEEL";
    /// <summary>
    /// NPC状态变更
    /// </summary>
    public static string CHANGE_NPC_STATE = "CHANGE_TYPE";
    /// <summary>
    /// 下一步对话
    /// </summary>
    public static string DIALOGUE_NEXT = "DIALOGUE_NEXT";
    /// <summary>
    /// 当前对话完成
    /// </summary>
    public static string DIALOGUE_END = "DIALOGUE_END";
    /// <summary>
    /// 对话红点变更
    /// </summary>
    public static string DIALOGUE_RED_REFRESH = "DIALOGUE_RED_REFRESH";
    /// <summary>
    /// 资源刷新
    /// </summary>
    public static string BAG_UPDATA_ITEM = "S2C_BG_UPDATA_ITEM";
    /// <summary>
    /// 聊天点击按钮
    /// </summary>
    public static string DIALOGUE_CLICK_BTN = "DIALOGUE_CLICK_BTN";
    /// <summary>
    /// 开始剧情
    /// </summary>
    public static string PERFORM_START = "PERFORM_START";
    /// <summary>
    /// 进入下一幕剧情
    /// </summary>
    public static string PERFORM_NEXT = "PERFORM_NEXT";
    /// <summary>
    /// 结束剧情
    /// </summary>
    public static string PERFORM_OVER = "PERFORM_OVER";
    /// <summary>
    /// 剧情等待结束
    /// </summary>
    public static string PERFORM_AWAIT_END = "PERFORM_AWAIT_END";
    /// <summary>
    /// 舞台人数增加（界面表现）
    /// </summary>
    public static string STAGE_NUM_REFRESH = "STAGE_NUM_ADD";

    /// <summary>
    /// 跨天
    /// </summary>
    public static string ACROSS_THE_DAY = "ACROSS_THE_DAY";
    
    /// <summary>
    /// 加载玩家数据完成
    /// </summary>
    public static string C2C_LOAD_PLAYERDATA_FINISH = "C2C_LOAD_PLAYERDATA_FINISH";

    /// <summary>
    /// 微信返回前台回调
    /// </summary>
    public static string WX_ON_SHOW = "WX_ON_SHOW";

    /// <summary>
    /// 主界面广告回调 
    /// </summary>
    public static string MAIN_ADS_CALL = "MAIN_ADS_CALL";


    public static string C2C_SHARE_REWARD = "C2C_SHARE_REWARD";

    #endregion
}
