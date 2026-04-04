using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

public class ReiniciarEscena : MonoBehaviour
{
    [SerializeField] private Transform[] objectsToReset;
    [SerializeField] private ControladorValoresPanel controladorValoresPanel;
    [SerializeField] private XRSocketInteractor socketReinicio;

    private Vector3[] initialPositions;
    private Quaternion[] initialRotations;

    private bool[] initialKinematic;
    private bool[] initialGravity;


    private void Start()
    {
        int n = objectsToReset.Length;

        initialPositions = new Vector3[n];
        initialRotations = new Quaternion[n];

        initialKinematic = new bool[n];
        initialGravity = new bool[n];

        for (int i = 0; i < n; i++)
        {
            Transform obj = objectsToReset[i];
            initialPositions[i] = objectsToReset[i].position;
            initialRotations[i] = objectsToReset[i].rotation;
            Rigidbody rb = obj.GetComponent<Rigidbody>();

            if (rb != null)
            {
                initialKinematic[i] = rb.isKinematic;
                initialGravity[i] = rb.useGravity;
            }
        }        
    }

    public void ResetAll()
    {
        socketReinicio.enabled = false; // Evita que se vuelva a activar mientras se resetea
        for (int i = 0; i < objectsToReset.Length; i++)
        {
            Transform obj = objectsToReset[i];
            Rigidbody rb = obj.GetComponent<Rigidbody>();
            // Evita que la física afecte durante el reseteo
            if (rb != null)
            {
                rb.isKinematic = true; 
                rb.velocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;                
            }
            obj.position = initialPositions[i];
            obj.rotation = initialRotations[i];
            // Reactiva la física después de reposicionar
            if (rb != null)
            {
                rb.isKinematic = false;
                rb.isKinematic = initialKinematic[i];
                rb.useGravity = initialGravity[i];
            }
        }
        controladorValoresPanel.valorDisplay.ForEach(v => 
        {
            v.valorSlider.value = 0f;
            v.valorTexto.text = $"0.0 {v.unidad}";
            v.dato = 0f;
        });
        socketReinicio.enabled = true; // Reactiva el socket después de resetear
    }
}