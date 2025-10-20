using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.ProBuilder.Shapes;

public class ExecuteTrajectories : MonoBehaviour
{
    [Header("Trajectories Settings")]
    [SerializeField] private GameObject axis;
    [SerializeField] private float trajectoriesDuration = 10f; // time between a execute articulation and the next one
    [SerializeField] private AnimationCurve curve; //animation curve to simulate a smooth movement

    //List trajectories for each articulation
    [SerializeField] private List<Vector3> rotation = new List<Vector3>();
    [SerializeField] private List<Vector3> trajectory = new List<Vector3>();
    
    [SerializeField] private LineRenderer welderLine; //load render line for welder
    [SerializeField] private Transform endEffector; //load transform of the end effector object
    [SerializeField] private ParticleSystem sparkEffect; // load sparkeffect for weldering system
    [SerializeField] private List<Vector3> points = new List<Vector3>();

    private int trajectoryCount=0;

    /// <summary>
    /// When press the button this function allows to program save current angle for each articulation
    /// </summary>
    private void Start()
    {
        welderLine.enabled = false;
        sparkEffect=Instantiate(sparkEffect);
    }
    public void ListMovements()
    {       
        trajectory.Add(axis.transform.localPosition);
        rotation.Add(axis.transform.localEulerAngles);
        points.Add(endEffector.position);
        trajectoryCount++;
    }

    /// <summary>
    /// When press the button this function eliminates all the previous trajectories and clear the workspace
    /// </summary>
    public void CleanMovements()
    {
    
        trajectory.Clear();
        rotation.Clear();
        points.Clear();
        trajectoryCount = 0;
    }

    /// <summary>
    /// When press the button execute the movement trajectories to all positions in the list when there is more than one
    /// </summary>
    public void ExecuteMovement()
    {
        if (trajectory.Count>=2)
            StartCoroutine(AngularAxisMovement(trajectoriesDuration));
    }
    IEnumerator AngularAxisMovement(float totalTime)
    {        
        float timeElapsed = 0f;
        int currentTrajectoryIndex = 0;
        welderLine.enabled = true;
        welderLine.positionCount = 2;
        welderLine.SetPosition(currentTrajectoryIndex, points[currentTrajectoryIndex]);
        sparkEffect.Play();
        while (currentTrajectoryIndex < trajectoryCount-1)
        {
            
            axis.transform.localPosition= Vector3.Lerp(trajectory[currentTrajectoryIndex], trajectory[currentTrajectoryIndex + 1], curve.Evaluate(timeElapsed / totalTime));
            axis.transform.localRotation = Quaternion.Lerp(Quaternion.Euler(rotation[currentTrajectoryIndex]), Quaternion.Euler(rotation[currentTrajectoryIndex + 1]), curve.Evaluate(timeElapsed / totalTime));
            timeElapsed += Time.deltaTime;
            if (timeElapsed >= totalTime || Vector3.Distance(axis.transform.localPosition, trajectory[currentTrajectoryIndex + 1]) < 0.001f && Quaternion.Angle(axis.transform.localRotation, Quaternion.Euler(rotation[currentTrajectoryIndex + 1])) < 0.1f)
            {
                currentTrajectoryIndex++;
                timeElapsed = 0f;
                if (currentTrajectoryIndex <= trajectoryCount-2)
                    welderLine.positionCount++;
            }
            if (Vector3.Distance(axis.transform.position, points[currentTrajectoryIndex]) > 0.1f && timeElapsed > 0.1f)                
                welderLine.SetPosition(currentTrajectoryIndex+1,endEffector.position);
            sparkEffect.transform.position = endEffector.position;
            yield return null;
        }
        //welderLine.enabled = false;
        sparkEffect.Stop();
    }

    

    
}
