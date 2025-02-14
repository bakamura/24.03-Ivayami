using UnityEngine;
using UnityEngine.Events;

namespace Ivayami.Puzzle
{
    public class PressButtonInOrderPuzzle : MonoBehaviour
    {
        [SerializeField] private byte[] _sequence;
        [SerializeField] private UnityEvent _onOptionCorrect;
        [SerializeField] private UnityEvent _onActivate;
        [SerializeField] private UnityEvent _onFailActivate;

        private byte _currentIndex;

        public void SetOption(int value)
        {
            if (_sequence[_currentIndex] == (byte)value)
            {
                _currentIndex++;
                if (_currentIndex == _sequence.Length)
                {
                    _onActivate?.Invoke();
                    _currentIndex = 0;
                }
                else _onOptionCorrect?.Invoke();

            }
            else
            {
                _currentIndex = 0;
                _onFailActivate?.Invoke();
            }
        }
    }
}