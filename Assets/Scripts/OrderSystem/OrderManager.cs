using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// Author: Tristan Wauthier
/// <summary>
/// This class makes the orders and relays the info to the necessary gameObjects.
/// </summary>
public class OrderManager : Singleton<OrderManager>
{
    [Header("Order size calculation")]
    [SerializeField] private int m_MaxAmountCardboard = 12;
    [SerializeField] private int m_MaxAmountWood = 8;
    [SerializeField] private int m_MaxAmountMetal = 4;
    [Space]
    [SerializeField] private int m_MaxAmountAddedCardboard = 2;
    [SerializeField] private int m_MaxAmountAddedWood = 2;
    [SerializeField] private int m_MaxAmountAddedMetal = 1;

    [Header("Order time calculation")]
    [SerializeField] private float m_OrderTimeMin = 40.0f;
    [SerializeField] private float m_OrderTimeMax = 65.0f;

    [Header("CollectionZones")]
    [SerializeField] private int m_AmountOfInitialActiveZones = 1;
    [SerializeField] private float m_OrderCompleteCooldown = 1.5f;


    private int m_AmountOfActiveZones = 1;
    private List<CollectionZone> m_CollectionZones = new List<CollectionZone>();
    private bool m_IsInitialized = false;
    
    void Start()
    {
        m_AmountOfActiveZones = m_AmountOfInitialActiveZones;
        foreach (CollectionZone zone in FindObjectsOfType<CollectionZone>())
        {
            m_CollectionZones.Add(zone);
        }
        

        foreach (CollectionZone zone in m_CollectionZones)
        {
            zone.OrderManager = this;
            zone.IsActive = false;
            zone.OrderCompleteCooldown = m_OrderCompleteCooldown;
        }

        Random.InitState((int)System.DateTime.Now.Ticks);
    }

    public void Initialize()
    {
        if (m_IsInitialized) return;
        m_IsInitialized = true;
        AssignZones();
    }

    void AssignZones()
    {
        ShuffleZones();

        int amountOfActiveZones = 0;
        foreach (CollectionZone zone in m_CollectionZones)
        {
            if (amountOfActiveZones >= m_AmountOfActiveZones)
                break;

            if (zone.IsActive == true)
            {
                amountOfActiveZones++;
                continue;
            }
            amountOfActiveZones++;
            zone.IsActive = true;
            GiveZoneOrder(zone);
        }
 
    }

    void GiveZoneOrder(CollectionZone zone)
    {
        GenerateOrderSize(zone);
        GenerateOrderTime(zone);
        zone.SpawnGhosts();
    }

    void GenerateOrderSize(CollectionZone zone)
    {
        

      

        int minAddCardB = 1;
        int minAddWood = 1;
        int minAddMetal = 0;

    

        int addAmountCardB = Random.Range(minAddCardB, m_MaxAmountAddedCardboard + 1 );
        int addAmountWood = Random.Range(minAddWood, m_MaxAmountAddedWood + 1);
        int addAmountMetal = Random.Range(minAddMetal, m_MaxAmountAddedMetal + 1);

        zone.RequiredAmountCardboard = Mathf.Clamp(addAmountCardB, minAddCardB, m_MaxAmountCardboard);
        zone.RequiredAmountWood = Mathf.Clamp(addAmountWood, minAddWood, m_MaxAmountWood);
        zone.RequiredAmountMetal = Mathf.Clamp(addAmountMetal, minAddMetal, m_MaxAmountMetal);


       
    }

    void GenerateOrderTime(CollectionZone zone)
    {
        // Order gets less time depending on the difficulty
        zone.OrderTimer = Random.Range(m_OrderTimeMin, m_OrderTimeMax) - TimeManager.Instance.DifficultyLevel;
        zone.OrderTimer = Mathf.Clamp(zone.OrderTimer,m_OrderTimeMin,m_OrderTimeMax);
       
    }

    public void CompleteOrder(CollectionZone zone)
    {
        zone.IsActive = false;
        m_AmountOfActiveZones++;

        AssignZones();
    }

    public void FailOrder(CollectionZone zone)
    {
        zone.IsActive = false;

        AssignZones();
    }

    void ShuffleZones()
    {
        for (int i = 0; i < m_CollectionZones.Count; i++)
        {
            CollectionZone temp = m_CollectionZones[i];
            int randomIndex = Random.Range(i, m_CollectionZones.Count);
            m_CollectionZones[i] = m_CollectionZones[randomIndex];
            m_CollectionZones[randomIndex] = temp;
        }
        
    }

}
