using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

public class SetPoint : MonoBehaviour
{
    [Header("Render Line Settings")]
    [SerializeField] private LineRenderer baseLine; //load the base component of line trajectory
    [SerializeField] private GameObject sphere; //load the prefab for this case a sphere
    [SerializeField] private Transform endEffector;//load transform of the end effector object

    [SerializeField] private bool drawObject = true; //boolean to draw or not the line
   
    private GameObject newSphere;
    private List<GameObject> points = new List<GameObject>();
    private void Start()
    {
        // Show or hide the LineRenderer
        if (baseLine != null)
            baseLine.enabled = drawObject;

        // Show or hide the instantiate spheres
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
            newSphere = Instantiate(sphere, endEffector.transform.position, Quaternion.identity); //instance a new object taken the prefab that you load it before with a position and rotation stablished previously
            points.Add(newSphere); // add a new object at list
            baseLine.positionCount = points.Count; //stablished the quantity of points created in scene
            for (int index = 0; index < baseLine.positionCount; index++)
                baseLine.SetPosition(index, points[index].transform.position); // run the array and position points on stablished positions
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
        baseLine.positionCount = 0;
    }    
}
