using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialFiguras : MonoBehaviour
{
    public GameObject[] steps;
    private int currentStep = 0;

    void Start()
    {
        ShowStep(currentStep);
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
            currentStep = steps.Length - 1; // va al último

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
