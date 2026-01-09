#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEngine.UI;

[CustomEditor(typeof(ArtNumberTool))]
public class ArtNunmberToolEditor : Editor
{
    public override void OnInspectorGUI()
    {
        // 获取目标脚本
        ArtNumberTool tool = (ArtNumberTool)target;
        
        // 绘制默认Inspector
        DrawDefaultInspector();
        
        // 添加一些空间
        EditorGUILayout.Space();
        
        // 创建测试按钮
        if (GUILayout.Button("测试显示数字"))
        {
            tool.DisplayNumber(tool.testNumber);
        }
        //
        // // 添加一些空间
        // EditorGUILayout.Space();
        //
        // // 快速设置颜色按钮
        // EditorGUILayout.LabelField("快速设置颜色:");
        // GUILayout.BeginHorizontal();
        // if (GUILayout.Button("红色")) { tool.displayColor = Color.red; tool.ApplyColor(); }
        // if (GUILayout.Button("绿色")) { tool.displayColor = Color.green; tool.ApplyColor(); }
        // if (GUILayout.Button("蓝色")) { tool.displayColor = Color.blue; tool.ApplyColor(); }
        // if (GUILayout.Button("白色")) { tool.displayColor = Color.white; tool.ApplyColor(); }
        // GUILayout.EndHorizontal();
    }
}
#endif