using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using UnityEngine.Events;
using Ivayami.Save;
using System;

namespace Ivayami.Scene
{
    public class SceneController : MonoSingleton<SceneController>
    {
        [SceneDropdown, SerializeField] private string _mainMenuSceneName;
        [SerializeField] private bool _debugLogs;

        private Dictionary<string, int> _chapterPointers;
        private List<SceneData> _sceneList = new List<SceneData>();
        private Queue<SceneUpdateRequestData> _sceneUpdateRequests = new Queue<SceneUpdateRequestData>();
        private bool _canUpdateScenes = true;

        public Action OnAllSceneRequestEnd;
        public Action<string> OnLoadScene;
        public Action<string> OnUnloadScene;
        public Action<string> OnStartUnloadScene;
        public Action<string> OnStartLoadScene;
#if UNITY_EDITOR
        public Action OnAllSceneRequestEndDebug;
#endif

        [Serializable]
        private class SceneData
        {
            public string SceneName;
            public bool IsLoaded;
            public bool IsBeingLoaded;

            public SceneData(string sceneName)
            {
                SceneName = sceneName;
                IsBeingLoaded = false;
                IsLoaded = false;
            }
        }

        [Serializable]
        private struct SceneUpdateRequestData
        {
            public SceneData SceneData;
            public UnityEvent OnSceneUpdate;

            public SceneUpdateRequestData(SceneData data, UnityEvent onSceneUpdate)
            {
                SceneData = data;
                OnSceneUpdate = onSceneUpdate;
            }
        }

        protected override void Awake()
        {
            base.Awake();

            _chapterPointers = new Dictionary<string, int>();
            foreach (ChapterPointers chapterPointer in Resources.LoadAll<ChapterPointers>("ChapterPointers"))
            {
                _chapterPointers.Add(chapterPointer.name, chapterPointer.GetInstanceID());
            }
            Resources.UnloadUnusedAssets();
        }

        private void Start()
        {
            LoadMainMenuScene();
        }

        public void LoadMainMenuScene()
        {
            if (!string.IsNullOrEmpty(_mainMenuSceneName))
            {
#if UNITY_EDITOR
                if (Ivayami.debug.CustomSettingsHandler.GetEditorSettings().StartOnCurrentScene && !string.IsNullOrEmpty(Ivayami.debug.CustomSettingsHandler.CurrentSceneName))
                {
                    OnAllSceneRequestEndDebug += Ivayami.debug.CustomSettingsHandler.OnSceneLoad;
                    _mainMenuSceneName = Ivayami.debug.CustomSettingsHandler.CurrentSceneName;
                }
#endif
                LoadScene(_mainMenuSceneName);
            }
        }

        public void LoadScene(string sceneId, UnityEvent onSceneLoad = null)
        {
            SceneData data = UpdateSceneList(sceneId);
            if (data.IsBeingLoaded || data.IsLoaded)
            {
                Debug.LogWarning($"Tried to Load scene {sceneId} but it is already loaded");
                return;
            }
            OnStartLoadScene?.Invoke(sceneId);
            StartLoad(data, onSceneLoad);
        }

        public void UnloadScene(string sceneId, UnityEvent onSceneUnload = null)
        {
            SceneData data = UpdateSceneList(sceneId);
            if (data.IsBeingLoaded || !data.IsLoaded)
            {
                Debug.LogWarning($"Tried to Unload scene {sceneId} but it is already Unloaded");
                return;
            }
            OnStartUnloadScene?.Invoke(sceneId);
            StartLoad(data, onSceneUnload);
        }

        private void StartLoad(SceneData sceneData, UnityEvent onSceneUpdate = null)
        {
            sceneData.IsBeingLoaded = true;
            _sceneUpdateRequests.Enqueue(new SceneUpdateRequestData(sceneData, onSceneUpdate));
            if (_sceneUpdateRequests.Count == 1) UpdateScene(sceneData);
        }

        public void UnloadAllScenes(Action onAllScenesUnload)
        {
            OnAllSceneRequestEnd += onAllScenesUnload;
            for (int i = 0; i < _sceneList.Count; i++)
            {
                if (_sceneList[i].IsLoaded) UnloadScene(_sceneList[i].SceneName);
            }
        }

        private void UpdateScene(SceneData data)
        {
            if (_canUpdateScenes)
            {
                if (_debugLogs)
                {
                    if (data.IsLoaded) Debug.Log($"Unloading Scene {data.SceneName}");
                    else Debug.Log($"Loading Scene {data.SceneName}");
                }
                AsyncOperation operation;
                if (data.IsLoaded) operation = SceneManager.UnloadSceneAsync(data.SceneName);
                else operation = SceneManager.LoadSceneAsync(data.SceneName, LoadSceneMode.Additive);
                if (operation != null) operation.completed += HandleOnSceneUpdate;
            }
        }

        private SceneData UpdateSceneList(string sceneName)
        {
            for (int i = 0; i < _sceneList.Count; i++)
            {
                if (_sceneList[i].SceneName == sceneName)
                {
                    return _sceneList[i];
                }
            }
            SceneData temp = new SceneData(sceneName);
            _sceneList.Add(temp);
            return temp;
        }

        public Vector2 PointerInChapter(string chapterId)
        {
            ChapterPointers chapterPointer = (ChapterPointers)Resources.InstanceIDToObject(_chapterPointers[chapterId]);
            Vector2 pointer = chapterPointer.SubChapterPointer((byte)SaveSystem.Instance.Progress.GetProgressOfType(chapterId));
            Resources.UnloadAsset(chapterPointer);
            return pointer;
        }

        private void HandleOnSceneUpdate(AsyncOperation operation)
        {
            SceneUpdateRequestData requestData = _sceneUpdateRequests.Dequeue();
            requestData.SceneData.IsLoaded = !requestData.SceneData.IsLoaded;
            requestData.SceneData.IsBeingLoaded = false;

            if (requestData.SceneData.IsLoaded)
            {
                OnLoadScene?.Invoke(requestData.SceneData.SceneName);
                if (_debugLogs) Debug.Log($"Scene Loaded {requestData.SceneData.SceneName}");
            }
            else
            {
                OnUnloadScene?.Invoke(requestData.SceneData.SceneName);
                if (_debugLogs) Debug.Log($"Scene Unloaded {requestData.SceneData.SceneName}");
            }

            requestData.OnSceneUpdate?.Invoke();

            if (_sceneUpdateRequests.Count > 0) UpdateScene(_sceneUpdateRequests.Peek().SceneData);
            else
            {
                if (_debugLogs) Debug.Log("AllSceneRequestEnd Callback");
                OnAllSceneRequestEnd?.Invoke();
#if UNITY_EDITOR
                StartCoroutine(WaitEndOfFrameCoroutine());
#endif
            }
        }

        public void UpdateSceneControllerActiveState(bool canUpdateScenes)
        {
            _canUpdateScenes = canUpdateScenes;
            if (canUpdateScenes && _sceneUpdateRequests.Count > 0) UpdateScene(_sceneUpdateRequests.Peek().SceneData);
        }

#if UNITY_EDITOR
        private System.Collections.IEnumerator WaitEndOfFrameCoroutine()
        {
            yield return new WaitForEndOfFrame();
            OnAllSceneRequestEndDebug?.Invoke();
        }
#endif
    }
}