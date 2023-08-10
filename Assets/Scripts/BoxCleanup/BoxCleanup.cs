using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoxCleanup : MonoBehaviour
{
    [SerializeField] GameObject m_DestroyParticle;
    private List<GameObject> m_BoxList = new List<GameObject>();
    private int m_BoxCounter = 0;
    private float m_BoxDeletionTimer = 0.0f;
    private float m_MaxBoxTime = 5.0f;

    private bool m_DeleteBoxes = false;


    private void OnTriggerEnter(Collider other)
    {
        if(other.GetComponent<BoxBehaviour>() != null && other.transform.parent == null)
        {
            m_BoxList.Add(other.gameObject);
            m_BoxCounter++;
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if(m_DeleteBoxes)
        {
            if(other.GetComponent<BoxBehaviour>() && other.transform.parent == null)
            {
                m_BoxList.Remove(other.gameObject);
             
                Destroy(other.gameObject);
                Instantiate(m_DestroyParticle, transform.position, transform.rotation);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.GetComponent<BoxBehaviour>() != null && other.transform.parent == null)
        {
            m_BoxList.Remove(other.gameObject);
      
        }
    }

    private void Update()
    {


        if (m_BoxDeletionTimer >= m_MaxBoxTime)
        {
            m_DeleteBoxes = true;

        }


        if (m_BoxList.Count() > 0)
        {
            m_BoxDeletionTimer += Time.deltaTime;
        }
        else
        {
            m_DeleteBoxes = false;
            m_BoxDeletionTimer = 0.0f;
        }


    }


}
