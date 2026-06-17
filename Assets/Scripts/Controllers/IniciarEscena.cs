using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IniciarEscena : MonoBehaviour
{
    [SerializeField] private GameObject[] objetosActivables;
    [SerializeField] private GameObject[] objetosPosicionables;
    [SerializeField] private GameObject[] posicionesSocketInteractor;
    private GameObject newFigure;
    private bool activacion = true;
    public GameObject panel;
    public GameObject[] steps;
    
    private int currentStep = 0;
    public void CambiarEstadoObjetos()
    {
        foreach (GameObject objeto in objetosActivables)
            objeto.SetActive(!activacion);
        foreach (GameObject objeto in posicionesSocketInteractor)
            objeto.SetActive(activacion);
        panel.SetActive(activacion);
        
        if(activacion)
            newFigure = Instantiate(objetosPosicionables[0], new Vector3(0.30f, 0.92f, 2.48f), Quaternion.identity);
        else
            Destroy(newFigure);

        activacion = !activacion;

    }
    public void CambiarPosicionObjetos()
    {
        float x, y, z;
        if (!activacion)
        {
            y = 0.90f; z = 2.40f;
            foreach (GameObject objeto in objetosPosicionables)
            {
                objeto.transform.position = new Vector3(-1.02f, y, z);
                y += 0.04f; z += 0.02f;
            }
        }
        else
        {
            x = -1.35f; y = 0.74f;
            foreach (GameObject objeto in objetosPosicionables)
            {
                objeto.transform.position = new Vector3(x, y, 1.49f);
                x -= 0.1f; y += 0.04f;
            }
        }
    }

    public void NextStep()
    {
        currentStep++;

        if (currentStep >= steps.Length)
            currentStep = 0;

        ShowStep(currentStep);
    }

    public void PreviousStep()
    {
        currentStep--;

        if (currentStep < 0)
            currentStep = steps.Length - 1; 

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
