using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuAudioBehavior : MonoBehaviour
{
    [Header("Sound effects")]
    [SerializeField] private AudioSource m_MoveSelectionAudio = null;
    [SerializeField] private AudioSource m_SelectButtonAudio = null;

    private void Awake()
    {
        if (m_MoveSelectionAudio == null || m_SelectButtonAudio == null)
            throw new MissingReferenceException("MenuAudioBehavior Awake(): Not all components initialized!");
    }
    public void PlayAudioChangeSelection()
    {
        
        m_MoveSelectionAudio.Play();
    }

    public void PlayAudioConfirmSelection()
    {
        
        m_SelectButtonAudio.Play();
    }
}
