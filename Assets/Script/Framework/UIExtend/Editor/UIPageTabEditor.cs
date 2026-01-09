using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(UIPageTab))]
public class UIPageTabEditor : Editor
{
    private UIPageTab tab;

    private MonoScript mScript;

    void OnEnable()
    {
        tab = target as UIPageTab;
        mScript = MonoScript.FromMonoBehaviour(tab);
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        EditorGUILayout.BeginVertical();
        GUILayout.Space(10);
        EditorGUI.BeginDisabledGroup(mScript != null);
        mScript = EditorGUILayout.ObjectField("Script", mScript, typeof(MonoScript), false) as MonoScript;
        EditorGUI.EndDisabledGroup();
        if (mScript == null)
        {
            EditorGUILayout.HelpBox("UIPageTab脚本丢失", MessageType.Warning);
        }

        if (tab.ListTab != null && tab.ListTab.Count > 0)
        {
            UIPageTab.PageTabContent tabContent = null;
            for (int i = 0; i < tab.ListTab.Count; i++)
            {
                GUILayout.Space(20);
                GUI.color = Color.green;
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("页签组" + (i + 1));
                GUI.color = Color.red;
                if (GUILayout.Button("点击删除页签组" + (i + 1)))
                {
                    EditorApplication.Beep();
                    tab.ListTab.RemoveAt(i);
                    break;
                }
                EditorGUILayout.EndHorizontal();
                GUI.color = Color.white;
                tabContent = tab.ListTab[i];
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("页签组名字", GUILayout.Width(60));
                tabContent.Name = EditorGUILayout.TextField(tabContent.Name, GUILayout.MinWidth(100));
                EditorGUILayout.LabelField("选中缩放", GUILayout.Width(60));
                tabContent.Scale = EditorGUILayout.FloatField(tabContent.Scale, GUILayout.MinWidth(100));
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("普通时图片", GUILayout.Width(60));
                tabContent.NormalIcon = EditorGUILayout.TextField(tabContent.NormalIcon, GUILayout.MinWidth(100));
                EditorGUILayout.LabelField("选中时图片", GUILayout.Width(60));
                tabContent.SelectIcon = EditorGUILayout.TextField(tabContent.SelectIcon, GUILayout.MinWidth(100));
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("普通文字色", GUILayout.Width(60));
                tabContent.normalColor = EditorGUILayout.ColorField(tabContent.normalColor, GUILayout.MinWidth(100));
                EditorGUILayout.LabelField("选中文字色", GUILayout.Width(60));
                tabContent.selectColor = EditorGUILayout.ColorField(tabContent.selectColor, GUILayout.MinWidth(100));
                EditorGUILayout.EndHorizontal();
                tabContent.autoTxtColor = EditorGUILayout.Toggle("是否自动改变文字颜色", tabContent.autoTxtColor);
                GUILayout.Space(5);
                if (GUILayout.Button("点击添加 普通 按钮页签 当前按钮个数:" +
                    (tabContent.PageTabList != null ? tabContent.PageTabList.Count : 0).ToString()))
                {
                    if (tabContent.PageTabList == null)
                    {
                        tabContent.PageTabList = new List<GameObject>();
                    }
                    int len = tabContent.PageTabList.Count;
                    tabContent.PageTabList.Add(len > 0 ? tabContent.PageTabList[len - 1] : null);
                }
                if (tabContent.PageTabList != null && tabContent.PageTabList.Count > 0)
                {
                    GUILayout.Space(5);
                    for (int j = 0; j < tabContent.PageTabList.Count; j++)
                    {
                        EditorGUILayout.BeginHorizontal();
                        EditorGUILayout.LabelField("页签按钮" + (j + 1).ToString(), GUILayout.Width(60));
                        tabContent.PageTabList[j] =
                            (GameObject)EditorGUILayout.ObjectField(tabContent.PageTabList[j], typeof(GameObject), true);
                        if (GUILayout.Button("删除"))
                        {
                            EditorApplication.Beep(); // 播放系统鸣叫
                            tabContent.PageTabList.RemoveAt(j);
                            break;
                        }
                        EditorGUILayout.EndHorizontal();
                    }
                }
                GUILayout.Space(5);
            }
        }

        GUILayout.Space(10);
        GUI.color = Color.green;
        if (GUILayout.Button("点击添加新页签组", GUILayout.Height(30)))
        {
            if (tab.ListTab == null)
            {
                tab.ListTab = new List<UIPageTab.PageTabContent>();
            }
            tab.ListTab.Add(new UIPageTab.PageTabContent());
        }
        EditorGUILayout.EndVertical();

        if (GUI.changed)
        {
            serializedObject.ApplyModifiedProperties();
            Undo.RecordObject(target, "target change");
            EditorUtility.SetDirty(target);            
        }
    }
}
