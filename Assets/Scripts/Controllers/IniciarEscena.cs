using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IniciarEscena : MonoBehaviour
{
    [SerializeField] private GameObject[] objetosActivables;
    [SerializeField] private GameObject[] posicionesSocketInteractor;
    [SerializeField] private GameObject[] objetosDuplicables;
    private bool activacion = true;
    public GameObject panel;
    public GameObject[] steps;
    private List<GameObject> figurasInstanciadas = new List<GameObject>();
    public int currentStep = 0;
    public int indicepublic = 0;


    public void CambiarEstadoObjetos()
    {
        MostrarPasoAleatorio();

        foreach (GameObject objeto in objetosActivables)
            objeto.SetActive(!activacion);

        if (activacion)
        {
            int indiceAleatorio = Random.Range(0, posicionesSocketInteractor.Length);
            indiceAleatorio = indicepublic;

            for (int i = 0; i < posicionesSocketInteractor.Length; i++)
            {
                posicionesSocketInteractor[i].SetActive(i == indiceAleatorio);
            }
        }
        else
        {
            foreach (GameObject objeto in posicionesSocketInteractor)
            {
                objeto.SetActive(false);
            }
        }

        panel.SetActive(activacion);

        if (activacion)
        {
            figurasInstanciadas.Clear();

            foreach (GameObject objeto in objetosDuplicables)
            {
                for (int i = 0; i < 3; i++)
                {
                    GameObject copia = Instantiate(
                        objeto,
                        objeto.transform.position,
                        objeto.transform.rotation);

                    figurasInstanciadas.Add(copia);
                }
            }
        }
        else
        {
            foreach (GameObject figura in figurasInstanciadas)
            {
                if (figura != null)
                    Destroy(figura);
            }

            figurasInstanciadas.Clear();
        }

        activacion = !activacion;
        

    }
 
    private void MostrarPasoAleatorio()
    {
        if (steps.Length == 0)
            return;

        currentStep = Random.Range(0, steps.Length);

        ShowStep(currentStep);
    }

    void ShowStep(int index)
    {
        for (int i = 0; i < steps.Length; i++)
        {
            steps[i].SetActive(i == index);
        }
    }

}

