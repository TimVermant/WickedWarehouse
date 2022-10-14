using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ForkliftBehavior : MonoBehaviour
{
    [Header("Animator")]
    [SerializeField] private Animator m_ForkliftAnimator = null;
    [Space(3.5f)]
    [Header("Layers")]
    [SerializeField] private int m_PlayerLayerID = 6;
    private bool m_CheckCollision = true;
    private bool m_GoingUp = true;

    private float m_StateChangeCooldown = 2.49f;
    private float m_Timer = 0.0f;

    private void Awake()
    {
        m_ForkliftAnimator.SetBool("GoingUp", m_GoingUp);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (m_GoingUp) return;

        if (!m_CheckCollision) return;

        if (other.gameObject.layer == m_PlayerLayerID)
        {
            m_ForkliftAnimator.speed = 0.0f;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.layer == m_PlayerLayerID)
        {
            m_ForkliftAnimator.speed = 1.0f;
        }
    }

    private void Update()
    {
        m_Timer += Time.deltaTime;
        if (m_Timer > m_StateChangeCooldown)
        {
            m_Timer = 0;
            m_GoingUp = !m_GoingUp;
            m_ForkliftAnimator.SetBool("GoingUp", m_GoingUp);
        }
    }
}
