using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// Author = Tristan Wauthier
/// <summary>
/// This scripts makes destroys boxes that move through it.
/// </summary>
public class BoxVoid : MonoBehaviour
{
    [Header("Layers")]
    [SerializeField] private int m_DynamicBoxLayer = 11;


    private void OnTriggerEnter(Collider other)
    {
        GameObject go = other.gameObject;

        if(go.layer == m_DynamicBoxLayer)
            Destroy(go);
    }
}
