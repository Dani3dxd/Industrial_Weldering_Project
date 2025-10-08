using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class PositionObjects : MonoBehaviour
{
    [Header("Placement Points")]
    public Transform[] placementPoints;
    public List<Transform> availablePoints;

    [Header("Interactable Prefabs")]
    public GameObject cubePrefab;
    public GameObject trianglePrefab;
    public GameObject parallelogramPrefab;

    [Header("Preview Prefabs")]
    public GameObject cubePreviewPrefab;
    public GameObject trianglePreviewPrefab;
    public GameObject parallelogramPreviewPrefab;

    private GameObject currentPreview;
    private UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable currentInteractable;
    private bool isInside = false;

    private string firstShapeType = "";
    private int placedCount = 0;

    private void Start()
    {
        availablePoints = new List<Transform>(placementPoints);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Interactable") && currentPreview == null && availablePoints.Count > 0)
        {
            string shapeType = GetShapeType(other.gameObject);
            GameObject previewToUse = GetPreviewPrefabByType(shapeType);
            if (previewToUse == null) return;

            Transform nextPoint = GetClosestPoint(other.transform.position);
            currentPreview = Instantiate(previewToUse, nextPoint.position, nextPoint.rotation);

            currentInteractable = other.GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable>();
            currentInteractable.selectExited.AddListener(OnReleasedInside);
            isInside = true;
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (currentInteractable == null || other.gameObject != currentInteractable.gameObject) return;
        UpdatePreviewPosition(other.transform);
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

    private void UpdatePreviewPosition(Transform draggingTransform)
    {
        if (availablePoints.Count == 0 || currentPreview == null) return;

        Transform closestPoint = GetClosestPoint(draggingTransform.position);
        if (closestPoint == null) return;

        currentPreview.transform.position = closestPoint.position;
        currentPreview.transform.rotation = closestPoint.rotation;
    }

    private void OnReleasedInside(SelectExitEventArgs args)
    {
        if (!isInside || currentPreview == null || currentInteractable == null) return;
        if (availablePoints.Count == 0) return;

        string incomingType = GetShapeType(currentInteractable.gameObject);
        if (!IsValidPlacement(incomingType)) return;

        Transform closestPoint = GetClosestPoint(args.interactorObject.transform.position);
        if (closestPoint == null) return;

        GameObject prefabToUse = GetPrefabByType(incomingType);
        if (prefabToUse == null) return;

        GameObject placedObject = Instantiate(prefabToUse, closestPoint.position, closestPoint.rotation);
        availablePoints.Remove(closestPoint);

        currentInteractable.selectExited.RemoveListener(OnReleasedInside);
        Destroy(currentInteractable.gameObject);
        Destroy(currentPreview);

        placedCount++;
        if (placedCount == 1) firstShapeType = incomingType;

        currentInteractable = null;
        currentPreview = null;
        isInside = false;
    }

    private Transform GetClosestPoint(Vector3 position)
    {
        Transform closestPoint = null;
        float minDistance = Mathf.Infinity;

        foreach (Transform point in availablePoints)
        {
            float dist = Vector3.Distance(position, point.position);
            if (dist < minDistance)
            {
                minDistance = dist;
                closestPoint = point;
            }
        }

        return closestPoint;
    }

    private string GetShapeType(GameObject obj)
    {
        string name = obj.name.ToLower();
        if (name.Contains("cube")) return "Cube";
        if (name.Contains("triangle")) return "Triangle";
        if (name.Contains("parallelogram")) return "Parallelogram";
        return "";
    }

    private GameObject GetPrefabByType(string type)
    {
        switch (type)
        {
            case "Cube": return cubePrefab;
            case "Triangle": return trianglePrefab;
            case "Parallelogram": return parallelogramPrefab;
            default: return null;
        }
    }

    private GameObject GetPreviewPrefabByType(string type)
    {
        switch (type)
        {
            case "Cube": return cubePreviewPrefab;
            case "Triangle": return trianglePreviewPrefab;
            case "Parallelogram": return parallelogramPreviewPrefab;
            default: return null;
        }
    }

    private bool IsValidPlacement(string incomingType)
    {
        if (placedCount == 0) return true;

        switch (firstShapeType)
        {
            case "Cube":
                return incomingType == "Cube" || incomingType == "Triangle";
            case "Triangle":
                return incomingType == "Triangle" || incomingType == "Parallelogram";
            case "Parallelogram":
                return incomingType == "Triangle" || incomingType == "Parallelogram";
            default:
                return false;
        }
    }
}