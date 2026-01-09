function OpenWebPage(url) {
    window.open(url, "_blank");
}

function ResizeCanvas()
{
    document.body.scrollTop = 0 
    const scrollHeight = document.documentElement.scrollTop || document.body.scrollTop || 0;
    window.scrollTo(0, Math.max(scrollHeight - 1, 0));

    document.activeElement.focus()
    
    if (unityInstance && unityInstance.Module && unityInstance.Module.canvas) {
        unityInstance.Module.canvas.focus();
    }
}

function SetTelgramUnityEvent() {
    console.log('Received message:SetTelgramUnityEvent');
    window.Telegram.WebApp.onEvent('activated', function(data) {
        // 处理接收到的消息
        console.log('Received message:', data);
        // 可以在这里调用 Unity 的方法传递消息
        if (window && window.Telegram && window.Telegram.WebApp) {
            window.unityInstance.SendMessage("GameManager", "ReflashTelegramActive", `${window.Telegram.WebApp.isActive}`);
          }
      });
      window.Telegram.WebApp.onEvent('deactivated', function(data) {
        // 处理接收到的消息
        console.log('Received message:', data);
        // 可以在这里调用 Unity 的方法传递消息
        if (window && window.Telegram && window.Telegram.WebApp) {
            window.unityInstance.SendMessage("GameManager", "ReflashTelegramActive", `${window.Telegram.WebApp.isActive}`);
          }
      });
   
}
