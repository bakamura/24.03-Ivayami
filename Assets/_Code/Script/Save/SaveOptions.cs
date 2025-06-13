
using System;

namespace Ivayami.Save {
    [Serializable]
    public class SaveOptions {

        public float musicVol = 0.5f;
        public float sfxVol = 0.5f;

        public float cameraSensitivityX = 0.5f;
        public float cameraSensitivityY = 0.5f;
        public float cameraDeadzone = .125f;
        public float movementDeadzone = .125f;
        public float brightness;

        public bool invertCamera;
        public bool holdToRun;

        public Int32 language;
    }
}