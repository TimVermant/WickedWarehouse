using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;


/// Author = Tim Vermant
/// <summary>
/// Class that manages the timer per day and keeps track of the current day
/// </summary>
public class TimeManager : Singleton<TimeManager>
{
    [Header("Timer variables")]
    [SerializeField] private float m_TimePerDay = 60.0f;
    [SerializeField] private int m_DifficultyThreshold = 5;

    [Header("Visual elements")]
    [SerializeField] private GameObject m_DayTextObject;
    [SerializeField] private GameObject m_ClockHandObject;

    // Time logic
    private int m_DayCounter;
    private float m_CurrentTime;
    private float m_ClockAngle = 0.0f;
    private float m_AngleMultiplier;
    private float m_TotalTime = 0.0f;


    // Difficulty variables
    private int m_DifficultyLevel = 1; 
    private float m_TimeSpeedMultiplier = 1.0f;


    // UI variables
    private TMP_Text m_DayText;
    private Image m_ClockHand;

    // Properties
    public int DifficultyLevel { get { return m_DifficultyLevel; } }

    public float TotalTime { get { return m_TotalTime; } }

    private void Awake()
    {
        m_DayCounter = 1;
        m_CurrentTime = 0.0f;

        m_AngleMultiplier = 360.0f / m_TimePerDay;



        if (m_DayTextObject != null)
            m_DayText = m_DayTextObject.GetComponent<TMP_Text>();
        if(m_ClockHandObject != null)
            m_ClockHand = m_ClockHandObject.GetComponent<Image>();
    }

    private void Update()
    {
        // Calculates time
        UpdateTimer();
        // Updates visual elements
        //UpdateText();
        

    }



    private void UpdateTimer()
    {
        // Get current time with timespeedmultiplier for potentially increasing difficulty
        m_CurrentTime += Time.deltaTime * m_TimeSpeedMultiplier;
        m_TotalTime += Time.deltaTime * m_TimeSpeedMultiplier;
        if (m_CurrentTime >= m_TimePerDay)
        {
            m_CurrentTime = 0.0f;
            m_DayCounter++;
            
            // Increase speed every {m_DifficultyThreshold} days
            if(m_DayCounter % m_DifficultyThreshold == 0)
            {
                m_TimeSpeedMultiplier += 0.1f;
                m_DifficultyLevel++;
            }
        }
        // Convert the current time to angles in 360 degrees
        m_ClockAngle = m_CurrentTime * m_AngleMultiplier;

    }

    //private void UpdateText()
    //{
       
        

    //    // Rotating clock using degrees
    //    m_ClockHand.transform.rotation = Quaternion.Euler(0.0f, 0.0f, -m_ClockAngle);


    //}


}
