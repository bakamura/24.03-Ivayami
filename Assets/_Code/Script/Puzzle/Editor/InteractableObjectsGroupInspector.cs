using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
using System.IO;
using UnityEngine.UI;

namespace Ivayami.Puzzle
{
    [CustomEditor(typeof(InteractableObjectsGroup))]
    public class InteractableObjectsGroupInspector : Editor
    {
        SerializedProperty cancelInteractionInput, options, onInteract, onCancelInteraction;
        [Serializable]
        public class ButtonChanges
        {
            public Rect[] Values;
        }

        public override void OnInspectorGUI()
        {
            EditorGUILayout.PropertyField(cancelInteractionInput, new GUIContent("Cancel Interaction Input"));
            EditorGUILayout.PropertyField(options, new GUIContent("Buttons COnfiguration"));
            EditorGUILayout.PropertyField(onInteract, new GUIContent("On Interact"));
            EditorGUILayout.PropertyField(onCancelInteraction, new GUIContent("On Cancel Interaction"));

            if (Application.isPlaying && GUILayout.Button("SaveUIChangesDuringPlay")) Save();
            if (!Application.isPlaying && GUILayout.Button("LoadUIChangesDuringPlay")) Load();

            serializedObject.ApplyModifiedProperties();
        }

        private void OnEnable()
        {
            cancelInteractionInput = serializedObject.FindProperty("_cancelInteractionInput");
            options = serializedObject.FindProperty("_options");
            onInteract = serializedObject.FindProperty("_onInteract");
            onCancelInteraction = serializedObject.FindProperty("_onCancelInteraction");
        }

        private void Save()
        {
            if (!Application.isPlaying) return;
            InteractableObjectsGroup instance = target as InteractableObjectsGroup;
            Button[] btns = instance.GetComponentInChildren<CanvasGroup>().GetComponentsInChildren<Button>();
            ButtonChanges changes = new ButtonChanges();
            changes.Values = new Rect[btns.Length];
            RectTransform temp;
            for (int i = 0; i < changes.Values.Length; i++)
            {
                temp = btns[i].GetComponent<RectTransform>();
                changes.Values[i] = new Rect(temp.anchoredPosition, temp.rect.size);
            }
            File.WriteAllText($"{Application.persistentDataPath}/ChangesDuringPlay", JsonUtility.ToJson(changes));
            Debug.Log($"Changes Saved for {nameof(InteractableObjectsGroup)}");
        }
        private void Load()
        {
            if (Application.isPlaying || !File.Exists($"{Application.persistentDataPath}/ChangesDuringPlay")) return;
            InteractableObjectsGroup instance = target as InteractableObjectsGroup;
            Button[] btns = instance.GetComponentInChildren<CanvasGroup>().GetComponentsInChildren<Button>();
            ButtonChanges changes = JsonUtility.FromJson<ButtonChanges>(File.ReadAllText($"{Application.persistentDataPath}/ChangesDuringPlay"));
            File.Delete($"{Application.persistentDataPath}/ChangesDuringPlay");
            RectTransform temp;
            for (int i = 0; i < changes.Values.Length; i++)
            {
                temp = btns[i].GetComponent<RectTransform>();
                temp.anchoredPosition = changes.Values[i].position;
                temp.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, changes.Values[i].width);
                temp.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, changes.Values[i].height);
                //btns[i].GetComponent<RectTransform>().rect.Set(changes.Values[i].x, changes.Values[i].y, changes.Values[i].width, changes.Values[i].height);
            }
            Debug.Log($"Changes Applied for {nameof(InteractableObjectsGroup)}");
        }
    }
}