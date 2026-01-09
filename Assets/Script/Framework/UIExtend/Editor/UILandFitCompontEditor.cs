using System;
using UnityEngine;
using UnityEditor;

/// <summary>
/// 横屏UI适配脚本
/// </summary>
[CustomEditor(typeof(UILandFitCompont))]
public class UILandFitCompontEditor : Editor
{
    private UILandFitCompont _adaptation;

    private GUIStyle _customStyle;

    void OnEnable()
    {
        _adaptation = serializedObject.targetObject as UILandFitCompont;
        _customStyle = new GUIStyle(EditorStyles.popup)
        {
            fontStyle = FontStyle.Bold,
            fontSize = 12,
            normal = { textColor = Color.green }
        };
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        EditorGUILayout.BeginVertical();

        _adaptation.adaptationType = (EUIAdaptationType)EditorGUILayout.EnumPopup("适配类型", _adaptation.adaptationType, _customStyle);
        switch (_adaptation.adaptationType)
        {
            case EUIAdaptationType.None:
                EditorGUILayout.HelpBox("确认是否需要挂载该脚本", MessageType.Warning, false);
                break;
            case EUIAdaptationType.Scale:
                _drawScaleGUI();
                break;
            case EUIAdaptationType.Cutout:
                _drawCutoutGUI(_customStyle);
                break;
            case EUIAdaptationType.Offset:
                _drawUIOffsetGUI();
                break;
            case EUIAdaptationType.ScaleCutout:
                _drawScaleGUI();
                GUILayout.Space(10);
                _drawCutoutGUI(_customStyle);
                break;
            case EUIAdaptationType.ScaleOffset:
                _drawScaleGUI();
                GUILayout.Space(10);
                _drawUIOffsetGUI();
                break;
            case EUIAdaptationType.CutoutOffset:
                _drawCutoutGUI(_customStyle);
                GUILayout.Space(10);
                _drawUIOffsetGUI();
                break;
            case EUIAdaptationType.ScaleCutoutOffset:
                _drawScaleGUI();
                GUILayout.Space(10);
                _drawCutoutGUI(_customStyle);
                GUILayout.Space(10);
                _drawUIOffsetGUI();
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }

        EditorGUILayout.EndVertical();

        if (_adaptation.adaptationType == EUIAdaptationType.Scale ||
            _adaptation.adaptationType == EUIAdaptationType.ScaleCutout ||
            _adaptation.adaptationType == EUIAdaptationType.ScaleOffset ||
            _adaptation.adaptationType == EUIAdaptationType.ScaleCutoutOffset)
        {
            _adaptation.UIScaleAdaptation();
        }

        if (_adaptation.adaptationType == EUIAdaptationType.Offset ||
                 _adaptation.adaptationType == EUIAdaptationType.ScaleOffset ||
                 _adaptation.adaptationType == EUIAdaptationType.CutoutOffset ||
                 _adaptation.adaptationType == EUIAdaptationType.ScaleCutoutOffset)
        {
            _adaptation.UIOffsetAdaptation();
        }

        Repaint();

        if (GUI.changed) serializedObject.ApplyModifiedProperties();
    }

    private void _drawScaleGUI()
    {
        var p = serializedObject.FindProperty("UIScaleData");
        EditorGUILayout.PropertyField(p, new GUIContent("缩放参数"), true);
    }

    private void _drawCutoutGUI(GUIStyle s)
    {
        _adaptation.adaptationOrientation = (UILandFitCompont.EUIAdaptationOrientation)EditorGUILayout.EnumPopup("适配方向", _adaptation.adaptationOrientation, s);
    }

    private void _drawUIOffsetGUI()
    {
        var p = serializedObject.FindProperty("UIOffsetData");
        EditorGUILayout.PropertyField(p, new GUIContent("偏移参数"), true);
    }
}
