#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using Ivayami.Enemy;

[CustomEditor(typeof(IluminatedEnemyDetector))]
public class IluminatedEnemyDetectorInspector : Editor
{
    SerializedProperty lightBehaviour, finalSpeed, paraliseDuration, interpolateDuration, enemyAnimator, paraliseAnimationRandomAmount, interpolateCurve,  detectLightRange, checkLightTickFrequency, gizmoColor;
    public override void OnInspectorGUI()
    {
        EditorGUILayout.PropertyField(lightBehaviour, new GUIContent("Light Behaviour"));
        //paralise
        if(lightBehaviour.enumValueIndex == 0)
        {
            EditorGUILayout.PropertyField(enemyAnimator, new GUIContent("Enemy Animator"));
            EditorGUILayout.PropertyField(finalSpeed, new GUIContent("Final Speed"));
            EditorGUILayout.PropertyField(paraliseDuration, new GUIContent("Paralise Duration", "If value is 0 the enemy will stay still until all lights are away from it"));
            EditorGUILayout.PropertyField(interpolateDuration, new GUIContent("Slow Effect Interpolation Duration", "If value is 0 the enemy will have its speed change immediately"));
            EditorGUILayout.PropertyField(interpolateCurve, new GUIContent("Slow Effect Interpolation Curve"));
            EditorGUILayout.PropertyField(paraliseAnimationRandomAmount, new GUIContent("Paralise Animation Random", "The Range bettwen 0 and this value to select a random animation"));
        }
        //follow light
        else if (lightBehaviour.enumValueIndex == 1)
        {
            EditorGUILayout.PropertyField(detectLightRange, new GUIContent("Detection Light Range"));
            EditorGUILayout.PropertyField(checkLightTickFrequency, new GUIContent("Check For Light Tick Frequency"));
            EditorGUILayout.PropertyField(gizmoColor, new GUIContent("Gizmo Color"));
        }
        serializedObject.ApplyModifiedProperties();
    }

    private void OnEnable()
    {
        lightBehaviour = serializedObject.FindProperty("_lightBehaviour");
        finalSpeed = serializedObject.FindProperty("_finalSpeed");
        paraliseDuration = serializedObject.FindProperty("_paraliseDuration");
        interpolateDuration = serializedObject.FindProperty("_interpolateDuration");
        enemyAnimator = serializedObject.FindProperty("_enemyAnimator");
        paraliseAnimationRandomAmount = serializedObject.FindProperty("_paraliseAnimationRandomAmount");
        interpolateCurve = serializedObject.FindProperty("_interpolateCurve");
        detectLightRange = serializedObject.FindProperty("_detectLightRange");
        checkLightTickFrequency = serializedObject.FindProperty("_checkLightTickFrequency");
        gizmoColor = serializedObject.FindProperty("_gizmoColor");
    }
}
#endif