using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MusicChanger : MonoBehaviour
{
    [SerializeField] private AudioClip newTrack; // The new music track to play
    public AudioSource audioSrc;

    private void OnTriggerEnter(Collider other)
    {
        // Change the music track only if it's different from the current one
        if (audioSrc.clip != newTrack)
        {
            audioSrc.clip = newTrack;
            audioSrc.Play();
        }
    }

    void OnDrawGizmos()
    {
        Color cube_color = new Color(0, 100, 0, .2f);
        Gizmos.color = cube_color;
        Gizmos.DrawCube(transform.position, this.transform.localScale);
    }
}
