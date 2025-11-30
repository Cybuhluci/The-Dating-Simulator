using UnityEngine;

namespace Luci
{
    public class SonicDeathZone : MonoBehaviour
    {
        public LayerMask playerlayer;
        public bool active;

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player") || other.gameObject.layer == playerlayer)
            {
                // Assuming you have a central manager or player script that handles lives
                MinaLifeSystem.Instance.PlayerDied();
            }
        }
    }
}