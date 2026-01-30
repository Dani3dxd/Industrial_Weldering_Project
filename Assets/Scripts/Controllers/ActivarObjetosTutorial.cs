using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActivarObjetosTutorial : MonoBehaviour
{
    [SerializeField] private GameObject[] objetosActivables;
    [SerializeField] private GameObject objetoActivo;
    bool activacion = false;  
    public void CambiarEstadoObjetos()
    {
        foreach (GameObject objeto in objetosActivables)
            objeto.SetActive(!activacion);
        activacion = !activacion;
    }
    private void OnEnable()
    {
        EventoActivador.SalidaTutorial += DesactivarObjetos;
    }

    private void OnDisable()
    {
        EventoActivador.SalidaTutorial -= DesactivarObjetos;
    }

    private void DesactivarObjetos()
    {
        foreach (GameObject obj in objetosActivables)
            obj.SetActive(false);

        objetoActivo.SetActive(true);
        activacion = false;
    }

}
