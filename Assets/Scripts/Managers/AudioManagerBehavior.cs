using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : Singleton<AudioManager>
{
    private List<AudioSource> m_AudioSources = new List<AudioSource>();
    private List<AudioSource> m_BackgroundNoise = new List<AudioSource>();

    public List<AudioSource> AudioSources
    {
        get { return m_AudioSources; }

    }

    public List<AudioSource> BackgroundNoise
    {
        get { return m_BackgroundNoise; }

    }

    public void AddAudio(AudioSource audioSource)
    {
        if (audioSource != null)
            m_AudioSources.Add(audioSource);
    }

    public void AddMusic(AudioSource audioSource)
    {
        if (audioSource != null)
            m_BackgroundNoise.Add(audioSource);
    }

    public void StartPlayingBackground()
    {
        foreach (AudioSource source in m_BackgroundNoise)
        {
            source.Play();
        }
    }

    public void AudioOnGamePause()
    {
        foreach (AudioSource source in m_AudioSources)
        {
            source.Pause();
        }

        foreach (AudioSource source in m_BackgroundNoise)
        {
            source.Pause();
        }
    }

    public void AudioOnGameContinue()
    {
        print("Continue");
        foreach (AudioSource source in m_AudioSources)
        {
            source.UnPause();
        }

        foreach (AudioSource source in m_BackgroundNoise)
        {
            source.UnPause();
        }
    }
}
