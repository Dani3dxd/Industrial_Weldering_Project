using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActivarObjetosTutorial : MonoBehaviour
{
    [SerializeField] private GameObject[] objetosParaActivar;
    bool activacion = false;  
    public void CambiarEstadoObjetos()
    {
        foreach (GameObject objeto in objetosParaActivar)
            objeto.SetActive(!activacion);
        activacion = !activacion;
    }
}
