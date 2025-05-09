using UnityEngine;
using UnityEngine.Events;

namespace Ivayami.Puzzle
{
    public class RotatingShadowPuzzleGroup : MonoBehaviour
    {
        [SerializeField] private UnityEvent _onActivate;
        [SerializeField] private UnityEvent _onFailActivate;

        private RotatingShadowPuzzle[] _rotatingShadows;

        private void Awake()
        {
            _rotatingShadows = GetComponentsInChildren<RotatingShadowPuzzle>(true);
            for(int i =0; i < _rotatingShadows.Length; i++)
            {
                _rotatingShadows[i].OnRotateObjectEnd.AddListener(CheckForCompletion);
            }
        }

        private void CheckForCompletion()
        {
            for (int i = 0; i < _rotatingShadows.Length; i++)
            {
                if (!_rotatingShadows[i].IsCorrect())
                {
                    _onFailActivate?.Invoke();
                    return;
                }
            }
            _onActivate?.Invoke();
        }
    }
}