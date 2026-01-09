import chxsdk from "./chx/chx_sdk.v3.min.js";
const SendObjName = "ChuXinWeChatSDK";
var secretId = "";
var secretKey = "";

const sdk = {
  Init(game, apiSecretId, apiSecretKey, sandbox, gameVersion, packageName) {
    secretId = apiSecretId;
    secretKey = apiSecretKey;
    console.log('chxsdk.init params:', game, apiSecretId, apiSecretKey, sandbox, gameVersion, packageName);
    var options = wx.getLaunchOptionsSync();
    var scene = options == undefined ? 1000 : options.scene;
    let params = {
      game: game,
      platform: "wechat",
      apiSecretId: apiSecretId,
      apiSecretKey: apiSecretKey,
      sandbox: false,
      gameVersion: gameVersion,
      query: options.query,
      scene: scene,
      packageName: packageName,
    };
    chxsdk.init(params).then(function (result) {
      console.log('chxsdk.init:', result);
      GameGlobal.Module.SendMessage(SendObjName, "OnSDKInit", JSON.stringify(result));
    }).catch(function (result) {
      console.error('chxsdk.init:', result);
      GameGlobal.Module.SendMessage(SendObjName, "OnSDKInit", JSON.stringify(result));
    });
  },

  Login() {
    chxsdk.serverLogin().then(function (result) {
      console.log("chxsdk.serverLogin:", result);
      GameGlobal.Module.SendMessage(SendObjName, "OnSDKLogin", JSON.stringify(result));
    }).catch(function (result) {
      console.error("chxsdk.serverLogin:", result);
      GameGlobal.Module.SendMessage(SendObjName, "OnSDKLogin", JSON.stringify(result));
    });
  },

  LoginNoServer() {
    chxsdk.serverLogin().then((result) => {
      chxsdk.apiSecretId = secretId;
      chxsdk.apiSecretKey = secretKey;
      console.log("result:", result);
      chxsdk.userDetail(result.data).then((res) => {
		  console.log("LoginNoServer登录成功:", res);
          GameGlobal.Module.SendMessage(SendObjName, "OnSDKLoginNoServer", JSON.stringify(res)
          );
        })
        .catch((userDetailError) => {
          console.error("userDetail.catch.error:", userDetailError);
        });
    })
    .catch((serverLoginError) => {
      console.error("serverLogin.error:", serverLoginError);
    });
  },


  SetUserInfo(uid, session, open_id, user_name) {
    let userInfo = {
      uid: uid,
      sessionKey: session,
      openid: open_id,
      unionId: user_name,
    }
    chxsdk.setUserInfo(userInfo).then(function (result) {
      console.log("chxsdk.setUserInfo:", result)
      chxsdk.reportActive().then(function (result) {
        console.log("chxsdk.reportActive:", result)
      }).catch(function (result) {
        console.error('chxsdk.reportActive:', result);
      });
    }).catch(function (result) {
      console.error("chxsdk.setUserInfo:", result)
    });
  },

  ReportGateInfo(serverId, serverName, roleId, roleName, roleLevel, gateId, gateName) {
    let params = {
      serverId: serverId,
      serverName: serverName,
      roleId: roleId,
      roleName: roleName,
      roleLevel: roleLevel,
      gateId: gateId,
      gateName: gateName,
    }
    chxsdk.reportGateInfo(params).then(function (result) {
      console.log("chxsdk.reportGateInfo:", result)
    }).catch(function (result) {
      console.error('chxsdk.reportGateInfo:', result);
    });
  },

  ShowRewardAd(point_from, gate_id) {
    let params = {
      gate_id: gate_id,
      point_from: point_from,
    }
    var isShow = false
    if (chxsdk.isRewardAdInit) {
      chxsdk.offErrorRewardAd({ params: params })
      //添加onClose下发奖励
      chxsdk.onCloseRewardAd({
        params: params,
        onClose: res => {
          //销毁onClose防止多次下发
          chxsdk.offCloseRewardAd({ params: params });
          GameGlobal.Module.SendMessage(SendObjName, "OnSDKShowRewardAd", JSON.stringify(res));
        },
      })
      //广告是否加载成功
      if (chxsdk.isRewardAdLoaded) {
        //加载成功则显示
        chxsdk.showRewardAd({ params: params }).then(() => {
          isShow = true
        }).catch(err => {})
      } else {
        //加载失败再重新加载
        chxsdk.reLoadRewardAd({ params: params }).then(() => {
          chxsdk.showRewardAd({ params: params })
        }).catch(err => {})
      }
    } else {
      //初始化
      chxsdk.initRewardAd({
        params: params,
        onLoad: () => {
          if (!isShow) {
            chxsdk.offErrorRewardAd({ params: params })
            chxsdk.showRewardAd({ params: params }).then(() => {
              isShow = true
            }).catch(err => {})
          }
        },
        onError: err => {},
        onClose: res => {
          chxsdk.offErrorRewardAd({ params: params })
          //销毁onClose防止多次下发
          chxsdk.offCloseRewardAd({ params: params })
          GameGlobal.Module.SendMessage(SendObjName, "OnSDKShowRewardAd", JSON.stringify(res));
        }
      })
    }
  },

  SetShareQuery(shareAppMessageQuery) {
    chxsdk.setShareQuery({
      params: {
        shareAppMessageQuery: shareAppMessageQuery
      }
    })
  },

  ShareAppMessage(event, gate_id) {
    chxsdk.shareAppMessage({
      callback: res => {
        console.log('chxsdk.onShareAppMessage', '转发参数：', res);
        GameGlobal.Module.SendMessage(SendObjName, "OnShareAppMessageSDK", JSON.stringify(res));
      },
      params: {
        point_from: {
          gate_id: gate_id,//关卡id
          event: event,
        }
      }
    })
  },

  ShareTimeline() {
    chxsdk.onShareTimeline({
      callback: res => {
        console.log('chxsdk.onShareTimeline', '触发分享事件', res);
        GameGlobal.Module.SendMessage(SendObjName, "OnShareTimelineSDK", JSON.stringify(res));
      },
    })
  },

  AddToFavorites() {
    chxsdk.onAddToFavorites({
      callback: res => {
        console.log('chxsdk.onAddToFavorites', '触发收藏事件', res);
        GameGlobal.Module.SendMessage(SendObjName, "OnAddToFavoritesSDK", JSON.stringify(res));
      },
    })
  },

  ReportLoginServer(serverId, serverName, roleId, roleName, roleLevel, vipLevel) {
    let params = {
      serverId: serverId,
      serverName: serverName,
      roleId: roleId,
      roleName: roleName,
      roleLevel: roleLevel,
      vipLevel: vipLevel,
    }
    chxsdk.reportLoginServer(params).then(function (result) {
      console.log("chxsdk.reportLoginServer:", result)
    }).catch(function (result) {
      console.error('chxsdk.reportLoginServer:', result);
    });
  },

  ReportCreateRole(serverId, serverName, roleId, roleName, roleLevel, packageName) {
    let params = {
      serverId: serverId,
      serverName: serverName,
      roleId: roleId,
      roleName: roleName,
      roleLevel: roleLevel,
      packageName: packageName,
    }
    chxsdk.reportCreateRole(params).then(function (result) {
      console.log("chxsdk.reportCreateRole:", result)
    }).catch(function (result) {
      console.error('chxsdk.reportCreateRole:', result);
    });
  },

  ReportIAAJump(gameId, game) {
    let params = {
      to_game_id: gameId,
      to_game: game,
    }
    chxsdk.reportIAAJump(params).then(function (result) {
      console.log("chxsdk.reportIAAJump:", result)
    }).catch(function (error) {
      console.error('chxsdk.reportIAAJump:', error);
    });
  },

  GetIAAJumpInfo() {
    chxsdk.getIaaJumpInfo().then(res => {
      console.log('chxsdk.getIaaJumpInfo', res);
      GameGlobal.Module.SendMessage(SendObjName, "OnGetIAAJumpInfoSDK", JSON.stringify(res));
    }).catch(err => {
      console.error('chxsdk.getIaaJumpInfo:', err.msg);
    })
  },

  //自定义事件上报
  ReportCustom(event_name, event_type, event_params) {
    let params = {
      event_name: event_name,
      event_type: event_type,
      event_params: event_params
    };
    chxsdk.reportCustom(params).then(function (result) {
      console.log("chxsdk.reportCustom:", result);
      GameGlobal.Module.SendMessage(SendObjName, "OnReportCustomSDK", JSON.stringify(result));
    }).catch(function (result) {
      console.error('chxsdk.reportCustom:', result);
      GameGlobal.Module.SendMessage(SendObjName, "OnReportCustomSDK", JSON.stringify(result));
    });
  },
};

GameGlobal.unitychxsdk = sdk;
