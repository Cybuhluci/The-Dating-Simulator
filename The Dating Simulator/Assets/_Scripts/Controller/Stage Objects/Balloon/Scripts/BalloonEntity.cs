using UnityEngine;

namespace RagdollEngine
{
    public class BalloonEntity : Entity
    {
        [SerializeField] Animator animator;

        [SerializeField] ParticleSystem[] particleSystems;

        [SerializeField] AudioSource audioSourceInstance;

        public void Break()
        {
            Instantiate(audioSourceInstance).transform.position = transform.position;

            animator.SetTrigger("Balloon");

            foreach (ParticleSystem thisParticleSystem in particleSystems)
            {
                ParticleSystem newParticleSystem = Instantiate(thisParticleSystem);

                newParticleSystem.transform.position = transform.position;
            }

            animator.Play("Break");
        }
    }
}
