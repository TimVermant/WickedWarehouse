using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoxElevatorBehavior : MonoBehaviour
{
    [SerializeField] private GameObject m_StartPoint = null;
    [SerializeField] private GameObject m_EndPoint = null;
    [Header("Game variables")]
    [SerializeField] private float m_ElevateTime = 1.0f;

    private float m_ElapsedSec = 0.0f;


    private float m_MoveDirection = -1.0f;

    private bool m_IsMoving = true;
    private float m_WaitElapsed = 0.0f;
    private float m_MoveDelay = 1.0f;


    private Vector3 m_StartPointPosition;
    private Vector3 m_EndPointPosition;
    private void Awake()
    {
        m_StartPointPosition = m_StartPoint.transform.position;
        m_EndPointPosition = m_EndPoint.transform.position;
        m_MoveDirection = 1.0f;
        transform.position = m_StartPointPosition;
    }

    private void FixedUpdate()
    {
        if (m_IsMoving)
        {
            m_ElapsedSec += Time.fixedDeltaTime * m_MoveDirection;
            float tVal = (m_ElapsedSec) / m_ElevateTime;

            transform.position = Vector3.Lerp(m_StartPointPosition, m_EndPointPosition, tVal);
            if (m_ElapsedSec >= m_ElevateTime)
            {

                transform.position = m_EndPointPosition;
                m_MoveDirection = -1.0f;
                m_IsMoving = false;

            }

            if (m_ElapsedSec < 0.0f)
            {
               
                transform.position = m_StartPointPosition;
                m_MoveDirection = 1.0f;
                m_IsMoving = false;

            }

        }
        else
        {
            m_WaitElapsed += Time.fixedDeltaTime;
            if(m_WaitElapsed >= m_MoveDelay)
            {
                m_WaitElapsed = 0.0f;
                m_IsMoving = true;
            }
        }

    }



    private void OnTriggerEnter(Collider other)
    {
        if (m_MoveDirection == 1.0f)
        {
            m_MoveDirection = -1.0f;
        }
    }


}
