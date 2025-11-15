using UnityEngine;

public class CollectibleEntity : MonoBehaviour
{
    [SerializeField] public AudioClip collectSound;
    [SerializeField] private GameObject collectEffect;

    protected virtual void OnCollect(GameObject collector)
    {
        if (collectSound)
            AudioSource.PlayClipAtPoint(collectSound, transform.position);

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
