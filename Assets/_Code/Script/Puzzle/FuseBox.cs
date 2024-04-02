using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using System;
using UnityEngine.Events;

namespace Paranapiacaba.Puzzle
{
    public class FuseBox : Activator, IInteractable
    {

        [SerializeField] private Vector2 _matrixDimensions;
        [SerializeField] private Vector2 _distanceBetweenFuses;
        [SerializeField] private LayerMask _fuseLayer;
        [SerializeField] private Transform _fuseObjectsParent;
        [SerializeField] private CanvasGroup _fuseUIParent;
        [SerializeField] private InputActionReference _changeFuseInput;
        [SerializeField] private InputActionReference _activateFuseInput;
        [SerializeField] private InputActionReference _cancelInteractionInput;
        [SerializeField] private InputActionAsset _inputActionMap;
        [SerializeField] private UnityEvent _onInteract;
        [SerializeField] private UnityEvent _onInteractionCancelled;
        [SerializeField] private Color _selectedColor = Color.yellow;
        [SerializeField] private Color _activatedColor = Color.red;
        [SerializeField] private Color _deactivatedColor = Color.white;
        private MeshRenderer[] _meshRenderers;
        private MeshRenderer _currentSelected;
        private Color _previousColor;
        private bool _updateSelected;
        private bool _updateActivated;
        private bool _isActive;

        private void Awake()
        {
            _changeFuseInput.action.performed += HandleUINavigation;
            _activateFuseInput.action.performed += HandleActivateFuse;
            _cancelInteractionInput.action.performed += HandleCancelInteraction;

            MeshRenderer[] temp = _fuseObjectsParent.GetComponentsInChildren<MeshRenderer>(false);
            _meshRenderers = new MeshRenderer[temp.Length];
            for (int i = 0; i < temp.Length; i++)
            {
                _meshRenderers[i] = temp[i];
            }
            _previousColor = _deactivatedColor;
        }

        [ContextMenu("Interact")]
        public void Interact()
        {
            if (!_isActive)
            {
                Setup();
            }
        }

        private void Setup()
        {
            EventSystem.current.SetSelectedGameObject(_fuseUIParent.GetComponentInChildren<Button>(false).gameObject);
            int index = Int32.Parse(EventSystem.current.currentSelectedGameObject.name);
            _previousColor = _meshRenderers[index].material.color;
            _currentSelected = _meshRenderers[index];
            _currentSelected.material.color = _selectedColor;
            _isActive = true;
            UpdateInputsAndUI(_isActive);
            _onInteract?.Invoke();
        }

        private void UpdateInputsAndUI(bool isActive)
        {
            if (isActive)
            {
                //gameplay inputs
                _inputActionMap.actionMaps[0].Disable();
                //UI inputs
                _inputActionMap.actionMaps[1].Enable();
            }
            else
            {
                _inputActionMap.actionMaps[0].Enable();
                _inputActionMap.actionMaps[1].Disable();
            }
            _fuseUIParent.interactable = isActive;
            _fuseUIParent.blocksRaycasts = isActive;
        }

        private void HandleUINavigation(InputAction.CallbackContext context)
        {
            if (context.ReadValue<Vector2>() != Vector2.zero && EventSystem.current.currentSelectedGameObject)
            {
                _updateSelected = true;
            }
        }

        private void HandleActivateFuse(InputAction.CallbackContext context)
        {
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
                _currentSelected.material.color = _previousColor;
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
                //up
                if (Physics.Raycast(_meshRenderers[index].transform.position, _meshRenderers[index].transform.up, out hit, _distanceBetweenFuses.y, _fuseLayer))
                {
                    mesh = hit.collider.GetComponent<MeshRenderer>();
                    mesh.material.color = mesh.material.color == _activatedColor ? _deactivatedColor : _activatedColor;
                }
                //if (index - _matrixDimensions.y >= 0)
                //_meshRenderers[index - (int)_matrixDimensions.y].material.color = _meshRenderers[index - (int)_matrixDimensions.y].material.color == Color.red ? Color.white : Color.red;
                //down
                if (Physics.Raycast(_meshRenderers[index].transform.position, -_meshRenderers[index].transform.up, out hit, _distanceBetweenFuses.y, _fuseLayer))
                {
                    mesh = hit.collider.GetComponent<MeshRenderer>();
                    mesh.material.color = mesh.material.color == _activatedColor ? _deactivatedColor : _activatedColor;
                }
                //if (index + _matrixDimensions.y < _meshRenderers.Length)
                //_meshRenderers[index + (int)_matrixDimensions.y].material.color = _meshRenderers[index + (int)_matrixDimensions.y].material.color == Color.red ? Color.white : Color.red;
                //left
                if (Physics.Raycast(_meshRenderers[index].transform.position, -_meshRenderers[index].transform.right, out hit, _distanceBetweenFuses.x, _fuseLayer))
                {
                    mesh = hit.collider.GetComponent<MeshRenderer>();
                    mesh.material.color = mesh.material.color == _activatedColor ? _deactivatedColor : _activatedColor;
                }
                //if (index % _matrixDimensions.y != 0)
                //_meshRenderers[index - 1].material.color = _meshRenderers[index - 1].material.color == Color.red ? Color.white : Color.red;
                //right
                if (Physics.Raycast(_meshRenderers[index].transform.position, _meshRenderers[index].transform.right, out hit, _distanceBetweenFuses.x, _fuseLayer))
                {
                    mesh = hit.collider.GetComponent<MeshRenderer>();
                    mesh.material.color = mesh.material.color == _activatedColor ? _deactivatedColor : _activatedColor;
                }
                //if ((index + 1) % _matrixDimensions.y != 0)
                //_meshRenderers[index + 1].material.color = _meshRenderers[index + 1].material.color == Color.red ? Color.white : Color.red;

                _previousColor = _previousColor == _activatedColor ? _deactivatedColor : _activatedColor;
                _updateActivated = false;
                CheckCompletion();
            }
        }

        private void CheckCompletion()
        {
            for (int i = 0; i < _meshRenderers.Length; i++)
            {
                if (_meshRenderers[i].material.color == _deactivatedColor)
                    return;
            }
            _isActive = false;
            _currentSelected.material.color = _activatedColor;
            UpdateInputsAndUI(_isActive);
            IsActive = !IsActive;
            onActivate?.Invoke();
        }

        private void UpdateFuseSelected()
        {
            if (_updateSelected)
            {
                //Debug.Log($"parse {Int32.Parse(_eventSystem.currentSelectedGameObject.name)}. name {_eventSystem.currentSelectedGameObject.name}");
                _currentSelected.material.color = _previousColor;
                _currentSelected = _meshRenderers[Int32.Parse(EventSystem.current.currentSelectedGameObject.name)];
                _previousColor = _currentSelected.material.color;
                _currentSelected.material.color = _selectedColor;
                _updateSelected = false;
            }
        }

        #region Utilities
        public void RenameObjects()
        {
            if (_fuseObjectsParent)
            {
                MeshRenderer[] temp = _fuseObjectsParent.GetComponentsInChildren<MeshRenderer>(false);
                for (int i = 0; i < temp.Length; i++)
                {
                    temp[i].name = $"{i}";
                }
            }
            if (_fuseUIParent)
            {
                Button[] temp = _fuseUIParent.GetComponentsInChildren<Button>(false);
                for (int i = 0; i < temp.Length; i++)
                {
                    temp[i].name = $"{i}";
                }
            }
        }

        public void RepositionFuses()
        {
            sbyte currentX = 0;
            sbyte currentY = 0;
            MeshRenderer[] temp = _fuseObjectsParent.GetComponentsInChildren<MeshRenderer>(false);
            for (int i = 0; i < temp.Length; i++)
            {
                temp[i].transform.localPosition = new Vector3(currentX * _distanceBetweenFuses.x, currentY * _distanceBetweenFuses.y, 0);
                currentX++;
                if (currentX == _matrixDimensions.x)
                {
                    currentX = 0;
                    currentY++;
                }
            }
        }

        public void UpdateFusesActiveState()
        {
            if (_fuseObjectsParent && _fuseUIParent)
            {
                Button[] uiElements = _fuseUIParent.GetComponentsInChildren<Button>(true);
                MeshRenderer[] objectElements = _fuseObjectsParent.GetComponentsInChildren<MeshRenderer>(true);
                if (uiElements.Length == objectElements.Length)
                {
                    for (int i = 0; i < uiElements.Length; i++)
                    {
                        objectElements[i].gameObject.SetActive(uiElements[i].gameObject.activeInHierarchy);
                    }
                }
            }
        }
        #endregion

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