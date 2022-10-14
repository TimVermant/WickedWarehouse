using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;

public class PerformanceMeter : Singleton<PerformanceMeter>
{
    [Header("Performance values")]
    [SerializeField] private float m_MaxSatisfaction = 150.0f;
    [SerializeField][Range(0.0f, 100.0f)] private float m_BeginSatisfactionPercentage = 60.0f;

    [Space(10)]
    [Header("Boss image UI")]
    [SerializeField] private Image m_BossTargetImage;
    [SerializeField] private Sprite m_BossHappy;
    [SerializeField] private Sprite m_BossNeutral;
    [SerializeField] private Sprite m_BossMad;

    [Space(10)]
    [SerializeField] private float m_SliderChangeSpeed = 12.5f;

    [Space(10)]
    [Header("Performance visualisation")]
    [SerializeField] private GameObject m_SteamParticleObject;
    private List<GameObject> m_SteamParticlesList = new List<GameObject>();

    [SerializeField] private Volume m_GlobalVolume;
    private Vignette m_Vignette;

    private float m_LowVolumeIntensity = 0.55f;
    private float m_MediumVolumeIntensity = 0.6f;
    private float m_HighVolumeIntensity = 0.65f;

    private float m_StartValuePulsing = 0.17f;
    private float m_MinValuePulsing = 0.05f;
    private float m_MaxValuePulsing = 0.25f;
    private float m_CurrentValuePulsing = 0.17f;
    private bool m_IsVignettePulsing = false;
    private float m_PulsingValueDirection = 1.0f;

    private Slider m_Slider;
    private float m_CurrentSatisfaction;
    private float m_TargetSatisfaction;

    private void Start()
    {

        if (m_SteamParticleObject)
        {
            foreach (ParticleSystem particle in m_SteamParticleObject.GetComponentsInChildren<ParticleSystem>())
            {
                m_SteamParticlesList.Add(particle.gameObject);

            }
        }
        m_TargetSatisfaction = m_MaxSatisfaction * (m_BeginSatisfactionPercentage / 100.0f);
        m_CurrentSatisfaction = m_TargetSatisfaction;

        m_Slider = GetComponentInChildren<Slider>();
        SetSliderValue();

        //m_GlobalVolume = FindObjectOfType<Volume>();
        if (!m_GlobalVolume)
            throw new MissingReferenceException("PerformanceMeter Start(): Not all serialized variables initialized!");

        m_GlobalVolume.sharedProfile.TryGet<Vignette>(out m_Vignette);

        m_CurrentValuePulsing = m_StartValuePulsing;
    }

    private void Update()
    {
        if (m_CurrentSatisfaction <= 0 && GameManager.Instance.CurrentState != GameState.GameOver)
            GameManager.Instance.ChangeGameState(GameState.GameOver);

        if (m_CurrentSatisfaction < m_TargetSatisfaction - 1.0f ||
            m_CurrentSatisfaction > m_TargetSatisfaction + 1.0f)
        {
            ChangeSlider();

        }

        if(m_IsVignettePulsing)
        {
            m_CurrentValuePulsing += (Time.deltaTime * 0.2f) * m_PulsingValueDirection;
            if(m_CurrentValuePulsing > m_MaxValuePulsing)
            {
                m_PulsingValueDirection = -1.0f;
            }
            if(m_CurrentValuePulsing < m_MinValuePulsing)
            {
                m_PulsingValueDirection = 1.0f;
            }
            m_GlobalVolume.sharedProfile.TryGet<Vignette>(out var vignette);

            vignette.smoothness.overrideState = true;
            vignette.smoothness.Override(m_CurrentValuePulsing);
          
        }
    }

    public void RemoveSatisfaction(float removeAmount)
    {
        m_TargetSatisfaction -= removeAmount;
    }

    public void AddSatisfaction(float addAmount)
    {
        m_TargetSatisfaction += addAmount;
        m_TargetSatisfaction = Mathf.Clamp(m_TargetSatisfaction, 0.0f, m_MaxSatisfaction);
    }

    private void SetSliderValue()
    {
        m_Slider.value = m_CurrentSatisfaction / m_MaxSatisfaction;
        SetBossImage();

    }


    private void SetBossImage()
    {
        float satisfactionPercentage = m_CurrentSatisfaction / m_MaxSatisfaction * 100.0f;
        satisfactionPercentage = Mathf.Clamp(satisfactionPercentage, 0.0f, 100.0f);

        VolumeProfile profile = m_GlobalVolume.sharedProfile;
        if (!profile.TryGet<Vignette>(out var vignette))
        {
            throw new MissingReferenceException("PerformanceMeter SetBossImage(): Cannot find effect of type [Vignette] inside global volume!");
        }


        vignette.intensity.overrideState = true;

        int steamDisplayAmount;
        if (satisfactionPercentage <= 33.0f)
        {
            m_IsVignettePulsing = true;
            m_BossTargetImage.sprite = m_BossMad;
            steamDisplayAmount = m_SteamParticlesList.Count + 1;

            vignette.intensity.Override(m_HighVolumeIntensity);

        }
        else if (satisfactionPercentage <= 66.0f)
        {
            m_IsVignettePulsing = false;
            m_BossTargetImage.sprite = m_BossNeutral;
            steamDisplayAmount = m_SteamParticlesList.Count / 2;
            vignette.intensity.Override(m_MediumVolumeIntensity);
        }
        else
        {
            m_IsVignettePulsing = false;
            m_BossTargetImage.sprite = m_BossHappy;
            steamDisplayAmount = 0;
            vignette.intensity.Override(m_LowVolumeIntensity);
        }


        for (int i = 0; i < m_SteamParticlesList.Count; i++)
        {
            if (i < steamDisplayAmount)
            {
                m_SteamParticlesList[i].SetActive(true);
            }
            else
            {
                m_SteamParticlesList[i].SetActive(false);
            }
        }
    }




    void ChangeSlider()
    {
        if (m_TargetSatisfaction > m_CurrentSatisfaction)
            m_CurrentSatisfaction += Time.deltaTime * m_SliderChangeSpeed;
        else
            m_CurrentSatisfaction -= Time.deltaTime * m_SliderChangeSpeed;

        SetSliderValue();
    }
}


