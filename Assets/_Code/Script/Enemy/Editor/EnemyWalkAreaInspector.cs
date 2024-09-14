#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

namespace Ivayami.Enemy
{
    [CustomEditor(typeof(EnemyWalkArea))]
    public class EnemyWalkAreaInspector : Editor
    {
        SerializedProperty movementData, patrolBehaviour, points, delayToNextPoint, debugDraw, gizmoSize, debugColor;
        public override void OnInspectorGUI()
        {
            EditorGUILayout.PropertyField(movementData, new GUIContent("Movement Data"));
            EditorGUILayout.PropertyField(patrolBehaviour, new GUIContent("Patrol Behaviour"));
            if(patrolBehaviour.enumValueIndex > 0)
            {
                EditorGUILayout.PropertyField(points, new GUIContent("Patrol Points"));
            }
            else
            {
                EditorGUILayout.PropertyField(delayToNextPoint, new GUIContent("Delay To Next Point"));
            }            

            EditorGUILayout.PropertyField(debugDraw, new GUIContent("Draw Gizmos"));
            EditorGUILayout.PropertyField(gizmoSize, new GUIContent("Gizmos Size"));
            EditorGUILayout.PropertyField(debugColor, new GUIContent("Gizmos Color"));

            serializedObject.ApplyModifiedProperties();
        }

        private void OnEnable()
        {
            movementData = serializedObject.FindProperty("_movementData");
            patrolBehaviour = serializedObject.FindProperty("_patrolBehaviour");
            points = serializedObject.FindProperty("_points");
            delayToNextPoint = serializedObject.FindProperty("_delayToNextPoint");
            debugDraw = serializedObject.FindProperty("_debugDraw");
            gizmoSize = serializedObject.FindProperty("_gizmoSize");
            debugColor = serializedObject.FindProperty("_debugColor");
        }
    }
}
#endif