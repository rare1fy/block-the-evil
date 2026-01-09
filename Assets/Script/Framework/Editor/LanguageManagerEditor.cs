using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(LanguageManager), true)]
[CanEditMultipleObjects]
public class LanguageManagerEditor : Editor
{
    private LanguageManager _language;
    private LanguageStyle curRegionStyle;
    void OnEnable()
    {
        curRegionStyle = LanguageManager.Instance.CurLanguageStyle;
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        serializedObject.Update();
        _language = serializedObject.targetObject as LanguageManager;
        _language.CurLanguageStyle = (LanguageStyle)PlayerPrefs.GetInt(LanguageManager.LanguageSavaKey, 0);
        curRegionStyle =(LanguageStyle)EditorGUILayout.EnumPopup("多语言类型,默认是英文", _language.CurLanguageStyle);
        if (curRegionStyle != _language.CurLanguageStyle)
        {
            _language.CurLanguageStyle = curRegionStyle;
            _language.ClearData();
            _language.LangInterfaceRefresh();
            PlayerPrefs.SetInt(LanguageManager.LanguageSavaKey, (int)_language.CurLanguageStyle);
            Debug.Log("切换语言成功" + curRegionStyle);
            EditorUtility.SetDirty(_language);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
        
        if (GUILayout.Button("删除多语言缓存"))
        {
            _language.ClearData();
            PlayerPrefs.DeleteKey(LanguageManager.LanguageSavaKey);
            Debug.Log("删除多语言缓存成功");
            EditorUtility.SetDirty(_language);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
    }
}
