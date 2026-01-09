using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(UIBase), true)]
public class UIBaseEditor : Editor
{
	private const string RuntimeGoPattern = @"^(_)([a-zA-Z][^_]*)(_)(.*)";
	private bool _isCheckSameName = true;
    private UIBase _uiBase;

	public override void OnInspectorGUI()
	{
		_uiBase = serializedObject.targetObject as UIBase;
        _uiBase.IsOpenBlur = EditorGUILayout.Toggle("是否开启界面虚化效果", _uiBase.IsOpenBlur);
        _uiBase.IsNeedFullScreenSkew = EditorGUILayout.Toggle("是否需要全面屏偏移", _uiBase.IsNeedFullScreenSkew);
        //_isCheckSameName = EditorGUILayout.Toggle("是否检查包含相同名字", _isCheckSameName);
		if (GUILayout.Button("添加控制的UI元素"))
		{
			_WalkUiBaseNode(_uiBase, _isCheckSameName);

			EditorUtility.SetDirty(_uiBase);
			AssetDatabase.SaveAssets();
			AssetDatabase.Refresh();
		}
		base.OnInspectorGUI();
	}

	private static void _WalkUiBaseNode(UIBase uiBase, bool isCheckSameName)
	{
		var targetNodes = new List<GameObject>();
		_WalkNode(uiBase.transform, targetNodes, isCheckSameName);
		if (isCheckSameName)
		{
			var checkInfo = _IsContainsSameNameNode(targetNodes);
			if (checkInfo.value1)
			{
				var tipsInfo = string.Format("{0} -> 包含相同名字节点:{1}",
					uiBase.name,
					checkInfo.value2);
				EditorUtility.DisplayDialog(
					"Title",
					tipsInfo,
					"cancel");
				throw new System.Exception(tipsInfo);
			}
		}

		uiBase.TargetContainer = new UIBaseItem[targetNodes.Count];
		for (var i = 0; i < targetNodes.Count; i++)
		{
			var uiBaseItem = new UIBaseItem
			{
				goNode = targetNodes[i],
				goName = targetNodes[i].name
			};
			uiBase.TargetContainer[i] = uiBaseItem;
			if (ReferenceEquals(uiBaseItem.goNode, null))
			{
				Debug.LogErrorFormat("{0} error goNode is null ", uiBaseItem.goName);
			}
		}
	}

	private static void _WalkNode(Transform node, List<GameObject> targetNodes, bool isCheckSameName)
	{
		for (var i = 0; i < node.childCount; i++)
		{
			var child = node.GetChild(i);
			var childUiBase = child.GetComponent<UIBase>();
			if (ReferenceEquals(childUiBase, null) || !childUiBase.enabled)
			{
				_WalkNode(child, targetNodes, isCheckSameName);
			}
			else
			{
				if (Regex.Matches(child.name, RuntimeGoPattern).Count > 0)
				{
					targetNodes.Add(child.gameObject);
				}
				_WalkUiBaseNode(childUiBase, isCheckSameName);
			}
		}

		var nodeUiBase = node.GetComponent<UIBase>();
		if (!ReferenceEquals(nodeUiBase, null))
		{
			return;
		}
		if (Regex.Matches(node.name, RuntimeGoPattern).Count > 0)
		{
			targetNodes.Add(node.gameObject);
		}
	}

	private static Tuple<bool, string> _IsContainsSameNameNode(List<GameObject> targetNodes)
	{
		var names = new HashSet<string>();
		foreach (var node in targetNodes)
		{
			var nodeName = node.name;
			if (names.Contains(nodeName))
			{
				return new Tuple<bool, string>(true, nodeName);
			}
			names.Add(nodeName);
		}
		return new Tuple<bool, string>(false, string.Empty);
	}
}