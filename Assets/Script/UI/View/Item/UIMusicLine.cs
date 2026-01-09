using System.Collections.Generic;
using Pb;

public class UIMusicLine : UIItemBase
{
    [BindNode] UIMusicItem _Obj_MusicItem1;
    [BindNode] UIMusicItem _Obj_MusicItem2;
    [BindNode] UIMusicItem _Obj_MusicItem3;
    List<UIMusicItem> uIMusicItems;
    protected override void InitItem()
    {
        uIMusicItems = new List<UIMusicItem>() { _Obj_MusicItem1, _Obj_MusicItem2, _Obj_MusicItem3 };
    }

    protected override void OnDestroy()
    {
    }

    private MusicPlayer _musicCfg;
    public void RefreshItem(List<MusicPlayer> musicPlayerList, bool isMainOpen = false)
    {
        for (int i = 0; i < uIMusicItems.Count; i++)
        {
            var item = uIMusicItems[i];
            item.gameObject.SetActiveEx(i < musicPlayerList.Count);
            if (i < musicPlayerList.Count)
            {
                item.RefreshItem(musicPlayerList[i], isMainOpen);
            }
        }
    }
}
