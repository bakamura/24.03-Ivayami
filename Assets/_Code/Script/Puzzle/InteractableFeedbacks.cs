using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Ivayami.Puzzle
{
    public class InteractableFeedbacks : MonoBehaviour
    {
        [SerializeField] private Color _highlightedColor = new Color(0.03921569f, 0.03921569f, 0.03921569f, 1);
        [SerializeField] private bool _applyToChildrens = true;
        [SerializeField] private Sprite _controllerInteractionIcon;
        [SerializeField] private Sprite _keyboardInteractionIcon;

        private List<Material> _materials;
        private List<Color> _baseColors;
        private static readonly int _colorVarName = Shader.PropertyToID("_EmissionColor");
        private SpriteRenderer _icon;
        private Animator _interactionAnimation;
        private Transform _cameraTransform;
        private bool _interactionIconSetupDone;
        private static int PULSE = Animator.StringToHash("pulse");

        public void UpdateFeedbacks(bool isActive)
        {
            Setup();
            for (int i = 0; i < _materials.Count; i++)
            {
                _materials[i].SetColor(_colorVarName, isActive ? _highlightedColor : _baseColors[i]);
            }
            if (_icon) _icon.enabled = isActive;
        }

        public void UpdateInteractionIcon(bool isActive)
        {
            if (_icon) _icon.enabled = isActive;
        }

        public void PlayInteractionAnimation()
        {
            _interactionAnimation.Play(PULSE, 0);
        }

        private void Setup()
        {
            //setup materials
            if (_materials == null)
            {
                _materials = new List<Material>();
                _baseColors = new List<Color>();
                if (_applyToChildrens)
                {
                    Color emissiveColor;
                    foreach (Renderer render in GetComponentsInChildren<Renderer>())
                    {
                        if (render.material.HasColor(_colorVarName))
                        {
                            emissiveColor = render.material.GetColor(_colorVarName);
                            if (emissiveColor == Color.black)
                            {
                                _materials.Add(render.material);
                                _baseColors.Add(emissiveColor);
                            }
                        }
                    }
                }
                else
                {
                    Renderer renderer = GetComponentInChildren<Renderer>();
                    _materials.Add(renderer.material);
                    _baseColors.Add(renderer.material.GetColor(_colorVarName));
                }
            }
            //setup popup
            if (!_interactionIconSetupDone)
            {
                _icon = GetComponentInChildren<SpriteRenderer>();
                _cameraTransform = Camera.main.transform;
                if (_icon)
                {
                    _interactionAnimation = _icon.GetComponent<Animator>();
                    UpdateVisualIcon(InputCallbacks.Instance.CurrentControlScheme);
                    InputCallbacks.Instance.AddEventToOnChangeControls(HandleDeviceUpdate);
                }                
                _interactionIconSetupDone = true;
            }
        }

        private void HandleDeviceUpdate(PlayerInput script)
        {
            UpdateVisualIcon(script.currentControlScheme);
        }

        private void UpdateVisualIcon(string currentControlSchemeName)
        {
            _icon.sprite = currentControlSchemeName.Equals("Gamepad") ? _controllerInteractionIcon : _keyboardInteractionIcon;
        }

        private void Update()
        {
            if (_icon && _icon.enabled)
                _icon.transform.rotation = Quaternion.LookRotation(_cameraTransform.forward);
        }

        private void OnDestroy()
        {
            if (_icon && InputCallbacks.Instance) InputCallbacks.Instance.RemoveEventToOnChangeControls(HandleDeviceUpdate);
        }

        //private void OnValidate()
        //{
        //    if (!GetComponentInChildren<SpriteRenderer>(true))
        //    {
        //        SpriteRenderer sprite = Instantiate(new GameObject("InteractablePopup"), transform).AddComponent<SpriteRenderer>();
        //        Collider temp = GetComponent<Collider>();
        //        if (temp) sprite.transform.position += new Vector3(0, temp.bounds.extents.y * 1.5f, 0);
        //        sprite.enabled = false;
        //    }
        //}
    }
}