using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

/// Author: Tristan Wauthier
/// <summary>
/// This class collects orders and notifies the ordermanager if one has been completed.
/// </summary>
[RequireComponent(typeof(BoxCollider))]
public class CollectionZone : MonoBehaviour
{
    //Editor assigned fields
    [Header("Logic")]
    [SerializeField] private int m_DynamicBoxLayer = 11;
    [SerializeField] private int m_StaticLevelLayer = 8;

    [Header("Order debug")]
    [SerializeField] private GameObject m_OrderDisplay = null;
    [SerializeField] private Slider m_FrontTimer = null;
    [SerializeField] private GameObject m_SliderObject = null;

    //UI Gameobjects
    private GameObject m_CardBoardUI;
    private GameObject m_WoodBoxUI;
    private GameObject m_MetalBoxUI;

    private GameObject m_FillBackgroundScreenUI;
    private GameObject m_ClockTimerUI;

    private GameObject m_SuccesUI;
    private GameObject m_FailureUI;

    //UI Components
    private TextMeshProUGUI m_OrderCardboardText;
    private TextMeshProUGUI m_OrderWoodText;
    private TextMeshProUGUI m_OrderMetalText;
    private TextMeshPro m_OrderTimeText;
    private Image m_ClockImage;


    private List<GameObject> m_ZoneDisplayListUI = new List<GameObject>();

    [Header("Player feedback")]
    [SerializeField] private GameObject m_CollectionArrow = null;
    [SerializeField] private GameObject m_CollectionNumber = null;
    [SerializeField] private Material m_GradientMaterial = null;
    [SerializeField] private Material m_GradientMaterialEmissive = null;
    // Arrow flashing
    [SerializeField] private float m_FlashDuration = 0.5f;
    private float m_CurrentFlashTime = 0.0f;
    private bool m_IsFlashing = false;

    [Header("Audio")]
    [SerializeField] private AudioSource m_AudioOrderComplete = null;
    [SerializeField] private AudioSource m_AudioOrderFail = null;
    [SerializeField] private AudioSource m_AudioOrderAlarm = null;
    [SerializeField] private AudioSource m_AudioBoxSnap = null;

    [Header("Particles")]
    [SerializeField] private GameObject m_ParticlesOrderComplete = null;
    [SerializeField] private GameObject m_ParticlesOrderFail = null;

    // Score calculations
    private float m_LeftOverTime = 0.0f;

    // Drop alarm
    [Header("Alarm variables")]
    [SerializeField] private float m_DropAlarmThreshold = 5.0f;
    private bool m_IsDropAlarm = false;

    [Header("Ghost spawn points")]
    [SerializeField] private Transform m_CardboardGhostSpawn;
    [SerializeField] private Transform m_WoodGhostSpawn;
    [SerializeField] private Transform m_MetalGhostSpawn;

    [Header("Box prefabs")]
    [SerializeField] private float m_BoxHeight = 1.0f;
    [SerializeField] private GameObject m_CardboardPrefab;
    [SerializeField] private GameObject m_WoodPrefab;
    [SerializeField] private GameObject m_MetalPrefab;

    //Other fields
    private List<int> m_RequiredAmounts = new List<int>();

    private int m_RequiredAmountCardboard = 4;
    private int m_CurrentAmountCardboard = 0;
    public int RequiredAmountCardboard
    {
        get { return m_RequiredAmountCardboard; }
        set { m_RequiredAmountCardboard = value; }
    }

    private int m_RequiredAmountWood = 2;
    private int m_CurrentAmountWood = 0;
    public int RequiredAmountWood
    {
        get { return m_RequiredAmountWood; }
        set { m_RequiredAmountWood = value; }
    }

    private int m_RequiredAmountMetal = 1;
    private int m_CurrentAmountMetal = 0;
    public int RequiredAmountMetal
    {
        get { return m_RequiredAmountMetal; }
        set { m_RequiredAmountMetal = value; }
    }

    private bool m_IsActive = false;
    public bool IsActive
    {
        get { return m_IsActive; }
        set => m_IsActive = value;
    }

    private float m_OrderCompleteCooldown = 1.50f;
    public float OrderCompleteCooldown
    {
        set => m_OrderCompleteCooldown = value;
    }

    private float m_SatisfactionOnComplete = 10.0f;
    private float m_SatisfactionOnFail = 15.0f;

    private List<GameObject> m_CardBoardGhosts = new List<GameObject>();
    private List<GameObject> m_WoodGhosts = new List<GameObject>();
    private List<GameObject> m_MetalGhosts = new List<GameObject>();
    private List<GameObject> m_CollectedBoxes = new List<GameObject>();

    // Const strings used by Find()
    private const string DESTROYSTRING = "Destroy";

    private const string CARDBOARDBOX = "BoxType_Placeholder_top";
    private const string WOODBOX = "BoxType_Placeholder_mid";
    private const string METALBOX = "BoxType_Placeholder_bot";
    private const string CLOCKTIMER = "Timer_Positioning";

    private const string SUCCESUI = "OrderSucces";
    private const string FAILUREUI = "OrderFail";
    private const string FILLSCREENUI = "Fill_Area";


    // Animations
    private Animator m_Animator;
    private List<Animator> m_AlarmAnimators = new List<Animator>();

    //Properties
    private OrderManager m_OrderManager;
    public OrderManager OrderManager
    { set { m_OrderManager = value; } }

    private float m_StartOrderTimer;
    private float m_OrderTimer;

    public float OrderTimer
    {
        get { return m_OrderTimer; }
        set { m_OrderTimer = value; m_StartOrderTimer = value; }
    }

    private void Awake()
    {
        // Filling up required amounts list
        m_RequiredAmounts.Add(m_RequiredAmountCardboard);
        m_RequiredAmounts.Add(m_RequiredAmountWood);
        m_RequiredAmounts.Add(m_RequiredAmountMetal);



        // Getting the required UI gameobjects from the m_OrderDisplay gameobject
        m_CardBoardUI = m_OrderDisplay.transform.Find(CARDBOARDBOX).gameObject;
        m_WoodBoxUI = m_OrderDisplay.transform.Find(WOODBOX).gameObject;
        m_MetalBoxUI = m_OrderDisplay.transform.Find(METALBOX).gameObject;
        m_ClockTimerUI = m_OrderDisplay.transform.Find(CLOCKTIMER).gameObject;
        m_ClockImage = m_ClockTimerUI.GetComponentInChildren<Image>();



        m_SuccesUI = m_OrderDisplay.transform.Find(SUCCESUI).gameObject;
        m_FailureUI = m_OrderDisplay.transform.Find(FAILUREUI).gameObject;
        m_FillBackgroundScreenUI = m_OrderDisplay.transform.Find(FILLSCREENUI).gameObject;


        m_ZoneDisplayListUI.Add(m_CardBoardUI);
        m_ZoneDisplayListUI.Add(m_WoodBoxUI);
        m_ZoneDisplayListUI.Add(m_MetalBoxUI);


        // Getting the required UI components from the found gameobjects
        m_OrderCardboardText = m_CardBoardUI.GetComponentInChildren<TextMeshProUGUI>();
        m_OrderWoodText = m_WoodBoxUI.GetComponentInChildren<TextMeshProUGUI>();
        m_OrderMetalText = m_MetalBoxUI.GetComponentInChildren<TextMeshProUGUI>();
        m_OrderTimeText = transform.GetComponentInChildren<TextMeshPro>();

        // Animation setup
        m_Animator = GetComponentInChildren<Animator>();
        m_AlarmAnimators.AddRange(GetComponentsInChildren<Animator>());
        m_AlarmAnimators.Remove(m_Animator);

        // Add audio to audio manager
        AudioManager.Instance.AddAudio(m_AudioBoxSnap);
        AudioManager.Instance.AddAudio(m_AudioOrderFail);
        AudioManager.Instance.AddAudio(m_AudioOrderAlarm);
        AudioManager.Instance.AddAudio(m_AudioOrderComplete);

    }

    private void Update()
    {
        if (m_IsActive == false)
        {
            DisableColllectionZone();
            return;
        }
        m_SliderObject.GetComponent<Image>().enabled = true;

        UpdateArrowFlashing();



        m_ClockImage.gameObject.SetActive(true);
        m_OrderTimeText.gameObject.SetActive(true);

        // Check whether we need to display that certain element
        for (int i = 0; i < m_ZoneDisplayListUI.Count; i++)
        {
            if (m_RequiredAmounts[i] <= 0)
            {
                m_ZoneDisplayListUI[i].SetActive(false);
            }
            else
            {
                m_ZoneDisplayListUI[i].SetActive(true);
            }
        }


        // Display required box type
        m_OrderCardboardText.text = (m_RequiredAmountCardboard - m_CurrentAmountCardboard).ToString();
        m_OrderWoodText.text = (m_RequiredAmountWood - m_CurrentAmountWood).ToString();
        m_OrderMetalText.text = (m_RequiredAmountMetal - m_CurrentAmountMetal).ToString();

        // Update timer visualization
        m_OrderTimeText.text = Mathf.CeilToInt(m_OrderTimer).ToString();
        float timerPercentage = m_OrderTimer / m_StartOrderTimer;


        m_ClockImage.fillAmount = timerPercentage;
        if (m_FrontTimer)
        {
            m_FrontTimer.value = timerPercentage;

        }
        m_FillBackgroundScreenUI.transform.localScale = new Vector3(1.0f, timerPercentage, 1.0f);

        if (IsOrderComplete())
        {
            m_OrderTimer = 0;
        }

        if (m_OrderTimer > 0)
        {
            m_OrderTimeText.color = Color.green;
            m_OrderTimer -= Time.deltaTime;
            if ((m_OrderTimer <= m_DropAlarmThreshold) && !m_IsDropAlarm)
            {
                AlarmTrigger();
            }
        }
        else
            ProcessOrder();

    }

    private void DisableColllectionZone()
    {
        m_CardBoardUI.SetActive(false);
        m_WoodBoxUI.SetActive(false);
        m_MetalBoxUI.SetActive(false);

        m_OrderTimeText.gameObject.SetActive(false);
        m_FillBackgroundScreenUI.transform.localScale = Vector3.zero;
        m_ClockImage.gameObject.SetActive(false);

        // Player feedback
        m_CollectionArrow.GetComponent<Renderer>().sharedMaterial = m_GradientMaterial;
        m_CollectionNumber.GetComponent<Renderer>().sharedMaterial = m_GradientMaterial;
        m_CurrentFlashTime = 0.0f;
        m_LeftOverTime = 0.0f;

        // Turn green slider off
        m_SliderObject.GetComponent<Image>().enabled = false;
    }

    public void AlarmTrigger()
    {

        // Drop timer to threshhold
        m_OrderTimer = m_DropAlarmThreshold;
        m_IsDropAlarm = true;
    }

    private void UpdateArrowFlashing()
    {

        // Alarm animation + audio feedback
        if (m_IsDropAlarm)
        {
            if (m_AudioOrderAlarm)
            {
                if (!m_AudioOrderAlarm.isPlaying && GameManager.Instance.CurrentState == GameState.Game)
                    m_AudioOrderAlarm.Play();
            }

            foreach (Animator animator in m_AlarmAnimators)
                animator.SetBool("AlarmRed", true);
        }

        // Update arrow flashing when active
        m_CollectionNumber.GetComponent<Renderer>().sharedMaterial = m_GradientMaterialEmissive;


        m_CurrentFlashTime += Time.deltaTime;
        if (m_CurrentFlashTime >= m_FlashDuration)
        {
            m_CurrentFlashTime = 0;
            if (m_IsFlashing)
            {
                m_IsFlashing = false;
                m_CollectionArrow.GetComponent<MeshRenderer>().material = m_GradientMaterial;
            }
            else
            {
                m_IsFlashing = true;
                m_CollectionArrow.GetComponent<MeshRenderer>().material = m_GradientMaterialEmissive;
            }
        }
    }

    private void OnTriggerStay(Collider other)
    {

        if (!m_IsActive || other.CompareTag(DESTROYSTRING)) return;

        BoxBehaviour box = other.GetComponent<BoxBehaviour>();
        if (box == null || other.gameObject.layer != m_DynamicBoxLayer)
            return;

        if (!NeedBoxType(box.BoxType)) return;

        if (m_AudioBoxSnap)
        {
            if (!m_AudioBoxSnap.isPlaying)
            {
                m_AudioBoxSnap.Play();
            }
        }
        CollectBox(box);
    }

    private bool NeedBoxType(BoxBehaviour.Type type)
    {
        switch (type)
        {
            case BoxBehaviour.Type.cardboard:
                if (m_CurrentAmountCardboard < m_RequiredAmountCardboard) return true;
                break;
            case BoxBehaviour.Type.wood:
                if (m_CurrentAmountWood < m_RequiredAmountWood) return true;
                break;
            case BoxBehaviour.Type.metal:
                if (m_CurrentAmountMetal < m_RequiredAmountMetal) return true;
                break;
        }
        return false;
    }

    private void CollectBox(BoxBehaviour box)
    {
        SnapBox(box);



        switch (box.BoxType)
        {
            case BoxBehaviour.Type.cardboard:

                m_CurrentAmountCardboard++;
                break;
            case BoxBehaviour.Type.wood:
                m_CurrentAmountWood++;
                break;
            case BoxBehaviour.Type.metal:
                m_CurrentAmountMetal++;
                break;
        }
        if (IsOrderComplete() && m_LeftOverTime == 0.0f)
        {
            // Extra time that gets added at the end for bonus points
            m_LeftOverTime = m_StartOrderTimer - m_OrderTimer;
        }
        m_CollectedBoxes.Add(box.gameObject);
        StripBox(box);

    }

    private bool IsOrderComplete()
    {
        return m_RequiredAmountWood == m_CurrentAmountWood &&
           m_RequiredAmountMetal == m_CurrentAmountMetal &&
           m_RequiredAmountCardboard == m_CurrentAmountCardboard;
    }

    private void ProcessOrder()
    {
        foreach (Animator animator in m_AlarmAnimators)
        {
            animator.SetBool("AlarmGreen", false);
            animator.SetBool("AlarmRed", false);
        }
        bool orderSucces = IsOrderComplete();
        m_IsDropAlarm = false;
        m_AudioOrderAlarm.Stop();

        ResetGhosts();
        MakeBoxesDrop();

        m_CurrentAmountCardboard = 0;
        m_CurrentAmountWood = 0;
        m_CurrentAmountMetal = 0;

        if (orderSucces)
        {
            PerformanceMeter.Instance.AddSatisfaction(m_SatisfactionOnComplete);
            ScoreManager.Instance.CompleteOrder(m_LeftOverTime);
            OrderCompleteFeedback();
        }
        else
        {
            PerformanceMeter.Instance.RemoveSatisfaction(m_SatisfactionOnFail);
            OrderFailureFeedback();
        }
        m_IsActive = false;
        _ = StartCoroutine(nameof(WaitForNewOrder), orderSucces);

        if (m_Animator != null)
            m_Animator.Play("CollectZone");
    }

    private void OrderCompleteFeedback()
    {
        m_SuccesUI.SetActive(true);
        foreach (Animator animator in m_AlarmAnimators)
        {
            animator.SetBool("AlarmGreen", true);
            StartCoroutine(nameof(TurnOfLight), animator);
        }


        if (m_AudioOrderComplete)
            m_AudioOrderComplete.Play();


        Instantiate(m_ParticlesOrderComplete, transform.position, transform.rotation);
    }

    private void OrderFailureFeedback()
    {
        m_FailureUI.SetActive(true);
        foreach (Animator animator in m_AlarmAnimators)
        {
            animator.SetBool("AlarmRed", true);
            StartCoroutine(nameof(TurnOfLight), animator);
        }

        if (m_AudioOrderFail)
            m_AudioOrderFail.Play();

        Instantiate(m_ParticlesOrderFail, transform.position, transform.rotation);
    }

    private IEnumerator TurnOfLight(Animator animator)
    {
        yield return new WaitForSeconds(m_OrderCompleteCooldown);
        animator.SetBool("AlarmGreen", false);
        animator.SetBool("AlarmRed", false);
    }

    private void MakeBoxesDrop()
    {
        foreach (GameObject box in m_CollectedBoxes)
        {
            if (box.GetComponent<Rigidbody>() == false)
                box.AddComponent<Rigidbody>();
            box.GetComponent<BoxCollider>().isTrigger = true;
            box.layer = m_DynamicBoxLayer;
        }
    }

    private IEnumerator WaitForNewOrder(bool orderSuccesfull)
    {
        yield return new WaitForSeconds(m_OrderCompleteCooldown);

        m_SuccesUI.SetActive(false);
        m_FailureUI.SetActive(false);

        if (orderSuccesfull)
            m_OrderManager.CompleteOrder(this);
        else
            m_OrderManager.FailOrder(this);
    }

    ///========================
    /// GHOST FUNCTIONS
    ///========================

    private void ResetGhosts()
    {
        foreach (var ghost in m_CardBoardGhosts)
        {
            ghost.tag = DESTROYSTRING;
            Destroy(ghost);
        }
        m_CardBoardGhosts.Clear();

        foreach (var ghost in m_WoodGhosts)
        {
            ghost.tag = DESTROYSTRING;
            Destroy(ghost);
        }
        m_WoodGhosts.Clear();

        foreach (var ghost in m_MetalGhosts)
        {
            ghost.tag = DESTROYSTRING;
            Destroy(ghost);
        }
        m_MetalGhosts.Clear();
    }

    private void StripBox(BoxBehaviour box)
    {
        GameObject go = box.gameObject;
        go.layer = m_StaticLevelLayer;
        Destroy(box);
        Destroy(go.GetComponent<Rigidbody>());
    }

    private void SnapBox(BoxBehaviour box)
    {
        Vector3 newPos = transform.position;
        Quaternion newRot = transform.rotation;

        switch (box.BoxType)
        {
            case BoxBehaviour.Type.cardboard:
                if (m_CurrentAmountCardboard >= m_CardBoardGhosts.Count) throw new UnityException("Current amount of cardboard boxes out of bounds");
                newPos = m_CardBoardGhosts[m_CurrentAmountCardboard].transform.position;
                newRot = m_CardBoardGhosts[m_CurrentAmountCardboard].transform.rotation;

                break;

            case BoxBehaviour.Type.wood:
                if (m_CurrentAmountWood >= m_WoodGhosts.Count) throw new UnityException("Current amount of wood boxes out of bounds");
                newPos = m_WoodGhosts[m_CurrentAmountWood].transform.position;
                newRot = m_WoodGhosts[m_CurrentAmountWood].transform.rotation;

                break;

            case BoxBehaviour.Type.metal:
                if (m_CurrentAmountMetal >= m_MetalGhosts.Count) throw new UnityException("Current amount of metal boxes out of bounds");
                newPos = m_MetalGhosts[m_CurrentAmountMetal].transform.position;
                newRot = m_MetalGhosts[m_CurrentAmountMetal].transform.rotation;

                break;
        }
        box.gameObject.transform.position = newPos;
        box.gameObject.transform.rotation = newRot;
        box.GetComponent<BoxBehaviour>().DeselectBox();

    }

    public void SpawnGhosts()
    {
        for (int i = 0; i < m_RequiredAmountCardboard; i++)
        {
            Vector3 spawn = m_CardboardGhostSpawn.position + i * m_BoxHeight * Vector3.up;
            m_CardBoardGhosts.Add(Instantiate(m_CardboardPrefab, spawn, Quaternion.identity));
        }
        for (int i = 0; i < m_RequiredAmountWood; i++)
        {
            Vector3 spawn = m_WoodGhostSpawn.position + i * m_BoxHeight * Vector3.up;
            m_WoodGhosts.Add(Instantiate(m_WoodPrefab, spawn, Quaternion.identity));
        }
        for (int i = 0; i < m_RequiredAmountMetal; i++)
        {
            Vector3 spawn = m_MetalGhostSpawn.position + i * m_BoxHeight * Vector3.up;
            m_MetalGhosts.Add(Instantiate(m_MetalPrefab, spawn, Quaternion.identity));
        }
    }
}
