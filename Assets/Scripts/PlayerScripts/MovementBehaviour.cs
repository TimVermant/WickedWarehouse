using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// Author = Tristan Wauthier
/// <summary>
/// Script handling the movement of the player(s)
/// </summary> 
public class MovementBehaviour : MonoBehaviour
{
    [Header("Movement settings")]
    [SerializeField] private float m_MaxMovementSpeed = 6.0f;
    [SerializeField] private float m_MinMovementSpeed = 1.0f;
    [SerializeField] private float m_Accerleration = 16.0f;
    [SerializeField] private float m_TurnSpeed = 0.1f;


    private float m_CurrentVelocity = 0f;
    public float MaxMovementSpeed
    {
        get { return m_MaxMovementSpeed; }
    }
    public float CurrentVelocity
    {
        get { return m_CurrentVelocity; }
    }

    private Vector3 m_TargetDir;
    public Vector3 TargetDir
    {
        get { return m_TargetDir; }
        set { m_TargetDir = value; }
    }

    [Header("Component References")]
    [SerializeField] private Rigidbody m_RigidBody;
    public Rigidbody PlayerRigidBody 
    { get { return m_RigidBody; }}

    private PlayerBoxBehaviour m_PlayerBoxBehaviour;
    public PlayerBoxBehaviour PlayerBoxBehaviour
    {
        set { m_PlayerBoxBehaviour = value; }
    }



    private Animator m_PlayerAnimator;
    public Animator PlayerAnimator
    {
        set { m_PlayerAnimator = value; }
    }

    private void Start()
    {
    }

    protected virtual void Update()
    {
         HandleRotation();
    }

    protected virtual void FixedUpdate()
    {
        HandleMovement();
    }

    protected virtual void HandleMovement()
    {
        float magnitude = TargetDir.magnitude;
        if (magnitude < 0.1f)
        {
            TargetDir = Vector3.zero;
            m_CurrentVelocity = 0;
            m_PlayerAnimator.SetFloat("SpeedX", 0);
            m_PlayerAnimator.SetFloat("SpeedY", 0);
            return;
        }

        float currentMaxSpeed = magnitude * m_MaxMovementSpeed;

        if(!Mathf.Approximately(currentMaxSpeed, m_CurrentVelocity))
        {
            if (currentMaxSpeed > m_CurrentVelocity)
            {
                //Accelerate
                m_CurrentVelocity += m_Accerleration * Time.fixedDeltaTime;
            }
            else
            {
                //Decelerate
                m_CurrentVelocity -= + m_Accerleration * Time.fixedDeltaTime;
            }
        }
        //Make sure the speed doesn't exceed the max speed and give it a min
        m_CurrentVelocity = Mathf.Clamp(m_CurrentVelocity, m_MinMovementSpeed, m_MaxMovementSpeed);

        m_PlayerAnimator.SetFloat("SpeedX",m_CurrentVelocity);
        m_PlayerAnimator.SetFloat("SpeedY",m_CurrentVelocity);

        //Apply velocity to position
        m_RigidBody.MovePosition(transform.position + m_CurrentVelocity * Time.fixedDeltaTime * TargetDir);
        
    }

    protected virtual void HandleRotation()
    {
        if (TargetDir.magnitude > 0.1f)
        {
            Quaternion rotationGoal = Quaternion.LookRotation(TargetDir);
            float angle = Mathf.Abs(Quaternion.Angle(transform.rotation, rotationGoal));
            
            // We disable this mechanic for now
            //m_PlayerBoxBehaviour.BoxSwayDrop(angle);
            rotationGoal = Quaternion.Slerp(m_RigidBody.rotation, rotationGoal, m_TurnSpeed);
            m_RigidBody.MoveRotation(rotationGoal);
        }
    }
}
