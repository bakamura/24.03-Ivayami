using UnityEngine;
using UnityEditor;
using System;
using System.IO;
using UnityEngine.UI;
using Ivayami.Dialogue;

namespace Ivayami.Puzzle
{
    [CustomEditor(typeof(InteractableObjectsGroup))]
    public class InteractableObjectsGroupInspector : Editor
    {
        SerializedProperty cancelInteractionInput, options, onInteract, onCancelInteraction;
        [Serializable]
        public class SaveSceneChanges
        {
            public Rect[] ButtonRects;
            public Vector3 CameraPos;
            public Quaternion CameraRot;
        }

        public override void OnInspectorGUI()
        {
            EditorGUILayout.PropertyField(cancelInteractionInput, new GUIContent("Cancel Interaction Input"));
            EditorGUILayout.PropertyField(options, new GUIContent("Buttons Configuration"));
            EditorGUILayout.PropertyField(onInteract, new GUIContent("On Interact"));
            EditorGUILayout.PropertyField(onCancelInteraction, new GUIContent("On Cancel Interaction"));

            if (Application.isPlaying && GUILayout.Button("SaveUIChangesDuringPlay")) Save();
            if (!Application.isPlaying && File.Exists($"{Application.persistentDataPath}/ChangesDuringPlay") && GUILayout.Button("LoadUIChangesDuringPlay")) Load();

            serializedObject.ApplyModifiedProperties();
        }

        private void OnEnable()
        {
            InteractableObjectsGroup instance = target as InteractableObjectsGroup;
            instance.UpdateButtonsArray();
            cancelInteractionInput = serializedObject.FindProperty("_cancelInteractionInput");
            options = serializedObject.FindProperty("_options");
            onInteract = serializedObject.FindProperty("_onInteract");
            onCancelInteraction = serializedObject.FindProperty("_onCancelInteraction");
        }

        private void Save()
        {
            InteractableObjectsGroup instance = target as InteractableObjectsGroup;
            SaveSceneChanges changes = new SaveSceneChanges();
            Button[] btns = instance.GetComponentInChildren<CanvasGroup>().GetComponentsInChildren<Button>();
            changes.ButtonRects = new Rect[btns.Length];

            RectTransform temp;
            for (int i = 0; i < changes.ButtonRects.Length; i++)
            {
                temp = btns[i].GetComponent<RectTransform>();
                changes.ButtonRects[i] = new Rect(temp.anchoredPosition, temp.rect.size);
            }
            Transform cameraTransform = instance.GetComponentInChildren<CameraAnimationInfo>().GetComponent<Transform>();
            changes.CameraPos = cameraTransform.localPosition;
            changes.CameraRot = cameraTransform.localRotation;

            File.WriteAllText($"{Application.persistentDataPath}/ChangesDuringPlay", JsonUtility.ToJson(changes));
            Debug.Log($"Changes Saved for {nameof(InteractableObjectsGroup)}");
        }
        private void Load()
        {
            InteractableObjectsGroup instance = target as InteractableObjectsGroup;
            SaveSceneChanges changes = JsonUtility.FromJson<SaveSceneChanges>(File.ReadAllText($"{Application.persistentDataPath}/ChangesDuringPlay"));
            File.Delete($"{Application.persistentDataPath}/ChangesDuringPlay");
            
            Button[] btns = instance.GetComponentInChildren<CanvasGroup>().GetComponentsInChildren<Button>();
            RectTransform temp;
            for (int i = 0; i < changes.ButtonRects.Length; i++)
            {
                temp = btns[i].GetComponent<RectTransform>();
                temp.anchoredPosition = changes.ButtonRects[i].position;
                temp.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, changes.ButtonRects[i].width);
                temp.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, changes.ButtonRects[i].height);
                //btns[i].GetComponent<RectTransform>().rect.Set(changes.Values[i].x, changes.Values[i].y, changes.Values[i].width, changes.Values[i].height);
            }
            Transform cameraTransform = instance.GetComponentInChildren<CameraAnimationInfo>().GetComponent<Transform>();
            cameraTransform.SetLocalPositionAndRotation(changes.CameraPos, changes.CameraRot);
            Debug.Log($"Changes Applied for {nameof(InteractableObjectsGroup)}");
        }
    }
}