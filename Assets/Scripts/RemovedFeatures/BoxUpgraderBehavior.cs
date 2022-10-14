using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class BoxUpgraderBehavior : MonoBehaviour
{
    [SerializeField] private GameObject m_BoxSpawnPoint = null;
    [SerializeField] private GameObject m_BoxCardboardPrefab = null;
    [SerializeField] private GameObject m_BoxWoodPrefab = null;
    [SerializeField] private GameObject m_BoxMetalPrefab = null;
    [SerializeField] private GameObject m_CurrentBox = null;



    [Header("Gameplay parameters")]
    [SerializeField] private int m_RequiredBoxes = 4;
    [SerializeField] private float m_BoxSpawnSpeed = 20.0f;
    private Vector3 m_BoxSpawnDirection;
    private int m_StoredBoxes = 0;
    private BoxBehaviour.Type m_CurrentType = BoxBehaviour.Type.none;

    // Start is called before the first frame update
    void Start()
    {
        m_CurrentBox.GetComponent<Renderer>().enabled = false;
        m_BoxSpawnDirection = transform.right;
        m_BoxSpawnDirection.y = 2.0f;
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.gameObject.GetComponent<BoxBehaviour>() != null && other.transform.parent == null)
        {
            if (IsValidBox(other.gameObject.GetComponent<BoxBehaviour>().BoxType))
            {
                m_CurrentType = other.gameObject.GetComponent<BoxBehaviour>().BoxType;
                InsertBox();
                Destroy(other.gameObject);

            }
        }
    }

    private bool IsValidBox(BoxBehaviour.Type type)
    {
        // Metal cannot be upgraded any further
        return (m_CurrentType == BoxBehaviour.Type.none || m_CurrentType == type);
    }

    private void InsertBox()
    {
        // Metal box gets thrown out of machine instantly
        if (m_CurrentType == BoxBehaviour.Type.metal)
        {
            GameObject box = Instantiate(m_BoxMetalPrefab, m_BoxSpawnPoint.transform);
            m_StoredBoxes = 0;
            m_CurrentType = BoxBehaviour.Type.none;

            box.GetComponent<Rigidbody>().AddForce(m_BoxSpawnDirection * m_BoxSpawnSpeed);
            box.transform.parent = null;
            return;
        }


        ++m_StoredBoxes;

        if (m_StoredBoxes >= m_RequiredBoxes)
        {
            GameObject box;

            if (m_CurrentType == BoxBehaviour.Type.cardboard)
            {
                // spawn wooden box
                box = Instantiate(m_BoxWoodPrefab, m_BoxSpawnPoint.transform);

            }
            else
            {
                // spawn metal box
                box = Instantiate(m_BoxMetalPrefab, m_BoxSpawnPoint.transform);

            }
            m_StoredBoxes = 0;
            m_CurrentType = BoxBehaviour.Type.none;

            box.GetComponent<Rigidbody>().AddForce(m_BoxSpawnDirection * m_BoxSpawnSpeed);
            box.transform.parent = null;
            m_CurrentBox.GetComponent<Renderer>().enabled = false;
        }
        else
        {
            m_CurrentBox.GetComponent<Renderer>().enabled = true;
            if (m_CurrentType == BoxBehaviour.Type.cardboard)
            {
                m_CurrentBox.GetComponent<Renderer>().material = m_BoxCardboardPrefab.GetComponent<Renderer>().sharedMaterial;
            }
            else
            {
                m_CurrentBox.GetComponent<Renderer>().material = m_BoxWoodPrefab.GetComponent<Renderer>().sharedMaterial;

            }
        }
    }
}


