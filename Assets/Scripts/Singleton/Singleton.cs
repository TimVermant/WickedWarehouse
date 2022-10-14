using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Singleton<T> : MonoBehaviour where T : MonoBehaviour
{
    private static T s_Instance;
    public static T Instance
    {
        get 
        {
            if (s_Instance == null)
            {
                s_Instance = FindObjectOfType<T>();
                if(s_Instance == null)
                {
                    GameObject go = new GameObject();
                    go.name = typeof(T).Name;
                    s_Instance = go.AddComponent<T>();
                }
            }
            return s_Instance;
        }
    }
    private void Awake()
    {
        if (s_Instance == null)
        {
            s_Instance = this as T;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
}
