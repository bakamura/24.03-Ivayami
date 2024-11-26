using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;
using System.Collections;
using PSX;

public class PsxScaler : MonoBehaviour {

    //[Tooltip("In a Full HD Screen [1920x1080]")]
    //[SerializeField] private Vector2 _basePixelation;
    [SerializeField] private float _ditheringScaleBase;
    [SerializeField, Min(0f)] private float _windowResizeCheckInterval;
    private WaitForSeconds _windowResizeCheckWait;

    private Vector2Int _windowSizePrevious;
    private Pixelation _pixelation;
    private Dithering _dithering;

    private void Awake() {
        //_pixelation = GetComponent<Volume>().profile.components.FirstOrDefault(component => component.GetType() == typeof(Pixelation)) as Pixelation;
        _dithering = GetComponent<Volume>().profile.components.FirstOrDefault(component => component.GetType() == typeof(Dithering)) as Dithering;
        ReScale();

        _windowResizeCheckWait = new WaitForSeconds(_windowResizeCheckInterval);
        StartCoroutine(CheckForWindowReSized());
    }

    private void ReScale() {
        _windowSizePrevious = new Vector2Int(Screen.width, Screen.height);
        //_pixelation.widthPixelation.Override(_basePixelation.x * _windowSizePrevious.y / 1080f); // was '_windowSizePrevious.x / 1920f', changed so its based only on height scale for consistent appearance
        //_pixelation.heightPixelation.Override(_basePixelation.y * _windowSizePrevious.y / 1080f);
        _dithering.ditherScale.Override(_ditheringScaleBase * _windowSizePrevious.y / 1080f);
    }

    private IEnumerator CheckForWindowReSized() {
        while (true) {
            if(_windowSizePrevious.x != Screen.width || _windowSizePrevious.y != Screen.height) ReScale();

            yield return _windowResizeCheckWait;
        }
    }

}
