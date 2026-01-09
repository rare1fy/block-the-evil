using Pb;
using System.Collections.Generic;
using UnityEngine;

public partial class Config_BuildBase : ConfigBase
{
    public Dictionary<string, BuildBase> m_buildNodeDic;

    private void AnalysisBuildNode()
    {
        if (m_buildNodeDic == null) 
        {
            m_buildNodeDic = new();
            foreach (var item in m_BuildBaseDic.Values)
            {
                m_buildNodeDic.TryAdd(item.Buildnod, item);
            }
        }


    }

    public BuildBase GetBuildNode(string node)
    {
        AnalysisBuildNode();
        if (m_buildNodeDic.TryGetValue(node, out var buildBase))
        {
            return buildBase;
        }
        else
        {
            Debug.LogError($"未找到对应节点{node}");
            return null;
        }
    }
}
