#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(EchoesLevelBootstrap))]
public class EchoesLevelBootstrapEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        var bootstrap = (EchoesLevelBootstrap)target;

        EditorGUILayout.Space();
        EditorGUILayout.HelpBox(
            "Procedural objects (Ground_Left, platforms, walls, pit) are created by this script. " +
            "If you do not see them in the Scene view, enable Build Layout In Edit Mode and use the button below after changing settings.",
            MessageType.Info);

        if (GUILayout.Button("Clear procedural objects & rebuild level"))
            bootstrap.EditorClearProceduralObjectsAndRebuild();
    }
}
#endif
