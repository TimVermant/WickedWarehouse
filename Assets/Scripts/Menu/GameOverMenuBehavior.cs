using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using TMPro;

public class GameOverMenuBehavior : MonoBehaviour
{

    [SerializeField] private GameObject m_ReturnButton = null;
    [SerializeField] private GameObject m_QuitButton = null;
    [SerializeField] private GameObject m_ScoreText = null;
    [SerializeField] private GameObject m_LevelLoader;


    private void OnEnable()
    {

        if (m_ReturnButton == null || m_ScoreText == null || m_QuitButton == null)
            throw new MissingReferenceException("GameOverMenuBehavior OnEnable(): Not all components initialized!");

        EventSystem.current.SetSelectedGameObject(m_ReturnButton);

        // Set Score text
        m_ScoreText.GetComponent<TMP_Text>().SetText($"Score: {(int)ScoreManager.Instance.CurrentScore}");


    }


    public void OpenMenu()
    {
   

        LevelLoader.Instance.GetComponent<LevelLoader>().DoLevelTransition = true;

    }

    public void QuitGame()
    {
        Application.Quit();
    }


}
