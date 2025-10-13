using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.ProBuilder;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

public class PreviewZone : MonoBehaviour
{
    public enum ZonePosition { Izquierda, Centro, Derecha }

    [Header("Configuración de zona")]
    public ZonePosition posicionZona = ZonePosition.Centro;

    [Header("Punto de colocación (si no se asigna, usa el transform de este GameObject)")]
    [SerializeField] private Transform placementPoint;

    [Header("Prefabs de figuras (definitivas)")]
    public GameObject cuadradoPrefab;
    public GameObject paralelogramoPrefab;
    public GameObject trianguloPrefab;

    [Header("Prefabs de previsualización")]
    public GameObject cuadradoPreview;
    public GameObject paralelogramoPreview;
    public GameObject trianguloPreview;

    private GameObject currentPreview;
    private XRGrabInteractable currentInteractable;
    private bool isInside = false;

    // Referencia global (o compartida) que indica qué figura hay en el centro.
    public static string figuraCentral = "";

    private void Start()
    {
        if (placementPoint == null)
            placementPoint = transform;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Interactable") || currentPreview != null) return;

        string shapeType = GetShapeType(other.gameObject);
        if (!IsValidPlacement(shapeType)) return;

        GameObject previewPrefab = GetPreviewPrefabByType(shapeType);
        if (previewPrefab == null) return;

        // Instanciar preview con la rotación correcta desde el inicio
        Quaternion previewRot = GetRotationForType(shapeType);
        currentPreview = Instantiate(previewPrefab, placementPoint.position, previewRot);

        currentInteractable = other.GetComponent<XRGrabInteractable>();
        if (currentInteractable != null)
            currentInteractable.selectExited.AddListener(OnReleasedInside);

        isInside = true;
    }

    private void OnTriggerStay(Collider other)
    {
        if (currentInteractable == null || other.gameObject != currentInteractable.gameObject) return;
        if (currentPreview != null)
        {
            // Mantener preview en el punto y con la rotación actualizada (por si cambia figuraCentral mientras se arrastra)
            string shapeType = GetShapeType(currentInteractable.gameObject);
            currentPreview.transform.position = placementPoint.position;
            currentPreview.transform.rotation = GetRotationForType(shapeType);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Interactable")) return;

        if (currentPreview != null) Destroy(currentPreview);
        if (currentInteractable != null)
            currentInteractable.selectExited.RemoveListener(OnReleasedInside);

        currentPreview = null;
        currentInteractable = null;
        isInside = false;
    }

    private void OnReleasedInside(SelectExitEventArgs args)
    {
        if (!isInside || currentPreview == null || currentInteractable == null) return;

        string shapeType = GetShapeType(currentInteractable.gameObject);
        if (!IsValidPlacement(shapeType)) return;

        Quaternion finalRotation = GetRotationForType(shapeType);

        GameObject prefabToPlace = GetPrefabByType(shapeType);
        if (prefabToPlace == null) return;

        Instantiate(prefabToPlace, placementPoint.position, finalRotation);

        if (posicionZona == ZonePosition.Centro)
            figuraCentral = shapeType; // Actualizar quién está en el centro

        currentInteractable.selectExited.RemoveListener(OnReleasedInside);
        Destroy(currentInteractable.gameObject);
        Destroy(currentPreview);

        currentInteractable = null;
        currentPreview = null;
        isInside = false;
    }

    // --- Lógica de rotación según la zona y la figura central ---
    private Quaternion GetRotationForType(string shapeType)
    {
        Quaternion baseRot = placementPoint.rotation;
        float angleY = 0f;

        if (shapeType == "Triangle")
        {
            // Triángulo tiene rotaciones condicionales según zona y figura central
            if (posicionZona == ZonePosition.Izquierda)
            {
                if (figuraCentral == "Parallelogram") angleY = 0;
                else if (figuraCentral == "Cube") angleY = 90f;
            }
            else if (posicionZona == ZonePosition.Derecha)
            {
                if (figuraCentral == "Parallelogram") angleY = 180f;
                else if (figuraCentral == "Cube") angleY = 0f;
            }
            // En Centro no se coloca triángulo según reglas, pero si llegase, angleZ = 0
        }
        // Cuadrado y Paralelogramo mantienen la rotación base
        return baseRot * Quaternion.Euler(0f, angleY, 0);
    }

    // --- Utilidades ---
    private string GetShapeType(GameObject obj)
    {
        string name = obj.name.ToLower();
        if (name.Contains("cube") || name.Contains("cube")) return "Cube";
        if (name.Contains("triangle") || name.Contains("triangle")) return "Triangle";
        if (name.Contains("parallelogram") || name.Contains("parallelogram")) return "Parallelogram";
        return "";
    }

    private GameObject GetPrefabByType(string type)
    {
        switch (type)
        {
            case "Cube": return cuadradoPrefab;
            case "Triangle": return trianguloPrefab;
            case "Parallelogram": return paralelogramoPrefab;
            default: return null;
        }
    }

    private GameObject GetPreviewPrefabByType(string type)
    {
        switch (type)
        {
            case "Cube": return cuadradoPreview;
            case "Triangle": return trianguloPreview;
            case "Parallelogram": return paralelogramoPreview;
            default: return null;
        }
    }

    private bool IsValidPlacement(string type)
    {
        switch (posicionZona)
        {
            case ZonePosition.Centro:
                return (type == "Cube" || type == "Parallelogram");

            case ZonePosition.Izquierda:
                if (figuraCentral == "Parallelogram") return (type == "Triangle");
                if (figuraCentral == "Cube") return (type == "Cube" || type == "Triangle");
                return false;

            case ZonePosition.Derecha:
                if (figuraCentral == "Parallelogram") return (type == "Triangle");
                if (figuraCentral == "Cube") return (type == "Cube" || type == "Triangle");
                return false;
        }
        return false;
    }
}
