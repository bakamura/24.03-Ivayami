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
    //[SerializeField] private Range _heavyBreathSoundRange;

    [Header("Cache")]

    private EventInstance _stepSound;
    private EventInstance _heavyBreathSound;

    private void Awake() {
        _stepSound = InstantiateEvent(_stepSoundRef);
        _heavyBreathSound = InstantiateEvent(_heavyBreathSoundRef);
    }

    private void Start() {
        PlayerStress.Instance.onStressChange.AddListener(HeavyBreathCheck);
    }

    public void StepSound() {
        PlayOneShot(_stepSound, true, _stepSoundRange);
    }

    public void HeavyBreathCheck(float stressAmount) {
        if (_heavyBreathSound.getPlaybackState(out PLAYBACK_STATE playbackState) == FMOD.RESULT.OK) {
            if (stressAmount > _heavyBreathStressMin) {
                _heavyBreathSound.set3DAttributes(RuntimeUtils.To3DAttributes(transform.position));
                if (playbackState == PLAYBACK_STATE.STOPPED) _heavyBreathSound.start();
                //if (playbackState == PLAYBACK_STATE.STOPPED) Debug.Log("Player HeavyBreathing Starting");
            }
            else {
                if(playbackState == PLAYBACK_STATE.PLAYING) _heavyBreathSound.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
                //if(playbackState == PLAYBACK_STATE.PLAYING) Debug.Log("Player HeavyBreathing Stopping");
            }
        }
    }

}
