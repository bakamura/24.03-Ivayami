using System.Collections.Generic;
using UnityEngine;
using Ivayami.Player;

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
        private SpriteRenderer _icon;
        private Animator _interactionAnimation;
        private Sprite _defaultIcon;
        private bool _interactionIconSetupDone;
        private bool _showingInputIcon;
        private static Transform _cameraTransform;
        private static readonly int _colorVarName = Shader.PropertyToID("_EmissionColor");
        private static int PULSE = Animator.StringToHash("pulse");

        //private void Awake()
        //{
        //    Setup();
        //}

        private void Update()
        {
            if (_icon && _cameraTransform && _icon.enabled)
                _icon.transform.rotation = Quaternion.LookRotation(_cameraTransform.forward);
        }

        private void OnDestroy()
        {
            if (InputCallbacks.Instance && _icon && _showingInputIcon) InputCallbacks.Instance.UnsubscribeToOnChangeControls(UpdateVisualIcon);
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
                    if (renderer.material.HasColor(_colorVarName))
                    {
                        _materials.Add(renderer.material);
                        _baseColors.Add(renderer.material.GetColor(_colorVarName));

                    }
                }
            }
            //setup popup
            if (!_interactionIconSetupDone)
            {
                _icon = GetComponentInChildren<SpriteRenderer>();
                if (!_cameraTransform && PlayerCamera.Instance.MainCamera) _cameraTransform = PlayerCamera.Instance.MainCamera.transform;
                if (_icon)
                {
                    _defaultIcon = _icon.sprite;
                    _interactionAnimation = _icon.GetComponentInParent<Animator>();
                }
                _interactionIconSetupDone = true;
            }
        }

        public void UpdateFeedbacks(bool isActive, bool forcePopupIconActivationUpdate = false)
        {
            Setup();
            for (int i = 0; i < _materials.Count; i++)
            {
                _materials[i].SetColor(_colorVarName, isActive ? _highlightedColor : _baseColors[i]);
            }
            if (_icon)
            {
                _showingInputIcon = isActive;
                if (forcePopupIconActivationUpdate) _icon.enabled = isActive;
                if (isActive)
                {
                    InputCallbacks.Instance.SubscribeToOnChangeControls(UpdateVisualIcon);
                }
                else
                {
                    InputCallbacks.Instance.UnsubscribeToOnChangeControls(UpdateVisualIcon);
                    _icon.sprite = _defaultIcon;
                }
            }
        }

        public void PlayInteractionAnimation()
        {
            _interactionAnimation.Play(PULSE, 0);
        }

        private void UpdateVisualIcon(bool isGamepad)
        {
            Setup();
            _icon.sprite = isGamepad ? _controllerInteractionIcon : _keyboardInteractionIcon;
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