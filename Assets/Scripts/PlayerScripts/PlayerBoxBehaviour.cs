using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// Authors = Tim Vermant and Tristan Wauthier
/// <summary>
/// This script contains all player <-> box interactions (stacking, dropping, throwing etc)
/// </summary>
public class PlayerBoxBehaviour : MonoBehaviour
{
    [Header("Game Objects")]
    [SerializeField] private GameObject m_BoxCardboardPrefab = null;
    [SerializeField] private GameObject m_BoxWoodPrefab = null;
    [SerializeField] private GameObject m_BoxMetalPrefab = null;
    [SerializeField] private GameObject m_BoxSpawn = null;
    [SerializeField] private GameObject m_BoxDrop = null;


    [Header("Game values")]
    [SerializeField] private const int MAXSTACKSIZE = 8;
    [SerializeField] private float m_BoxHeight = 1.0f;
    [SerializeField] private float m_BoxThrowHeightMultiplier = 1.2f;

    [Header("Particles")]
    [SerializeField] private GameObject m_BoxThrowParticle;
    [Header("Audio")]
    [SerializeField] private AudioSource m_BoxPickupAudio;

    private float m_PickupStartPitch = 1.0f;
    private float m_PickupCurrentPitch = 1.0f;
    private float m_PickupPitchIncrease = 0.2f;

    //Layer logic
    private const string LAYER_STATIC_BOXES = "Static Box";
    private const string LAYER_DYNAMIC_BOXES = "Dynamic Box";

    // External scripts
    private MovementBehaviour m_MovementBehaviour;

    private Animator m_PlayerAnimator;


    // Box drop logic
    private bool m_StackIsFalling = false;

    // Box positions
    private Vector3 m_BoxSpawnStart;
    private Vector3 m_BoxSpawnDifference = new Vector3(0f, 1.001f, 0f);
    private Vector2 m_MovementInput = Vector2.zero;
    

    private List<GameObject> m_BoxList = new List<GameObject>();



    public Animator PlayerAnimator
    {
        set { m_PlayerAnimator = value; }
    }


    public bool StackIsFalling
    {
        get { return m_StackIsFalling; }
    }

    public List<GameObject> BoxList
    {
        get { return m_BoxList; }
    }

    public MovementBehaviour PlayerMovement
    {
        get { return m_MovementBehaviour; }
        set { m_MovementBehaviour = value; }
    }
    public Vector2 MovementInput
    {
        get { return m_MovementInput; }
        set { m_MovementInput = value; }
    }

    private void Awake()
    {
        if (m_BoxSpawn != null)
        {
            m_BoxSpawnStart = m_BoxSpawn.transform.localPosition;
        }

        AudioManager.Instance.AddAudio(m_BoxPickupAudio);

    }


    private void Update()
    {
        if (m_StackIsFalling)
        {
            if (!IsStackStillFalling())
            {
                m_StackIsFalling = false;
            }
        }
    }


    private bool IsStackStillFalling()
    {
        foreach (GameObject box in m_BoxList)
        {
            if (box.GetComponent<BoxBehaviour>().isFallingInStack)
            {
                return true;
            }
        }
        return false;
    }
    public bool PickupBox(BoxBehaviour.Type type = BoxBehaviour.Type.cardboard)
    {
        if (m_BoxList.Count >= MAXSTACKSIZE)
            return false;

        // Raycast to see if there is enough room above
        if (m_BoxList.Count > 0)
        {
            float boxHeight = 0.999f;
            GameObject topBox = m_BoxList[m_BoxList.Count - 1];
            if (Physics.Raycast(topBox.transform.position, Vector3.up, boxHeight))
            {
                
                return false;
            }
        }
        GameObject boxPrefab = null;
        switch (type)
        {
            case BoxBehaviour.Type.cardboard:
                boxPrefab = m_BoxCardboardPrefab;
                break;
            case BoxBehaviour.Type.wood:
                boxPrefab = m_BoxWoodPrefab;
                break;
            case BoxBehaviour.Type.metal:
                boxPrefab = m_BoxMetalPrefab;
                break;
            default:
                break;
        }
        GameObject newBox = Instantiate(boxPrefab, m_BoxSpawn.transform.position, m_BoxSpawn.transform.rotation);



        newBox.GetComponent<Rigidbody>().isKinematic = true;


        newBox.layer = LayerMask.NameToLayer(LAYER_STATIC_BOXES);
        newBox.transform.SetParent(transform);



        m_BoxList.Add(newBox);
        ResetBoxStack();
        m_BoxSpawn.transform.localPosition += m_BoxSpawnDifference;
        SetAnimationWeights();

        if (m_BoxPickupAudio)
        {
            m_BoxPickupAudio.pitch = m_PickupCurrentPitch;
            m_BoxPickupAudio.Play();
            m_PickupCurrentPitch += m_PickupPitchIncrease;
        }
        return true;
    }



    public void DropAllBoxes()
    {
       if (m_BoxList.Count == 0)
            return;



        foreach (GameObject box in m_BoxList)
        {

            box.GetComponent<BoxBehaviour>().IsDroppingStack = true;
            box.GetComponent<BoxBehaviour>().StartDroppingStack();
            box.GetComponent<BoxBehaviour>().DeselectBox();

        }

        //Reset spawn start position
        m_BoxSpawn.transform.localPosition = m_BoxSpawnStart;
        m_BoxList.Clear();
        SetAnimationWeights();

        //Reset audio pitch
        m_PickupCurrentPitch = m_PickupStartPitch;

    }

    public void DropTopBox(Vector3 dropDir)
    {

        GameObject box = m_BoxList[m_BoxList.Count - 1];
        m_BoxList.Remove(box);

        box.GetComponent<BoxBehaviour>().DropBox();

        box.GetComponent<Rigidbody>().AddForce(dropDir);
        m_BoxSpawn.transform.localPosition -= m_BoxSpawnDifference;
        SetAnimationWeights();
        m_PickupCurrentPitch -= m_PickupPitchIncrease;
    }

    public void DropSingleBox(float throwForce)
    {
        if (m_BoxList.Count == 0)
            return;
        GameObject box = m_BoxList[0];
        m_BoxList.Remove(box);
        // Only drop boxes from m_BoxDrop when holding more then 1 box
        if (m_BoxList.Count >= 1)
        {
            box.transform.localPosition = m_BoxDrop.transform.localPosition;
            // Makes all other boxes fall into place
            MakeStackFall();
        }

        Vector3 boxThrowPosition = box.transform.position;
        box.GetComponent<BoxBehaviour>().DropBox();

        Vector3 forceDirection = (box.transform.localPosition - transform.localPosition).normalized * throwForce;
        forceDirection.y *= m_BoxThrowHeightMultiplier;
        box.GetComponent<Rigidbody>().AddForce(forceDirection);
        
        Quaternion boxThrowRotation = Quaternion.LookRotation(forceDirection * -1.0f);

        m_BoxSpawn.transform.localPosition -= m_BoxSpawnDifference;

        // Particles
        if(m_BoxThrowParticle)
        {
            Instantiate(m_BoxThrowParticle, boxThrowPosition, boxThrowRotation);
        }

        SetAnimationWeights();
        m_PickupCurrentPitch -= m_PickupPitchIncrease;
    }
    
    private void MakeStackFall(int boxAmount = 1)
    {
        for (int i = 0; i < m_BoxList.Count; i++)
        {
            GameObject box = m_BoxList[i];

            box.GetComponent<BoxBehaviour>().isFallingInStack = true;
            box.GetComponent<BoxBehaviour>().StartDroppingStack(boxAmount);

            box.GetComponent<Rigidbody>().useGravity = false;
        }
        m_StackIsFalling = true;

        SetAnimationWeights();
    }



    public void RemoveBox(GameObject box)
    {
        bool boxRemoved = false;
        int startRange = 0;
        int elementCount = 1;
        for (int i = 0; i < BoxList.Count; i++)
        {
            GameObject playerBox = BoxList[i];
            if (boxRemoved)
            {


                playerBox.GetComponent<BoxBehaviour>().DropBox();

                elementCount++;
            }
            if (playerBox == box)
            {


                m_BoxSpawn.transform.localPosition = box.transform.localPosition;
                startRange = i;
                boxRemoved = true;

            }


        }
        box.GetComponent<BoxBehaviour>().DropBox();
        BoxList.RemoveRange(startRange, elementCount);
        if (BoxList.Count == 0)
            transform.gameObject.GetComponent<PlayerController>().CurrentActionPlayer = PlayerController.CurrentAction.none;
        m_PickupCurrentPitch -= m_PickupPitchIncrease  * elementCount;

        SetAnimationWeights();

    }

    public void DropBoxes(int boxes)
    {
        int currIndex = boxes;
        if (currIndex >= m_BoxList.Count)
            currIndex = m_BoxList.Count;
        for (int i = 0; i < currIndex; i++)
        {
            GameObject box = m_BoxList[i];
            
            box.transform.localPosition = m_BoxDrop.transform.localPosition + (m_BoxSpawnDifference * i);
            box.GetComponent<BoxBehaviour>().DropBox();
            box.GetComponent<BoxBehaviour>().DeselectBox();
            m_BoxSpawn.transform.localPosition -= m_BoxSpawnDifference;
            SetAnimationWeights();

        }
        m_BoxList.RemoveRange(0, currIndex);
        MakeStackFall(currIndex);
        m_PickupCurrentPitch -= m_PickupPitchIncrease * boxes;
    }

    public void UpdateBoxMaterial(int index)
    {
        m_BoxList[index].GetComponent<BoxBehaviour>().SelectBox();
    }

    public void ResetBoxMaterial()
    {
        foreach (GameObject box in BoxList)
        {
            box.GetComponent<BoxBehaviour>().DeselectBox();
        }
    }

    public void ResetBoxStack()
    {

        Vector3 correctPos = Vector3.zero;
        float correctY = m_BoxSpawnStart.y;
        foreach (GameObject box in BoxList)
        {

            if (box.transform.localPosition.y != correctY)
            {
                correctPos = m_BoxSpawn.transform.localPosition;
                correctPos.y = correctY;
                box.transform.localPosition = correctPos;
            }
            correctY += m_BoxHeight;
        }
    }

    void SetAnimationWeights()
    {
        int WeightLightLayerIndex = m_PlayerAnimator.GetLayerIndex("Carry_Light");
        int WeightMediumLayerIndex = m_PlayerAnimator.GetLayerIndex("Carry_Medium");
        int WeightHeavyLayerIndex = m_PlayerAnimator.GetLayerIndex("Carry_Heavy");


        if (m_BoxList.Count == 0)
        {
            m_PlayerAnimator.SetLayerWeight(WeightLightLayerIndex, 0);
            m_PlayerAnimator.SetLayerWeight(WeightMediumLayerIndex, 0);
            m_PlayerAnimator.SetLayerWeight(WeightHeavyLayerIndex, 0);
        }
        else if (m_BoxList.Count < MAXSTACKSIZE / 3)
        {
            m_PlayerAnimator.SetLayerWeight(WeightLightLayerIndex, 1);
            m_PlayerAnimator.SetLayerWeight(WeightMediumLayerIndex, 0);
            m_PlayerAnimator.SetLayerWeight(WeightHeavyLayerIndex, 0);
        }
        else if (m_BoxList.Count < 2 * MAXSTACKSIZE / 3)
        {
            m_PlayerAnimator.SetLayerWeight(WeightLightLayerIndex, 0);
            m_PlayerAnimator.SetLayerWeight(WeightMediumLayerIndex, 1);
            m_PlayerAnimator.SetLayerWeight(WeightHeavyLayerIndex, 0);
        }
        else if (m_BoxList.Count <= MAXSTACKSIZE)
        {
            m_PlayerAnimator.SetLayerWeight(WeightLightLayerIndex, 0);
            m_PlayerAnimator.SetLayerWeight(WeightMediumLayerIndex, 0);
            m_PlayerAnimator.SetLayerWeight(WeightHeavyLayerIndex, 1);
        }
    }
}
