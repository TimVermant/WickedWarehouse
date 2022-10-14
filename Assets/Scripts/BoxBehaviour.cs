using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// Author = Tim Vermant
/// <summary>
/// Behaviour class for the boxes that handles their individual collision and the swap from static to dynamic box
/// </summary>

public class BoxBehaviour : MonoBehaviour
{

    public enum Type
    {
        cardboard,
        wood,
        metal,
        none
    }

    [Header("Materials")]
    [SerializeField] private Material m_RegularMaterial = null;
    [SerializeField] private Material m_GlowMaterial = null;
    [SerializeField] private Type m_BoxType = Type.cardboard;

    [Header("Particles")]
    [SerializeField] private GameObject m_DropParticle = null;

    // Box falling booleans 
    private bool m_IsFalling = false;
    private bool m_IsFallingInStack = false;
    private bool m_IsDroppingStack = false;

    // Box falling calculation logic
    private float m_CurrentFallTime = 0.0f;
    [SerializeField] private float m_MaxFallTime = 0.2f;
    private int m_DropHeight = 1;
    private Vector3 m_StartPosition;
    private Vector3 m_EndPosition;
    private float m_BoxHeight = 1.0f;

    // Layers
    private const string PLAYER_LAYER = "Player";
    private const string GROUND_LAYER = "Ground";
    private const string STATIC_LEVEL_LAYER = "Static Level";
    private const string DYNAMIC_LEVEL_LAYER = "Dynamic Level";
    private const string STATIC_BOX_LAYER = "Static Box";
    private const string DYNAMIC_BOX_LAYER = "Dynamic Box";


    // Properties
    public bool isFalling
    {

        get { return m_IsFalling; }
        set { m_IsFalling = value; }
    }

    public bool isFallingInStack
    {
        get { return m_IsFallingInStack; }
        set { m_IsFallingInStack = value; }
    }

    public bool IsDroppingStack
    {
        get { return m_IsDroppingStack; }
        set { m_IsDroppingStack = value; }
    }

    public Type BoxType
    {
        get { return m_BoxType; }
        set { m_BoxType = value; }
    }

    private void Start()
    {
        GetComponent<Renderer>().material = m_RegularMaterial;
    }

    private void FixedUpdate()
    {
        if (isFallingInStack || IsDroppingStack)
        {

            m_CurrentFallTime += Time.fixedDeltaTime;
            float tVal = m_CurrentFallTime / (m_MaxFallTime * m_DropHeight);
            transform.localPosition = Vector3.Lerp(m_StartPosition, m_EndPosition, tVal);
            if (m_CurrentFallTime > (m_MaxFallTime * m_DropHeight))
            {
                transform.localPosition = m_EndPosition;
                isFallingInStack = false;

            }


        }

    }

    private void OnCollisionStay(Collision collision)
    {

        //Make sure the player doesn't immediately pick it up after dropping it
        if (collision.gameObject.layer != LayerMask.NameToLayer(PLAYER_LAYER))
        {

            if (IsDroppingStack)
            {
                IsDroppingStack = false;
                DropBox();
            }

            if (isFalling)
            {
                isFalling = false;

                GameObject particle = Instantiate(m_DropParticle, transform);
                particle.transform.position = transform.position - new Vector3(0.0f, m_DropHeight / 2.0f, 0.0f);
                particle.GetComponent<ParticleSystem>().Play();
            }
        }


        // Dropping of box only allowed when not sticking

        // Boxes get dropped when hitting environment
        if (transform.parent != null)
        {
            PlayerBoxBehaviour playerBoxBehaviour = transform.parent.GetComponent<PlayerBoxBehaviour>();
           
            // Colliding with the environment
            if (collision.gameObject.layer == LayerMask.NameToLayer(DYNAMIC_LEVEL_LAYER))
            {
                DeselectBox();
                playerBoxBehaviour.RemoveBox(gameObject);
                

            }

        }


    }



    public void StartDroppingStack(int boxAmount = 1)
    {
        m_StartPosition = transform.localPosition;
        m_EndPosition = m_StartPosition;
        m_EndPosition.y -= (m_BoxHeight * boxAmount); // Height of a box

        m_DropHeight = boxAmount;
        m_CurrentFallTime = 0.0f;
    }



    public void DropBox()
    {

        if (gameObject.GetComponent<Rigidbody>() == null)
            gameObject.AddComponent<Rigidbody>();
        gameObject.GetComponent<Rigidbody>().isKinematic = false;
        gameObject.GetComponent<Rigidbody>().useGravity = true;
        transform.parent = null;

        m_IsFalling = true;
        gameObject.layer = LayerMask.NameToLayer(DYNAMIC_BOX_LAYER);
        DeselectBox();
    }

    public void SelectBox()
    {
        GetComponent<Renderer>().material = m_GlowMaterial;
    }

    public void DeselectBox()
    {
        GetComponent<Renderer>().material = m_RegularMaterial;
    }
}
