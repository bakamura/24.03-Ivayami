using UnityEngine;
using UnityEditor;
using Ivayami.Enemy;

[CustomEditor(typeof(EnemyAnimator))]
public class EnemyAnimatorInspector : Editor
{
    SerializedProperty movementAnimationScaleWithMovementSpeed, walkSpeedFactor, chaseSpeedFactor, attackAnimationLayer, attackAnimationsSpeed;
    public override void OnInspectorGUI()
    {
        EditorGUILayout.PropertyField(movementAnimationScaleWithMovementSpeed, new GUIContent("Movement Animations Scale With Enemy Movement Speed"));
        if (movementAnimationScaleWithMovementSpeed.boolValue)
        {
            EditorGUILayout.PropertyField(walkSpeedFactor, new GUIContent("Walk Speed Factor", "Applies a percentage in the final value. Example: the enemy have a walk speed of 10 if the animation nedds to be played at half speed you set the value at .5"));
            EditorGUILayout.PropertyField(chaseSpeedFactor, new GUIContent("Chase Speed Factor", "Applies a percentage in the final value. Example: the enemy have a chase speed of 10 if the animation nedds to be played at half speed you set the value at .5"));
        }
        EditorGUILayout.PropertyField(attackAnimationLayer, new GUIContent("Attack Animation Layer"));
        EditorGUILayout.PropertyField(attackAnimationsSpeed, new GUIContent("Attack Animations Speed", "The speed of each attack animation in order that apears in the animator inspector"));
        serializedObject.ApplyModifiedProperties();
    }

    private void OnEnable()
    {
        movementAnimationScaleWithMovementSpeed = serializedObject.FindProperty("_movementAnimationScaleWithMovementSpeed");
        walkSpeedFactor = serializedObject.FindProperty("_walkSpeedFactor");
        chaseSpeedFactor = serializedObject.FindProperty("_chaseSpeedFactor");
        attackAnimationLayer = serializedObject.FindProperty("_attackAnimationLayer");
        attackAnimationsSpeed = serializedObject.FindProperty("_attackAnimationsSpeed");
    }
}
