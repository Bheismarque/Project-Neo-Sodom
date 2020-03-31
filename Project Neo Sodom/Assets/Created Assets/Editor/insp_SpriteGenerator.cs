using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(scr_2DObject),true)]
public class insp_SpriteGenerator : Editor
{
    private scr_2DObject obj;

    private SerializedObject obj_target = null;
    private SerializedProperty Sprite = null;


    private void OnEnable()
    {
        obj = (scr_2DObject)target;
        obj_target = new SerializedObject(target);
        Sprite = obj_target.FindProperty("spritePrefab");
    }


    public override void OnInspectorGUI()
    {
        obj_target.Update();
        EditorGUI.BeginChangeCheck();

        EditorGUILayout.PropertyField(Sprite, new GUIContent("Sprite"));
        obj_target.ApplyModifiedProperties();

        if ( EditorGUI.EndChangeCheck() )
        {
            obj.setSprite(obj.getSpritePrefab());
        }
    }
}
