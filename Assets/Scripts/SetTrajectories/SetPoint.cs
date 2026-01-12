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
    [SerializeField] private LineRenderer initLine; //load an initial line renderer to copy its settings    
    [SerializeField] private LineRenderer welderLine; //load the base component of line trajectory
    [SerializeField] private ParticleSystem sparkEffect; // load sparkeffect for weldering system

    [Header("Welding Settings")]
    [SerializeField] private float widthLine = 0.02f; //width of the line
    [SerializeField, Range(0, 20)] private float colorLine; //color of the line
    [SerializeField] private ControladorValoresPanel controlPanel;
    [SerializeField] private AudioSource weldingSound;

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
                welderLine.startWidth = widthLine; 
                welderLine.endWidth = widthLine;
                initLine.startWidth = widthLine-0.005f;
                initLine.endWidth = widthLine-0.005f;
            }
        );
        controlPanel.GetComponentInChildren<ControladorValoresPanel>().valorDisplay[1].valorSlider.onValueChange.AddListener( val =>
            {
                Color Blend(Color c1, Color c2, float st, float end, float v)
                {
                    return Color.Lerp(c1, c2, Mathf.InverseLerp(st, end, v));
                }
                float valueAmpLine = val*20f;
                Color finalColor;

                if (valueAmpLine <= 5f) finalColor = Blend(Color.white, Color.yellow, 0f, 5f, valueAmpLine);
                else if (valueAmpLine <= 10f) finalColor = Blend(Color.yellow, Color.gray, 5f, 10f, valueAmpLine);
                else if (valueAmpLine <= 15f) finalColor = Blend(Color.gray, new Color(1f, 0.5f, 0f), 10f, 15f, valueAmpLine);
                else finalColor = Blend(new Color(1f, 0.5f, 0f), Color.red, 15f, 20f, valueAmpLine);

                welderLine.material.color = finalColor;
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
        initLine.positionCount = points.Count;
        for (int index = 0; index < initLine.positionCount; index++)
            initLine.SetPosition(index, points[index].transform.position); // run the array and position points on stablished positions
    }

    /// <summary>
    /// When press the button this function eliminates all gameobjects of type sphere for this case and clear the workspace
    /// </summary>
    public void CleanSpheres()
    {
        for (int i = 0; i <= points.Count; i++)
            GameObject.Destroy(points[i]);
        points.Clear();
        initLine.positionCount = 0;
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
        float totalTime = 12f * Vector3.Distance(points[currentIndex].transform.position, points[currentIndex + 1].transform.position);
        welderLine.enabled = true;
        welderLine.positionCount = 2;
        welderLine.SetPosition(currentIndex, points[currentIndex].transform.position);
        sparkEffect.Play();
        weldingSound.Play();
        while (currentIndex < points.Count - 1)
        {
            timeElapsed += Time.deltaTime;
            if (timeElapsed >= totalTime)
            {
                Debug.Log("Tiempo total soldador: "+totalTime);
                currentIndex++;
                timeElapsed = 0f;
                if (currentIndex <= points.Count - 2)
                    welderLine.positionCount++;
                if (currentIndex < points.Count - 1)
                    totalTime = 12f * Vector3.Distance(points[currentIndex].transform.position, points[currentIndex + 1].transform.position);
            }
            if (Vector3.Distance(endEffector.transform.position, points[currentIndex].transform.position) > 0.001f && timeElapsed > 0.001f)
                welderLine.SetPosition(currentIndex + 1, endEffector.position);
            sparkEffect.transform.position = endEffector.position;
            yield return null;
        }
        sparkEffect.Stop();
        weldingSound.Stop();
    }
}
