using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using System.Reflection;
using System;
using Newtonsoft.Json;
using Google.Protobuf;

public class DBQuickProcessTool
{
    [MenuItem("Assets/*将DB bytes文件导为json文件")]
    public static void ExportBytesToJson()
    {
        var assetGUIDs = Selection.assetGUIDs;
        if (assetGUIDs != null)
        {
            foreach (var guid in assetGUIDs)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var ext = Path.GetExtension(path);
                var fileName = Path.GetFileNameWithoutExtension(path);
                if (ext.Equals(".bytes"))
                {
                    var pbFileName = "Pb." + fileName + "Config";
                    Debug.Log(pbFileName);
                    var typeList = GetTypes(fileName+"Config");
                    Type pbType = typeList[0]; 
                    var asset = AssetDatabase.LoadAssetAtPath<TextAsset>(path);
                    if (asset != null)
                    {
                        Debug.Log("class name ==>>> " + pbType.Name);
                        dynamic instance = Activator.CreateInstance(pbType);
                        instance = (instance as IMessage).Descriptor.Parser.ParseFrom(asset.bytes);
                        var json = JsonConvert.SerializeObject(instance);
                        Debug.Log(json);
                        var jsonPath = path.Replace(".bytes", ".json");
                        var jsonFile = File.CreateText(jsonPath);
                        jsonFile.Write(json.ToString());
                        jsonFile.Flush();
                        jsonFile.Close();
                        Debug.Log("转换成功");
                    }
                }
            }
            AssetDatabase.Refresh();
        }
    }
    
    //使用Google.Protobuf 转换
    [MenuItem("Assets/*将json文件导为 DB bytes")]
    public static void ExportJsonToBytes()
    {
        var assetGUIDs = Selection.assetGUIDs;
        if (assetGUIDs != null)
        {
            foreach (var guid in assetGUIDs)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var ext = Path.GetExtension(path);
                var fileName = Path.GetFileNameWithoutExtension(path);
                if (path.Contains("Assets/Res/Config") && ext.Equals(".json"))
                {
                    var pbFileName = "Pb." + fileName + "Config";
                    Debug.Log(pbFileName);
                    var typeList = GetTypes(fileName + "Config");
                    Type pbType = typeList[0]; 
                    var asset = AssetDatabase.LoadAssetAtPath<TextAsset>(path);
                    if (asset != null)
                    {
                        dynamic instance = Activator.CreateInstance(pbType);
                        instance = JsonConvert.DeserializeObject(asset.text, pbType);
                        int size = instance.CalculateSize();
                        Debug.Log(size);
                        byte[] byffer = new byte[size];
                        CodedOutputStream output = new CodedOutputStream(byffer);
                        instance.WriteTo(output);
                        var binPath = path.Replace(".json", ".bytes");
                        FileStream fs = new FileStream(binPath, FileMode.OpenOrCreate);
                        BinaryWriter w = new BinaryWriter(fs);
                        w.Write(byffer);
                        fs.Close();
                        Debug.Log("转换成功");
                    }
                }
            }
            AssetDatabase.Refresh();
        }
    }

    /// <summary>
    /// 获取类型
    /// </summary>
    /// <param name="className"></param>
    /// <returns></returns>
    private static List<Type> GetTypes(string className)
    {
        List<Type> res = new List<Type>();
        foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
        {
            var assemblyTypes = assembly.GetTypes();
            for (int i = 0; i < assemblyTypes.Length; i++)
            {
                if (assemblyTypes[i].Name == className)
                {
                    res.Add(assemblyTypes[i]);
                }
            }
        }
        return res;
    }
}
