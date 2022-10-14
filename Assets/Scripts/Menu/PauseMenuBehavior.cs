using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class PauseMenuBehavior : MonoBehaviour
{

    [SerializeField] private GameObject m_ResumeButton = null;

    
    private void OnEnable()
    {
        
        EventSystem.current.SetSelectedGameObject(m_ResumeButton);

    }

}
