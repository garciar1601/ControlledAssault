using UnityEngine;
using System.Collections.Generic;

public class SoundManager : MonoBehaviour {
    public AudioSource audioPrefab;
    public AudioClip hitSound;
    public AudioClip bgSound;
    public float volume = 1.0f;
    private List<AudioSource> audios = new List<AudioSource>();
	// Update is called once per frame
	void Update () 
    {
        List<AudioSource> deadAudios = new List<AudioSource>();
        foreach (AudioSource audio in audios)
        {
            if (!audio.isPlaying)
            {
                deadAudios.Add(audio);
            }
        }
        foreach (AudioSource audio in deadAudios)
        {
            audios.Remove(audio);
            Destroy(audio.gameObject);
        }
	}

    public void PlayHit()
    {
        AudioSource audio = Instantiate(audioPrefab) as AudioSource;
        audio.clip = hitSound;
        audio.volume = volume;
        audio.Play();
        audios.Add(audio);
    }

    public void PlayBackground()
    {
        AudioSource audio = Instantiate(audioPrefab) as AudioSource;
        audio.clip = bgSound;
        audio.volume = volume * .2f;
        audio.loop = true;
        audio.Play();
        audios.Add(audio);
    }

    public void StopSounds()
    {
        foreach (AudioSource audio in audios)
        {
            audio.Stop();
            Destroy(audio.gameObject);
        }
        audios.Clear();
    }
}
