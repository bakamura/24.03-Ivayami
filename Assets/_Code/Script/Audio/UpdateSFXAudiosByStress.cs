using UnityEngine;
using FMODUnity;
using FMOD.Studio;
using Ivayami.Player;

namespace Ivayami.Audio
{
    public class UpdateSFXAudiosByStress : MonoBehaviour
    {
        private PARAMETER_ID _variableId;

        private void Start()
        {
            RuntimeManager.StudioSystem.getParameterDescriptionByName("globalStress", out PARAMETER_DESCRIPTION description);
            _variableId = description.id;
            PlayerStress.Instance.onStressChange.AddListener(HandleStressUpdate);
        }

        private void HandleStressUpdate(float value)
        {
            RuntimeManager.StudioSystem.setParameterByID(_variableId, value / PlayerStress.Instance.MaxStress);
        }
    }
}