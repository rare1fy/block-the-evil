using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;

[CustomEditor(typeof(LoopListView2), true)]
public class LoopListViewEditor2 : Editor
{
    SerializedProperty mItemSnapEnable;
    SerializedProperty mArrangeType;
    SerializedProperty mItemPrefabDataList;
    SerializedProperty mItemSnapPivot;
    SerializedProperty mViewPortSnapPivot;

    GUIContent mItemSnapEnableContent = new GUIContent("ItemSnapEnable");
    GUIContent mArrangeTypeGuiContent = new GUIContent("ArrangeType");
    GUIContent mItemPrefabListContent = new GUIContent("ItemPrefabList");
    GUIContent mItemSnapPivotContent = new GUIContent("ItemSnapPivot");
    GUIContent mViewPortSnapPivotContent = new GUIContent("ViewPortSnapPivot");

    //====================================
    SerializedProperty mIsGrid;
    SerializedProperty mIsVariable;
    SerializedProperty mItemCountPerCell;
    SerializedProperty mItemGroupSpacing;
    SerializedProperty mListStartOffset;
    SerializedProperty mListEndOffset;

    GUIContent mIsGridContent = new GUIContent("IsGrid", "是否是格子列表");
    GUIContent mIsVariableContent = new GUIContent("IsVariable", "是否自动适配个数");
    GUIContent mItemCountPerCellContent = new GUIContent("ItemCountPerCell");
    GUIContent mItemGroupSpacingContent = new GUIContent("ItemGroupSpacing");
    GUIContent mListStartOffsetContent = new GUIContent("ListStartOffset");
    GUIContent mListEndOffsetContent = new GUIContent("ListEndOffset");
    //====================================

    protected virtual void OnEnable()
    {
        mIsGrid = serializedObject.FindProperty("mIsGrid");
        mIsVariable = serializedObject.FindProperty("_isVariable");
        mItemCountPerCell = serializedObject.FindProperty("mItemCountPerCell");
        mItemGroupSpacing = serializedObject.FindProperty("mItemGroupSpacing");
        mListStartOffset = serializedObject.FindProperty("mListStartOffset");
        mListEndOffset = serializedObject.FindProperty("mListEndOffset");

        mItemSnapEnable = serializedObject.FindProperty("mItemSnapEnable");
        mArrangeType = serializedObject.FindProperty("mArrangeType");
        mItemPrefabDataList = serializedObject.FindProperty("mItemPrefabDataList");
        mItemSnapPivot = serializedObject.FindProperty("mItemSnapPivot");
        mViewPortSnapPivot = serializedObject.FindProperty("mViewPortSnapPivot");
    }

    void ShowItemPrefabDataList(LoopListView2 listView)
    {
        EditorGUILayout.PropertyField(mItemPrefabDataList, mItemPrefabListContent);
        if (mItemPrefabDataList.isExpanded == false)
        {
            return;
        }
        EditorGUI.indentLevel += 1;
        if (GUILayout.Button("Add New"))
        {
            mItemPrefabDataList.InsertArrayElementAtIndex(mItemPrefabDataList.arraySize);
            if (mItemPrefabDataList.arraySize > 0)
            {
                SerializedProperty itemData = mItemPrefabDataList.GetArrayElementAtIndex(mItemPrefabDataList.arraySize - 1);
                SerializedProperty mItemPrefab = itemData.FindPropertyRelative("mItemPrefab");
                mItemPrefab.objectReferenceValue = null;
            }
        }
        int removeIndex = -1;
        EditorGUILayout.PropertyField(mItemPrefabDataList.FindPropertyRelative("Array.size"));
        for (int i = 0; i < mItemPrefabDataList.arraySize; i++)
        {
            SerializedProperty itemData = mItemPrefabDataList.GetArrayElementAtIndex(i);
            SerializedProperty mInitCreateCount = itemData.FindPropertyRelative("mInitCreateCount");
            SerializedProperty mItemPrefabPadding = itemData.FindPropertyRelative("mPadding");
            SerializedProperty mItemStartPosOffset = itemData.FindPropertyRelative("mStartPosOffset");
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PropertyField(itemData);
            if (GUILayout.Button("Remove"))
            {
                removeIndex = i;
            }
            EditorGUILayout.EndHorizontal();
            if (itemData.isExpanded == false)
            {
                continue;
            }
            mItemPrefabPadding.floatValue = EditorGUILayout.FloatField("ItemPadding", mItemPrefabPadding.floatValue);
            if (listView.ArrangeType == ListItemArrangeType.TopToBottom || listView.ArrangeType == ListItemArrangeType.BottomToTop)
            {
                mItemStartPosOffset.floatValue = EditorGUILayout.FloatField("XPosOffset", mItemStartPosOffset.floatValue);
            }
            else
            {
                mItemStartPosOffset.floatValue = EditorGUILayout.FloatField("YPosOffset", mItemStartPosOffset.floatValue);
            }
            mInitCreateCount.intValue = EditorGUILayout.IntField("InitCreateCount", mInitCreateCount.intValue);
            EditorGUILayout.Space();
            EditorGUILayout.Space();
        }
        if (removeIndex >= 0)
        {
            mItemPrefabDataList.DeleteArrayElementAtIndex(removeIndex);
        }
        EditorGUI.indentLevel -= 1;
    }

    public override void OnInspectorGUI()
    {
        //base.OnInspectorGUI();
        serializedObject.Update();
        LoopListView2 tListView = serializedObject.targetObject as LoopListView2;
        if (tListView == null)
        {
            return;
        }
        ShowItemPrefabDataList(tListView);
        // Add Start
        EditorGUILayout.Space();
        EditorGUILayout.PropertyField(mListStartOffset, mListStartOffsetContent);
        EditorGUILayout.PropertyField(mListEndOffset, mListEndOffsetContent);
        EditorGUILayout.Space();
        EditorGUILayout.PropertyField(mIsGrid, mIsGridContent);
        if (mIsGrid.boolValue)
        {
            EditorGUILayout.PropertyField(mItemGroupSpacing, mItemGroupSpacingContent);
            EditorGUILayout.Space();
            EditorGUILayout.PropertyField(mIsVariable, mIsVariableContent);
            if (!mIsVariable.boolValue)
            {
                EditorGUILayout.PropertyField(mItemCountPerCell, mItemCountPerCellContent);
            }
        }
        // Add End
        EditorGUILayout.Space();
        //==================================临时处理后续优化==============================================
        EditorGUILayout.PropertyField(mItemSnapEnable, mItemSnapEnableContent);
        if (mItemSnapEnable.boolValue)
        {
            EditorGUILayout.PropertyField(mItemSnapPivot, mItemSnapPivotContent);
            EditorGUILayout.PropertyField(mViewPortSnapPivot, mViewPortSnapPivotContent);
        }
        EditorGUILayout.PropertyField(mArrangeType, mArrangeTypeGuiContent);

        var ll = target as LoopListView2;
        ScrollRect list = ll != null ? ll.GetComponent<ScrollRect>() : null;
        if (list != null && list.content != null)
        {
            var lg = list.content.GetComponent<LayoutGroup>();
            if (lg != null && GUILayout.Button("复制Content数据"))
            {
                var t = ll.GetType();
                var itemList = t.GetField("mItemPrefabDataList", BindingFlags.Instance | BindingFlags.NonPublic);
                if (itemList != null)
                {
                    var prefabList = itemList.GetValue(ll);
                    if (prefabList != null)
                    {
                        var realList = prefabList as List<ItemPrefabConfData>;
                        if (realList != null && realList.Count > 0)
                        {
                            if (realList.Count == 1)
                            {
                                // todo 贴左 l to r 或者 贴上 t to b 只处理这些
                                var isV = ll.ArrangeType == ListItemArrangeType.BottomToTop ||
                                          ll.ArrangeType == ListItemArrangeType.TopToBottom;
                                var prefab = realList[0];
                                prefab.mStartPosOffset = isV ? lg.padding.left : lg.padding.top;
                                var StartOffset = t.GetField("mListStartOffset", BindingFlags.Instance | BindingFlags.NonPublic);
                                if (StartOffset != null)
                                    StartOffset.SetValue(ll, isV ? lg.padding.top : lg.padding.left);
                                var EndOffset = t.GetField("mListEndOffset", BindingFlags.Instance | BindingFlags.NonPublic);
                                if (EndOffset != null)
                                    EndOffset.SetValue(ll, isV ? lg.padding.bottom : lg.padding.right);

                                if (lg is GridLayoutGroup)
                                {
                                    var o = prefab.mItemPrefab;
                                    if (o)
                                    {
                                        var rect = o.transform as RectTransform;
                                        if (rect)
                                        {
                                            var glg = lg as GridLayoutGroup;
                                            if (rect.rect.height != glg.cellSize.y || rect.rect.width != glg.cellSize.x)
                                            {
                                                EditorUtility.DisplayDialog("提示", "布局组件的cellSize错误，不是prefab的宽高"
                                                    + rect.rect.height + "   " + rect.rect.width, "确认");
                                            }
                                            else
                                            {
                                                prefab.mPadding = isV ? glg.spacing.y : glg.spacing.x;
                                                var isGrid = t.GetField("mIsGrid", BindingFlags.Instance | BindingFlags.NonPublic);
                                                if (isGrid != null && (bool)isGrid.GetValue(ll) && !prefab.mIsItemGroup)
                                                {
                                                    var igs = t.GetField("mItemGroupSpacing", BindingFlags.Instance | BindingFlags.NonPublic);
                                                    if (igs != null)
                                                    {
                                                        igs.SetValue(ll, isV ? glg.spacing.x : glg.spacing.y);
                                                    }
                                                }
                                            }
                                        }
                                        else
                                        {
                                            EditorUtility.DisplayDialog("提示", "prefab没有RectTransform组件", "确认");
                                        }
                                    }
                                    else
                                    {
                                        EditorUtility.DisplayDialog("提示", "加载prefab错误", "确认");
                                    }
                                }
                                else if (lg is HorizontalOrVerticalLayoutGroup)
                                {
                                    prefab.mPadding = (lg as HorizontalOrVerticalLayoutGroup).spacing;
                                }
                                else
                                {
                                    EditorUtility.DisplayDialog("提示", "未知布局", "确认");
                                }
                            }
                            else
                            {
                                EditorUtility.DisplayDialog("提示", "复制不支持多预制", "确认");
                            }
                        }
                        else
                        {
                            EditorUtility.DisplayDialog("提示", "预制不正确", "确认");
                        }
                    }
                }
            }
            if (lg != null)
                EditorGUILayout.HelpBox("布局组件预览需要将content铺满viewport", MessageType.Info);
        }

        serializedObject.ApplyModifiedProperties();
    }
}
