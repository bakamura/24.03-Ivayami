using UnityEngine;
using UnityEngine.Events;

namespace Ivayami.Puzzle
{
    public class TempleDoorAnimation : MonoBehaviour
    {
        [SerializeField] private string _boolName;
        [SerializeField] private UnityEvent _onOpen;
        [SerializeField] private UnityEvent _onClose;
        private Animator _animator
        {
            get
            {
                if (!m_animator) m_animator = GetComponent<Animator>();
                return m_animator;
            }
        }
        private Animator m_animator;
        private int _boolHash
        {
            get
            {
                if (m_boolHash == 0) m_boolHash = Animator.StringToHash(_boolName);
                return m_boolHash;
            }
        }
        private int m_boolHash;

        public void SetBool(bool value)
        {
            _animator.SetBool(_boolHash, value);
            if (value) _onOpen?.Invoke();
            else _onClose?.Invoke();
        }
    }

}
