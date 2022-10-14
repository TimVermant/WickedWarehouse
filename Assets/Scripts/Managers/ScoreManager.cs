using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScoreManager : Singleton<ScoreManager>
{

    static private float m_CurrentScore = 0.0f;

    [Header("Score values")]
    [SerializeField] private float m_OrderCompletionValue = 50.0f;
    [SerializeField] private float m_OrderLeftoverTimeValue = 1.0f;
    [SerializeField] private float m_BackgroundTimerValue = 1.0f;


    public float CurrentScore
    {
        get { return m_CurrentScore; }
        set { m_CurrentScore = value;  }
    }


    public void CompleteOrder(float timeLeft)
    {
        CurrentScore += m_OrderCompletionValue;
        CurrentScore += ((int)timeLeft * m_OrderLeftoverTimeValue);
    }


    public void CompleteGame()
    {
        CurrentScore += (int)TimeManager.Instance.TotalTime * m_BackgroundTimerValue;

    }
   
}
