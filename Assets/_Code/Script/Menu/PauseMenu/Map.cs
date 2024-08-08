using UnityEngine;
using System.Linq;
using Ivayami.Scene;
using Ivayami.Player;

namespace Ivayami.UI {
    public class Map : MonoBehaviour {

        [SerializeField] private RectTransform _mapRectTranform;
        [SerializeField] private RectTransform _playerPointer;
        [SerializeField, Tooltip("Every pointer should be named Pointer_{ProgressNameCaseSensitive}")] private RectTransform[] _goalPointers;

        [SerializeField] private RectTransform[] _placesInMap;
        [SerializeField] private Vector2 _mapWorldSize;

        private Transform _cam;

        private void Awake() {
            _cam = Camera.main.transform;
        }

        public void PointersUpdate() {
            Vector2 playerPosInMap = Vector2.zero;
            playerPosInMap[0] = PlayerMovement.Instance.transform.position.x / _mapWorldSize.x;
            playerPosInMap[1] = PlayerMovement.Instance.transform.position.z / _mapWorldSize.y;
            playerPosInMap *= _mapRectTranform.anchoredPosition;

            _playerPointer.anchoredPosition = _placesInMap.OrderBy(rect => Vector2.Distance(rect.anchoredPosition, playerPosInMap)).FirstOrDefault().anchoredPosition;
            _playerPointer.rotation = Quaternion.Euler(0f, 0f, _cam.transform.eulerAngles.y); //

            foreach (RectTransform goalPointer in _goalPointers) {
                Vector2 goalPosInMap = SceneController.Instance.PointerInChapter(goalPointer.name.Split('_')[1]);
                goalPointer.gameObject.SetActive(goalPosInMap != Vector2.zero);
                if (goalPosInMap != Vector2.zero) goalPointer.anchoredPosition = goalPosInMap;
            }
        }

    }
}