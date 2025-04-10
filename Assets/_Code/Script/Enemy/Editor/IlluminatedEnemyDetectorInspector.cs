#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using Ivayami.Enemy;

[CustomEditor(typeof(IlluminatedEnemyDetector))]
public class IlluminatedEnemyDetectorInspector : Editor
{
    SerializedProperty lightBehaviour, finalSpeed, paraliseDuration, interpolateDuration, willInterruptAttack, enemyAnimator, paraliseAnimationRandomAmount, interpolateCurve, detectLightRange, /*checkLightTickFrequency,*/ gizmoColor, blockLayers;
    public override void OnInspectorGUI()
    {
        EditorGUILayout.LabelField("Basic Parameters", EditorStyles.boldLabel);
        EditorGUILayout.Space(1);
        EditorGUI.indentLevel++;
        EditorGUILayout.PropertyField(lightBehaviour, new GUIContent("Light Behaviour"));
        EditorGUILayout.PropertyField(detectLightRange, new GUIContent("Detection Light Range"));
        //EditorGUILayout.PropertyField(checkLightTickFrequency, new GUIContent("Check For Light Tick Frequency"));
        EditorGUILayout.PropertyField(blockLayers, new GUIContent("Light Block Layers"));
        EditorGUILayout.PropertyField(gizmoColor, new GUIContent("Gizmo Color"));
        EditorGUI.indentLevel--;

        if (lightBehaviour.enumValueIndex == 0)
        {
            EditorGUILayout.LabelField("Paralise", EditorStyles.boldLabel);
            EditorGUILayout.Space(1);
            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(enemyAnimator, new GUIContent("Enemy Animator"));
            EditorGUILayout.PropertyField(finalSpeed, new GUIContent("Final Speed"));
            EditorGUILayout.PropertyField(willInterruptAttack, new GUIContent("Will Interrupt Attack On Start", "The attack will be immediately interrupted when it enters in contact with light, works only if enemy has the paralise animation state"));
            EditorGUILayout.PropertyField(paraliseDuration, new GUIContent("Paralise Duration", "If value is 0 the enemy will stay still until all lights are away from it"));
            EditorGUILayout.PropertyField(interpolateDuration, new GUIContent("Slow Effect Interpolation Duration", "If value is 0 the enemy will have its speed change immediately"));
            EditorGUILayout.PropertyField(interpolateCurve, new GUIContent("Slow Effect Interpolation Curve"));
            EditorGUILayout.PropertyField(paraliseAnimationRandomAmount, new GUIContent("Paralise Animation Random", "The Range bettwen 0 and this value to select a random animation"));
            EditorGUI.indentLevel--;
        }
        serializedObject.ApplyModifiedProperties();
    }

    private void OnEnable()
    {
        lightBehaviour = serializedObject.FindProperty("_lightBehaviour");
        finalSpeed = serializedObject.FindProperty("_finalSpeed");
        paraliseDuration = serializedObject.FindProperty("_paraliseDuration");
        interpolateDuration = serializedObject.FindProperty("_interpolateDuration");
        willInterruptAttack = serializedObject.FindProperty("_willInterruptAttack");
        enemyAnimator = serializedObject.FindProperty("_enemyAnimator");
        paraliseAnimationRandomAmount = serializedObject.FindProperty("_paraliseAnimationRandomAmount");
        interpolateCurve = serializedObject.FindProperty("_interpolateCurve");
        detectLightRange = serializedObject.FindProperty("_detectLightRange");
        //checkLightTickFrequency = serializedObject.FindProperty("_checkLightTickFrequency");
        gizmoColor = serializedObject.FindProperty("_gizmoColor");
        blockLayers = serializedObject.FindProperty("_blockLayers");
    }
}
#endif