using UnityEngine;

namespace Ivayami.Save {
    public class EnemyEntryProgressRef : SaveProgressRef {

        private void OnTriggerEnter(Collider other) {
            SetEntryProgressTo(0);
        }

        public void DiscoverSpecialInfo() {
            SetEntryProgressTo(1);
        }

        // Shouldn't be called
        public override void SetProgressTo(int progress) { }

    }
}
