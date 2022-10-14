using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MainMenuBehavior : MonoBehaviour
{


    private void Awake()
    {
        Time.timeScale = 1;
    }


    public void PlayGame()
    {

        LevelLoader.Instance.GetComponentInChildren<Image>().color = Color.white;
        LevelLoader.Instance.GetComponent<LevelLoader>().DoLevelTransition = true;


    }


    public void QuitGame()
    {
        Application.Quit();
    }


}
