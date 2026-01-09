using UnityEngine;
using UnityEditor;

public class PrefDataTools
{
    [MenuItem("Tools/清除PlayerPref缓存")]
    public static void ClearAllPrefData()
    {
        PlayerPrefs.DeleteAll();
        PlayerDataManager.instance.Clear();
    }
}
