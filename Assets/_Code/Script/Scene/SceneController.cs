using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using Ivayami.Save;
using UnityEngine.Events;
using Ivayami.Player;

namespace Ivayami.Scene
{
    public class SceneController : MonoSingleton<SceneController>
    {
        [SerializeField] private string _mainMenuSceneName;
        [SerializeField] private bool _debugLogs;

        private ChapterPointers[] _chapterPointers;
        private List<SceneData> _sceneList = new List<SceneData>();
        private Queue<SceneUpdateRequestData> _sceneUpdateRequests = new Queue<SceneUpdateRequestData>();

        [System.Serializable]
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

        [System.Serializable]
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

            _chapterPointers = Resources.LoadAll<ChapterPointers>("ChapterPointers");

            LoadMainMenuScene();
        }

        public void LoadMainMenuScene()
        {
            if (!string.IsNullOrEmpty(_mainMenuSceneName)) StartLoad(_mainMenuSceneName);//SceneManager.LoadScene(_baseSceneName);
        }

        public void PositionPlayer()
        {
            PlayerMovement.Instance.transform.position = _chapterPointers[SaveSystem.Instance.Progress.currentChapter].playerPositionOnChapterStart;
        }

        public void StartLoad(string sceneId, UnityEvent onSceneUpdate = null)
        {
            SceneData data = UpdateSceneList(sceneId);
            if (!data.IsBeingLoaded)
            {
                data.IsBeingLoaded = true;
                _sceneUpdateRequests.Enqueue(new SceneUpdateRequestData(data, onSceneUpdate));
                if(_sceneUpdateRequests.Count == 1)UpdateScene(data);
            }
        }

        private void UpdateScene(SceneData data)
        {
            if (_debugLogs)
            {
                if (data.IsLoaded) Debug.Log($"Unloading Scene {data.SceneName}");
                else Debug.Log($"Loading Scene {data.SceneName}");
            }
            AsyncOperation operation;
            if (data.IsLoaded) operation = SceneManager.UnloadSceneAsync(data.SceneName);
            else operation = SceneManager.LoadSceneAsync(data.SceneName, LoadSceneMode.Additive);
            operation.completed += HandleOnSceneUpdate;
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

        public Vector2 PointerInChapter()
        {            
            return _chapterPointers[SaveSystem.Instance.Progress.currentChapter].SubChapterPointer(SaveSystem.Instance.Progress.currentSubChapter);
        }

        private void HandleOnSceneUpdate(AsyncOperation operation)
        {
            SceneUpdateRequestData requestData = _sceneUpdateRequests.Dequeue();
            requestData.SceneData.IsLoaded = !requestData.SceneData.IsLoaded;
            requestData.SceneData.IsBeingLoaded = false;
            if (_debugLogs)
            {
                if (requestData.SceneData.IsLoaded) Debug.Log($"Scene Loaded {requestData.SceneData.SceneName}");
                else Debug.Log($"Scene Unloaded {requestData.SceneData.SceneName}");
            }
            requestData.OnSceneUpdate?.Invoke();
            if (_sceneUpdateRequests.Count > 0) UpdateScene(_sceneUpdateRequests.Peek().SceneData);
        }
    }
}