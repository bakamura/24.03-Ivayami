using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using System;
using UnityEngine.Events;

namespace Paranapiacaba.Scene
{
    public class SceneController : MonoSingleton<SceneController>
    {
        [SerializeField] private string _baseSceneName;
        [SerializeField] private bool _debugLogs;

        private ChapterPointers[] _chapterPointers;
        private List<string> _currentLoadedScenes = new List<string>();
        private string _currentSceneBeingUnloaded;
        private string _currentSceneBeingLoaded;
        private UnityEvent _onSceneLoad;
        private UnityEvent _onSceneUnload;

        protected override void Awake()
        {
            base.Awake();

            _chapterPointers = Resources.LoadAll<ChapterPointers>("ChapterPointers");
        }

        public void LoadBaseScene()
        {
            if (!string.IsNullOrEmpty(_baseSceneName)) SceneManager.LoadScene(_baseSceneName);
        }

        public void StartLoad(string sceneId, UnityEvent onSceneLoad = null, UnityEvent onSceneUnload = null)
        {
            if (_currentLoadedScenes.Contains(sceneId) && _currentSceneBeingUnloaded == null)
            {
                _currentSceneBeingUnloaded = sceneId;
                _onSceneUnload = onSceneUnload;
                if (_debugLogs) Debug.Log($"Unloading Scene {sceneId}");
                AsyncOperation operation = SceneManager.UnloadSceneAsync(sceneId);
                operation.completed += HandleOnSceneUnload;
            }
            else if (!_currentLoadedScenes.Contains(sceneId) && _currentSceneBeingLoaded == null)
            {
                _currentSceneBeingLoaded = sceneId;
                _onSceneLoad = onSceneLoad;
                if (_debugLogs) Debug.Log($"Loading Scene {sceneId}");
                AsyncOperation operation = SceneManager.LoadSceneAsync(sceneId, LoadSceneMode.Additive);
                operation.completed += HandleOnSceneLoad;
            }
        }

        public Vector2 PointerInChapter(byte chapter, byte subChapter)
        {
            return _chapterPointers[chapter].SubChapterPointer(subChapter);
        }

        private void HandleOnSceneLoad(AsyncOperation operation)
        {
            if (_debugLogs) Debug.Log($"Scene Loaded {_currentSceneBeingLoaded}");
            _currentLoadedScenes.Add(_currentSceneBeingLoaded);
            _currentSceneBeingLoaded = null;
            _onSceneLoad?.Invoke();
        }

        private void HandleOnSceneUnload(AsyncOperation operation)
        {
            if (_debugLogs) Debug.Log($"Scene Unloaded {_currentSceneBeingUnloaded}");
            _currentLoadedScenes.Remove(_currentSceneBeingUnloaded);
            _currentSceneBeingUnloaded = null;
            _onSceneUnload?.Invoke();
        }

    }
}