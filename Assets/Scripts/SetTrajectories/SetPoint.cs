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
    
    [SerializeField] private ControladorValoresPanel controlPanel;
    [SerializeField] private AudioSource weldingSound;
    [SerializeField] private Material[] colorLine; //color of the line
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
                initLine.startWidth = widthLine-0.008f;
                initLine.endWidth = widthLine-0.008f;
            }
        );
        controlPanel.GetComponentInChildren<ControladorValoresPanel>().valorDisplay[1].valorSlider.onValueChange.AddListener( val =>
            {
                float valueAmpLine = val * 20f;
                int index = Mathf.FloorToInt( Mathf.InverseLerp(0f,20f,valueAmpLine) * colorLine.Length );
                index = Mathf.Clamp( index, 0, colorLine.Length - 1 );
                welderLine.material = colorLine[index];
                // 1 Violeta → Azul
                // 2 Azul → Cian
                // 3 Cian → Verde
                // 4 Verde → Amarillo
                // 5 Amarillo → Naranja
                // 6 Naranja → Rojo
            }
        );
    }
    /// <summary>
    /// This function instantiate a new object when it pressed the button at scene
    /// </summary>
    public void instanceNewObject()
    {
        if (points.Count == 0 || Vector3.Distance(points[points.Count - 1].transform.position, endEffector.transform.position) > 0.001f)
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
    }

    /// <summary>
    /// When press the button this function eliminates all gameobjects of type sphere for this case and clear the workspace
    /// </summary>
    public void CleanSpheres()
    {
        for (int i = 0; i < points.Count; i++)
            GameObject.Destroy(points[i]);
        points.Clear();
        initLine.positionCount = 0;
        welderLine.positionCount = 0;
        initLine.enabled = false;
        welderLine.enabled = false;
        initLine.enabled = true;
        initLine.enabled = true;
    }

    /// <summary>
    /// When press the button this function eliminates only the last gameobjects of type sphere
    /// </summary
    public void RemoveLastSphere()
    {
        if (points.Count == 0) return;

        int lastIndex = points.Count - 1;
        GameObject.Destroy(points[lastIndex]);
        points.RemoveAt(lastIndex);

        initLine.positionCount = points.Count;
        welderLine.positionCount = points.Count;
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
