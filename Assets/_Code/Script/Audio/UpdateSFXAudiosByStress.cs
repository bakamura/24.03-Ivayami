using UnityEngine;
using FMOD.Studio;
using FMODUnity;
using Ivayami.Player;
using Ivayami.UI;

namespace Ivayami.Audio
{
    public class UpdateSFXAudiosByStress : MonoBehaviour
    {
        private PARAMETER_ID _variableId;

        private void Start()
        {
            RuntimeManager.StudioSystem.getParameterDescriptionByName("globalStress", out PARAMETER_DESCRIPTION description);
            _variableId = description.id;
            StressIndicatorSmoother.Instance.OnStressSmoothed.AddListener(HandleStressUpdate);
        }

        private void HandleStressUpdate(float value)
        {
            RuntimeManager.StudioSystem.setParameterByID(_variableId, value / PlayerStress.Instance.MaxStress);
        }
    }
}