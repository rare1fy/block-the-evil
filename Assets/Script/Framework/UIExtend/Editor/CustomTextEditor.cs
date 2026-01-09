using UnityEngine;
using UnityEditor;
using System.Text.RegularExpressions;
using UnityEditor.UI;

[CustomEditor(typeof(CustomText), true)]
[CanEditMultipleObjects]
public class CustomTextEditor : GraphicEditor
{
    protected CustomText mInstance;
    SerializedProperty m_Text;
    SerializedProperty m_FontData;
    private static LanguageStyle currentLanguageStyle;
    private bool isUpperActive = false;

    private void Awake()
    {
        mInstance = target as CustomText;
        currentLanguageStyle = LanguageManager.Instance.CurLanguageStyle;
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        m_Text = serializedObject.FindProperty("m_Text");
        m_FontData = serializedObject.FindProperty("m_FontData");
    }

    public override void OnInspectorGUI()
    {
        GUI.color = Color.green;
        EditorGUILayout.LabelField("当前选中的语言是:" + currentLanguageStyle, GUILayout.Width(200));
        GUI.color = Color.white;
        isUpperActive = EditorGUILayout.Toggle("是否字母全转化为大写", mInstance.IsCapital);
        if (mInstance.IsCapital != isUpperActive)
        {
            mInstance.IsCapital = isUpperActive;
            mInstance.SetVerticesDirty();
        }

        //显示转化后的文字
        string strText = mInstance.text;
        if (GUILayout.Button("查找并替换多语言"))
        {
            var isFind = LanguageManager.Instance.FindAndReplaceString(ref strText);
            if (isFind)
            {
                mInstance.text = strText;
                Debug.Log("找到并替换成功了！！！！！！");
                EditorUtility.SetDirty(mInstance);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }
        }

        EditorGUILayout.PropertyField(m_Text);
        if (Regex.IsMatch(strText, @"\[(?:LID):\d+\]") || Regex.IsMatch(strText, @"\[(?:FID):\d+\]"))
        {
            EditorGUILayout.TextArea(LanguageManager.Instance.AnalysiseLanguageText(strText, mInstance.IsCapital), EditorStyles.textArea);
        }
        EditorGUILayout.PropertyField(m_FontData);
        AppearanceControlsGUI();
        RaycastControlsGUI();
        serializedObject.ApplyModifiedProperties();
    }

    [MenuItem("GameObject/UI/CustomText_Font", false, 0)]
    public static void CreateCustomText()
    {
        GameObject parent = Selection.activeGameObject;
        GameObject go = new GameObject("CustomText");
        if (parent != null)
        {
            go.transform.SetParent(parent.transform, false);
        }
        else
        {
            var canvas = FindObjectOfType<Canvas>();
            if (canvas != null)
                go.transform.SetParent(canvas.transform, false);
        }
        go.AddComponent<RectTransform>();
        go.AddComponent<CanvasRenderer>();
        go.AddComponent<CustomText>();
        Selection.activeGameObject = go;
    }

    [MenuItem("GameObject/UI/Text_Font", false, 12)]
    public static void CreateText()
    {
        GameObject parent = Selection.activeGameObject;
        GameObject go = new GameObject("Text");
        if (parent != null)
        {
            go.transform.SetParent(parent.transform, false);
        }
        else
        {
            var canvas = FindObjectOfType<Canvas>();
            if (canvas != null)
                go.transform.SetParent(canvas.transform, false);
        }
        go.AddComponent<RectTransform>();
        go.AddComponent<CanvasRenderer>();
        go.AddComponent<CustomText>();
        Selection.activeGameObject = go;
    }
}