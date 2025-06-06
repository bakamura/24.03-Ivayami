using UnityEngine;
using FMODUnity;
using FMOD.Studio;
using Ivayami.Player;
using System.Collections.Generic;
using System;

namespace Ivayami.Audio
{
    public class PlayerSounds : EntitySound
    {
        [Header("Parameters")]

        [SerializeField] private EventReference _stepSound;
        [SerializeField] private LayerMask _groundLayers;
        [SerializeField] private StepSoundData[] _stepSoundsData;

        [Space(16)]

        [SerializeField] private float _heavyBreathStressMin;
        [SerializeField] private EventReference _heavyBreathSoundRef;

        [Header("Cache")]

        private EventInstance _stepSoundInstance;
        private EventInstance _heavyBreathSound;
        private List<StepSoundData> _currentStepSounds = new List<StepSoundData>();
        private RaycastHit[] _hits = new RaycastHit[1];
        private PARAMETER_ID _stepSoundTypeVariableID;
        private const float _raycastDistance = .1f;
        private const string _albedoTextureName = "_MainTex";

        [Serializable]
        private struct StepSoundData
        {
            public GroundTypes GroundType;
            public Texture[] Textures;
            [NonSerialized] public float Volume;
            [NonSerialized] public bool AutoRemoveFromList;
            public static StepSoundData Empty = new StepSoundData();
        }

        public enum GroundTypes
        {
            Concrete,
            Grass,
            Glass,
            Rock,
            Wood,
            Gravel,
            Tile,
            Dirt
        }

        private void Awake()
        {
            _stepSoundInstance = InstantiateEvent(_stepSound);
            _stepSoundInstance.getDescription(out EventDescription instanceDesc);
            instanceDesc.getParameterDescriptionByName("groundType", out PARAMETER_DESCRIPTION parameterDesc);
            _stepSoundTypeVariableID = parameterDesc.id;

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
            UpdateCurrentStepSurface();
            if (_currentStepSounds.Count > 0)
            {
                for (int i = 0; i < _currentStepSounds.Count; i++)
                {
                    _stepSoundInstance.setParameterByIDWithLabel(_stepSoundTypeVariableID, _currentStepSounds[i].GroundType.ToString());
                    if (_currentStepSounds[i].Volume > 0) _stepSoundInstance.setVolume(_currentStepSounds[i].Volume);
                }
            }
            else _stepSoundInstance.setParameterByIDWithLabel(_stepSoundTypeVariableID, GroundTypes.Concrete.ToString());
            PlayOneShot(_stepSoundInstance, true, Range.Empty);
        }

        public void AddStepToPlaylist(GroundTypes groundType)
        {
            if (!IsCurrentlyInPlaylist(groundType, out _))
            {
                _currentStepSounds.Add(FindSoundDataByGroundType(groundType));
            }
        }

        public void RemoveStepFromPlaylist(GroundTypes groundType)
        {
            if (IsCurrentlyInPlaylist(groundType, out int index))
            {
                _currentStepSounds.RemoveAt(index);
            }
        }

        private StepSoundData FindSoundDataByGroundType(GroundTypes groundType)
        {
            for (int i = 0; i < _stepSoundsData.Length; i++)
            {
                if (_stepSoundsData[i].GroundType == groundType) return _stepSoundsData[i];
            }
            return StepSoundData.Empty;
        }

        private bool IsCurrentlyInPlaylist(GroundTypes groundType, out int index)
        {
            for (int i = 0; i < _currentStepSounds.Count; i++)
            {
                if (_currentStepSounds[i].GroundType == groundType)
                {
                    index = i;
                    return true;
                }
            }
            index = 0;
            return false;
        }

        private void UpdateCurrentStepSurface()
        {
            StepSoundData data;
            ClearUnusedStepSounds();
            Physics.RaycastNonAlloc(transform.position, Vector3.down, _hits, _raycastDistance, _groundLayers);
            if (_hits[0].collider.TryGetComponent<Terrain>(out Terrain terrain))
            {
                Vector3 terrainPos = _hits[0].point - terrain.transform.position;
                Vector3 splatMapPosition = new Vector3(terrainPos.x / terrain.terrainData.size.x, 0, terrainPos.z / terrain.terrainData.size.z);
                float[,,] alphaMap = terrain.terrainData.GetAlphamaps(Mathf.FloorToInt(splatMapPosition.x * terrain.terrainData.alphamapWidth), Mathf.FloorToInt(splatMapPosition.z * terrain.terrainData.alphamapHeight), 1, 1);

                for (int i = 0; i < alphaMap.Length; i++)
                {
                    if (alphaMap[0, 0, i] > 0)
                    {
                        data = FindSoundDataByTexture(terrain.terrainData.terrainLayers[i].diffuseTexture);
                        data.AutoRemoveFromList = true;
                        data.Volume = alphaMap[0, 0, i];

                        if (!IsCurrentlyInPlaylist(data.GroundType, out int index)) _currentStepSounds.Add(data);
                        else _currentStepSounds[index] = data;
                        //Debug.Log($"the sound terrain of {data.GroundType} with volume {data.Volume} will play");
                    }
                }
            }
            else if (_hits[0].collider.TryGetComponent<MeshRenderer>(out MeshRenderer renderer))
            {
                data = FindSoundDataByTexture(renderer.sharedMaterial.GetTexture(_albedoTextureName));
                if (!IsCurrentlyInPlaylist(data.GroundType, out _))
                {
                    //Debug.Log($"the sound mesh renderer of {data.GroundType} will play");
                    data.AutoRemoveFromList = true;
                    _currentStepSounds.Add(data);
                }
            }

            StepSoundData FindSoundDataByTexture(Texture texture)
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

            void ClearUnusedStepSounds()
            {
                for (int i = 0; i < _currentStepSounds.Count; i++)
                {
                    if (_currentStepSounds[i].AutoRemoveFromList) _currentStepSounds.RemoveAt(i);
                }
            }
        }
        #endregion
    }
}