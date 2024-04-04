using UnityEngine;
using UnityEngine.Events;

namespace Paranapiacaba.Scene {
    [RequireComponent(typeof(BoxCollider))]
    public class SceneLoader : MonoBehaviour {

        [SerializeField] private string _sceneId;
        [SerializeField] private UnityEvent _onSceneLoad;
        [SerializeField] private UnityEvent _onSceneUnload;
#if UNITY_EDITOR
        [SerializeField] private bool _drawGizmos;
        [SerializeField] private Color _gizmoColor = Color.red;
        private BoxCollider _boxCollider;
#endif

        private void OnTriggerEnter(Collider other) 
        {
            SceneController.Instance.StartLoad(_sceneId, _onSceneLoad);
        }

        private void OnTriggerExit(Collider other)
        {
            SceneController.Instance.StartLoad(_sceneId, _onSceneUnload);
        }

        public void LoadScene()
        {
            SceneController.Instance.StartLoad(_sceneId, _onSceneLoad);
        }

        public void UnloadScene()
        {
            SceneController.Instance.StartLoad(_sceneId, _onSceneLoad);
        }

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            if (_drawGizmos)
            {
                if (!_boxCollider) _boxCollider = GetComponent<BoxCollider>();                
                Gizmos.color = _gizmoColor;
                Gizmos.DrawCube(transform.position, new Vector3(
                    _boxCollider.size.x * transform.localScale.x,
                    _boxCollider.size.y * transform.localScale.y,
                    _boxCollider.size.z * transform.localScale.z));
            }
        }
#endif
    }
}