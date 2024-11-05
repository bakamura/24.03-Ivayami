
using System;

namespace Ivayami.Save {
    [System.Serializable]
    public class SaveOptions {

        public float musicVol = 0.5f;
        public float sfxVol = 0.5f;

        public float cameraSensitivityX = 0.5f;
        public float cameraSensitivityY = 0.5f;
        public float cameraDeadzone = .125f;

        public bool invertCamera;

        public Int32 language;
        public LanguageTypes Language { get { return (LanguageTypes)language; } }

    }
}