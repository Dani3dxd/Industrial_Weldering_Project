using UnityEngine;
using System;
using System.Collections;


public class CableComponent : MonoBehaviour
{
    #region Class members

    [SerializeField] private Transform endPoint;
    [SerializeField] private Material cableMaterial;

    // Cable config
    [SerializeField] private float cableLength = 0.5f;
    [SerializeField] private int totalSegments = 5;
    [SerializeField] private float segmentsPerUnit = 2f;
    private int segments = 0;
    [SerializeField] private float cableWidth = 0.1f;

    // Solver config
    [SerializeField] private int verletIterations = 1;
    [SerializeField] private int solverIterations = 1;

    /*[Range(0,3)]
    [SerializeField] private float stiffness = 1f;*/

    private LineRenderer line;
    private CableParticle[] points;

    #endregion


    #region Initial setup

    void Start()
    {
        InitCableParticles();
        InitLineRenderer();
    }

    /**
	 * Init cable particles
	 * 
	 * Crea las particulas del cable a lo largo de dos posiciones
	 * e inicia desde el inicio hasta el final del cable con sus respectivos gamObjects.
	 */
    void InitCableParticles()
    {
        // Calcula el segmento a utilizar dependiendo de la longitud del cable y la cantidad de segmentos por unidad
        if (totalSegments > 0)
            segments = totalSegments;
        else
            segments = Mathf.CeilToInt(cableLength * segmentsPerUnit);

        Vector3 cableDirection = (endPoint.position - transform.position).normalized;
        float initialSegmentLength = cableLength / segments;
        points = new CableParticle[segments + 1];

        // Para cada punto del cable, se crea una nueva particula y se posiciona a lo largo de la direccion del cable dependiendo de su indice
        for (int pointIdx = 0; pointIdx <= segments; pointIdx++)
        {
            Vector3 initialPosition = transform.position + (cableDirection * (initialSegmentLength * pointIdx));
            points[pointIdx] = new CableParticle(initialPosition);
        }

        CableParticle start = points[0];
        CableParticle end = points[segments];
        start.Bind(this.transform);
        end.Bind(endPoint.transform);
    }

    /**
	 * Inicializa el line renderer
	 */
    void InitLineRenderer()
    {
        line = this.gameObject.AddComponent<LineRenderer>();
        line.startWidth = cableWidth;
        line.endWidth = cableWidth;
        line.positionCount = segments+1;
        line.material = cableMaterial;
        line.GetComponent<Renderer>().enabled = true;
    }

    #endregion


    #region Render Pass

    void Update()
    {
        RenderCable();
    }

    /**
	 * Render Cable
	 * 
	 * Actualiza cada posicion de las particulas en el LineRenderer.
	 */
    void RenderCable()
    {
        for (int pointIdx = 0; pointIdx < segments + 1; pointIdx++)
        {
            line.SetPosition(pointIdx, points[pointIdx].Position);
        }
    }

    #endregion


    #region Verlet integration & solver pass

    void FixedUpdate()
    {
        for (int verletIdx = 0; verletIdx < verletIterations; verletIdx++)
        {
            VerletIntegrate();
            SolveConstraints();
        }
    }

    /**
	 * Verler integration pass
	 * 
	 * En este paso cada particula actualiza su posicion y velocidad.
	 */
    void VerletIntegrate()
    {
        Vector3 gravityDisplacement = Time.fixedDeltaTime * Time.fixedDeltaTime * Physics.gravity;
        foreach (CableParticle particle in points)
        {
            particle.UpdateVerlet(gravityDisplacement);
        }
    }

    /**
	 * Constrains solver pass
	 * 
	 * En este paso se actualiza cada restriccion en secuencia
	 */
    void SolveConstraints()
    {
        // For each solver iteration..
        for (int iterationIdx = 0; iterationIdx < solverIterations; iterationIdx++)
        {
            SolveDistanceConstraint();
            SolveStiffnessConstraint();
        }
    }

    #endregion


    #region Solver Constraints

    /**
	 * Distancia de restriccion para cada segmento del cable
	 **/
    void SolveDistanceConstraint()
    {
        float segmentLength = cableLength / segments;
        for (int SegIdx = 0; SegIdx < segments; SegIdx++)
        {
            CableParticle particleA = points[SegIdx];
            CableParticle particleB = points[SegIdx + 1];

            // Solve for this pair of particles
            SolveDistanceConstraint(particleA, particleB, segmentLength);
        }
    }

    /**
	 * Distance Constraint 
	 * 
	 * Esta es la resticcion principal que mantiene la pasticulas del cable juntas.
	 */
    void SolveDistanceConstraint(CableParticle particleA, CableParticle particleB, float segmentLength)
    {
        // Find current vector between particles
        Vector3 delta = particleB.Position - particleA.Position;
        // 
        float currentDistance = delta.magnitude;
        float errorFactor = (currentDistance - segmentLength) / currentDistance;

        // Only move free particles to satisfy constraints
        if (particleA.IsFree() && particleB.IsFree())
        {
            particleA.Position += errorFactor * 0.5f * delta;
            particleB.Position -= errorFactor * 0.5f * delta;
        }
        else if (particleA.IsFree())
        {
            particleA.Position += errorFactor * delta;
        }
        else if (particleB.IsFree())
        {
            particleB.Position -= errorFactor * delta;
        }
    }

    /**
	 * Restriccion de rigidez para cada segmento del cable
	 **/
    void SolveStiffnessConstraint()
    {
        float distance = (points[0].Position - points[segments].Position).magnitude;
        if (distance > cableLength)
        {
            foreach (CableParticle particle in points)
            {
                SolveStiffnessConstraint(particle, distance);
            }
        }
    }

    /**
	 * TODO: I'll implement this constraint to reinforce cable stiffness 
	 * 
	 * As the system has more particles, the verlet integration aproach 
	 * may get way too loose cable simulation. This constraint is intended 
	 * to reinforce the cable stiffness.
	 * // throw new System.NotImplementedException ();
	 **/
    void SolveStiffnessConstraint(CableParticle cableParticle, float distance)
    {


    }

    #endregion
}
