using UnityEditor;
using UnityEngine;
using Unity.EditorCoroutines.Editor;    // EditorCoroutineUtility
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class UnusedTextureFinder : EditorWindow
{
    private Vector2 scrollPos;
    private List<string> unusedTexturePaths;
    private bool isScanning = false;

    [MenuItem("Tools/异步扫描未引用贴图")]
    public static void ShowWindow()
    {
        GetWindow<UnusedTextureFinder>("未引用贴图（Async）");
    }

    private void OnGUI()
    {
        GUILayout.Label("异步扫描全项目 Prefab 的未引用贴图", EditorStyles.boldLabel);

        if (!isScanning)
        {
            if (GUILayout.Button("开始扫描"))
            {
                unusedTexturePaths = new List<string>();
                isScanning = true;
                EditorCoroutineUtility.StartCoroutineOwnerless(ScanAllPrefabs());
            }
        }
        else
        {
            GUILayout.Label("扫描中，请稍候...");
        }

        if (unusedTexturePaths != null && unusedTexturePaths.Count > 0)
        {
            GUILayout.Space(10);
            GUILayout.Label($"共找到 {unusedTexturePaths.Count} 个未引用贴图：");
            scrollPos = EditorGUILayout.BeginScrollView(scrollPos, GUILayout.Height(300));
            for (int i = 0; i < unusedTexturePaths.Count; i++)
            {
                string path = unusedTexturePaths[i];
                EditorGUILayout.BeginHorizontal();
                
                // 显示贴图缩略图
                Texture2D tex = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
                EditorGUILayout.ObjectField(tex, typeof(Texture2D), false, GUILayout.Width(64), GUILayout.Height(64));

                EditorGUILayout.LabelField(path);

                if (GUILayout.Button("Ping", GUILayout.Width(40)))
                {
                    var obj = AssetDatabase.LoadMainAssetAtPath(path);
                    EditorGUIUtility.PingObject(obj);
                }

                if (GUILayout.Button("删除", GUILayout.Width(60)))
                {
                    if (EditorUtility.DisplayDialog("确认删除", $"确定要删除资源：{path} ?", "删除", "取消"))
                    {
                        AssetDatabase.DeleteAsset(path);
                        AssetDatabase.Refresh();
                        unusedTexturePaths.RemoveAt(i);
                        break;
                    }
                }

                EditorGUILayout.EndHorizontal();
            }
            EditorGUILayout.EndScrollView();
        }
    }

    private IEnumerator ScanAllPrefabs()
    {
        var allTexGuids = AssetDatabase.FindAssets("t:Texture");
        var allPrefabGuids = AssetDatabase.FindAssets("t:Prefab");

        var usedTexGuids = new HashSet<string>();
        int total = allPrefabGuids.Length;
        for (int i = 0; i < total; i++)
        {
            string prefabPath = AssetDatabase.GUIDToAssetPath(allPrefabGuids[i]);
            var deps = AssetDatabase.GetDependencies(prefabPath, true);
            foreach (var dep in deps)
            {
                var type = AssetDatabase.GetMainAssetTypeAtPath(dep);
                if (type == typeof(Texture) || type.IsSubclassOf(typeof(Texture)))
                    usedTexGuids.Add(AssetDatabase.AssetPathToGUID(dep));
            }

            EditorUtility.DisplayProgressBar(
                "扫描未引用贴图",
                $"Processing {i + 1}/{total} Prefabs",
                (float)(i + 1) / total);
            if (i % 5 == 0)
                yield return null;
        }

        EditorUtility.ClearProgressBar();

        unusedTexturePaths = allTexGuids
            .Except(usedTexGuids)
            .Select(g => AssetDatabase.GUIDToAssetPath(g))
            .ToList();

        isScanning = false;
    }
}
