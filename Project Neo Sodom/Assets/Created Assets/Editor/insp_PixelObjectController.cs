using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(scr_PixelObjectController))]
public class insp_PixelObjectController : Editor
{
    private SerializedObject obj_target = null;
    private scr_PixelObjectController pixelObjectController;

    private SerializedProperty isUnique = null;
    private SerializedProperty emission_color = null;
    private SerializedProperty emission_intensity = null;


    private void OnEnable()
    {
        pixelObjectController = (scr_PixelObjectController)target;
        obj_target = new SerializedObject(target);

        isUnique = obj_target.FindProperty("isUnique");
        emission_color = obj_target.FindProperty("emission_color");
        emission_intensity = obj_target.FindProperty("emission_intensity");
    }

    public override void OnInspectorGUI()
    {
        EditorGUI.BeginChangeCheck();

        //Uniqueness
        EditorGUILayout.PropertyField(isUnique, new GUIContent("Unique"));
        obj_target.ApplyModifiedProperties();

        if (EditorGUI.EndChangeCheck())
        {
            if (pixelObjectController.getUniqueness()) { pixelObjectController.makeMaterialUnique(); }
            else { pixelObjectController.makeMaterialUnified(); }
        }

        //If Unique
        if (pixelObjectController.getUniqueness())
        {
            EditorGUI.BeginChangeCheck();

            //Emission Color
            EditorGUILayout.PropertyField(emission_color, new GUIContent("Emission Color"));

            //Emission Intensity
            EditorGUILayout.Slider(emission_intensity, 0, 50, new GUIContent("Emission Intensity"));

            if (EditorGUI.EndChangeCheck())
            {
                pixelObjectController.setEissionColor(pixelObjectController.getEissionColor());
                pixelObjectController.setEmissionIntensity(pixelObjectController.getEmissionIntensity());
            }
        }
        //If not Unique
        else
        {
            EditorGUI.BeginChangeCheck();

            //Emission Color
            Material originalMaterial = pixelObjectController.getOriginalPixelObjectMaterial();
            Color newColor = EditorGUILayout.ColorField(new GUIContent("Public Emission Color"), originalMaterial.GetColor("_EmissionColor"), true, false, true); 
            originalMaterial.SetColor("_EmissionColor", newColor);
        }
    }
}
