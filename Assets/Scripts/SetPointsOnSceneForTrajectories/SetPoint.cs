using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class SetPoint : MonoBehaviour
{
    [Header("Render Line Settings")]
    [SerializeField] private LineRenderer lr; //load the component linerenderer at scene
    [SerializeField] private GameObject sphere; //load the prefab for this case a sphere
    [SerializeField] private Transform spherePosition;
    [SerializeField] private bool drawObject = true; //boolean to draw or not the line

    private GameObject newSphere;
    private List<GameObject> points = new List<GameObject>();
    private void Start()
    {
        // Oculta o muestra el LineRenderer
        if (lr != null)
            lr.enabled = drawObject;

        // Oculta o muestra todas las esferas instanciadas
        foreach (var point in points)
        {
            if (point != null)
                point.SetActive(drawObject);
        }
    }
    /// <summary>
    /// This function instantiate a new object when it pressed the button at scene
    /// </summary>
    public void instanceNewObject()
    {
        if (drawObject)
        {
            newSphere = Instantiate(sphere, spherePosition.transform.position, Quaternion.identity); //instance a new object taken the prefab that you load it before with a position and rotation stablished previously
            points.Add(newSphere); // add a new object at list
            lr.positionCount = points.Count; //stablished the quantity of points created in scene
            for (int index = 0; index < lr.positionCount; index++)
                lr.SetPosition(index, points[index].transform.position); // run the array and position points on stablished positions
        }
    }

    /// <summary>
    /// When press the button this function eliminates all gameobjects of type sphere for this case and clear the workspace
    /// </summary>
    public void CleanSpheres()
    {
        for (int i = 0; i < points.Count; i++)
            GameObject.Destroy(points[i]);
        for (int i = 0; i < points.Count; i++)
            points.Clear();
        lr.positionCount = 0;
    }
}
