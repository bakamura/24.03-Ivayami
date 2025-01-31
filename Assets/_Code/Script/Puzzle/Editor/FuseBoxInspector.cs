#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.IO;
using UnityEngine.UI;
using System;
using Ivayami.Dialogue;

namespace Ivayami.Puzzle
{
    [CustomEditor(typeof(FuseBox))]
    public class FuseBoxInspector : Editor
    {
        SerializedProperty matrixDimensions, distanceBetweenLeds, fusesParent, fuseUIParent,
            fuseLayer, onInteract, onInteractionCancelled, onActivate, selectedColor, activatedColor, cancelInteractionInput, fusePrefab,
            ledsParent, fusesOffset, ledPrefab, elementsOffset, deactivatedColor;
        [Serializable]
        public class SaveSceneChanges
        {
            public Rect UIPanelRect;
            public Rect[] ButtonRects;
            public Vector3 CameraPos;
            public Quaternion CameraRot;
        }

        public override void OnInspectorGUI()
        {
            GUILayout.Label("INPUTS", EditorStyles.boldLabel);
            EditorGUILayout.Space(5);
            EditorGUILayout.PropertyField(cancelInteractionInput, new GUIContent("Exit Puzzle Input Reference"));
            EditorGUILayout.Space(10);

            GUILayout.Label("FUSE BOX SETTINGS", EditorStyles.boldLabel);
            EditorGUILayout.Space(5);
            EditorGUILayout.PropertyField(matrixDimensions, new GUIContent("Fuse Box Dimensions"));
            EditorGUILayout.PropertyField(distanceBetweenLeds, new GUIContent("Distance Between Leds"));
            EditorGUILayout.PropertyField(fusesOffset, new GUIContent("Fuse Container Offset"));
            EditorGUILayout.PropertyField(elementsOffset, new GUIContent("All Elements Offset"));
            EditorGUILayout.PropertyField(fuseLayer, new GUIContent("Fuse Collision Layer"));
            EditorGUILayout.PropertyField(selectedColor, new GUIContent("Fuse Selected Color"));
            EditorGUILayout.PropertyField(activatedColor, new GUIContent("Led Activated Color"));
            EditorGUILayout.PropertyField(deactivatedColor, new GUIContent("Led Deactivated Color"));
            EditorGUILayout.Space(10);

            GUILayout.Label("COMPONENTS", EditorStyles.boldLabel);
            EditorGUILayout.Space(5);
            EditorGUILayout.PropertyField(fuseUIParent, new GUIContent("Fuse UI Container"));
            EditorGUILayout.PropertyField(fusesParent, new GUIContent("Fuse Container"));
            EditorGUILayout.PropertyField(fusePrefab, new GUIContent("Fuse Prefab"));
            EditorGUILayout.PropertyField(ledsParent, new GUIContent("Led Container"));
            EditorGUILayout.PropertyField(ledPrefab, new GUIContent("Led Prefab"));
            EditorGUILayout.Space(10);

            GUILayout.Label("CALLBACKS", EditorStyles.boldLabel);
            EditorGUILayout.Space(5);
            EditorGUILayout.PropertyField(onInteract, new GUIContent("On Interact"));
            EditorGUILayout.PropertyField(onInteractionCancelled, new GUIContent("On Exit Interaction"));
            EditorGUILayout.PropertyField(onActivate, new GUIContent("On Activate"));

            FuseBox instance = (FuseBox)target;

            if (GUILayout.Button("Update Fuses"))
            {
                instance.CreateFuses();
                instance.RepositionFuses();
                instance.UpdateFusesActiveState();
                //instance.RenameObjects();
            }

            if (Application.isPlaying && GUILayout.Button("SaveUIChangesDuringPlay")) Save();
            if (!Application.isPlaying && File.Exists($"{Application.persistentDataPath}/{nameof(FuseBox)}ChangesDuringPlay") && GUILayout.Button("LoadUIChangesDuringPlay")) Load();

            serializedObject.ApplyModifiedProperties();
        }

        private void OnEnable()
        {
            matrixDimensions = serializedObject.FindProperty("_matrixDimensions");
            distanceBetweenLeds = serializedObject.FindProperty("_distanceBetweenLeds");
            fusesParent = serializedObject.FindProperty("_fusesParent");
            fuseUIParent = serializedObject.FindProperty("_fuseUIParent");
            fuseLayer = serializedObject.FindProperty("_fuseLayer");
            onInteract = serializedObject.FindProperty("_onInteract");
            onInteractionCancelled = serializedObject.FindProperty("_onInteractionCancelled");
            onActivate = serializedObject.FindProperty("onActivate");
            selectedColor = serializedObject.FindProperty("_selectedColor");
            activatedColor = serializedObject.FindProperty("_activatedColor");
            cancelInteractionInput = serializedObject.FindProperty("_cancelInteractionInput");
            fusePrefab = serializedObject.FindProperty("_fusePrefab");
            ledsParent = serializedObject.FindProperty("_ledsParent");
            fusesOffset = serializedObject.FindProperty("_fusesOffset");
            ledPrefab = serializedObject.FindProperty("_ledPrefab");
            elementsOffset = serializedObject.FindProperty("_elementsOffset");
            deactivatedColor = serializedObject.FindProperty("_deactivatedColor");
        }

        private void Save()
        {
            FuseBox instance = target as FuseBox;
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

            RectTransform rect = instance.GetComponentInChildren<CanvasGroup>().GetComponent<RectTransform>();
            changes.UIPanelRect = new Rect(rect.anchoredPosition, rect.rect.size);

            File.WriteAllText($"{Application.persistentDataPath}/{nameof(FuseBox)}ChangesDuringPlay", JsonUtility.ToJson(changes));
            Debug.Log($"Changes Saved for {nameof(FuseBox)}");
        }
        private void Load()
        {
            FuseBox instance = target as FuseBox;
            SaveSceneChanges changes = JsonUtility.FromJson<SaveSceneChanges>(File.ReadAllText($"{Application.persistentDataPath}/{nameof(FuseBox)}ChangesDuringPlay"));
            File.Delete($"{Application.persistentDataPath}/{nameof(FuseBox)}ChangesDuringPlay");

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

            RectTransform rect = instance.GetComponentInChildren<CanvasGroup>().GetComponent<RectTransform>();
            rect.anchoredPosition = changes.UIPanelRect.position;
            rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, changes.UIPanelRect.width);
            rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, changes.UIPanelRect.height);

            EditorUtility.SetDirty(instance);
            Debug.Log($"Changes Applied for {nameof(InteractableObjectsGroup)}");
        }
    }
}
#endif