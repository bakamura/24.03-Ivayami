using UnityEngine;
using FMODUnity;
using FMOD.Studio;
using Ivayami.Audio;
using Ivayami.Player;
using System.Collections.Generic;
using System;

public class PlayerSounds : EntitySound
{
    [Header("Parameters")]

    [SerializeField] private EventReference _defaultStepSound;
    [SerializeField] private LayerMask _groundLayers;
    [SerializeField] private StepSoundData[] _stepSoundsData;

    [Space(16)]

    [SerializeField] private float _heavyBreathStressMin;
    [SerializeField] private EventReference _heavyBreathSoundRef;

    [Header("Cache")]

    private EventInstance _defaultStep;
    private EventInstance _heavyBreathSound;
    private List<StepSoundData> _currentStepSounds = new List<StepSoundData>();
    private RaycastHit[] _hits = new RaycastHit[1];
    private const float _raycastDistance = 3;
    private const string _textureName = "_MainTex";

    [Serializable]
    private struct StepSoundData
    {
        public EventReference SoundReference;
        //public Range SoundRage;
        public Texture[] Textures;
        [NonSerialized] public EventInstance SoundInstance;
        [NonSerialized] public float Volume;
        public static StepSoundData Empty = new StepSoundData();

        public bool IsValid()
        {
            return !SoundReference.IsNull;
        }
    }

    private void Awake()
    {
        _defaultStep = InstantiateEvent(_defaultStepSound);
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
        //PlayOneShot(_defaultStep, true, Range.Empty);
        GetCurrentStepSound();
        if (_currentStepSounds.Count == 0)
        {
            PlayOneShot(_defaultStep, true, Range.Empty);
        }
        else
        {
            for (int i = 0; i < _currentStepSounds.Count; i++)
            {
                PlayOneShot(_currentStepSounds[i].SoundInstance, true, Range.Empty/*_currentStepSounds[i].SoundRage*/, volume: _currentStepSounds[i].Volume);
            }
        }
    }

    private void GetCurrentStepSound()
    {        
        StepSoundData data;
        StopAllStepSounds();
        Physics.RaycastNonAlloc(transform.position, Vector3.down, _hits, _raycastDistance, _groundLayers);
        if (_hits[0].collider.TryGetComponent<Terrain>(out Terrain terrain))
        {
            Vector3 terrainPos = _hits[0].point - terrain.transform.position;
            Vector3 splatMapPosition = new Vector3(terrainPos.x / terrain.terrainData.size.x, 0, terrainPos.z / terrain.terrainData.size.z);
            float[,,] alphaMap = terrain.terrainData.GetAlphamaps((int)(splatMapPosition.x * terrain.terrainData.alphamapWidth), (int)(splatMapPosition.y * terrain.terrainData.alphamapHeight), 1, 1);

            bool soundStoppedOnce = false;
            for (int i = 0; i < alphaMap.Length; i++)
            {
                if (alphaMap[0, 0, i] > 0)
                {
                    data = GetSoundData(terrain.terrainData.terrainLayers[i].diffuseTexture);
                    if (_currentStepSounds.Count == 0 || !_currentStepSounds.Contains(data))
                    {
                        if (!soundStoppedOnce)
                        {
                            StopAllStepSounds();
                            soundStoppedOnce = true;
                        }
                        data.SoundInstance = InstantiateEvent(data.SoundReference);
                        data.Volume = alphaMap[0, 0, i];
                        _currentStepSounds.Add(data);
                    }
                }
            }
        }
        else if (_hits[0].collider.TryGetComponent<MeshRenderer>(out MeshRenderer renderer))
        {
            data = GetSoundData(renderer.sharedMaterial.GetTexture(_textureName));
            if (_currentStepSounds.Count == 0 || !_currentStepSounds.Contains(data))
            {
                StopAllStepSounds();
                data.SoundInstance = InstantiateEvent(data.SoundReference);
                _currentStepSounds.Add(data);
            }
        }

        StepSoundData GetSoundData(Texture texture)
        {
            for (int i = 0; i < _stepSoundsData.Length; i++)
            {
                for (int a = 0; a < _stepSoundsData[i].Textures.Length; a++)
                {
                    if (texture == _stepSoundsData[i].Textures[a]) return _stepSoundsData[i];
                }
            }
            return StepSoundData.Empty;
        }

        void StopAllStepSounds()
        {
            for (int i = 0; i < _currentStepSounds.Count; i++)
            {
                _currentStepSounds[i].SoundInstance.release();
            }
            _currentStepSounds.Clear();
        }
    }
    #endregion
}
