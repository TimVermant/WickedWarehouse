using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;


public class LevelLoader : Singleton<LevelLoader>
{
    [SerializeField] private Animator m_Transition;
    [SerializeField] private float m_TransitionTime = 1.0f;
    public int m_Level = -1;
    private LevelIntro m_LevelIntro;

    private bool m_DoLevelTransition = false;
    public bool DoLevelTransition
    {
        get { return m_DoLevelTransition; }
        set { m_DoLevelTransition = value; }
    }

  



    private void Update()
    {

        if (m_DoLevelTransition)
        {
            
            LoadNextLevel();
            m_DoLevelTransition = false;
         
        }


    }


    public void LoadNextLevel()
    {

        int sceneIndex = SceneManager.GetActiveScene().buildIndex + 1;
        if (sceneIndex >= SceneManager.sceneCountInBuildSettings)
        {
            sceneIndex = 0;
        }
        if (m_Level != -1)
            sceneIndex = m_Level;

        StartCoroutine(LoadLevel(sceneIndex));
        

    }

    public IEnumerator LoadLevel(int sceneIndex)
    {
        m_Transition.SetTrigger("Start");
      

        yield return new WaitForSecondsRealtime(m_TransitionTime);

        SceneManager.LoadScene(sceneIndex);

        StartCoroutine(ResetLevel());
        
      

    }

    public IEnumerator ResetLevel()
    {


        yield return new WaitForSeconds(m_TransitionTime * 0.5f);

        m_Transition.SetTrigger("End");
        
    }

}
