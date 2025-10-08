using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowMe : MonoBehaviour
{
    [SerializeField] private Transform objetoASeguir;

    void Update()
    {
        if (objetoASeguir != null)
            transform.position = objetoASeguir.position;
    }
}
