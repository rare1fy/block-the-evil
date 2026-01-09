using System.Collections.Generic;
using UnityEngine;

public class ChapterScene : MonoBehaviour
{
    public List<ChapterBuildItem> chapterBuildItems = new();
    public List<GameObject> Others;

    public void Awake()
    {
        var builds = GetComponentsInChildren<ChapterBuildItem>();
        //animator = GetComponent<Animator>();
        chapterBuildItems.AddRange(builds);
    }

    public void RefreshView() 
    {
        foreach (var item in Others)
        {
            item.SetActiveEx(false);
        }
        foreach (var build in chapterBuildItems) 
        {
            var cfg = Config.GetConfig<Config_BuildBase>().GetBuildNode(build.gameObject.name);
            build.SetItem(cfg.Id);
        }
    }

    public void OnBuilding(int buildId)
    {
        var cfg = Config.GetConfig<Config_BuildBase>().GetConfigById(buildId);
        foreach (var item in chapterBuildItems) 
        {
            if(item.gameObject.name == cfg.Buildnod)
            {
                item.OnBuilding();
            }
        }
    }

    public void OnPlayAnimator()
    {
        foreach (var item in Others)
        {
            item.SetActiveEx(true);
        }
        
    }
}
