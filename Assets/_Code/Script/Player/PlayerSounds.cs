using UnityEngine;
using FMODUnity;
using FMOD.Studio;
using Ivayami.Audio;
using Ivayami.Player;

public class PlayerSounds : EntitySound
{
    [Header("Parameters")]

    [SerializeField] private EventReference _defaultStepSound;
    [SerializeField] private StepSoundData[] _stepSoundsData;

    [Space(16)]

    [SerializeField] private float _heavyBreathStressMin;
    [SerializeField] private EventReference _heavyBreathSoundRef;

    [Header("Cache")]

    private EventInstance _stepSound;
    private EventInstance _heavyBreathSound;
    private StepSoundData _currentStepSound;

    [System.Serializable]
    private struct StepSoundData
    {
        public EventReference SoundReference;
        public Range SoundRage;
        public Texture[] Textures;
        [HideInInspector] public EventInstance SoundInstance;

        public bool IsValid()
        {
            return !SoundReference.IsNull;
        }
    }

    private void Awake()
    {
        _stepSound = InstantiateEvent(_defaultStepSound);
        _heavyBreathSound = InstantiateEvent(_heavyBreathSoundRef);
    }

    private void Start()
    {
        PlayerStress.Instance.onStressChange.AddListener(HeavyBreathCheck);
    }

    public void HeavyBreathCheck(float stressAmount)
    {
        if (_heavyBreathSound.getPlaybackState(out PLAYBACK_STATE playbackState) == FMOD.RESULT.OK)
        {
            if (stressAmount > _heavyBreathStressMin)
            {
                _heavyBreathSound.set3DAttributes(RuntimeUtils.To3DAttributes(transform.position));
                if (playbackState == PLAYBACK_STATE.STOPPED) _heavyBreathSound.start();
                //if (playbackState == PLAYBACK_STATE.STOPPED) Debug.Log("Player HeavyBreathing Starting");
            }
            else
            {
                if (playbackState == PLAYBACK_STATE.PLAYING) _heavyBreathSound.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
                //if(playbackState == PLAYBACK_STATE.PLAYING) Debug.Log("Player HeavyBreathing Stopping");
            }
        }
    }

    #region StepSound
    public void StepSound()
    {
        PlayOneShot(_stepSound, true, Range.Empty);

        //StepSoundData sound = GetCurrentStepSound();
        //if (!sound.IsValid())
        //{
        //    PlayOneShot(_stepSound, true, Range.Empty);
        //    return;
        //}
        //if (sound.SoundReference.Guid != _currentStepSound.SoundReference.Guid)
        //{
        //    _currentStepSound = sound;
        //    _currentStepSound.SoundInstance = InstantiateEvent(_currentStepSound.SoundReference);
        //}
        //PlayOneShot(_currentStepSound.SoundInstance, true, _currentStepSound.SoundRage);
    }

    private StepSoundData GetCurrentStepSound()
    {
        return new StepSoundData();
    }
    #endregion
}
