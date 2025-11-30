using Luci;
using UnityEngine;

public class Entity : StageObject
{
    [SerializeField] public AudioClip collectSound;
    [SerializeField] private GameObject collectEffect;
    [SerializeField] private AudioSource source;

    protected virtual void OnCollect(GameObject collector)
    {
        if (collectSound)
        {
            source.clip = collectSound;
            source.Play();
        }

        if (collectEffect)
            Instantiate(collectEffect, transform.position, Quaternion.identity);

        Destroy(gameObject);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player")) // Ensure your player has this tag
        {
            OnCollect(other.gameObject);
        }
    }
}