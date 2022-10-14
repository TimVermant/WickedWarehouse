using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// Author: Tristan Wauthier
/// <summary>
/// This class tells the boxspawners what to spawn.
/// </summary>
public class BoxSpawnerManager : Singleton<BoxSpawnerManager>
{
    [Header("SpawnPoints")]
    private List<BoxSpawner> m_Spawns = new List<BoxSpawner>();

    [Space(10)]
    [Header("Spawn times")]
    [SerializeField] private float m_CardboardTime = 2.5f;
    [SerializeField] private float m_WoodTime = 3.5f;
    [SerializeField] private float m_MetalTime = 5.0f;

    private bool m_IsInitialized = false;

    void Start()
    {
        foreach(BoxSpawner spawner in FindObjectsOfType<BoxSpawner>())
        {
            m_Spawns.Add(spawner);
            spawner.enabled = false;
        }
        AssignSpawners();
    
    }

    public void Initialize()
    {
        if (m_IsInitialized) return;
        m_IsInitialized = true;

        foreach (BoxSpawner spawner in FindObjectsOfType<BoxSpawner>())
            spawner.enabled = true;
    }


    void AssignSpawners()
    {

        foreach (var spawner in m_Spawns)
        {
            switch (spawner.BoxType)
            {
                case BoxBehaviour.Type.cardboard:
                    spawner.SpawnTime = m_CardboardTime;
                    break;
                case BoxBehaviour.Type.wood:
                    spawner.SpawnTime = m_WoodTime;
                    break;
                case BoxBehaviour.Type.metal:
                    spawner.SpawnTime = m_MetalTime;
                    break;
                default:
                    spawner.SpawnTime = m_CardboardTime;
                    break;
            }
        }

    }

}
