using System.Collections.Generic;
using UnityEngine;

namespace Ivayami.Puzzle
{
    public class InteractableFeedbacks : MonoBehaviour
    {
        [SerializeField] private Color _highlightedColor = new Color(0.03921569f, 0.03921569f, 0.03921569f, 1);
        [SerializeField] private bool _applyToChildrens = true;

        private List<Material> _materials;
        private List<Color> _baseColors;       
        private static readonly int _colorVarName = Shader.PropertyToID("_EmissionColor");
        private SpriteRenderer _icon;
        private Transform _cameraTransform;

        public void UpdateFeedbacks(bool isActive)
        {
            Setup();
            for (int i = 0; i < _materials.Count; i++)
            {
                _materials[i].SetColor(_colorVarName, isActive ? _highlightedColor : _baseColors[i]);
            }
            _icon.enabled = isActive;
        }

        private void Setup()
        {
            //setup materials
            if(_materials == null)
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
            if (!_icon)
            {
                _icon = GetComponentInChildren<SpriteRenderer>();
                _cameraTransform = Camera.main.transform;
            }
        }        

        private void Update()
        {
            if (_icon && _icon.enabled)
                _icon.transform.rotation = Quaternion.LookRotation(_cameraTransform.forward);
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