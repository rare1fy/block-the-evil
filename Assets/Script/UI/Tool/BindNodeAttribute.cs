using System;
using System.Reflection;
using UnityEngine;

[AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
public class BindNodeAttribute : Attribute
{
    public bool IsGameObject { get; private set; }

    public bool IsDebug { get; private set; }

    public string NodeName { get; private set; }

    public BindNodeAttribute(bool isGameObject = false, bool isDebug = true, string nodeName = "")
    {
        IsGameObject = isGameObject;
        IsDebug = isDebug;
        NodeName = nodeName;
    }
}

public static class NodeBinder
{
    public static void BindNodes(UIBase uIBase, MonoBehaviour target)
    {
        FieldInfo[] fields = target.GetType().GetFields(
            BindingFlags.Public |
            BindingFlags.NonPublic |
            BindingFlags.Instance);
        foreach (FieldInfo field in fields)
        {
            BindNodeAttribute attribute = field.GetCustomAttribute<BindNodeAttribute>();
            if (ReferenceEquals(attribute,null))
            {
                continue;
            }
            var path = !string.IsNullOrEmpty(attribute.NodeName) ? attribute.NodeName : field.Name;
            var gameObject = uIBase.GetNodeByName(path);
            if (ReferenceEquals(gameObject, null))
            {
                if (attribute.IsDebug)
                {
                    Debug.LogError($"节点名字不存在!!!!! {path}");
                }
                continue;
            }
            if (attribute.IsGameObject)
            {
                field.SetValue(target, gameObject);
            }
            else
            {
                if (field.FieldType.Name == "GameObject") 
                {
                    Debug.LogError($"节点绑定失败: 节点 {path} 上没有 {field.FieldType.Name} 组件");
                    continue;
                }
                Component component = gameObject.GetComponent(field.FieldType);
                if (ReferenceEquals(component, null))
                {
                    Debug.LogError($"节点绑定失败: 节点 {path} 上没有 {field.FieldType.Name} 组件");
                    continue;
                }
                field.SetValue(target, component);
            }
        }
    }
}
