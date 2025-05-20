using System.Linq;
//using UnityEngine;
using UnityEngine.Rendering;
//using System.Collections;
using PSX;
using UnityEngine.Rendering.Universal;
using UnityEngine;

public class PostProcessManager : MonoSingleton<PostProcessManager> {

    //[SerializeField] private float _heightPixelation;
    //[SerializeField, Min(0f)] private float _windowResizeCheckInterval;
    //private WaitForSeconds _windowResizeCheckWait;

    //private Vector2Int _windowSizePrevious;
    // base falue takken from 1920x180 res
    //private float _ditheringScaleBase;
    //private Pixelation _pixelation;
    private Dithering _dithering;
    private LiftGammaGain _gamma;
    private Fog _psxFog;  
    //private const float _baseResolutionFactor = 1920f / 1080f;

    protected override void Awake() {
        //_pixelation = GetComponent<Volume>().profile.components.FirstOrDefault(component => component.GetType() == typeof(Pixelation)) as Pixelation;
        base.Awake();
        Volume volume = GetComponent<Volume>();
        _dithering = volume.profile.components.FirstOrDefault(component => component.GetType() == typeof(Dithering)) as Dithering;
        _psxFog = volume.profile.components.FirstOrDefault(component => component.GetType() == typeof(Fog)) as Fog;
        _gamma = volume.profile.components.FirstOrDefault(component => component.GetType() == typeof(LiftGammaGain)) as LiftGammaGain;
        //_ditheringScaleBase = _dithering.ditherScale.value;

        //_pixelation.widthPixelation.Override(_heightPixelation);
        //ReScale();

        //_windowResizeCheckWait = new WaitForSeconds(_windowResizeCheckInterval);
        //StartCoroutine(CheckForWindowReSized());
    }

    //private void ReScale() {
    //    _windowSizePrevious = new Vector2Int(Screen.width, Screen.height);
    //    //_pixelation.heightPixelation.Override(_heightPixelation * Screen.width / Screen.height);
    //    _dithering.ditherScale.Override(_ditheringScaleBase * _baseResolutionFactor / (Screen.width / 1080f));
    //    //Debug.Log($"New scale {_dithering.ditherScale.value}");
    //}

    public void ChangeDitheringScale(float value)
    {
        _dithering.ditherScale.Override(value);
    }

    public void ChangeBrightness(float value)
    {
        _gamma.gamma.Override(new Vector4(value, value, value, value));
    }

    public void ChangePSXFog(float distance, Color fogColor)
    {
        _psxFog.fogDistance.value = distance;
        _psxFog.fogColor.value = fogColor;
    }

    public void ChangePSXFog(float distance)
    {
        _psxFog.fogDistance.value = distance;
    }

    public void ChangePSXFog(Color fogColor)
    {
        _psxFog.fogColor.value = fogColor;
    }

    public void GetPSXFogValues(out float distance, out Color fogColor)
    {
        distance = _psxFog.fogDistance.value;
        fogColor = _psxFog.fogColor.value;
    }

    public Fog GetPSXFog() => _psxFog;

    //private IEnumerator CheckForWindowReSized() {
    //    while (true) {
    //        if(_windowSizePrevious.x != Screen.width || _windowSizePrevious.y != Screen.height) ReScale();

    //        yield return _windowResizeCheckWait;
    //    }
    //}

}
