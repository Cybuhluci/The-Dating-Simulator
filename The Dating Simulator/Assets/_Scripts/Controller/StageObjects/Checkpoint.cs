using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    [SerializeField] Transform spawnPoint; // override if different

    [Header("Visuals & Effects")]
    [SerializeField] GameObject pointLaser;
    [SerializeField] AudioSource audioSource;
    [SerializeField] Animator animatorL;
    [SerializeField] Animator animatorR;
    [SerializeField] bool _2D;

    private bool isCurrentCheckpoint = false;

    void OnTriggerEnter(Collider other)
    {
        if (!isCurrentCheckpoint && other.CompareTag("Player"))
        {
            MinaLifeSystem.Instance.UpdateCheckpoint(this);
        }
    }

    public void Activate()
    {
        isCurrentCheckpoint = true;

        if (pointLaser)
            pointLaser.SetActive(false);

        if (audioSource)
            audioSource.Play();

        if (animatorL)
            animatorL.Play(_2D ? "Checkpoint 2D" : "Checkpoint");

        if (animatorR)
            animatorR.Play(_2D ? "Checkpoint 2D" : "Checkpoint");
    }

    public void Deactivate()
    {
        isCurrentCheckpoint = false;

        // Reset the visual state so the checkpoint looks untouched
        if (pointLaser)
            pointLaser.SetActive(true);

        if (animatorL)
            animatorL.Play(_2D ? "Checkpoint 2D Reverse" : "Checkpoint Reverse");

        if (animatorR)
            animatorR.Play(_2D ? "Checkpoint 2D Reverse" : "Checkpoint Reverse");
    }

    public Transform GetRespawnPoint()
    {
        return spawnPoint != null ? spawnPoint : transform;
    }
}
