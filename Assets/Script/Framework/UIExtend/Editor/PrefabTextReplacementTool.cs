using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using System;

public class PrefabTextReplacementTool : EditorWindow
{
    private GameObject targetPrefab;
    private MonoScript customTextComponent;
    
    [MenuItem("Tools/预制体文本替换")]
    public static void ShowWindow()
    {
        GetWindow<PrefabTextReplacementTool>("文本替换标题");
    }
    
    private void OnGUI()
    {
        GUILayout.Label("预制体文本替换", EditorStyles.boldLabel);
        
        targetPrefab = (GameObject)EditorGUILayout.ObjectField("Target Prefab", targetPrefab, typeof(GameObject), false);
        customTextComponent = (MonoScript)EditorGUILayout.ObjectField("Custom Text Component", customTextComponent, typeof(MonoScript), false);
        
        EditorGUILayout.Space();
        
        if (GUILayout.Button("开始替换"))
        {
            if (targetPrefab == null || customTextComponent == null)
            {
                EditorUtility.DisplayDialog("Error", "Please assign both prefab and custom text component", "OK");
                return;
            }
            
            if (!PrefabUtility.IsPartOfPrefabAsset(targetPrefab))
            {
                EditorUtility.DisplayDialog("Error", "Please select a prefab from Project view", "OK");
                return;
            }
            
            ReplaceTextComponentsInPrefab();
        }
    }
    
    private void ReplaceTextComponentsInPrefab()
    {
        string prefabPath = AssetDatabase.GetAssetPath(targetPrefab);
        GameObject prefabInstance = PrefabUtility.LoadPrefabContents(prefabPath);
        bool modified = false;
        int replacementCount = 0;
        
        try
        {
            // 获取所有Text组件（包括非激活的）
            Text[] textComponents = prefabInstance.GetComponentsInChildren<Text>(true);
            
            foreach (Text textComponent in textComponents)
            {
                // 跳过已经是自定义Text组件的情况
                if (textComponent.GetType() == customTextComponent.GetClass())
                    continue;
                
                GameObject gameObject = textComponent.gameObject;
                
                // 1. 首先保存旧组件的所有属性
                var savedProperties = SaveTextProperties(textComponent);
                
                // 2. 移除旧的Text组件
                DestroyImmediate(textComponent, true);
                
                // 3. 添加新的自定义Text组件
                Component newTextComponent = gameObject.AddComponent(customTextComponent.GetClass());
                
                // 4. 恢复保存的属性到新组件
                RestoreTextProperties(savedProperties, newTextComponent);
                
                modified = true;
                replacementCount++;
            }
            
            if (modified)
            {
                PrefabUtility.SaveAsPrefabAsset(prefabInstance, prefabPath);
                AssetDatabase.Refresh();
                EditorUtility.DisplayDialog("Success", $"Successfully replaced {replacementCount} Text components", "OK");
            }
            else
            {
                EditorUtility.DisplayDialog("Info", "No Text components found to replace", "OK");
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Error during replacement: {e.Message}");
            EditorUtility.DisplayDialog("Error", $"Failed to replace components: {e.Message}", "OK");
        }
        finally
        {
            PrefabUtility.UnloadPrefabContents(prefabInstance);
        }
    }
    
    private TextProperties SaveTextProperties(Text source)
    {
        return new TextProperties
        {
            text = source.text,
            font = source.font,
            fontSize = source.fontSize,
            lineSpacing = source.lineSpacing,
            supportRichText = source.supportRichText,
            alignment = source.alignment,
            color = source.color,
            resizeTextForBestFit = source.resizeTextForBestFit,
            resizeTextMinSize = source.resizeTextMinSize,
            resizeTextMaxSize = source.resizeTextMaxSize
        };
    }
    
    private void RestoreTextProperties(TextProperties properties, Component target)
    {
        if (properties == null || target == null) return;
        
        TrySetField(target, "text", properties.text);
        TrySetField(target, "font", properties.font);
        TrySetField(target, "fontSize", properties.fontSize);
        TrySetField(target, "lineSpacing", properties.lineSpacing);
        TrySetField(target, "supportRichText", properties.supportRichText);
        TrySetField(target, "alignment", properties.alignment);
        TrySetField(target, "color", properties.color);
        TrySetField(target, "resizeTextForBestFit", properties.resizeTextForBestFit);
        TrySetField(target, "resizeTextMinSize", properties.resizeTextMinSize);
        TrySetField(target, "resizeTextMaxSize", properties.resizeTextMaxSize);
    }
    
    private void TrySetField(object target, string fieldName, object value)
    {
        if (target == null || value == null) return;
        
        System.Type targetType = target.GetType();
        
        try
        {
            // 尝试设置字段
            var field = targetType.GetField(fieldName);
            if (field != null && field.FieldType == value.GetType())
            {
                field.SetValue(target, value);
                return;
            }
            
            // 尝试设置属性
            var property = targetType.GetProperty(fieldName);
            if (property != null && property.PropertyType == value.GetType() && property.CanWrite)
            {
                property.SetValue(target, value, null);
            }
        }
        catch (Exception e)
        {
            Debug.LogWarning($"Failed to set field '{fieldName}': {e.Message}");
        }
    }
    
    private class TextProperties
    {
        public string text;
        public Font font;
        public int fontSize;
        public float lineSpacing;
        public bool supportRichText;
        public TextAnchor alignment;
        public Color color;
        public bool resizeTextForBestFit;
        public int resizeTextMinSize;
        public int resizeTextMaxSize;
    }
}