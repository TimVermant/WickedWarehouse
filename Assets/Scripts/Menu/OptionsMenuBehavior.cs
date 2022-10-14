using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class OptionsMenuBehavior : MonoBehaviour
{
    [SerializeField] GameObject m_BackButton = null;
    [SerializeField] GameObject m_MenuOptionsButton = null;

    private void Awake()
    {
        if (m_BackButton == null || m_MenuOptionsButton == null)
            throw new MissingReferenceException("MenuAudioBehavior Awake(): Not all components initialized!");
    }

    private void OnEnable()
    {
        EventSystem.current.SetSelectedGameObject(m_BackButton);
    }

    private void OnDisable()
    {
        EventSystem.current.SetSelectedGameObject(m_MenuOptionsButton);
    }
}
