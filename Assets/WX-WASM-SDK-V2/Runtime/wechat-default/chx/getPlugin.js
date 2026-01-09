try {
  if (typeof requirePlugin !== 'undefined') {
    const createMiniGameCommon = requirePlugin('MiniGameCommon', {
      enableRequireHostModule: true,
      customEnv: {
        wx,
      },
    }).default;
    const miniGameCommon = createMiniGameCommon();
    if (typeof miniGameCommon === 'undefined' || typeof miniGameCommon.canIUse === 'undefined') {
      // 插件初始化失败
      console.error('miniGameCommon create error');
    } else {
      // 插件初始化成功
      GameGlobal.miniGameCommon = miniGameCommon;
    }
  }
} catch (e) {
  // 基础库版本过低
  console.error(e);
}