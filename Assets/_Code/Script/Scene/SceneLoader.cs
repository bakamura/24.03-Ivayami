using UnityEngine;
using UnityEngine.Events;

namespace Ivayami.Scene
{
    public class SceneLoader : MonoBehaviour
    {        
        [SceneDropdown, SerializeField] private string _sceneId;
        [SerializeField] private UnityEvent _onSceneLoad;
        [SerializeField] private UnityEvent _onSceneUnload;
        [SerializeField] private UnityEvent _onAllScenesRequestEnd;
#if UNITY_EDITOR
        [SerializeField] private bool _drawGizmos;
        [SerializeField] private Color _gizmoColor = Color.red;
        private BoxCollider _boxCollider;
#endif

        private void OnTriggerEnter(Collider other)
        {
            SceneController.Instance.LoadScene(_sceneId, _onSceneLoad);
        }

        private void OnTriggerExit(Collider other)
        {
            SceneController.Instance.UnloadScene(_sceneId, _onSceneUnload);
        }

        public void LoadScene()
        {
            SceneController.Instance.LoadScene(_sceneId, _onSceneLoad);
        }

        public void UnloadScene()
        {
            SceneController.Instance.UnloadScene(_sceneId, _onSceneUnload);
        }

        public void UnloadAllScenes()
        {
            SceneController.Instance.UnloadAllScenes(HandleOnAllScenesUnload);
        }

        private void HandleOnAllScenesUnload()
        {
            _onAllScenesRequestEnd?.Invoke();
            SceneController.Instance.OnAllSceneRequestEnd -= HandleOnAllScenesUnload;
        }

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            if (_drawGizmos)
            {
                _boxCollider = GetComponent<BoxCollider>();
                if (_boxCollider)
                {
                    Gizmos.color = _gizmoColor;
                    Gizmos.DrawCube(transform.position, new Vector3(
                        _boxCollider.size.x * transform.localScale.x,
                        _boxCollider.size.y * transform.localScale.y,
                        _boxCollider.size.z * transform.localScale.z));

                }
                else Debug.LogWarning("To use debug option add a Box Collider to this component");
            }
        }
#endif
    }
}