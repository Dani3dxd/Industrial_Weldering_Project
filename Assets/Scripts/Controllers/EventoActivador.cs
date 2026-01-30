using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EventoActivador : MonoBehaviour
{
    public static event Action SalidaTutorial;

    public void OnEnable()
    {
        SalidaTutorial?.Invoke();
        gameObject.SetActive(false);
    }

}
