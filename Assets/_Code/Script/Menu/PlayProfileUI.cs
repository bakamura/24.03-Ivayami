using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Ivayami.Scene;
using Ivayami.Player;
using Ivayami.Save;

namespace Ivayami.UI
{
    public class PlayProfileUI : MonoBehaviour
    {
        private TMP_Dropdown _dropdown;
        private PlayProfile[] _playProfiles;
        private PlayProfile _currentProfile;
        private SceneLoader _mainMenuSceneUnloader;
        private ManualTeleporter _teleporter;

        private void Awake()
        {
            _dropdown = GetComponentInChildren<TMP_Dropdown>();
            _mainMenuSceneUnloader = GetComponentInChildren<SceneLoader>();
            _teleporter = GetComponentInChildren<ManualTeleporter>();

            _playProfiles = Resources.LoadAll<PlayProfile>("PlayProfiles");

            if (_playProfiles == null) return;
            List<string> options = new List<string>();
            for (int i = 0; i < _playProfiles.Length; i++)
            {
                options.Add(_playProfiles[i].name);
            }

            _dropdown.AddOptions(options);
            _dropdown.onValueChanged.AddListener(OnDropdownValueChanged);
        }

        private void OnDropdownValueChanged(int index)
        {
            _currentProfile = _playProfiles[index];
            SceneTransition.Instance.OnOpenEnd.AddListener(StartPlayProfile);
            SceneTransition.Instance.Open();
        }
        /// <summary>
        /// to use in the Play Btn
        /// </summary>
        /// <param name="profile"></param>
        public void StartPlayProfile(PlayProfile profile)
        {
            _currentProfile = profile;
            StartPlayProfile();
        }

        private void StartPlayProfile()
        {
            SaveSystem.Instance.DeleteProgress(4);
            SaveSystem.Instance.LoadProgress(4, OnSaveFileInitialized);            
        }

        private void OnSaveFileInitialized()
        {
            if(_currentProfile.InitialSavePoint != -1) SaveSystem.Instance.Progress.pointId = _currentProfile.InitialSavePoint;
            _teleporter.transform.SetPositionAndRotation(_currentProfile.PlayerStartPosition, Quaternion.Euler(0, _currentProfile.PlayerStartRotation, 0));
            PlayerStress.Instance.AddStress(_currentProfile.InitialStress);
            for (int i = 0; i < _currentProfile.Items.Length; i++)
            {
                for (int a = 0; a < _currentProfile.Items[i].Amount; a++)
                {
                    PlayerInventory.Instance.AddToInventory(_currentProfile.Items[i].Item, false, false);
                }
            }
            for (int i = 0; i < _currentProfile.AreaProgress.Length; i++)
            {
                SaveSystem.Instance.Progress.SaveProgressOfType(_currentProfile.AreaProgress[i].AreaProgress.Id, _currentProfile.AreaProgress[i].Step);
            }
            for (int i = 0; i < _currentProfile.AreaProgress.Length; i++)
            {
                SaveSystem.Instance.Progress.SaveEntryProgressOfType(_currentProfile.AreaProgress[i].AreaProgress.Id, _currentProfile.AreaProgress[i].Step);
            }
            //SaveSystem.Instance.OnlySaveSpawnPosition = _currentProfile.OnlySaveSpawnPosition;
            _teleporter.Teleport();
            PlayerActions.Instance.ChangeInputMap("Player");
            PlayerMovement.Instance.ToggleMovement("MainMenu", true);
            Pause.Instance.ToggleCanPause("MainMenu", true);
            SavePoint.onSaveGame?.Invoke();
            _mainMenuSceneUnloader.UnloadScene();
        }
    }
}