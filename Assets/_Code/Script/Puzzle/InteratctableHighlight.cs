using System.Collections.Generic;
using UnityEngine;

namespace Ivayami.Puzzle
{
    public class InteratctableHighlight : MonoBehaviour
    {
        [SerializeField] private Color _highlightedColor;
        private List<Material> _materials;
        private List<Color> _baseColors;
        private static readonly int _colorVarName = Shader.PropertyToID("_EmissionColor");

        public void UpdateHighlight(bool isActive)
        {
            if (_materials == null) FindMaterial();
            for (int i = 0; i < _materials.Count; i++)
            {
                _materials[i].SetColor(_colorVarName, isActive ? _highlightedColor : _baseColors[i]);
            }
        }

        private void FindMaterial()
        {
            _materials = new List<Material>();
            _baseColors = new List<Color>();   
            foreach (Renderer render in GetComponentsInChildren<Renderer>())
            {
                if (render.material.GetColor(_colorVarName) == Color.black)
                {
                    _materials.Add(render.material);
                    _baseColors.Add(render.material.GetColor(_colorVarName));
                }
            }
        }
    }
}