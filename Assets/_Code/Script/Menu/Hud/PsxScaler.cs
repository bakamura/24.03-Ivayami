using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;
using System.Collections;
using PSX;

public class PsxScaler : MonoBehaviour {

    [SerializeField] private float _heightPixelation;
    [SerializeField] private float _ditheringScaleBase;
    [SerializeField, Min(0f)] private float _windowResizeCheckInterval;
    private WaitForSeconds _windowResizeCheckWait;

    private Vector2Int _windowSizePrevious;
    private Pixelation _pixelation;
    private Dithering _dithering;

    private void Awake() {
        _pixelation = GetComponent<Volume>().profile.components.FirstOrDefault(component => component.GetType() == typeof(Pixelation)) as Pixelation;
        _dithering = GetComponent<Volume>().profile.components.FirstOrDefault(component => component.GetType() == typeof(Dithering)) as Dithering;
        ReScale();

        _windowResizeCheckWait = new WaitForSeconds(_windowResizeCheckInterval);
        StartCoroutine(CheckForWindowReSized());
    }

    private void ReScale() {
        _windowSizePrevious = new Vector2Int(Screen.width, Screen.height);
        _pixelation.widthPixelation.Override(_heightPixelation);
        _pixelation.heightPixelation.Override(_heightPixelation * Screen.width / Screen.height);
        _dithering.ditherScale.Override(_ditheringScaleBase * _windowSizePrevious.y / 1080f);
    }

    private IEnumerator CheckForWindowReSized() {
        while (true) {
            if(_windowSizePrevious.x != Screen.width || _windowSizePrevious.y != Screen.height) ReScale();

            yield return _windowResizeCheckWait;
        }
    }

}
