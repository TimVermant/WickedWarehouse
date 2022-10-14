using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// Author = Tristan Wauthier
/// <summary>
/// This scripts makes objects on the conveyor belt move.
/// </summary>
[RequireComponent(typeof(Rigidbody))]
public class ConveyorBelt : MonoBehaviour
{
    [SerializeField] private float m_MoveSpeed = 2.5f;
    private Rigidbody m_RigidBody;

    void Start()
    {
        m_RigidBody = GetComponent<Rigidbody>();    
        m_RigidBody.isKinematic = true;
    }

    private void FixedUpdate()
    {
        Vector3 currentPos = m_RigidBody.position;
        m_RigidBody.position += -transform.right * m_MoveSpeed * Time.fixedDeltaTime;
        m_RigidBody.MovePosition(currentPos);
    }
}
