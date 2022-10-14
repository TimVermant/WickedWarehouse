using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static BoxBehaviour;

/// Author: Tristan Wauthier
/// <summary>
/// This class spawns a box (type given) when asked to.
/// </summary>
public class BoxSpawner : MonoBehaviour
{
    [SerializeField] private GameObject m_PrefabCardboard;
    [SerializeField] private GameObject m_PrefabWood;
    [SerializeField] private GameObject m_PrefabMetal;
    [SerializeField] private Type m_BoxType = Type.cardboard;
    [SerializeField] private GameObject m_SpawnPoint = null;
    private Transform m_SpawnPointTransform;
    private Animator m_Animator;

    private float m_SpawnTime = float.MaxValue;
    private float m_SpawnTimer = float.MaxValue;

    public float SpawnTime
    {
        set => m_SpawnTime = value;
    }

    public Type BoxType
    {
        get => m_BoxType;
        set => m_BoxType = value;
    }

    private void Start()
    {
        if (m_PrefabCardboard == null || m_PrefabWood == null || m_PrefabMetal == null)
            throw new UnityException("prefabs not set!");
        m_Animator = GetComponentInChildren<Animator>();

        if (m_SpawnPoint == null)
        {
            m_SpawnPointTransform = transform;
        }
        else
        {
            m_SpawnPointTransform = m_SpawnPoint.transform;
        }
    }

    private void Update()
    {
        m_SpawnTimer += Time.deltaTime;
        if (m_SpawnTimer >= m_SpawnTime)
        {
            Spawn();
            m_SpawnTimer = 0;
        }
    }

    public void Spawn()
    {
        StartCoroutine("DropBox");
        //Quaternion rotation = Quaternion.Euler(0, Random.Range(0.0f, 180.0f), 0);
        switch (m_BoxType)
        {
            case Type.cardboard:
                Instantiate(m_PrefabCardboard, m_SpawnPointTransform.position,m_SpawnPointTransform.rotation);
                break;
            case Type.wood:
                Instantiate(m_PrefabWood, m_SpawnPointTransform.position, m_SpawnPointTransform.rotation);
                break;
            case Type.metal:
                Instantiate(m_PrefabMetal, m_SpawnPointTransform.position, m_SpawnPointTransform.rotation);
                break;

        }
    }
    private IEnumerator DropBox()
    {
        yield return new WaitForSeconds(0.5f);
        if (m_Animator != null)
            m_Animator.Play("OpenSpawner");

    }
}
