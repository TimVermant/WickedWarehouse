using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoxDeleterBehavior : MonoBehaviour
{

    private void OnCollisionEnter(Collision collision)
    {
        if(collision.gameObject.GetComponent<BoxBehaviour>() != null && collision.transform.parent == null)
        {
            Destroy(collision.gameObject);
        }
    }
}
