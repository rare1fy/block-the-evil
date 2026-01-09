using UnityEditor;
using UnityEngine;
using System.IO;
using System.Text;
using System.Collections.Generic;
using System.Linq;

public class AdvancedEncodingConverter : EditorWindow
{
    private enum ConversionScope
    {
        整个项目,
        选中文件夹,
        选中文件
    }
    
    private ConversionScope conversionScope = ConversionScope.选中文件;
    
    [MenuItem("Tools/UTF-8 编码转换器")]
    public static void ShowWindow()
    {
        GetWindow<AdvancedEncodingConverter>("UTF-8 编码转换器");
    }
    
    private void OnGUI()
    {
        GUILayout.Label("脚本编码转换工具", EditorStyles.boldLabel);
        GUILayout.Label("目标编码：UTF-8 无BOM", EditorStyles.label);
        
        // Conversion scope selection
        conversionScope = (ConversionScope)EditorGUILayout.EnumPopup("转换范围", conversionScope);
        
        EditorGUILayout.HelpBox(
            conversionScope == ConversionScope.整个项目 ? "将转换项目中所有脚本为UTF-8无BOM格式" :
            conversionScope == ConversionScope.选中文件夹 ? "将转换选中文件夹内所有脚本为UTF-8无BOM格式" :
            "将转换选中的脚本文件为UTF-8无BOM格式", MessageType.Info);
        
        if (GUILayout.Button("转换为 UTF-8 无BOM"))
        {
            ConvertScriptsEncoding();
        }
    }
    
    private void ConvertScriptsEncoding()
    {
        Encoding targetEncoding = new UTF8Encoding(false);
        List<string> scriptsToConvert = GetScriptsToConvert();
        
        if (scriptsToConvert.Count == 0)
        {
            EditorUtility.DisplayDialog("提示", "未找到需要转换的脚本文件", "确定");
            return;
        }
        
        int successCount = 0;
        int failCount = 0;
        var failedFiles = new List<string>();
        
        foreach (string scriptPath in scriptsToConvert)
        {
            try
            {
                string content = File.ReadAllText(scriptPath);
                File.WriteAllText(scriptPath, content, targetEncoding);
                successCount++;
            }
            catch (System.Exception e)
            {
                Debug.LogError($"转换失败 {scriptPath}: {e.Message}");
                failCount++;
                failedFiles.Add($"{Path.GetFileName(scriptPath)}: {e.Message}");
            }
        }
        
        AssetDatabase.Refresh();
        
        string resultMessage = $"转换完成！\n\n成功：{successCount}\n失败：{failCount}";
        
        if (failCount > 0)
        {
            resultMessage += "\n\n失败文件：\n" + string.Join("\n", failedFiles.Take(5));
            if (failedFiles.Count > 5)
            {
                resultMessage += $"\n...以及其他{failedFiles.Count - 5}个文件";
            }
        }
        
        EditorUtility.DisplayDialog("结果", resultMessage, "确定");
    }
    
    private List<string> GetScriptsToConvert()
    {
        var scriptPaths = new List<string>();
        
        switch (conversionScope)
        {
            case ConversionScope.整个项目:
                scriptPaths.AddRange(Directory.GetFiles(Application.dataPath, "*.cs", SearchOption.AllDirectories));
                scriptPaths.AddRange(Directory.GetFiles(Path.Combine(Application.dataPath, "../Packages"), "*.cs", SearchOption.AllDirectories));
                break;
                
            case ConversionScope.选中文件夹:
                foreach (var obj in Selection.objects)
                {
                    string path = AssetDatabase.GetAssetPath(obj);
                    if (Directory.Exists(path))
                    {
                        scriptPaths.AddRange(Directory.GetFiles(path, "*.cs", SearchOption.AllDirectories));
                    }
                }
                break;
                
            case ConversionScope.选中文件:
                foreach (var obj in Selection.objects)
                {
                    string path = AssetDatabase.GetAssetPath(obj);
                    if (path.EndsWith(".cs"))
                    {
                        scriptPaths.Add(path);
                    }
                }
                break;
        }
        
        var fullPaths = new List<string>();
        foreach (string path in scriptPaths)
        {
            string fullPath = Path.GetFullPath(path);
            if (!fullPaths.Contains(fullPath))
            {
                fullPaths.Add(fullPath);
            }
        }
        
        return fullPaths;
    }
}