using Framework;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class UIMainHouseItem : UIItemBase
{
    [BindNode(true)] private GameObject _Obj_Build;//图标
    [BindNode] private RectTransform _Obj_BuildRoot;//图标
    [BindNode] private RawImage _Img_BG;

    List<GameObject> pool = new();

    int chapterId;
    protected override void InitItem()
    {
    }

    public void SetUI(int chapterId)
    {
        pool.Clear();
        for (int i = 0; i < _Obj_BuildRoot.childCount; i++)
        {
            var obj = _Obj_BuildRoot.GetChild(i).gameObject;
            pool.Add(obj);
            obj.SetActiveEx(false);
        }

        var chapterModel = GameManager.Instance.ChapterControl.Model;
        if (chapterModel.ChapterDic.TryGetValue(chapterId, out var chapterData))
        {
            foreach (var buildId in chapterData.buildedIds)
            {
                var item = GetGameObject(buildId).GetOrAddComponent<RawImage>();
                SetBuildIamge(buildId, item, true);
            }

            foreach (var buildId in chapterData.UnBuildIds)
            {
                var item = GetGameObject(buildId).GetOrAddComponent<RawImage>();
                SetBuildIamge(buildId, item, false);
            }
        }
        else
        {
            Debug.LogError("未解锁");
        }

        var chapterCfg = Config.GetConfig<Config_ChapterBase>().GetConfigById(chapterId);
        ResourceManagerNew.instance.LoadAssetAsync(chapterCfg.Background, delegate (Texture texture)
        {
            _Img_BG.texture = texture;
        });
    }

    private void SetBuildIamge(int buildid, RawImage item, bool isNew = false)
    {
        //var cfg = Config.GetConfig<Config_BuildBase>().GetConfigById(buildid);

        //var textureName = isNew ? cfg.Newbuild : cfg.Oldbuild;
        //if (string.IsNullOrEmpty(textureName))
        //{
        //    item.gameObject.SetActive(false);
        //    return;
        //}
        //ResourceManagerNew.instance.LoadAssetAsync(cfg.Oldbuild, delegate (Texture texture)
        //{
        //    item.texture = texture;
        //});
        //item.gameObject.SetActive(true);
    }


    public GameObject GetGameObject(int buildId)
    {
        GameObject obj;
        if (pool.Count > 0)
        {
            obj = pool[0];
            pool.RemoveAt(0);
        }
        else
        {
            obj = Instantiate(_Obj_Build, _Obj_BuildRoot);
        }
        obj.SetActiveEx(true);
        obj.name = buildId.ToString();
        return obj;

    }

    /// <summary>
    /// 建造建筑
    /// </summary>
    /// <param name="buildId"></param>
    public void OnBuild(int buildId)
    {
        for (int i = 0; i < _Obj_BuildRoot.childCount; i++)
        {
            var obj = _Obj_BuildRoot.GetChild(i).gameObject;
            if (int.Parse(obj.name) == buildId)
            {
                obj.SetActiveEx(true);
                //后面加动画
                SetBuildIamge(buildId, obj.GetOrAddComponent<RawImage>(), true);
                return;
            }
        }
    }
}
