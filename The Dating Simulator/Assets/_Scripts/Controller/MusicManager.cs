using Luci;
using UnityEngine;

public class MusicManager : MonoBehaviour
{
    MinaAttributes attributes;

    public static MusicManager Instance;
	public AudioSource MainAudio, DrowningAudio;
	public AudioClip main_music;
	public AudioClip results_music_start, results_music_loop;
	public AudioClip underwater_music;
	public AudioClip drowning_music;

	// Use this for initialization
	void Start() 
	{
		Instance = this;
        attributes = FindAnyObjectByType<MinaAttributes>().GetComponent<MinaAttributes>();
		MainAudio.clip = main_music;
		MainAudio.Play();
	}

	public void set_audio(AudioClip audio, bool is_looping=true)
	{
		MainAudio.loop = is_looping;
		MainAudio.Stop ();
		MainAudio.clip = audio;
		MainAudio.PlayDelayed(1f);
	}
}
