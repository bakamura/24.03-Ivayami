using UnityEngine;
using UnityEngine.InputSystem;
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
        [SerializeField] private InputActionReference _cancelInteractionInput;
        [SerializeField] private UnityEvent _onInteract;
        [SerializeField] private UnityEvent _onInteractionCancelled;
        [SerializeField] private Color _selectedColor = Color.yellow;
        [SerializeField] private Color _activatedColor = Color.green;
        [SerializeField] private Color _deactivatedColor = Color.red;
        private Color _baseFuseColor;
        private MeshRenderer[] _meshRendererFuses;
        private MeshRenderer[] _meshRenderersLeds;
        private FuseBoxButtonUI _currentButtonSelected;
        private FuseBoxButtonUI _previousButtonSelected;
        //private bool _updateSelected;
        //private bool _updateActivated;
        private bool _isActive;
        private FuseBoxButtonUI _defaultBtn;
        private InteractableFeedbacks _interatctableHighlight;
        private InteractableSounds _interactableSounds;
        private static readonly int _colorEmissionVarID = Shader.PropertyToID("_EmissionColor");

        public InteractableFeedbacks InteratctableFeedbacks { get => _interatctableHighlight; }

        private void Awake()
        {
            _interatctableHighlight = GetComponent<InteractableFeedbacks>();
            _interactableSounds = GetComponent<InteractableSounds>();

            int i;
            FuseBoxButtonUI[] buttons = _fuseUIParent.GetComponentsInChildren<FuseBoxButtonUI>(false);
            _defaultBtn = buttons[0];
            for (i = 0; i < buttons.Length; i++)
            {
                buttons[i].Setup(i);
            }

            MeshRenderer[] temp = _ledsParent.GetComponentsInChildren<MeshRenderer>(false);
            _meshRenderersLeds = new MeshRenderer[temp.Length];
            for (i = 0; i < temp.Length; i++)
            {
                _meshRenderersLeds[i] = temp[i];
            }

            temp = _fusesParent.GetComponentsInChildren<MeshRenderer>(false);
            _meshRendererFuses = new MeshRenderer[temp.Length];
            for (i = 0; i < temp.Length; i++)
            {
                _meshRendererFuses[i] = temp[i];
            }

            _baseFuseColor = _meshRendererFuses[0].material.GetColor(_colorEmissionVarID);
            _distanceBetweenLeds *= 1.05f;
        }

        [ContextMenu("Interact")]
        public PlayerActions.InteractAnimation Interact()
        {
            if (!_isActive)
            {
                _interatctableHighlight.UpdateFeedbacks(false, true);
                _interactableSounds.PlaySound(InteractableSounds.SoundTypes.Interact);
                Setup();
            }
            return PlayerActions.InteractAnimation.Default;
        }

        private void Setup()
        {
            _isActive = true;
            _defaultBtn.Button.Select();
            _meshRendererFuses[_currentButtonSelected.ButtonIndex].material.SetColor(_colorEmissionVarID, _selectedColor);
            UpdateInputsAndUI(_isActive);
            _onInteract?.Invoke();
        }

        private void UpdateInputsAndUI(bool isActive)
        {
            if (isActive)
            {
                _cancelInteractionInput.action.performed += HandleCancelInteraction;
                PlayerActions.Instance.ChangeInputMap("Menu");
            }
            else
            {
                _cancelInteractionInput.action.performed -= HandleCancelInteraction;
                PlayerActions.Instance.ChangeInputMap("Player");
            }
            _fuseUIParent.interactable = isActive;
            _fuseUIParent.blocksRaycasts = isActive;
        }

        public void ActivateFuse()
        {
            if (_isActive)
            {
                UpdateActivateFuse();
            }
        }

        public void ChangeCurrentSelected(FuseBoxButtonUI button)
        {
            if (_isActive)
            {
                _currentButtonSelected = button;
                UpdateFuseSelected();
            }
        }

        private void HandleCancelInteraction(InputAction.CallbackContext context)
        {
            if (context.ReadValue<float>() == 1)
            {
                _isActive = false;
                _meshRendererFuses[_currentButtonSelected.ButtonIndex].material.SetColor(_colorEmissionVarID, _baseFuseColor);
                _interatctableHighlight.UpdateFeedbacks(true, true);
                UpdateInputsAndUI(_isActive);
                _onInteractionCancelled?.Invoke();
            }
        }

        private void UpdateActivateFuse()
        {
            RaycastHit hit;
            MeshRenderer mesh;
            //self
            _meshRenderersLeds[_currentButtonSelected.ButtonIndex].material.SetColor(_colorEmissionVarID, _meshRenderersLeds[_currentButtonSelected.ButtonIndex].material.GetColor(_colorEmissionVarID) == _activatedColor ? _deactivatedColor : _activatedColor);

            //up
            if (Physics.Raycast(_meshRenderersLeds[_currentButtonSelected.ButtonIndex].transform.position, _meshRenderersLeds[_currentButtonSelected.ButtonIndex].transform.up, out hit, _distanceBetweenLeds.y, _fuseLayer))
            {
                mesh = hit.collider.GetComponent<MeshRenderer>();
                mesh.material.SetColor(_colorEmissionVarID, mesh.material.GetColor(_colorEmissionVarID) == _activatedColor ? _deactivatedColor : _activatedColor);
            }
            //if (index - _matrixDimensions.y >= 0)
            //_meshRenderers[index - (int)_matrixDimensions.y].material.color = _meshRenderers[index - (int)_matrixDimensions.y].material.color == Color.red ? Color.white : Color.red;
            //down
            if (Physics.Raycast(_meshRenderersLeds[_currentButtonSelected.ButtonIndex].transform.position, -_meshRenderersLeds[_currentButtonSelected.ButtonIndex].transform.up, out hit, _distanceBetweenLeds.y, _fuseLayer))
            {
                mesh = hit.collider.GetComponent<MeshRenderer>();
                mesh.material.SetColor(_colorEmissionVarID, mesh.material.GetColor(_colorEmissionVarID) == _activatedColor ? _deactivatedColor : _activatedColor);
            }
            //if (index + _matrixDimensions.y < _meshRenderers.Length)
            //_meshRenderers[index + (int)_matrixDimensions.y].material.color = _meshRenderers[index + (int)_matrixDimensions.y].material.color == Color.red ? Color.white : Color.red;
            //left
            if (Physics.Raycast(_meshRenderersLeds[_currentButtonSelected.ButtonIndex].transform.position, -_meshRenderersLeds[_currentButtonSelected.ButtonIndex].transform.right, out hit, _distanceBetweenLeds.x, _fuseLayer))
            {
                mesh = hit.collider.GetComponent<MeshRenderer>();
                mesh.material.SetColor(_colorEmissionVarID, mesh.material.GetColor(_colorEmissionVarID) == _activatedColor ? _deactivatedColor : _activatedColor);
            }
            //if (index % _matrixDimensions.y != 0)
            //_meshRenderers[index - 1].material.color = _meshRenderers[index - 1].material.color == Color.red ? Color.white : Color.red;
            //right
            if (Physics.Raycast(_meshRenderersLeds[_currentButtonSelected.ButtonIndex].transform.position, _meshRenderersLeds[_currentButtonSelected.ButtonIndex].transform.right, out hit, _distanceBetweenLeds.x, _fuseLayer))
            {
                mesh = hit.collider.GetComponent<MeshRenderer>();
                mesh.material.SetColor(_colorEmissionVarID, mesh.material.GetColor(_colorEmissionVarID) == _activatedColor ? _deactivatedColor : _activatedColor);
            }
            //if ((index + 1) % _matrixDimensions.y != 0)
            //_meshRenderers[index + 1].material.color = _meshRenderers[index + 1].material.color == Color.red ? Color.white : Color.red;

            //_previousColor = _previousColor == _activatedColor ? _baseFuseColor : _activatedColor;
            //_updateActivated = false;
            CheckCompletion();
        }

        private void CheckCompletion()
        {
            for (int i = 0; i < _meshRenderersLeds.Length; i++)
            {
                if (_meshRenderersLeds[i].material.GetColor(_colorEmissionVarID) == _deactivatedColor) return;
            }
            _isActive = false;
            _meshRendererFuses[_currentButtonSelected.ButtonIndex].material.SetColor(_colorEmissionVarID, _baseFuseColor);
            UpdateInputsAndUI(_isActive);
            _interactableSounds.PlaySound(InteractableSounds.SoundTypes.ActionSuccess);
            IsActive = !IsActive;
            onActivate?.Invoke();
        }

        private void UpdateFuseSelected()
        {
            if (_previousButtonSelected) _meshRendererFuses[_previousButtonSelected.ButtonIndex].material.SetColor(_colorEmissionVarID, _baseFuseColor);
            _meshRendererFuses[_currentButtonSelected.ButtonIndex].material.SetColor(_colorEmissionVarID, _selectedColor);
            _previousButtonSelected = _currentButtonSelected;
        }
#if UNITY_EDITOR
        #region Utilities
        //public void RenameObjects()
        //{
        //    MeshRenderer[] temp = null;
        //    if (_ledsParent)
        //    {
        //        temp = _ledsParent.GetComponentsInChildren<MeshRenderer>(false);
        //        for (int i = 0; i < temp.Length; i++)
        //        {
        //            temp[i].name = $"{i}";
        //        }
        //    }
        //    if (_fusesParent)
        //    {
        //        temp = _fusesParent.GetComponentsInChildren<MeshRenderer>(false);
        //        for (int i = 0; i < temp.Length; i++)
        //        {
        //            temp[i].name = $"{i}";
        //        }
        //    }
        //    if (_fuseUIParent)
        //    {
        //        Button[] btn = _fuseUIParent.GetComponentsInChildren<Button>(false);
        //        for (int i = 0; i < btn.Length; i++)
        //        {
        //            btn[i].name = $"{i}";
        //        }
        //    }
        //}

        private void OnValidate()
        {
            if (_fuseUIParent)
            {
                _fuseUIParent.GetComponent<UnityEngine.UI.GridLayoutGroup>().constraintCount = (int)_matrixDimensions.y;
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
                if (currentX >= _matrixDimensions.x)
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
                FuseBoxButtonUI[] uiElements = _fuseUIParent.GetComponentsInChildren<FuseBoxButtonUI>(true);
                MeshRenderer[] ledElements = _ledsParent.GetComponentsInChildren<MeshRenderer>(true);
                MeshRenderer[] fuseElements = _fusesParent.GetComponentsInChildren<MeshRenderer>(true);
                //if (uiElements.Length == ledElements.Length)
                //{
                for (int i = 0; i < uiElements.Length; i++)
                {
                    if (!uiElements[i].gameObject.activeSelf)
                    {
                        DestroyImmediate(ledElements[i].gameObject);
                        DestroyImmediate(fuseElements[i].gameObject);
                    }
                    else
                    {
                        ledElements[i].gameObject.SetActive(uiElements[i].gameObject.activeSelf);
                        fuseElements[i].gameObject.SetActive(uiElements[i].gameObject.activeSelf);
                    }
                }
                //}
            }
        }

        public void CreateFuses()
        {
            if (_ledsParent && _fuseUIParent && _fusesParent)
            {
                FuseBoxButtonUI[] uiElements = _fuseUIParent.GetComponentsInChildren<FuseBoxButtonUI>(true);
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
    }
}