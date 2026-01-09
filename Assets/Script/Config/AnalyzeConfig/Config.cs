using System;
using System.Collections;
using System.Collections.Generic;
using Framework;
using System.IO;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class Config : Singleton<Config>
{
	private static readonly HashSet<ConfigBase> ConfigList = new()
	{
		new	Config_AdDesc(),
		new	Config_BlockBase(),
		new	Config_BlockColor(),
		new	Config_BlockEffect(),
		new	Config_BlockLibrary(),
		new	Config_BuildBase(),
		new	Config_CallwordBase(),
		new	Config_CallwordLibrary(),
		new	Config_ChapterBase(),
		new	Config_DefineError(),
		new	Config_FightchatBase(),
		new	Config_FightchatLibrary(),
		new	Config_FighteffectBase(),
		new	Config_GdConstant(),
		new	Config_GiftBase(),
		new	Config_GoodfeelLv(),
		new	Config_GoodfeelNpc(),
		new	Config_GuildGroup(),
		new	Config_GuildPerform(),
		new	Config_GuildWord(),
		new	Config_ItemBase(),
		new	Config_LevelBase(),
		new	Config_LevelOrder(),
		new	Config_LevelTreasurechest(),
		new	Config_LevelWujingModle(),
		new	Config_MechanismGuild(),
		new	Config_MusicPlayer(),
		new	Config_NpcitemLibrary(),
		new	Config_NpcBase(),
		new	Config_NpcLibrary(),
		new	Config_NpcUnlock(),
		new	Config_OrderBlock(),
		new	Config_OutworldLibrary(),
		new	Config_OutWord(),
		new	Config_RandomwordComplete(),
		new	Config_RandomwordHarry(),
		new	Config_RankWujing(),
		new	Config_ShareWechatImage(),
		new	Config_SighinBase(),
	};  

	public static T GetConfig<T>() where T : ConfigBase
    {
        foreach (var item in ConfigList)
        {
            if (item is T)
            {
                var asset = item as T;
                return asset;
            }
        }
        Debug.LogError($"解析的配置表名字不存在!!!!! {typeof(T)}");
        return null;
    }

    public Dictionary<string, Action<Stream>> _configLoadData = new();

    public IEnumerator LoadConfigStep(Action callBack = null)
    {
        foreach (var item in ConfigList)
        {
            item.LoadConfigStep();
        }
        var lit = _configLoadData;
        foreach (var configList in lit)
        {
            var mStrName = configList.Key;
            var mAction = configList.Value;
            var isComplect = false;
            ResourceManagerNew.instance.LoadConfigAsync(mStrName, delegate (byte[] bytes)
            {
                if (!ReferenceEquals(bytes,null))
                {
                    mAction(new MemoryStream(bytes));
                }
                else
                {
                    Debug.LogError($"===Load Config Fail=== Config Name:{mStrName}");
                }
                isComplect = true;
            });
            
            while (!isComplect)
            {
                yield return null;
            }
        }

        if (!ReferenceEquals(callBack, null))
        {
            callBack();
        }
    }

    public async UniTask LoadConfigStepAsync()
    {
	    foreach (var item in ConfigList)
	    {
		    item.LoadConfigStep();
	    }
	    
	    var loadTasks = _configLoadData.Select(async configPair =>
	    {
		    var configName = configPair.Key;
		    var action = configPair.Value;
        
		    byte[] bytes = await ResourceManagerNew.instance.LoadConfigAsync(configName);
		    
		    if (bytes != null)
		    {
			    action(new MemoryStream(bytes));
		    }
		    else
		    {
			    Debug.LogError($"===Load Config Fail=== Config Name:{configName}");
		    }
	    });
	    
	    await UniTask.WhenAll(loadTasks);
    }

    public override void Dispose()
    {
        _configLoadData = null;
        ConfigList.Clear();
    }
}
