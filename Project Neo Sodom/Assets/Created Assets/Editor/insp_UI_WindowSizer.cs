using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(UI_Element), true)]
public class insp_UI_WindowSizer : Editor
{
    private UI_Element windowSizer;

    private SerializedObject serializedTarget;


    private void Awake()
    {
        windowSizer = (UI_Element)target;
        serializedTarget = new SerializedObject(target);
    }


    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        // Scale Setting
        EditorGUI.BeginChangeCheck();
        float scale_x = EditorGUILayout.Slider("X Scale", windowSizer.getScale().x, 0, 2f);
        float scale_y = EditorGUILayout.Slider("Y Scale", windowSizer.getScale().y, 0, 2f);
        if (EditorGUI.EndChangeCheck())
        {
            windowSizer.setScale(new Vector2(scale_x, scale_y));
            windowSizer.resize();
        }

        if (GUILayout.Button("Reset"))
        {
            windowSizer.setScale(new Vector2(1, 1));
            windowSizer.resize();
        }
        if (GUILayout.Button("Set Original Scale")) 
        {
            windowSizer.registerUIWindowComponents();
            windowSizer.componentsSatisfied = true;
            windowSizer.setScale(new Vector2(1, 1));
            windowSizer.resize();
        }
    }
}
