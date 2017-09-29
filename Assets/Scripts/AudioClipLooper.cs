using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioClipLooper : MonoBehaviour {

    public AudioClip[] audioClips;

    private AudioSource[] audioSources;
    private bool startedIntro = false;
    private bool startedLoop = false;
    private int current = 0;

    private void Start()
    {
        audioSources = new AudioSource[audioClips.Length];
        for (int i = 0; i < audioClips.Length; i++)
        {
            audioSources[i] = gameObject.AddComponent<AudioSource>();
            audioSources[i].clip = audioClips[i];
            audioSources[i].playOnAwake = false;
        }
        audioSources[audioSources.Length - 1].loop = true;
        audioSources[current].Play();
        startedIntro = true;
    }
    void FixedUpdate()
    {
        if (!startedIntro)
        {
            return;
        }
        if (!audioSources[current].isPlaying)
        {
            current++;
            audioSources[current].Play();
            if (current == audioSources.Length)
            {
                Debug.Log("Done playing intros");
                startedLoop = true;
            }
        }
    }
}
