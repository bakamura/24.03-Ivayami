using UnityEngine;
using FMODUnity;
using FMOD.Studio;
using Ivayami.Audio;
using Ivayami.Player;

public class PlayerSounds : EntitySound {

    [Header("Parameters")]

    [SerializeField] private EventReference _stepSoundRef;
    [SerializeField] private Range _stepSoundRange;

    [Space(16)]

    [SerializeField] private float _heavyBreathStressMin;
    [SerializeField] private EventReference _heavyBreathSoundRef;
    [SerializeField] private Range _heavyBreathSoundRange;

    [Header("Cache")]

    private EventInstance _stepSound;
    private EventInstance _heavyBreathSound;

    private void Awake() {
        _stepSound = InstantiateEvent(_stepSoundRef);
    }

    private void Start() {
        PlayerStress.Instance.onStressChange.AddListener(HeavyBreathCheck);
    }

    public void StepSound() {
        PlayOneShot(_stepSound, true, _stepSoundRange);
    }

    public void HeavyBreathCheck(float stressAmount) {
        if (stressAmount > _heavyBreathStressMin) _heavyBreathSound.start();
        else _heavyBreathSound.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
    }

}
