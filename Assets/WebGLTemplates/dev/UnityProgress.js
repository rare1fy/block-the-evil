function getQueryString(name) {
    let reg = new RegExp("(^|&)" + name + "=([^&]*)(&|$)", "i");
    let r = window.location.search.substr(1).match(reg);
    if (r != null) {
        return decodeURIComponent(r[2]);
    };
    return null;
}
 
function UnityProgress(unityInstance, progress) {
  if (!unityInstance.Module)
    return;
	console.log(progress)
  if (progress == 1) {
      document.getElementById("progress1").style.display = "none"
      
        //unityInstance.logo.style.display = unityInstance.progress.style.display = "none";
      unityInstance.SendMessage("[GameManager]", "SetName", getQueryString("name"));
      unityInstance.SendMessage("[GameManager]", "SetIcon", getQueryString("icon"));
      unityInstance.SendMessage("[GameManager]", "SetUserID", getQueryString("user_id"));
  } else {
      document.getElementById("progress1").style.display = "block"
      document.getElementById("progress2").style.width = (100 * progress) + "%";
  }
}