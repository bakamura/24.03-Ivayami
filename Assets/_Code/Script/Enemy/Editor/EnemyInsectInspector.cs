using UnityEngine;
using UnityEditor;
using UnityEngine.AI;
using Ivayami.Enemy;

[CustomEditor(typeof(EnemyInsect))]
public class EnemyInsectInspector : Editor
{
    //Stress Entity variables
    SerializedProperty stressIncreaseTickFrequency, stressAreas, debugLogsStressEntity, drawGizmos;
    //Enemy Dog variables
    SerializedProperty behaviourTickFrequency, startActive, chaseSpeed, minDetectionRange, afterAttackCooldownDuration, targetLayer, blockVisionLayer,
        delayBetweenPatrolPoints, patrolPoints,
        stressIncreaseWhileChasing, stressMaxWhileChasing, attackAreaInfos, fuelRemoveFromLantern,
        debugLogsEnemyPatrol, drawMinDistance, minDistanceAreaColor, drawPatrolPoints, patrolPointsColor, patrolPointRadius, drawStoppingDistance, stoppingDistanceColor;
    private NavMeshAgent _navMeshAgent;
    private const float _space = 2;

    public override void OnInspectorGUI()
    {
        EditorGUI.indentLevel++;
        RenderHeader("Stress Entity Parameters", true);
        EditorGUILayout.PropertyField(stressIncreaseTickFrequency, new GUIContent("Stress Increase Tick Frequency"));
        EditorGUILayout.PropertyField(stressAreas, new GUIContent("Stress Areas"));

        RenderHeader("Stress Entity Debug", true);
        EditorGUILayout.PropertyField(debugLogsStressEntity, new GUIContent("Debug Logs"));
        EditorGUILayout.PropertyField(drawGizmos, new GUIContent("Draw Gizmos"));

        RenderHeader("Enemy Insect Basic Parameters", true);
        EditorGUILayout.PropertyField(behaviourTickFrequency, new GUIContent("Tick Frequency"));
        EditorGUILayout.PropertyField(startActive, new GUIContent("Start Active"));

        RenderHeader("Enemy Insect Detection Paramaters", true);
        EditorGUILayout.PropertyField(chaseSpeed, new GUIContent("Chase Speed"));
        EditorGUILayout.PropertyField(minDetectionRange, new GUIContent("Min Detection Range"));
        EditorGUILayout.PropertyField(targetLayer, new GUIContent("Target Detection Layer"));
        EditorGUILayout.PropertyField(blockVisionLayer, new GUIContent("Block Vision Layer"));
        EditorGUILayout.PropertyField(afterAttackCooldownDuration, new GUIContent("After Attack Cooldown Duration", "the time that will take to the enemy be able to attack again"));

        RenderHeader("Enemy Insect Behaviour Paramaters", true);
        if (patrolPoints.arraySize > 1) EditorGUILayout.PropertyField(delayBetweenPatrolPoints, new GUIContent("Delay Between Patrol Points"));
        EditorGUILayout.PropertyField(patrolPoints, new GUIContent("Patrol Points"));

        RenderHeader("Enemy Insect Attack Paramaters", true);
        EditorGUILayout.PropertyField(stressIncreaseWhileChasing, new GUIContent("Stress Increase While Chasing", "Stress per second increased by"));
        EditorGUILayout.PropertyField(stressMaxWhileChasing, new GUIContent("Stress Max While Chasing"));
        EditorGUILayout.PropertyField(fuelRemoveFromLantern, new GUIContent("Fuel Removed From Lantern On Attack"));
;        EditorGUILayout.PropertyField(attackAreaInfos, new GUIContent("Attacks Hitbox Info"));

        RenderHeader("Enemy Insect Debug", true);
        EditorGUILayout.PropertyField(debugLogsEnemyPatrol, new GUIContent("Debug Logs"));
        if (minDetectionRange.floatValue > 0)
        {
            EditorGUILayout.PropertyField(drawMinDistance, new GUIContent("Draw Min Distance"));
            if (drawMinDistance.boolValue) EditorGUILayout.PropertyField(minDistanceAreaColor, new GUIContent("Min Distance Gizmo Color"));
        }
        if (patrolPoints.arraySize > 0)
        {
            EditorGUILayout.PropertyField(drawPatrolPoints, new GUIContent("Draw Patrol Points"));
            if (drawPatrolPoints.boolValue)
            {
                EditorGUILayout.PropertyField(patrolPointsColor, new GUIContent("Patrol Points Gizmo Color"));
                EditorGUILayout.PropertyField(patrolPointRadius, new GUIContent("Patrol Points Gizmo Size"));
            }
        }
        if (_navMeshAgent.stoppingDistance > 0)
        {
            EditorGUILayout.PropertyField(drawStoppingDistance, new GUIContent("Draw Stop Distance"));
            if (drawStoppingDistance.boolValue) EditorGUILayout.PropertyField(stoppingDistanceColor, new GUIContent("Stop Distance Gizmo Color"));
        }
        EditorGUI.indentLevel--;

        serializedObject.ApplyModifiedProperties();
    }

    private void RenderHeader(string title, bool changeIndentLevel)
    {
        if (changeIndentLevel) EditorGUI.indentLevel--;
        EditorGUILayout.Space(_space);
        EditorGUILayout.LabelField(title, EditorStyles.boldLabel/*, style, GUILayout.ExpandWidth(true)*/);
        EditorGUILayout.Space(_space);
        if (changeIndentLevel) EditorGUI.indentLevel++;
    }

    private void OnEnable()
    {
        //Stress Entity
        stressIncreaseTickFrequency = serializedObject.FindProperty("_stressIncreaseTickFrequency");
        stressAreas = serializedObject.FindProperty("_stressAreas");
        debugLogsStressEntity = serializedObject.FindProperty("_debugLogsStressEntity");
        drawGizmos = serializedObject.FindProperty("_drawGizmos");
        //Enemy Insect
        minDetectionRange = serializedObject.FindProperty("_minDetectionRange");
        afterAttackCooldownDuration = serializedObject.FindProperty("_afterAttackCooldownDuration");
        delayBetweenPatrolPoints = serializedObject.FindProperty("_delayBetweenPatrolPoints");
        behaviourTickFrequency = serializedObject.FindProperty("_behaviourTickFrequency");
        stressIncreaseWhileChasing = serializedObject.FindProperty("_stressIncreaseWhileChasing");
        stressMaxWhileChasing = serializedObject.FindProperty("_stressMaxWhileChasing");
        fuelRemoveFromLantern = serializedObject.FindProperty("_fuelRemoveFromLantern");
        chaseSpeed = serializedObject.FindProperty("_chaseSpeed");
        startActive = serializedObject.FindProperty("_startActive");
        attackAreaInfos = serializedObject.FindProperty("_attackAreaInfos");
        targetLayer = serializedObject.FindProperty("_targetLayer");
        blockVisionLayer = serializedObject.FindProperty("_blockVisionLayer");
        patrolPoints = serializedObject.FindProperty("_patrolPoints");

        debugLogsEnemyPatrol = serializedObject.FindProperty("_debugLogsEnemyPatrol");
        drawMinDistance = serializedObject.FindProperty("_drawMinDistance");
        minDistanceAreaColor = serializedObject.FindProperty("_minDistanceAreaColor");
        drawPatrolPoints = serializedObject.FindProperty("_drawPatrolPoints");
        patrolPointsColor = serializedObject.FindProperty("_patrolPointsColor");
        drawStoppingDistance = serializedObject.FindProperty("_drawStoppingDistance");
        stoppingDistanceColor = serializedObject.FindProperty("_stoppingDistanceColor");
        patrolPointRadius = serializedObject.FindProperty("_patrolPointRadius");
        EnemyInsect instance = (EnemyInsect)target;
        _navMeshAgent = instance.GetComponent<NavMeshAgent>();
    }
}
