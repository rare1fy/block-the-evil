mergeInto(LibraryManager.library, {

  GoBackInJS:function(){
    ResizeCanvas();
  },

	openTelegramLink: function (shareUrl) {
		if (window && window.Telegram && window.Telegram.WebApp.openTelegramLink) {
		   window.Telegram.WebApp.openTelegramLink(UTF8ToString(shareUrl));
		}else{
			OpenWebPage(UTF8ToString(shareUrl))
		}
	},

  OpenWebPage: function (text) {
      OpenWebPage(UTF8ToString(text))
  },

  GetAppActive: function () {
    if (window && window.Telegram && window.Telegram.WebApp) {
      return window.Telegram.WebApp.isActive;
    }
    return false;
  },

   RequestInitData: function () {
      if (window && window.Telegram && window.Telegram.WebApp) {
        window.unityInstance.SendMessage("GameManager", "ReflashInitData", window.Telegram.WebApp.initData);
      }
    },

  RequestUserData: function () {
    if (window.unityInstance) {
      window.unityInstance.SendMessage("GameManager", "SetWebAppUser", JSON.stringify(window.Telegram.WebApp.initDataUnsafe.user));
    }
  },

  CloseGameAll: function () {
    if (window && window.Telegram && window.Telegram.WebApp) {
      window.Telegram.WebApp.close();
    }
  },


  RequestThemeParams: function () {
    if (window.unityInstance) {
      window.unityInstance.SendMessage("GameManager", "SetThemeParams", JSON.stringify(window.Telegram.WebApp.themeParams));
    }
  },


  ShowMainButton: function (text) {
    if (window && window.Telegram && window.Telegram.WebApp) {
      if (text) {
        window.Telegram.WebApp.MainButton.setText(UTF8ToString(text));
      }
      window.Telegram.WebApp.MainButton.show();
    }
  },


  HideMainButton: function () {
    if (window && window.Telegram && window.Telegram.WebApp) {
      window.Telegram.WebApp.MainButton.hide();
    }
  },

  MainButtonShowProgress: function () {
    if (window && window.Telegram && window.Telegram.WebApp) {
      window.Telegram.WebApp.MainButton.showProgress();
    }
  },

  MainButtonHideProgress: function () {
    if (window && window.Telegram && window.Telegram.WebApp) {
      window.Telegram.WebApp.MainButton.hideProgress();
    }
  },


  ShowBackButton: function () {
    if (window && window.Telegram && window.Telegram.WebApp) {
      window.Telegram.WebApp.BackButton.show();
    }
  },


  HideBackButton: function () {
    if (window && window.Telegram && window.Telegram.WebApp) {
      window.Telegram.WebApp.BackButton.hide();
    }
  },

  ShowAlert: function (text) {
    if (window && window.Telegram && window.Telegram.WebApp) {
      window.Telegram.WebApp.BackButton.showAlert(UTF8ToString(text));
    }
  },

  ShowShareJoinCode: function (code) {

  },

  Ready: function () {
    if (window && window.Telegram && window.Telegram.WebApp) {
      window.Telegram.WebApp.ready();
    }
  },

  Close: function () {
    if (window && window.Telegram && window.Telegram.WebApp) {
      window.Telegram.WebApp.close();
    }
  },

  Expand: function () {
    if (window && window.Telegram && window.Telegram.WebApp) {
      window.Telegram.WebApp.expand();
    }
  },


  HapticFeedback: function (level) {
    if (window && window.Telegram && window.Telegram.WebApp) {
      window.Telegram.WebApp.HapticFeedback.notificationOccurred(UTF8ToString(level));
    }
  },

  ShowScanQrPopup: function (text) {
    if (window && window.Telegram && window.Telegram.WebApp) {
      window.Telegram.WebApp.showScanQrPopup({
        text: UTF8ToString(text)
      });
    }
  },

  CloseScanQrPopup: function () {
    if (window && window.Telegram && window.Telegram.WebApp) {
      window.Telegram.WebApp.closeScanQrPopup();
    }
  },

  Vibrate: function(duration) {
	if (window && window.Telegram && window.Telegram.WebApp)	
	{
		if (navigator.vibrate)
		{
			navigator.vibrate(duration);
		} 
		else
		{
			console.log("您的设备不支持震动功能");
		}
	}
  }
});