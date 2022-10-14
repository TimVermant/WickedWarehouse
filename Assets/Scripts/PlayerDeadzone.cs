using System;
using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using Unity.VisualScripting;
using UnityEngine;
using Vector3 = UnityEngine.Vector3;


/// Author = Tristan Wauthier
/// <summary>
/// This scripts moves the player to a target position, if they hit the trigger.
/// </summary>
[RequireComponent(typeof(BoxCollider))]
public class PlayerDeadzone : MonoBehaviour
{
    [Header("Dead zone")]
    [SerializeField] private BoxCollider m_TriggerZone = null;
    [Space(2.50f)]
    [Header("Extra info")]
    [SerializeField] private Transform m_TargetPosition = null;
    [SerializeField] private int m_PlayerLayerID = 6;

    // Start is called before the first frame update
    void Start()
    {
        if (m_TriggerZone == null)
            throw new Exception("PlayerDeadzone: TriggerZone not set.");
        m_TriggerZone.isTrigger = true;
        
        if (m_TargetPosition == null)
            throw new Exception("PlayerDeadzone: TriggerZone not set.");
    }

    private void OnTriggerEnter(Collider other)
    {
        GameObject go = other.gameObject;

        if (go.layer == m_PlayerLayerID)
            go.transform.position = m_TargetPosition.position;
    }
}
