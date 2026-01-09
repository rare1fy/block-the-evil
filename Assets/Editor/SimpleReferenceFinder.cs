using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;

public class SimpleReferenceFinder : EditorWindow
{
    // 添加右键菜单项
    [MenuItem("Assets/*查找所有引用", false)]
    private static void FindReferences()
    {
        UnityEngine.Object selected = Selection.activeObject;
        if (selected == null)
        {
            Debug.LogWarning("Please select an asset first.");
            return;
        }

        string assetPath = AssetDatabase.GetAssetPath(selected);
        string assetGuid = AssetDatabase.AssetPathToGUID(assetPath);

        Debug.Log($"<color=cyan>开始高级查找引用: {selected.name}</color>");
        Debug.Log($"路径: {assetPath}");
        Debug.Log($"GUID: {assetGuid}");
        Debug.Log("----------------------------------------");

        // 1. 查找所有可能包含引用的文件
        string[] allPrefabPaths = Directory.GetFiles(Application.dataPath, "*.prefab", SearchOption.AllDirectories);
        string[] allScenePaths = Directory.GetFiles(Application.dataPath, "*.unity", SearchOption.AllDirectories);
        string[] allMaterialPaths = Directory.GetFiles(Application.dataPath, "*.mat", SearchOption.AllDirectories);
        string[] allScriptableObjectPaths = Directory.GetFiles(Application.dataPath, "*.asset", SearchOption.AllDirectories);

        List<string> allFilePaths = new List<string>();
        allFilePaths.AddRange(allPrefabPaths);
        allFilePaths.AddRange(allScenePaths);
        allFilePaths.AddRange(allMaterialPaths);
        allFilePaths.AddRange(allScriptableObjectPaths);

        int referenceCount = 0;
        EditorUtility.DisplayProgressBar("查找引用中", "正在扫描项目文件...", 0f);

        try
        {
            for (int i = 0; i < allFilePaths.Count; i++)
            {
                string filePath = allFilePaths[i];
                string relativePath = "Assets" + filePath.Replace(Application.dataPath, "").Replace('\\', '/');

                // 更新进度条
                EditorUtility.DisplayProgressBar("查找引用中", $"正在检查: {Path.GetFileName(relativePath)}", (float)i / allFilePaths.Count);

                // 2. 根据文件类型使用不同的方法检查引用
                bool isReferenced = false;

                if (relativePath.EndsWith(".prefab") || relativePath.EndsWith(".unity"))
                {
                    // 对于prefab和scene，仍然使用GUID文本搜索（速度快）
                    string fileContent = File.ReadAllText(filePath);
                    isReferenced = fileContent.Contains(assetGuid);
                }
                else
                {
                    // 对于材质(.mat)、ScriptableObject(.asset)等二进制文件，使用AssetDatabase API
                    // 加载这个文件中的所有资源
                    UnityEngine.Object[] assetsInFile = AssetDatabase.LoadAllAssetsAtPath(relativePath);
                    
                    foreach (var assetInFile in assetsInFile)
                    {
                        if (assetInFile == null) continue;

                        // 如果是材质球，检查它的所有纹理属性、Shader等
                        if (assetInFile is Material material)
                        {
                            // 检查Shader
                            if (material.shader != null)
                            {
                                if (IsAssetReferenced(material.shader, assetGuid))
                                {
                                    isReferenced = true;
                                    break;
                                }
                            }

                            // 检查所有纹理属性
                            int propertyCount = ShaderUtil.GetPropertyCount(material.shader);
                            for (int p = 0; p < propertyCount; p++)
                            {
                                if (ShaderUtil.GetPropertyType(material.shader, p) == ShaderUtil.ShaderPropertyType.TexEnv)
                                {
                                    string propertyName = ShaderUtil.GetPropertyName(material.shader, p);
                                    Texture tex = material.GetTexture(propertyName);
                                    if (IsAssetReferenced(tex, assetGuid))
                                    {
                                        isReferenced = true;
                                        break;
                                    }
                                }
                            }
                        }
                        else
                        {
                            // 对于其他类型的资源（如ScriptableObject），使用序列化属性检查
                            isReferenced = IsAssetReferenced(assetInFile, assetGuid);
                            if (isReferenced) break;
                        }
                    }
                }

                if (isReferenced)
                {
                    referenceCount++;
                    Debug.Log($"被引用于: <color=yellow>{relativePath}</color>", AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(relativePath));
                }
            }
        }
        finally
        {
            EditorUtility.ClearProgressBar();
        }

        Debug.Log("----------------------------------------");
        if (referenceCount > 0)
        {
            Debug.Log($"<color=green>查找完成！共在 {referenceCount} 个文件中找到了引用。</color>");
        }
        else
        {
            Debug.Log($"<color=orange>查找完成！未在任何文件中找到对该资源的引用。</color>");
        }
    }

    // 辅助方法：检查一个UnityEngine.Object是否引用了目标GUID对应的资源
    private static bool IsAssetReferenced(UnityEngine.Object obj, string targetGuid)
    {
        if (obj == null) return false;

        string objPath = AssetDatabase.GetAssetPath(obj);
        string objGuid = AssetDatabase.AssetPathToGUID(objPath);

        return objGuid == targetGuid;
    }
    // 验证菜单项
    [MenuItem("Assets/*查找所有引用", true)]
    private static bool ValidateFindReferences()
    {
        return Selection.activeObject != null;
    }
    
}