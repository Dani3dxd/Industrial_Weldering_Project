using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.ProBuilder.Shapes;

public class ExecuteTrajectories : MonoBehaviour
{
    [Header("Trajectories Settings")]
    [SerializeField] private GameObject axis;
    [SerializeField] private AnimationCurve curve; //animation curve to simulate a smooth movement

    //List trajectories for each articulation
    [Header("Ubication Points")]
    [SerializeField] public List<Vector3> rotation = new List<Vector3>();
    [SerializeField] public List<Vector3> trajectory = new List<Vector3>();
    
    [Header("Show Results")]
    [SerializeField] public GameObject panelResults;
    [SerializeField] public TMP_Text textoTiempo;
    public float finalTime = 0f;
    public List<float> partialTime;

    private int trajectoryCount=0;

    /// <summary>
    /// When press the button this function allows to program save current angle for each articulation
    /// </summary>
    
    public void ListMovements()
    {
        if (trajectory.Count == 0 || Vector3.Distance(trajectory[trajectory.Count - 1], axis.transform.localPosition) > 0.001f)
        {
            trajectory.Add(axis.transform.localPosition);
            rotation.Add(axis.transform.localEulerAngles);
            trajectoryCount++;
        }
    }

    /// <summary>
    /// When press the button this function eliminates all the previous trajectories and clear the workspace
    /// </summary>
    public void CleanMovements()
    {
    
        trajectory.Clear();
        rotation.Clear();
        
        trajectoryCount = 0;
    }

    /// <summary>
    /// Removes the last movement entry from the trajectory and rotation lists, and decrements the trajectory count.
    /// </summary>
    public void RemoveLastMovement()
    {
        if (trajectory.Count==0 || rotation.Count == 0) return;
        
        int lastIndex = trajectory.Count - 1;
        trajectory.RemoveAt(lastIndex);
        rotation.RemoveAt(lastIndex);
            
        trajectoryCount--;
    }

    /// <summary>
    /// When press the button execute the movement trajectories to all positions in the list when there is more than one
    /// </summary>
    public void ExecuteMovement()
    {
        if (trajectory.Count>=2)
            StartCoroutine(AngularAxisMovement());
    }
    IEnumerator AngularAxisMovement()
    {
        //finalTime = 0f;
        float timeElapsed = 0f;
        int currentTrajectoryIndex = 0;
        float totalTime = 15f * Vector3.Distance(trajectory[currentTrajectoryIndex], trajectory[currentTrajectoryIndex + 1]); // adjust time according to distance between points
        partialTime.Add(totalTime);
        while (currentTrajectoryIndex < trajectoryCount-1)
        {
            axis.transform.localPosition= Vector3.Lerp(trajectory[currentTrajectoryIndex], trajectory[currentTrajectoryIndex + 1], curve.Evaluate(timeElapsed / totalTime));
            axis.transform.localRotation = Quaternion.Lerp(Quaternion.Euler(rotation[currentTrajectoryIndex]), Quaternion.Euler(rotation[currentTrajectoryIndex + 1]), curve.Evaluate(timeElapsed / totalTime));
            timeElapsed += Time.deltaTime;
            
            if (timeElapsed >= totalTime || Vector3.Distance(axis.transform.localPosition, trajectory[currentTrajectoryIndex + 1]) < 0.001f && Quaternion.Angle(axis.transform.localRotation, Quaternion.Euler(rotation[currentTrajectoryIndex + 1])) < 0.1f)
            {
                Debug.Log("Tiempo de la trayectoria no."+currentTrajectoryIndex+": "+totalTime);
                //finalTime += totalTime;
                currentTrajectoryIndex++;
                timeElapsed = 0f;
                if (currentTrajectoryIndex < trajectoryCount - 1)
                {
                    totalTime = 15f * Vector3.Distance(trajectory[currentTrajectoryIndex], trajectory[currentTrajectoryIndex + 1]);
                    partialTime.Add(totalTime);
 
                }
            }
            yield return null;   
        }
        panelResults.SetActive(true);
        textoTiempo.text = "Tiempo total de ejecución: " + finalTime.ToString("F2") + " seg \nCantidad de puntos utilizados: " + trajectory.Count;
    }
    public void StartTimer()
    {
        StartCoroutine(TimerCoroutine());
    }
    IEnumerator TimerCoroutine()
    {
        finalTime = 0f;
        while (!panelResults.activeSelf)
        {
            finalTime += Time.deltaTime;
            yield return null;
        }
        Debug.Log("Tiempo total de ejecución: " + finalTime.ToString("F2") + " seg");
    }
}