using UnityEditor;

[CustomEditor(typeof(GroundGlass))]
public class GroundGlassEditor : Editor
{
    private GroundGlass mInstance;
    private void Awake()
    {
        mInstance = target as GroundGlass;
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        if (mInstance.mMatCreateShinerMask == null)
        {
            mInstance.CreateMaterial();
        }

        EditorGUILayout.LabelField("简化计算值");
        EditorGUILayout.PropertyField(serializedObject.FindProperty("mDownSample"));
        if (serializedObject.ApplyModifiedProperties())
        {
            mInstance.RecreateRenderTexture();
            mInstance.CreateBlurMask();
        }

        EditorGUILayout.LabelField("卷积计算次数，次数越多，模糊效果越好");
        EditorGUILayout.PropertyField(serializedObject.FindProperty("mInterations"));
        EditorGUILayout.LabelField("卷积计算的偏移大小，值太大会导致有色阶");
        EditorGUILayout.PropertyField(serializedObject.FindProperty("mBlurSize"));
        if (serializedObject.ApplyModifiedProperties())
        {
            mInstance.CreateBlurMask();
        }
    }
}