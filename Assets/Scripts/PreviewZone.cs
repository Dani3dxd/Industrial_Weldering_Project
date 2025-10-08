using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class PreviewZone : MonoBehaviour
{
    public Transform[] placementPoints; // Asignados en el inspector
    public List<Transform> availablePoints;
    public GameObject previewPrefab;
    public GameObject interactablePrefab; // Objeto original para instanciar
    private GameObject currentPreview;
    private bool isInside = false;
    private UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable currentInteractable;

    private void Start()
    {
        availablePoints = new List<Transform>(placementPoints);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Interactable") && currentPreview == null && availablePoints.Count > 0)
        {
            // Usar pr�xima posici�n libre
            Transform nextPoint = availablePoints[0];

            currentPreview = Instantiate(previewPrefab, nextPoint.position, nextPoint.rotation);
            currentInteractable = other.GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable>();

            currentInteractable.selectExited.AddListener(OnReleasedInside);
            isInside = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Interactable"))
        {
            if (currentPreview != null) Destroy(currentPreview);
            if (currentInteractable != null)
                currentInteractable.selectExited.RemoveListener(OnReleasedInside);

            currentPreview = null;
            currentInteractable = null;
            isInside = false;
        }
    }

    private void OnTriggerStay(Collider other)
    {
        // Verificamos si el objeto que está dentro es el actual interactable
        if (currentInteractable == null || other.gameObject != currentInteractable.gameObject) return;

        // Actualizamos el preview a la posición más cercana del objeto
        UpdatePreviewPosition(other.transform);
    }

    private void UpdatePreviewPosition(Transform draggingTransform)
    {
        if (availablePoints.Count == 0 || currentPreview == null) return;

        Vector3 dragPos = draggingTransform.position;
        Transform closestPoint = null;
        float minDistance = Mathf.Infinity;

        foreach (Transform point in availablePoints)
        {
            float dist = Vector3.Distance(dragPos, point.position);
            if (dist < minDistance)
            {
                minDistance = dist;
                closestPoint = point;
            }
        }

        if (closestPoint == null) return;

        // Posicionar el preview en el punto más cercano
        currentPreview.transform.position = closestPoint.position;
        currentPreview.transform.rotation = closestPoint.rotation;
    }

    private void OnReleasedInside(SelectExitEventArgs args)
    {
        if (!isInside || currentPreview == null || currentInteractable == null) return;
        if (availablePoints.Count == 0) return;

        // 1. Obtener posición de soltado
        Vector3 releasePosition = args.interactorObject.transform.position;

        // 2. Buscar el punto más cercano
        Transform closestPoint = null;
        float minDistance = Mathf.Infinity;

        foreach (Transform point in availablePoints)
        {
            float dist = Vector3.Distance(releasePosition, point.position);
            if (dist < minDistance)
            {
                minDistance = dist;
                closestPoint = point;
            }
        }

        if (closestPoint == null) return;

        // 3. Instanciar y ocupar el punto
        GameObject placedObject = Instantiate(interactablePrefab, closestPoint.position, closestPoint.rotation);
        availablePoints.Remove(closestPoint);

        // 4. Eliminar objetos antiguos
        currentInteractable.selectExited.RemoveListener(OnReleasedInside);
        Destroy(currentInteractable.gameObject);
        if (currentPreview != null) Destroy(currentPreview);

        // 5. Resetear estado
        currentInteractable = null;
        currentPreview = null;
        isInside = false;
    }

}
