mergeInto(LibraryManager.library, {
    // 初始化SDK
    SDK_Init: function(gamePtr, apiSecretIdPtr, apiSecretKeyPtr, sandbox, gameVersionPtr, packageNamePtr) {
        var game = Pointer_stringify(gamePtr);
        var apiSecretId = Pointer_stringify(apiSecretIdPtr);
        var apiSecretKey = Pointer_stringify(apiSecretKeyPtr);
        var gameVersion = Pointer_stringify(gameVersionPtr);
        var packageName = Pointer_stringify(packageNamePtr);
        GameGlobal.unitychxsdk.Init(game, apiSecretId, apiSecretKey, sandbox, gameVersion, packageName);
    },
    // 登录SDK
    SDK_Login: function() {
        GameGlobal.unitychxsdk.Login();
    },
	
	SDK_LoginNoServer: function() {
        GameGlobal.unitychxsdk.LoginNoServer();
    },
	
    // 关卡信息上报SDK
    SDK_ReportGateInfo: function(serverIdPtr, serverNamePtr, roleIdPtr, roleNamePtr, roleLevel, gateIdPtr, gateNamePtr) {
        var serverId = Pointer_stringify(serverIdPtr);
        var serverName = Pointer_stringify(serverNamePtr);
        var roleId = Pointer_stringify(roleIdPtr);
        var roleName = Pointer_stringify(roleNamePtr);
        var gateId = Pointer_stringify(gateIdPtr);
        var gateName = Pointer_stringify(gateNamePtr);
        GameGlobal.unitychxsdk.ReportGateInfo(serverId, serverName, roleId, roleName, roleLevel, gateId, gateName);
    },
    // 展示广告SDK
    SDK_ShowRewardAd: function(point_fromPtr, gate_id) {
        var point_from = Pointer_stringify(point_fromPtr);
        GameGlobal.unitychxsdk.ShowRewardAd(point_from, gate_id);
    },
    // 设置转发参数SDK
    SDK_SetShareQuery: function(shareAppMessageQueryPtr) {
        var shareAppMessageQuery = Pointer_stringify(shareAppMessageQueryPtr);
        GameGlobal.unitychxsdk.SetShareQuery(shareAppMessageQuery);
    },
    // 主动转发SDK
    SDK_ShareAppMessage: function(eventPtr, gate_id) {
        var event = Pointer_stringify(eventPtr);
        GameGlobal.unitychxsdk.ShareAppMessage(event, gate_id);
    },
    // 设置分享监听SDK
    SDK_ShareTimeline: function() {
        GameGlobal.unitychxsdk.ShareTimeline();
    },
    // 设置收藏监听SDK
    SDK_AddToFavorites: function() {
        GameGlobal.unitychxsdk.AddToFavorites();
    },
    // 设置收藏监听SDK
    SDK_ReportLoginServer: function(serverIdPtr, serverNamePtr, roleIdPtr, roleNamePtr, roleLevel, vipLevel) {
        var serverId = Pointer_stringify(serverIdPtr);
        var serverName = Pointer_stringify(serverNamePtr);
        var roleId = Pointer_stringify(roleIdPtr);
        var roleName = Pointer_stringify(roleNamePtr);
        GameGlobal.unitychxsdk.ReportLoginServer(serverId, serverName, roleId, roleName, roleLevel, vipLevel);
    },
    // 激活上报SDK
    SDK_ReportActive: function() {
        GameGlobal.unitychxsdk.ReportActive();
    },
    // 设置用户信息SDK
    SDK_SetUserInfo: function(uidPtr, sessionPtr, open_idPtr, user_namePtr) {
        var uid = Pointer_stringify(uidPtr);
        var session = Pointer_stringify(sessionPtr);
        var open_id = Pointer_stringify(open_idPtr);
        var user_name = Pointer_stringify(user_namePtr);
        GameGlobal.unitychxsdk.SetUserInfo(uid, session, open_id, user_name);
    },
    // 角色创建上报SDK
    SDK_ReportCreateRole: function(serverIdPtr, serverNamePtr, roleIdPtr, roleNamePtr, roleLevel, packageNamePtr) {
        var serverId = Pointer_stringify(serverIdPtr);
        var serverName = Pointer_stringify(serverNamePtr);
        var roleId = Pointer_stringify(roleIdPtr);
        var roleName = Pointer_stringify(roleNamePtr);
        var packageName = Pointer_stringify(packageNamePtr);
        GameGlobal.unitychxsdk.ReportCreateRole(serverId, serverName, roleId, roleName, roleLevel, packageName);
    },
    // 角色创建上报SDK
    SDK_ReportIAAJump: function(gameId, gamePtr) {
        var game = Pointer_stringify(gamePtr);
        GameGlobal.unitychxsdk.ReportIAAJump(gameId, game);
    },
    // 角色创建上报SDK
    SDK_GetIAAJumpInfo: function() {
        GameGlobal.unitychxsdk.GetIAAJumpInfo();
    },
    
    // 自定义事件上报SDK
    SDK_ReportCustom: function (eventNamePtr, eventTypePtr, eventParamsPtr) {
        const eventName = UTF8ToString(eventNamePtr);
        const eventType = UTF8ToString(eventTypePtr);
        const eventParams = UTF8ToString(eventParamsPtr);
        if (typeof GameGlobal.unitychxsdk !== 'undefined') {
            GameGlobal.unitychxsdk.ReportCustom(eventName, eventType, eventParams);
        }
    },
});