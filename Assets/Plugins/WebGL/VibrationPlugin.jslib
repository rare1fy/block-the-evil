mergeInto(LibraryManager.library, {
  VibrateShortLight: function () {
    if (typeof wx !== 'undefined' && wx.vibrateShort) {
      try { wx.vibrateShort({ type: 'light' }); } catch(e) { wx.vibrateShort(); }
    }
  },

  VibrateShortMedium: function () {
    if (typeof wx !== 'undefined' && wx.vibrateShort) {
      try { wx.vibrateShort({ type: 'medium' }); } catch(e) { wx.vibrateShort(); }
    }
  },

  VibrateShortHeavy: function () {
    if (typeof wx !== 'undefined' && wx.vibrateShort) {
      try { wx.vibrateShort({ type: 'heavy' }); } catch(e) { wx.vibrateShort(); }
    }
  },

  VibrateLong: function () {
    if (typeof wx !== 'undefined' && wx.vibrateLong) {
      wx.vibrateLong();
    }
  },

  // 保存数据
  CustomWXWriteFile: function (fileKeyPtr, dataJsonPtr) {
    var fileKey = UTF8ToString(fileKeyPtr);
    var dataJson = UTF8ToString(dataJsonPtr);

    if (typeof wx !== 'undefined' && wx.getFileSystemManager) {
      var fs = wx.getFileSystemManager();
      fs.writeFile({
        filePath: `${wx.env.USER_DATA_PATH}/${fileKey}.json`,
        data: dataJson,
        encoding: "utf8",
        fail(err) {
          console.error("写入失败: " + fileKey, err);
        }
      });
    }
  },

  CustomWXReadFile: function (fileKeyPtr) {
    if (typeof wx === 'undefined' || !wx.getFileSystemManager) {
      console.error('微信环境未初始化或缺少文件系统支持');
      SendMessage("WxJsManager", "OnReadFileFromJs", "");
      return;
    }

    try {
      var fileKey = UTF8ToString(fileKeyPtr);
      var fs = wx.getFileSystemManager();
      var filePath = `${wx.env.USER_DATA_PATH}/${fileKey}.json`;

      fs.readFile({
        filePath: filePath,
        encoding: "utf8",
        success: function(res) {
          console.log(`文件读取成功: ${fileKey}`);
          SendMessage("WxJsManager", "OnReadFileFromJs", res.data || "");
        },
        fail: function(error) {
          console.warn(`文件读取失败: ${fileKey}`, error);
          SendMessage("WxJsManager", "OnReadFileFromJs", "");
        }
      });
    }
    catch (error) {
      console.error('文件读取异常:', error);
      SendMessage("WxJsManager", "OnReadFileFromJs", "");
    }
  },

  RegisterOnShowCallback: function () {
    if (typeof wx !== 'undefined' && wx.onShow) {
      wx.onShow(function() {
        SendMessage("WxJsManager", "_OnWxShow", "");
      });
    }
  },
  
  RegisterOnHideCallback: function () {
    if (typeof wx !== 'undefined' && wx.onHide) {
      wx.onHide(function() {
        SendMessage("WxJsManager", "_OnWxHide", "");
      });
    }
  },
});
