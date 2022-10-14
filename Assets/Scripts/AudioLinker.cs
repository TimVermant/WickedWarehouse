using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioLinker : MonoBehaviour
{
    void Start()
    {
        var audioSources = GetComponents<AudioSource>();
        foreach (AudioSource audioSource in audioSources)
        {
            AudioManager.Instance.AddAudio(audioSource);
        }
    }


}
