using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

public class SetPoint : MonoBehaviour
{
    [Header(" Settings")]
    [SerializeField] private GameObject sphere; //load the prefab for this case a sphere
    [SerializeField] private Transform endEffector;//load transform of the end effector object
    [SerializeField] private bool drawObject = true; //boolean to draw or not the line

    [Header("Render Line Settings")]
    [SerializeField] private LineRenderer welderLine; //load the base component of line trajectory
    [SerializeField] private ParticleSystem sparkEffect; // load sparkeffect for weldering system

    [Header("Welding Settings")]
    [SerializeField] private float widthLine = 0.02f; //width of the line
    [SerializeField, Range(0, 20)] private float colorLine; //color of the line
    [SerializeField] private ControladorValoresPanel controlPanel;

    private List<GameObject> points = new List<GameObject>();
    private GameObject newSphere;
    private void Start()
    {
        welderLine.material.color = Color.gray;
        welderLine.startWidth = widthLine; 
        welderLine.endWidth = widthLine;
        welderLine.enabled = false;
        sparkEffect = Instantiate(sparkEffect);
        controlPanel.GetComponentInChildren<ControladorValoresPanel>().valorDisplay[0].valorSlider.onValueChange.AddListener( val =>
            {
                widthLine = Mathf.InverseLerp(0f, 120f, val*120f) * 0.04f;
                welderLine.startWidth = widthLine; welderLine.endWidth = widthLine;
            }
        );
        controlPanel.GetComponentInChildren<ControladorValoresPanel>().valorDisplay[1].valorSlider.onValueChange.AddListener( val =>
            {
                colorLine = Mathf.InverseLerp(0f, 20f, val*20f);
                welderLine.material.color = Color.Lerp(Color.yellow, Color.red, colorLine);
            }
        );
    }
    /// <summary>
    /// This function instantiate a new object when it pressed the button at scene
    /// </summary>
    public void instanceNewObject()
    {
        newSphere = Instantiate(sphere, endEffector.transform.position, Quaternion.identity); //instance a new object taken the prefab that you load it before with a position and rotation stablished previously
        points.Add(newSphere); // add a new object at list
        foreach (var point in points)
            point.SetActive(drawObject);
        welderLine.positionCount = points.Count; //stablished the quantity of points created in scene
   
    }

    /// <summary>
    /// When press the button this function eliminates all gameobjects of type sphere for this case and clear the workspace
    /// </summary>
    public void CleanSpheres()
    {
        for (int i = 0; i < points.Count; i++){
            GameObject.Destroy(points[i]);
            points.Clear();
        }
        welderLine.positionCount = 0;
    }

    public void ExecuteSpark()
    {
        if (points.Count >= 2)
            StartCoroutine(MovementEndEffector());
    }
    IEnumerator MovementEndEffector()
    {
        float timeElapsed = 0f;
        int currentIndex = 0;
        float totalTime = 15f * Vector3.Distance(points[currentIndex].transform.position, points[currentIndex + 1].transform.position);
        welderLine.enabled = true;
        welderLine.positionCount = 2;
        welderLine.SetPosition(currentIndex, points[currentIndex].transform.position);
        sparkEffect.Play();
        while (currentIndex < points.Count - 1)
        {
            timeElapsed += Time.deltaTime;
            if (timeElapsed >= totalTime)
            {
                Debug.Log(totalTime);
                currentIndex++;
                timeElapsed = 0f;
                if (currentIndex <= points.Count - 2)
                    welderLine.positionCount++;
                if (currentIndex < points.Count - 1)
                    totalTime = 15f * Vector3.Distance(points[currentIndex].transform.position, points[currentIndex + 1].transform.position);
            }
            if (Vector3.Distance(endEffector.transform.position, points[currentIndex].transform.position) > 0.01f && timeElapsed > 0.01f)
                welderLine.SetPosition(currentIndex + 1, endEffector.position);
            sparkEffect.transform.position = endEffector.position;
            yield return null;
        }
        sparkEffect.Stop();
    }
}
