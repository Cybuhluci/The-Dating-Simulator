using UnityEngine;

namespace Luci
{
    public class HomingTarget : MonoBehaviour
    {
        [Tooltip("Where the reticle should appear. If not assigned, defaults to this object.")]
        public Transform reticlePosition;

        private void Awake()
        {
            if (reticlePosition == null)
                reticlePosition = transform;
        }
    }
}