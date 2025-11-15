using UnityEngine;

public class LuciParticleToggler : MonoBehaviour
{
    public static LuciParticleToggler Instance;
    public ParticleSystem[] Particles;

    private void Start()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    public void ToggleEffects(bool value)
    {
        foreach (ParticleSystem thisParticleSystem in Particles)
        {
            ParticleSystem.EmissionModule emissionModule = thisParticleSystem.emission;

            emissionModule.enabled = value;
        }
    }
}
