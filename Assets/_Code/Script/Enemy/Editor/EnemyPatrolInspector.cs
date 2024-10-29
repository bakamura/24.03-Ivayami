using UnityEngine;
using UnityEditor;
using UnityEngine.AI;

namespace Ivayami.Enemy
{
    [CustomEditor(typeof(EnemyPatrol))]
    public class EnemyPatrolInspector : Editor
    {
        //Stress Entity variables
        SerializedProperty stressIncreaseTickFrequency, stressAreas, debugLogsStressEntity, drawGizmos;
        //Enemy Patrol variables
        SerializedProperty minDetectionRange, minDetectionRangeInChase, detectionRange, delayToLoseTarget, visionAngle, visionOffset, delayBetweenPatrolPoints, delayToStopSearchTarget, delayToFinishTargetSearch, behaviourTickFrequency, stressIncreaseOnTargetDetected,
            stressIncreaseWhileChasing, stressMaxWhileChasing, chaseSpeed, startActive, goToLastTargetPosition, attackTarget, attackAreaInfos, loseTargetWhenHidden, targetLayer, blockVisionLayer, patrolPoints,
            debugLogsEnemyPatrol, drawMinDistance, minDistanceAreaColor, drawMinDistanceInChase, minDistanceInChaseAreaColor, drawDetectionRange, detectionRangeAreaColor, drawPatrolPoints, patrolPointsColor, drawStoppingDistance, stoppingDistanceColor, patrolPointRadius;
        private NavMeshAgent _navMeshAgent;
        private const float _space = 2;
        public override void OnInspectorGUI()
        {
            //GUIStyle style = new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter, fontStyle = FontStyle.Bold, fontSize = 12 };
            EditorGUI.indentLevel++;
            RenderHeader("Stress Entity Parameters", true);
            EditorGUILayout.PropertyField(stressIncreaseTickFrequency, new GUIContent("Stress Increase Tick Frequency"));
            EditorGUILayout.PropertyField(stressAreas, new GUIContent("Stress Areas"));

            RenderHeader("Stress Entity Debug", true);
            EditorGUILayout.PropertyField(debugLogsStressEntity, new GUIContent("Debug Logs"));
            EditorGUILayout.PropertyField(drawGizmos, new GUIContent("Draw Gizmos"));

            RenderHeader("Enemy Patrol Basic Parameters", true);
            EditorGUILayout.PropertyField(behaviourTickFrequency, new GUIContent("Tick Frequency"));
            EditorGUILayout.PropertyField(startActive, new GUIContent("Start Active"));

            RenderHeader("Enemy Patrol Detection Paramaters", true);
            EditorGUILayout.PropertyField(chaseSpeed, new GUIContent("Chase Speed"));
            EditorGUILayout.PropertyField(detectionRange, new GUIContent("Cone Detection Range"));
            EditorGUILayout.PropertyField(minDetectionRange, new GUIContent("Min Detection Range"));
            EditorGUILayout.PropertyField(minDetectionRangeInChase, new GUIContent("Min Detection Range In Chase"));
            EditorGUILayout.PropertyField(visionAngle, new GUIContent("Cone Vision Angle"));
            EditorGUILayout.PropertyField(visionOffset, new GUIContent("Cone Vision Offset"));
            EditorGUILayout.PropertyField(targetLayer, new GUIContent("Target Detection Layer"));
            EditorGUILayout.PropertyField(blockVisionLayer, new GUIContent("Block Vision Layer"));

            RenderHeader("Enemy Patrol Behaviour Paramaters", true);
            EditorGUILayout.PropertyField(goToLastTargetPosition, new GUIContent("Go To last Target Position", "Enemy will go to last target position on target lost"));
            EditorGUILayout.PropertyField(loseTargetWhenHidden, new GUIContent("Lose Target When Target Hidden", "Will instantly lose target"));            
            EditorGUILayout.PropertyField(delayToLoseTarget, new GUIContent("Delay To Lose Target"));
            if (goToLastTargetPosition.boolValue)
            {
                EditorGUILayout.PropertyField(delayToStopSearchTarget, new GUIContent("Delay To Stop Searching For Target", "after this amount of time the enemy will stop going to the last point of the target"));
                EditorGUILayout.PropertyField(delayToFinishTargetSearch, new GUIContent("Delay To Finish Target Search", "time to exit from the GoToLastTarget state"));
            }            
            if (patrolPoints.arraySize > 1) EditorGUILayout.PropertyField(delayBetweenPatrolPoints, new GUIContent("Delay Between Patrol Points"));
            EditorGUILayout.PropertyField(patrolPoints, new GUIContent("Patrol Points"));

            RenderHeader("Enemy Patrol Attack Paramaters", true);
            EditorGUILayout.PropertyField(stressIncreaseOnTargetDetected, new GUIContent("Stress Increase On Target Detected"));
            EditorGUILayout.PropertyField(stressIncreaseWhileChasing, new GUIContent("Stress Increase While Chasing"));
            EditorGUILayout.PropertyField(stressMaxWhileChasing, new GUIContent("Stress Max While Chasing"));
            EditorGUILayout.PropertyField(attackTarget, new GUIContent("Attack Target", "Attack range is defined by Stopping Distance + Target Collider Extent Z"));
            if (attackTarget.boolValue) EditorGUILayout.PropertyField(attackAreaInfos, new GUIContent("Attacks Hitbox Info"));

            RenderHeader("Enemy Patrol Debug", true);
            EditorGUILayout.PropertyField(debugLogsEnemyPatrol, new GUIContent("Debug Logs"));
            if (minDetectionRange.floatValue > 0)
            {
                EditorGUILayout.PropertyField(drawMinDistance, new GUIContent("Draw Min Distance"));
                if (drawMinDistance.boolValue) EditorGUILayout.PropertyField(minDistanceAreaColor, new GUIContent("Min Distance Gizmo Color"));
            }
            if (minDetectionRangeInChase.floatValue > 0)
            {
                EditorGUILayout.PropertyField(drawMinDistanceInChase, new GUIContent("Draw Min Distance In Chase"));
                if (drawMinDistanceInChase.boolValue) EditorGUILayout.PropertyField(minDistanceInChaseAreaColor, new GUIContent("Min Distance In Chase Gizmo Color"));
            }
            if (detectionRange.floatValue > 0)
            {
                EditorGUILayout.PropertyField(drawDetectionRange, new GUIContent("Draw Detection Range"));
                if (drawDetectionRange.boolValue) EditorGUILayout.PropertyField(detectionRangeAreaColor, new GUIContent("Cone Detection Range Gizmo Color"));
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
            //Enemy Patrol
            minDetectionRange = serializedObject.FindProperty("_minDetectionRange");
            minDetectionRangeInChase = serializedObject.FindProperty("_minDetectionRangeInChase");
            detectionRange = serializedObject.FindProperty("_detectionRange");
            delayToLoseTarget = serializedObject.FindProperty("_delayToLoseTarget");
            visionAngle = serializedObject.FindProperty("_visionAngle");
            visionOffset = serializedObject.FindProperty("_visionOffset");
            delayBetweenPatrolPoints = serializedObject.FindProperty("_delayBetweenPatrolPoints");
            delayToStopSearchTarget = serializedObject.FindProperty("_delayToStopSearchTarget");
            delayToFinishTargetSearch = serializedObject.FindProperty("_delayToFinishTargetSearch");
            behaviourTickFrequency = serializedObject.FindProperty("_behaviourTickFrequency");
            stressIncreaseOnTargetDetected = serializedObject.FindProperty("_stressIncreaseOnTargetDetected");
            stressIncreaseWhileChasing = serializedObject.FindProperty("_stressIncreaseWhileChasing");
            stressMaxWhileChasing = serializedObject.FindProperty("_stressMaxWhileChasing");
            chaseSpeed = serializedObject.FindProperty("_chaseSpeed");
            startActive = serializedObject.FindProperty("_startActive");
            goToLastTargetPosition = serializedObject.FindProperty("_goToLastTargetPosition");
            attackTarget = serializedObject.FindProperty("_attackTarget");
            attackAreaInfos = serializedObject.FindProperty("_attackAreaInfos");
            loseTargetWhenHidden = serializedObject.FindProperty("_loseTargetWhenHidden");
            targetLayer = serializedObject.FindProperty("_targetLayer");
            blockVisionLayer = serializedObject.FindProperty("_blockVisionLayer");
            patrolPoints = serializedObject.FindProperty("_patrolPoints");

            debugLogsEnemyPatrol = serializedObject.FindProperty("_debugLogsEnemyPatrol");
            drawMinDistance = serializedObject.FindProperty("_drawMinDistance");
            minDistanceAreaColor = serializedObject.FindProperty("_minDistanceAreaColor");
            drawMinDistanceInChase = serializedObject.FindProperty("_drawMinDistanceInChase");
            minDistanceInChaseAreaColor = serializedObject.FindProperty("_minDistanceInChaseAreaColor");
            drawDetectionRange = serializedObject.FindProperty("_drawDetectionRange");
            detectionRangeAreaColor = serializedObject.FindProperty("_detectionRangeAreaColor");
            drawPatrolPoints = serializedObject.FindProperty("_drawPatrolPoints");
            patrolPointsColor = serializedObject.FindProperty("_patrolPointsColor");
            drawStoppingDistance = serializedObject.FindProperty("_drawStoppingDistance");
            stoppingDistanceColor = serializedObject.FindProperty("_stoppingDistanceColor");
            patrolPointRadius = serializedObject.FindProperty("_patrolPointRadius");
            EnemyPatrol instance = (EnemyPatrol)target;
            _navMeshAgent = instance.GetComponent<NavMeshAgent>();
        }
    }
}