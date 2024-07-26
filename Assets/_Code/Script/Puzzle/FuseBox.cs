using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using System;
using UnityEngine.Events;
using Ivayami.Player;
using Ivayami.Audio;

namespace Ivayami.Puzzle
{
    [RequireComponent(typeof(InteractableFeedbacks), typeof(InteractableSounds))]
    public class FuseBox : Activator, IInteractable
    {
        [SerializeField] private Vector2 _matrixDimensions;
        [SerializeField] private Vector2 _distanceBetweenLeds;
        [SerializeField] private Vector3 _fusesOffset;
        [SerializeField] private Vector3 _elementsOffset;
        [SerializeField] private LayerMask _fuseLayer;
        [SerializeField] private Transform _ledsParent;
        [SerializeField] private Transform _fusesParent;
        [SerializeField] private CanvasGroup _fuseUIParent;
        [SerializeField] private GameObject _ledPrefab;
        [SerializeField] private GameObject _fusePrefab;
        [SerializeField] private InputActionReference _changeFuseInput;
        [SerializeField] private InputActionReference _activateFuseInput;
        [SerializeField] private InputActionReference _cancelInteractionInput;
        [SerializeField] private UnityEvent _onInteract;
        [SerializeField] private UnityEvent _onInteractionCancelled;
        [SerializeField] private Color _selectedColor = Color.yellow;
        [SerializeField] private Color _activatedColor = Color.green;
        [SerializeField] private Color _deactivatedColor = Color.red;
        private Color _baseFuseColor;
        private MeshRenderer[] _meshRenderFuses;
        private MeshRenderer[] _meshRenderersLights;
        private MeshRenderer _currentSelected;
        private bool _updateSelected;
        private bool _updateActivated;
        private bool _isActive;
        private GameObject _defaultBtn;
        private InteractableFeedbacks _interatctableHighlight;
        private InteractableSounds _interactableSounds; 
        private const string _colorEmissionVarName = "_EmissionColor";

        public InteractableFeedbacks InteratctableHighlight { get => _interatctableHighlight; }

        private void Awake()
        {
            _interatctableHighlight = GetComponent<InteractableFeedbacks>();
            _interactableSounds = GetComponent<InteractableSounds>();
            _defaultBtn = _fuseUIParent.GetComponentInChildren<Button>(false).gameObject;

            MeshRenderer[] temp = _ledsParent.GetComponentsInChildren<MeshRenderer>(false);
            _meshRenderersLights = new MeshRenderer[temp.Length];
            for (int i = 0; i < temp.Length; i++)
            {
                _meshRenderersLights[i] = temp[i];
            }

            temp = _fusesParent.GetComponentsInChildren<MeshRenderer>(false);
            _meshRenderFuses = new MeshRenderer[temp.Length];
            for (int i = 0; i < temp.Length; i++)
            {
                _meshRenderFuses[i] = temp[i];
            }

            _baseFuseColor = _meshRenderFuses[0].material.GetColor(_colorEmissionVarName);
            _distanceBetweenLeds *= 1.05f;
        }

        [ContextMenu("Interact")]
        public PlayerActions.InteractAnimation Interact()
        {
            if (!_isActive)
            {
                _interatctableHighlight.UpdateFeedbacks(false);
                _interactableSounds.PlaySound(InteractableSounds.SoundTypes.Interact);
                Setup();
            }
            return PlayerActions.InteractAnimation.Default;
        }

        private void Setup()
        {
            SelectDefaultButton();
            int index = Int32.Parse(EventSystem.current.currentSelectedGameObject.name);
            _currentSelected = _meshRenderFuses[index];
            _currentSelected.material.SetColor(_colorEmissionVarName, _selectedColor);
            _isActive = true;
            UpdateInputsAndUI(_isActive);
            _onInteract?.Invoke();
        }

        private void UpdateInputsAndUI(bool isActive)
        {
            if (isActive)
            {
                _changeFuseInput.action.performed += HandleUINavigation;
                _activateFuseInput.action.performed += HandleActivateFuse;
                _cancelInteractionInput.action.performed += HandleCancelInteraction;
                PlayerActions.Instance.ChangeInputMap("Menu");
            }
            else
            {
                _changeFuseInput.action.performed -= HandleUINavigation;
                _activateFuseInput.action.performed -= HandleActivateFuse;
                _cancelInteractionInput.action.performed -= HandleCancelInteraction;
                PlayerActions.Instance.ChangeInputMap("Player");
            }
            _fuseUIParent.interactable = isActive;
            _fuseUIParent.blocksRaycasts = isActive;
        }

        private void SelectDefaultButton()
        {
            EventSystem.current.SetSelectedGameObject(_defaultBtn);
        }

        private void HandleUINavigation(InputAction.CallbackContext context)
        {
            if (!EventSystem.current.currentSelectedGameObject) SelectDefaultButton();
            if (context.ReadValue<Vector2>() != Vector2.zero)
            {
                _updateSelected = true;
            }
        }

        private void HandleActivateFuse(InputAction.CallbackContext context)
        {
            if (!EventSystem.current.currentSelectedGameObject)
            {
                SelectDefaultButton();
                _updateSelected = true;
            }
            if (context.ReadValue<float>() == 1)
            {
                _updateActivated = true;
            }
        }

        public void ActivateFuse()
        {
            if (_isActive) _updateActivated = true;
        }

        public void UpdateUINavigation(GameObject gobj)
        {
            if (_isActive)
            {
                EventSystem.current.SetSelectedGameObject(gobj);
                _updateSelected = true;
            }
        }

        private void HandleCancelInteraction(InputAction.CallbackContext context)
        {
            if (context.ReadValue<float>() == 1)
            {
                _isActive = false;
                _currentSelected.material.SetColor(_colorEmissionVarName, _baseFuseColor);
                _interatctableHighlight.UpdateFeedbacks(true);
                UpdateInputsAndUI(_isActive);
                _onInteractionCancelled?.Invoke();
            }
        }

        private void UpdateActivateFuse()
        {
            if (_updateActivated)
            {
                int index = Int32.Parse(EventSystem.current.currentSelectedGameObject.name);
                RaycastHit hit;
                MeshRenderer mesh;
                //self
                _meshRenderersLights[index].material.SetColor(_colorEmissionVarName, _meshRenderersLights[index].material.GetColor(_colorEmissionVarName) == _activatedColor ? _deactivatedColor : _activatedColor);

                //up
                if (Physics.Raycast(_meshRenderersLights[index].transform.position, _meshRenderersLights[index].transform.up, out hit, _distanceBetweenLeds.y, _fuseLayer))
                {
                    mesh = hit.collider.GetComponent<MeshRenderer>();
                    mesh.material.SetColor(_colorEmissionVarName, mesh.material.GetColor(_colorEmissionVarName) == _activatedColor ? _deactivatedColor : _activatedColor);
                }
                //if (index - _matrixDimensions.y >= 0)
                //_meshRenderers[index - (int)_matrixDimensions.y].material.color = _meshRenderers[index - (int)_matrixDimensions.y].material.color == Color.red ? Color.white : Color.red;
                //down
                if (Physics.Raycast(_meshRenderersLights[index].transform.position, -_meshRenderersLights[index].transform.up, out hit, _distanceBetweenLeds.y, _fuseLayer))
                {
                    mesh = hit.collider.GetComponent<MeshRenderer>();
                    mesh.material.SetColor(_colorEmissionVarName, mesh.material.GetColor(_colorEmissionVarName) == _activatedColor ? _deactivatedColor : _activatedColor);
                }
                //if (index + _matrixDimensions.y < _meshRenderers.Length)
                //_meshRenderers[index + (int)_matrixDimensions.y].material.color = _meshRenderers[index + (int)_matrixDimensions.y].material.color == Color.red ? Color.white : Color.red;
                //left
                if (Physics.Raycast(_meshRenderersLights[index].transform.position, -_meshRenderersLights[index].transform.right, out hit, _distanceBetweenLeds.x, _fuseLayer))
                {
                    mesh = hit.collider.GetComponent<MeshRenderer>();
                    mesh.material.SetColor(_colorEmissionVarName, mesh.material.GetColor(_colorEmissionVarName) == _activatedColor ? _deactivatedColor : _activatedColor);
                }
                //if (index % _matrixDimensions.y != 0)
                //_meshRenderers[index - 1].material.color = _meshRenderers[index - 1].material.color == Color.red ? Color.white : Color.red;
                //right
                if (Physics.Raycast(_meshRenderersLights[index].transform.position, _meshRenderersLights[index].transform.right, out hit, _distanceBetweenLeds.x, _fuseLayer))
                {
                    mesh = hit.collider.GetComponent<MeshRenderer>();
                    mesh.material.SetColor(_colorEmissionVarName, mesh.material.GetColor(_colorEmissionVarName) == _activatedColor ? _deactivatedColor : _activatedColor);
                }
                //if ((index + 1) % _matrixDimensions.y != 0)
                //_meshRenderers[index + 1].material.color = _meshRenderers[index + 1].material.color == Color.red ? Color.white : Color.red;

                //_previousColor = _previousColor == _activatedColor ? _baseFuseColor : _activatedColor;
                _updateActivated = false;
                CheckCompletion();
            }
        }

        private void CheckCompletion()
        {
            for (int i = 0; i < _meshRenderersLights.Length; i++)
            {
                if (_meshRenderersLights[i].material.GetColor(_colorEmissionVarName) == _deactivatedColor) return;
            }
            _isActive = false;
            _currentSelected.material.SetColor(_colorEmissionVarName, _baseFuseColor);
            UpdateInputsAndUI(_isActive);
            _interactableSounds.PlaySound(InteractableSounds.SoundTypes.ActionSuccess);
            IsActive = !IsActive;
            onActivate?.Invoke();
        }

        private void UpdateFuseSelected()
        {
            if (_updateSelected)
            {
                _currentSelected.material.SetColor(_colorEmissionVarName, _baseFuseColor);
                _currentSelected = _meshRenderFuses[Int32.Parse(EventSystem.current.currentSelectedGameObject.name)];
                _currentSelected.material.SetColor(_colorEmissionVarName, _selectedColor);
                _updateSelected = false;
            }
        }
#if UNITY_EDITOR
        #region Utilities
        public void RenameObjects()
        {
            MeshRenderer[] temp = null;
            if (_ledsParent)
            {
                temp = _ledsParent.GetComponentsInChildren<MeshRenderer>(false);
                for (int i = 0; i < temp.Length; i++)
                {
                    temp[i].name = $"{i}";
                }
            }
            if (_fusesParent)
            {
                temp = _fusesParent.GetComponentsInChildren<MeshRenderer>(false);
                for (int i = 0; i < temp.Length; i++)
                {
                    temp[i].name = $"{i}";
                }
            }
            if (_fuseUIParent)
            {
                Button[] btn = _fuseUIParent.GetComponentsInChildren<Button>(false);
                for (int i = 0; i < btn.Length; i++)
                {
                    btn[i].name = $"{i}";
                }
            }
        }

        public void RepositionFuses()
        {
            sbyte currentX = 0;
            sbyte currentY = 0;
            MeshRenderer[] leds = _ledsParent.GetComponentsInChildren<MeshRenderer>(true);
            MeshRenderer[] fuses = _fusesParent.GetComponentsInChildren<MeshRenderer>(true);
            for (int i = 0; i < leds.Length; i++)
            {
                Vector3 pos = new Vector3(currentX * _distanceBetweenLeds.x, currentY * _distanceBetweenLeds.y, 0);
                leds[i].transform.localPosition = pos;
                fuses[i].transform.localPosition = pos;
                currentX++;
                if (currentX > _matrixDimensions.x)
                {
                    currentX = 0;
                    currentY++;
                }
            }
            _fusesParent.localPosition = _elementsOffset + _fusesOffset;
            _ledsParent.localPosition = _elementsOffset;
        }

        public void UpdateFusesActiveState()
        {
            if (_ledsParent && _fuseUIParent)
            {
                Button[] uiElements = _fuseUIParent.GetComponentsInChildren<Button>(true);
                MeshRenderer[] ledElements = _ledsParent.GetComponentsInChildren<MeshRenderer>(true);
                MeshRenderer[] fuseElements = _fusesParent.GetComponentsInChildren<MeshRenderer>(true);
                //if (uiElements.Length == ledElements.Length)
                //{
                for (int i = 0; i < uiElements.Length; i++)
                {
                    if (!uiElements[i].gameObject.activeInHierarchy)
                    {
                        DestroyImmediate(ledElements[i].gameObject);
                        DestroyImmediate(fuseElements[i].gameObject);
                    }
                    else
                    {
                        ledElements[i].gameObject.SetActive(uiElements[i].gameObject.activeInHierarchy);
                        fuseElements[i].gameObject.SetActive(uiElements[i].gameObject.activeInHierarchy);
                    }
                }
                //}
            }
        }

        public void CreateFuses()
        {
            if (_ledsParent && _fuseUIParent && _fusesParent)
            {
                Button[] uiElements = _fuseUIParent.GetComponentsInChildren<Button>(true);
                MeshRenderer[] ledElements = _ledsParent.GetComponentsInChildren<MeshRenderer>(true);
                MeshRenderer[] fuseElements = _fusesParent.GetComponentsInChildren<MeshRenderer>(true);
                int i;
                for (i = 0; i < ledElements.Length; i++)
                {
                    DestroyImmediate(ledElements[i].gameObject);
                    DestroyImmediate(fuseElements[i].gameObject);
                }
                for (i = 0; i < uiElements.Length; i++)
                {
                    Instantiate(_ledPrefab, _ledsParent).SetActive(true);
                    Instantiate(_fusePrefab, _fusesParent).SetActive(true);
                }
            }
        }
        #endregion
#endif

        private void OnGUI()
        {
            if (_isActive)
            {
                UpdateFuseSelected();
                UpdateActivateFuse();
            }
        }
    }
}