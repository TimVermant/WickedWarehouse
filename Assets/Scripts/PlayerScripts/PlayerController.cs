using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;


/// Authors = Tim Vermant and Tristan Wauthier
/// <summary>
/// Controller script that handles all the input and the picking up of boxes
/// </summary>
public class PlayerController : MonoBehaviour
{
    public enum CurrentAction
    {
        boxThrow,
        boxDrop,
        boxPickup,
        none
    }

    private CurrentAction m_CurrentAction = CurrentAction.none;
    public CurrentAction CurrentActionPlayer
    {
        get { return m_CurrentAction; } 
        set { m_CurrentAction = value; }
    }

    [Header("Game Objects")]
    [SerializeField] private GameObject m_BoxPickupCollider;
    [SerializeField] private GameObject m_ThrowIndicatorPrefab = null;

    [Header("Game values")]
    [SerializeField] private float m_ThrowStrength = 400.0f;
    [Header("Particles")]
    [SerializeField] private GameObject m_WalkDustParticle;



    [Header("Audio")]
    [SerializeField] private AudioSource m_ThrowAudio;
    [SerializeField] private AudioSource m_BoxSelectionAudio;
    [SerializeField] private AudioSource m_WalkingAudio;
    [SerializeField] private AudioSource m_ThrowChargeAudio;



    private float m_SelectionAudioPitch = 0.3f;
    private float m_StartSelectionPitch = 0.3f;
    private float m_SelectionAudioPitchIncrease = 0.3f;



    // Layer logic
    private const string GROUND_LAYER = "Ground";
    private const string DYNAMIC_BOX_LAYER = "Dynamic Box";

    // External scripts 
    private MovementBehaviour m_MovementBehaviour;
    private PlayerBoxBehaviour m_PlayerBoxBehaviour;
    private Animator m_PlayerAnimator;


    private GameObject m_ThrowIndicator = null;
    private Vector3 m_ThrowIndicatorOffset = new Vector3(0.0f, 0.0f, -1.0f);

    // Boolean logic
    private bool m_IsPickupButtonDown = false;
    private bool m_IsInputDisabled = false;

    // Box throw player feedback
    private float m_HoldDropTimeElapsed;
    private float m_HoldDropAmountTimeElapsed;
    private bool m_IsHoldingButtonDrop = false;
    private bool m_IsHoldingButtonDropAmount = false;
    private float m_MinDuration = 0.3f;
    private float m_MaxDuration = 0.7f;
    private float m_BoxSelectionDurationMax = 0.15f;
    private bool m_CanPlayThrowChargeAudio = true;


    private int m_BoxesSelected = 0;

    // Player collision
    private float m_MaxAllowedVelocityCollision = 5.0f;

    // Multiplayer
    private int m_PlayerID = -1;

    public bool IsPickupButtonDown
    {
        get { return m_IsPickupButtonDown; }
    }

    public int PlayerID
    {
        get { return m_PlayerID; }
        set { m_PlayerID = value; }
    }

    public void Awake()
    {
        m_MovementBehaviour = GetComponent<MovementBehaviour>();
        m_PlayerBoxBehaviour = GetComponent<PlayerBoxBehaviour>();
        m_PlayerAnimator = GetComponentInChildren<Animator>();

        if (m_MovementBehaviour == null
            || m_PlayerAnimator == null
            || m_PlayerBoxBehaviour == null)
            throw new MissingReferenceException("PLayerController Awake(): Not all components initialized!");

        m_PlayerBoxBehaviour.PlayerMovement = m_MovementBehaviour;
        m_MovementBehaviour.PlayerBoxBehaviour = m_PlayerBoxBehaviour;
        m_MovementBehaviour.PlayerAnimator = m_PlayerAnimator;
        m_PlayerBoxBehaviour.PlayerAnimator = m_PlayerAnimator;

        m_BoxPickupCollider.GetComponent<BoxCollider>().enabled = false;


        // Add audio to audio manager
        AudioManager.Instance.AddAudio(m_ThrowAudio);
        AudioManager.Instance.AddAudio(m_ThrowChargeAudio);
        AudioManager.Instance.AddAudio(m_BoxSelectionAudio);
        
    }

    private void OnCollisionStay(Collision collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer(GROUND_LAYER))
            return;

        // Hold down left click to pick up boxes
        if (collision.gameObject.layer == LayerMask.NameToLayer(DYNAMIC_BOX_LAYER))
        {
            // Change material of box
            if (!m_IsHoldingButtonDropAmount)
                collision.gameObject.GetComponent<BoxBehaviour>().SelectBox();
            if (m_IsPickupButtonDown)
            {

                if (collision.gameObject.GetComponent<BoxBehaviour>().isFalling || m_PlayerBoxBehaviour == null)
                {
                    return;
                }

                //Remove object from level and instantiate it attached to the player
                if (m_PlayerBoxBehaviour.PickupBox(collision.gameObject.GetComponent<BoxBehaviour>().BoxType))
                {

                    Destroy(collision.gameObject);
                }
                if (!m_BoxPickupCollider.GetComponent<BoxCollider>().enabled && m_IsPickupButtonDown)
                    m_BoxPickupCollider.GetComponent<BoxCollider>().enabled = true;

            }
        }

        if (collision.gameObject.GetComponent<PlayerController>() != null)
        {
            // calculate total collision
            float totalVelocity = m_MovementBehaviour.CurrentVelocity + collision.gameObject.GetComponent<MovementBehaviour>().CurrentVelocity;

            if (totalVelocity > m_MaxAllowedVelocityCollision)
            {


                m_PlayerBoxBehaviour.DropAllBoxes();
                m_CurrentAction = CurrentAction.none;
                collision.gameObject.GetComponent<PlayerBoxBehaviour>().DropAllBoxes();
            }
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer(DYNAMIC_BOX_LAYER))
        {
            // Change material of box
            collision.gameObject.GetComponent<BoxBehaviour>().DeselectBox();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // Hold down left click to pick up boxes
        if (other.gameObject.layer == LayerMask.NameToLayer(DYNAMIC_BOX_LAYER))
        {

            if (!m_IsHoldingButtonDropAmount && other.gameObject.GetComponent<BoxBehaviour>() != null)
                other.gameObject.GetComponent<BoxBehaviour>().SelectBox();
            if (m_IsPickupButtonDown)
            {

                if (other.gameObject.GetComponent<BoxBehaviour>().isFalling || m_PlayerBoxBehaviour == null)
                {
                    return;
                }

                //Remove object from level and instantiate it attached to the player
                if (m_PlayerBoxBehaviour.PickupBox(other.gameObject.GetComponent<BoxBehaviour>().BoxType))
                {
                   
                    Destroy(other.gameObject);
                }
                if (!m_BoxPickupCollider.GetComponent<BoxCollider>().enabled && m_IsPickupButtonDown)
                    m_BoxPickupCollider.GetComponent<BoxCollider>().enabled = true;

            }
        }

    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer(DYNAMIC_BOX_LAYER))
        {
            // Change material of box
            if(other.gameObject.GetComponent<BoxBehaviour>())
                other.gameObject.GetComponent<BoxBehaviour>().DeselectBox();
        }
    }
    private void Update()
    {

        if(m_WalkDustParticle)
        {
            if(m_MovementBehaviour.CurrentVelocity > m_MovementBehaviour.MaxMovementSpeed / 2.0f)
            {
                m_WalkDustParticle.SetActive(true);

                var main = m_WalkDustParticle.GetComponent<ParticleSystem>().main;
                main.startSpeed = m_MovementBehaviour.CurrentVelocity;
                
            }
            else
            {
                m_WalkDustParticle.SetActive(false);
            }
        }

        // Only update position if indicator is showing 
        if (m_ThrowIndicator != null)
        {
            Vector3 throwPos = transform.position + m_ThrowIndicatorOffset;
            m_ThrowIndicator.transform.position = throwPos;


        }
        if (m_IsHoldingButtonDrop)
        {
            if (m_PlayerBoxBehaviour.BoxList.Count > 0)
            {

                m_HoldDropTimeElapsed += Time.deltaTime;
                if (m_HoldDropTimeElapsed >= m_MaxDuration)
                {
                    m_HoldDropTimeElapsed = m_MaxDuration;
                }

                float sliderValue = m_HoldDropTimeElapsed / m_MaxDuration;
                m_ThrowIndicator.GetComponentInChildren<Slider>().value = sliderValue;
                if (m_ThrowChargeAudio)
                {
                    if (!m_ThrowChargeAudio.isPlaying && m_CanPlayThrowChargeAudio)
                    {
                        m_CanPlayThrowChargeAudio = false;
                        m_ThrowChargeAudio.Play();

                    }
                }
            }
            else
            {
                HideThrowIndicator();
                m_CanPlayThrowChargeAudio = true;
            }
        }
        if (m_IsHoldingButtonDropAmount)
        {
            m_HoldDropAmountTimeElapsed += Time.deltaTime;
            if (m_HoldDropAmountTimeElapsed >= m_BoxSelectionDurationMax)
            {
                m_HoldDropAmountTimeElapsed = 0.0f;
                if (m_BoxesSelected < m_PlayerBoxBehaviour.BoxList.Count)
                {

                    m_PlayerBoxBehaviour.UpdateBoxMaterial(m_BoxesSelected);
                    ++m_BoxesSelected;
                    if (m_BoxSelectionAudio)
                    {
                        m_BoxSelectionAudio.pitch = m_SelectionAudioPitch;
                        m_BoxSelectionAudio.Play();
                        m_SelectionAudioPitch += m_SelectionAudioPitchIncrease;
                    }

                }
            }

        }
        if (m_IsInputDisabled)
        {
            if (!m_PlayerBoxBehaviour.StackIsFalling)
            {
                SetEnableInput(true);
                HideThrowIndicator();

            }
        }


    }


    //INPUT SYSTEM ACTION METHODS =================
    //CALLED FROM WITHIN THE PLAYER INPUT COMPONENT.

    public void OnMovement(InputAction.CallbackContext val)
    {
        if (m_MovementBehaviour == null)
            return;

        Vector2 input = val.ReadValue<Vector2>();
        m_PlayerBoxBehaviour.MovementInput = input;
        m_MovementBehaviour.TargetDir = new Vector3(input.x, 0, input.y);

    }

    public void OnDropAllBoxes(InputAction.CallbackContext val)
    {
        if (m_PlayerBoxBehaviour == null)
            return;
        if (m_CurrentAction != CurrentAction.boxDrop && m_CurrentAction != CurrentAction.none)
            return;


        if (val.started && m_PlayerBoxBehaviour.BoxList.Count > 0)
        {
            ShowBoxSelection();
            m_CurrentAction = CurrentAction.boxDrop;
        }
        if (val.performed)
        {
            
            m_CurrentAction = CurrentAction.none;
            int boxesToDrop = m_BoxesSelected;


            if (boxesToDrop >= m_PlayerBoxBehaviour.BoxList.Count || boxesToDrop == 0)
            {

                m_PlayerBoxBehaviour.DropAllBoxes();
                m_BoxPickupCollider.GetComponent<BoxCollider>().enabled = false;
            }
            else
            {
                // Don't do anything when you dont need to drop any boxes
                if (boxesToDrop > 0)
                {

                    m_PlayerBoxBehaviour.DropBoxes(boxesToDrop);
                }
                else
                {
                    //Drop at least 1 box
                    m_PlayerBoxBehaviour.DropBoxes(1);
                }
            }

            m_IsHoldingButtonDropAmount = false;
            m_PlayerBoxBehaviour.ResetBoxMaterial();
            m_BoxesSelected = 0;
            m_SelectionAudioPitch = m_StartSelectionPitch;
        }
    }

    public void OnDropSingleBox(InputAction.CallbackContext val)
    {
        if (m_PlayerBoxBehaviour == null || m_PlayerBoxBehaviour.BoxList.Count == 0)
            return;
        if (m_CurrentAction != CurrentAction.boxThrow && m_CurrentAction != CurrentAction.none)
            return;




        if (val.started && m_PlayerBoxBehaviour.BoxList.Count > 0)
        {
            DisplayThrowIndicator();
            m_CurrentAction = CurrentAction.boxThrow;
        }

        if (val.performed)
        {
            m_CurrentAction = CurrentAction.none;


            float pressDuration = (float)val.duration;
            if (pressDuration > m_MaxDuration)
                pressDuration = m_MaxDuration;
            if (pressDuration <= m_MinDuration)
                pressDuration = m_MinDuration;
            HideThrowIndicator();
            m_PlayerBoxBehaviour.DropSingleBox(m_ThrowStrength * pressDuration);
            // Audio
            if(GameManager.Instance.CurrentState == GameState.Game)
                m_ThrowAudio.Play();
            m_ThrowChargeAudio.Stop();
            m_CanPlayThrowChargeAudio = true;

     
            if (m_PlayerBoxBehaviour.BoxList.Count == 0)
                m_BoxPickupCollider.GetComponent<BoxCollider>().enabled = false;



            if (m_PlayerBoxBehaviour.BoxList.Count >= 1)
            {
                // Disable input until box stops falling
                SetEnableInput(false);
            }
        }

    }

    private void SetEnableInput(bool enable)
    {
        PlayerInput input = GetComponent<PlayerInput>();
        if (!enable)
        {
            input.actions.Disable();

            m_IsInputDisabled = true;
        }
        else
        {
            input.actions.Enable();
            m_IsInputDisabled = false;
        }
    }


    private void DisplayThrowIndicator()
    {


        m_ThrowIndicator = Instantiate(m_ThrowIndicatorPrefab, m_ThrowIndicatorOffset, m_ThrowIndicatorPrefab.transform.rotation);
        m_IsHoldingButtonDrop = true;
        m_HoldDropTimeElapsed = 0.0f;
        m_ThrowIndicator.GetComponentInChildren<Slider>().value = 0;
    }

    private void HideThrowIndicator()
    {
        //m_ThrowIndicator.GetComponentInChildren<Slider>().enabled = false;
        Destroy(m_ThrowIndicator);
        m_IsHoldingButtonDrop = false;
        m_HoldDropTimeElapsed = 0.0f;

    }

    private void ShowBoxSelection()
    {
        m_IsHoldingButtonDropAmount = true;
        m_HoldDropAmountTimeElapsed = 0.0f;

    }


    public void OnPickUpBox(InputAction.CallbackContext val)
    {
        if (m_CurrentAction != CurrentAction.none && m_CurrentAction != CurrentAction.boxPickup)
            return;
        if (!m_IsHoldingButtonDropAmount)
        {
            
            m_IsPickupButtonDown = val.action.triggered;
            if (m_IsPickupButtonDown)
                m_CurrentAction = CurrentAction.boxPickup;
            else
                m_CurrentAction = CurrentAction.none;
        }
    }

    public void OnPauseGame(InputAction.CallbackContext val)
    {
        if (!val.performed)
            return;
        if (GameManager.Instance.CurrentState == GameState.PauseMenu)
            GameManager.Instance.ChangeGameState(GameState.Game);
        else
            GameManager.Instance.ChangeGameState(GameState.PauseMenu);
    }
}
