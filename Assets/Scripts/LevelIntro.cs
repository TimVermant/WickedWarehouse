using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.GlobalIllumination;

[RequireComponent(typeof(AudioSource))]
public class LevelIntro : MonoBehaviour
{
    [Header("Light settings")]
    [SerializeField] private List<GameObject> m_Lights = new List<GameObject>();
    [SerializeField] private float m_LightEnableTime = 0.5f;

    [Header("Timing settings")] [SerializeField]
    private float m_OrderDelay = 1.0f;
    private const float ANIMATIONTIMERDURATION = 3.0f;
    private bool m_Finished = false;
    private List<bool>m_FinishList = new List<bool>();
    private AudioSource m_StartSound;
    [SerializeField] private AudioSource m_TimerSound;
    private Animator m_TimerAnimator;


    // Start is called before the first frame update
    void Awake()
    {
        foreach (GameObject light in m_Lights)
        {
            m_FinishList.Add(false);
            light.GetComponent<Light>().enabled = false;
        }
        m_StartSound = GetComponent<AudioSource>();
        if (m_StartSound.clip == null)
            throw new Exception("Levelintro: No clip found on audio source");
        m_TimerAnimator = GetComponentInChildren<Animator>();
        if (m_TimerAnimator == null)
            throw new Exception("Levelintro: No animator found");


        AudioManager.Instance.AddAudio(m_StartSound);
        StartCoroutine(nameof(StartEnabling));
    }

    void Update()
    {
        if (m_Finished)
            return;
        if (m_FinishList.Contains(false))
            return;
        else
            m_Finished = true;

       // m_StartSound.Play();
        StartCoroutine(nameof(InitializeManagers));
    }

    public IEnumerator StartEnabling()
    {
        yield return new WaitForSecondsRealtime(3.0f);
        for (int i = 0; i < m_Lights.Count; i++)
        {
            StartCoroutine(nameof(Enablelights), i);
        }
    }

    private IEnumerator Enablelights(int currentLightId)
    {
        yield return new WaitForSecondsRealtime((currentLightId +1) * m_LightEnableTime);
        var lightSource = m_Lights[currentLightId].GetComponent<Light>();

        var audioSource = m_Lights[currentLightId].GetComponent<AudioSource>();
        if (audioSource != null)
            audioSource.Play();
        else
            throw new Exception("No audio source found on this light");

        yield return new WaitForSecondsRealtime(.5f);

        if (lightSource != null)
            lightSource.enabled = true;
        else
            throw new Exception("No light found on this light");
        m_FinishList[currentLightId] = true;
    }

    private IEnumerator InitializeManagers()
    {
        yield return new WaitForSecondsRealtime(m_StartSound.clip.length);

        GameManager.Instance.Initialize();
        BoxSpawnerManager.Instance.Initialize();
        StartCoroutine(nameof(InitializeOrders));
    }

    private IEnumerator InitializeOrders()
    {
        m_TimerSound.Play();
        m_TimerAnimator.SetBool("Play",true);
        yield return new WaitForSecondsRealtime(m_OrderDelay + ANIMATIONTIMERDURATION);
        m_TimerAnimator.SetBool("Play", false);
        m_StartSound.Play();
        OrderManager.Instance.Initialize();
    }
}
