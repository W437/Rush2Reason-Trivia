using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;
    // List of SFX clips
    public AudioClip[] sfxClips;

    public AudioSource sfxSource;

    public AudioSource musicSource;

    private void Awake()
    {
       Instance = this;
    }

    void Start()
    {
        // Start playing music
        musicSource.Play();
    }

    public void PlaySFX(int clipIndex)
    {
        if (clipIndex >= 0 && clipIndex < sfxClips.Length)
        {
            sfxSource.clip = sfxClips[clipIndex];
            sfxSource.Play();
        }
        else
        {
            Debug.LogError("Invalid SFX clip index!");
        }
    }
}
