using System;
using UnityEditor;
using UnityEngine;

namespace Ivayami.Puzzle
{
    [CustomEditor(typeof(RotatingPedestalPuzzle))]
    public class RotatingPedestalPuzzleInspector : Editor
    {
        SerializedProperty rotatingObjects, rotationAmount, rotationDuration, itemUsed, solutions,
            cancelInteractionInput, navigateUIInput, confirmInput,
            onInteract, onCancelInteraction, playNoSolutionEventOnceBySolution, onNoSolutionMeet;
        private int _solutionsArraySize;
        private static bool _solutionsFoldout;
        private static bool[] _possibleSolutionFoldout = new bool[1];
        private static bool[] _puzzleLayerFoldout = new bool[1];
        public override void OnInspectorGUI()
        {
            GUILayout.Label("BEHAVIOUR", EditorStyles.boldLabel);
            EditorGUILayout.Space(5);
            EditorGUILayout.PropertyField(rotationAmount, new GUIContent("Rotation Amount"));
            EditorGUILayout.PropertyField(rotationDuration, new GUIContent("Rotation Duration"));
            EditorGUILayout.PropertyField(playNoSolutionEventOnceBySolution, new GUIContent("Play no Solution event Once By Solution"));
            EditorGUILayout.PropertyField(itemUsed, new GUIContent("Item used"));
            EditorGUILayout.PropertyField(rotatingObjects, new GUIContent("Rotating Objects"));
            DrawSolutionVariable();
            EditorGUILayout.Space(10);

            GUILayout.Label("INPUTS", EditorStyles.boldLabel);
            EditorGUILayout.Space(5);
            EditorGUILayout.PropertyField(cancelInteractionInput, new GUIContent("Cancel Interaction Input"));
            EditorGUILayout.PropertyField(navigateUIInput, new GUIContent("Navigate UI Input"));
            EditorGUILayout.PropertyField(confirmInput, new GUIContent("Confirm Input"));
            EditorGUILayout.Space(10);

            GUILayout.Label("CALLBACKS", EditorStyles.boldLabel);
            EditorGUILayout.Space(5);
            EditorGUILayout.PropertyField(onInteract, new GUIContent("On Interact"));
            EditorGUILayout.PropertyField(onCancelInteraction, new GUIContent("On Cancel Interaction"));            
            EditorGUILayout.PropertyField(onNoSolutionMeet, new GUIContent("On No Solution Meet"));

            serializedObject.ApplyModifiedProperties();
        }

        private void DrawSolutionVariable()
        {
            _solutionsArraySize = EditorGUILayout.IntField("Solutions", _solutionsArraySize);
            solutions.arraySize = _solutionsArraySize;
            _solutionsFoldout = EditorGUILayout.Foldout(_solutionsFoldout, new GUIContent("Solutions"));
            if (_puzzleLayerFoldout.Length != _solutionsArraySize * rotatingObjects.arraySize) Array.Resize(ref _puzzleLayerFoldout, _solutionsArraySize * rotatingObjects.arraySize);
            if(_possibleSolutionFoldout.Length != solutions.arraySize) Array.Resize(ref _possibleSolutionFoldout, _solutionsArraySize);
            byte puzzleLayerFoldoutIndex = 0;
            Color defaultColor = GUI.backgroundColor;
            if (_solutionsFoldout)
            {
                for (int a = 0; a < solutions.arraySize; a++)
                {
                    EditorGUI.indentLevel = 1;
                    _possibleSolutionFoldout[a] = EditorGUILayout.Foldout(_possibleSolutionFoldout[a], new GUIContent($"Possible Solution {a}"));
                    if (_possibleSolutionFoldout[a])
                    {
                        for (int b = 0; b < solutions.GetArrayElementAtIndex(a).FindPropertyRelative("PuzzleLayer").arraySize; b++)
                        {
                            SerializedProperty puzzleLayer = solutions.GetArrayElementAtIndex(a).FindPropertyRelative("PuzzleLayer");
                            GUI.backgroundColor = rotatingObjects.GetArrayElementAtIndex(b).FindPropertyRelative("DebugColor").colorValue;
                            EditorGUI.indentLevel = 2;
                            _puzzleLayerFoldout[puzzleLayerFoldoutIndex] = EditorGUILayout.Foldout(_puzzleLayerFoldout[puzzleLayerFoldoutIndex], new GUIContent("PuzzleLayer"));
                            if (_puzzleLayerFoldout[puzzleLayerFoldoutIndex])
                            {
                                EditorGUI.indentLevel = 3;
                                EditorGUILayout.PropertyField(puzzleLayer.GetArrayElementAtIndex(b).FindPropertyRelative("Solution"), new GUIContent("Solution"));
                            }
                            puzzleLayerFoldoutIndex++;
                        }
                        GUI.backgroundColor = defaultColor;
                        EditorGUILayout.PropertyField(solutions.GetArrayElementAtIndex(a).FindPropertyRelative("OnActivate"));
                    }
                }
            }
            EditorGUI.indentLevel = 0;
        }

        private void OnEnable()
        {
            rotatingObjects = serializedObject.FindProperty("_rotatingObjects");
            rotationAmount = serializedObject.FindProperty("_rotationAmount");
            rotationDuration = serializedObject.FindProperty("_rotationDuration");
            itemUsed = serializedObject.FindProperty("_itemUsed");
            cancelInteractionInput = serializedObject.FindProperty("_cancelInteractionInput");
            navigateUIInput = serializedObject.FindProperty("_navigateUIInput");
            confirmInput = serializedObject.FindProperty("_confirmInput");
            onInteract = serializedObject.FindProperty("_onInteract");
            onCancelInteraction = serializedObject.FindProperty("_onCancelInteraction");
            playNoSolutionEventOnceBySolution = serializedObject.FindProperty("_playNoSolutionEventOnceBySolution");
            onNoSolutionMeet = serializedObject.FindProperty("_onNoSolutionMeet");
            solutions = serializedObject.FindProperty("_solutions");
            _solutionsArraySize = solutions.arraySize;
            Array.Resize(ref _possibleSolutionFoldout, _solutionsArraySize);
            Array.Resize(ref _puzzleLayerFoldout, _solutionsArraySize * rotatingObjects.arraySize);
        }
    }
}