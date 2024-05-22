using UnityEngine;

namespace Ivayami.Puzzle
{
    public class InteractionPopup : MonoBehaviour
    {
        //[SerializeField, Min(0f)] private float _rotationSpeed;
        private SpriteRenderer _icon
        {
            get
            {
                if (!m_icon)
                {
                    m_icon = GetComponentInChildren<SpriteRenderer>();                    
                }
                return m_icon;
            }
        }
        private SpriteRenderer m_icon;
        private Transform _cameraTransform
        {
            get
            {
                if (!m_cameraTransform)
                {
                    m_cameraTransform = Camera.main.transform;
                }
                return m_cameraTransform;
            }
        }
        private Transform m_cameraTransform;

        private void Update()
        {
            if (_icon.enabled)
                _icon.transform.rotation = Quaternion.LookRotation(_cameraTransform.forward);
            //if (_icon.enabled)
            //{
            //_icon.transform.rotation = Quaternion.RotateTowards(_icon.transform.rotation, Quaternion.LookRotation(Camera.main.transform.forward), Time.deltaTime * _rotationSpeed);
            //}
        }

        public void UpdateIcon(bool isActive)
        {
            _icon.enabled = isActive;
        }

        private void OnValidate()
        {
            //if (!GetComponentInChildren<SpriteRenderer>(true)) Debug.LogWarning("Create");
            if (!GetComponentInChildren<SpriteRenderer>(true))
            {
                SpriteRenderer sprite = Instantiate(new GameObject("InteractablePopup"), transform).AddComponent<SpriteRenderer>();
                Collider temp = GetComponent<Collider>();
                if (temp) sprite.transform.position += new Vector3(0, temp.bounds.extents.y * 1.5f, 0);
                sprite.enabled = false;
            }
        }
    }
}