using UnityEditor;

[CustomEditor(typeof(UIParticleMask))]
class UIParticleMaskEditor : Editor
{

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        //EditorGUILayout.PropertyField(serializedObject.FindProperty("updateRectClipFrame"));
    }
}
