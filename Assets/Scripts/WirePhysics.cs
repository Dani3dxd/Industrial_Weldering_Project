using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class WirePhysics : MonoBehaviour
{
    [Header("Structure")]
    public Transform startPoint;            // optional anchor
    public Transform endPoint;              // optional anchor
    public int segments = 20;
    public float totalLength = 5f;

    [Header("Simulation")]
    public int solverIterations = 8;        // constraint iterations per update
    public float gravityScale = 1f;
    public float damping = 0.01f;           // global velocity damping
    public float thickness = 0.05f;         // collision thickness of wire segments
    public float collisionPush = 1.0f;      // factor when pushing points out of colliders

    [Header("Rendering")]
    public LineRenderer lineRenderer;
    public bool render = true;

    // internals
    private Vector3[] points;
    private Vector3[] prevPoints;
    private float segmentLength;
    private Vector3 gravity => Physics.gravity * gravityScale;
    private bool initialized = false;

    void Awake()
    {
        if (lineRenderer == null) lineRenderer = GetComponent<LineRenderer>();
    }

    void Start()
    {
        InitializePoints();
    }

    void OnValidate()
    {
        // keep valid values in editor
        segments = Mathf.Max(2, segments);
        totalLength = Mathf.Max(0.01f, totalLength);
        solverIterations = Mathf.Clamp(solverIterations, 1, 50);
    }

    void InitializePoints()
    {
        segmentLength = totalLength / (segments - 1);
        points = new Vector3[segments];
        prevPoints = new Vector3[segments];

        Vector3 a = startPoint ? startPoint.position : transform.position;
        Vector3 b = endPoint ? endPoint.position : transform.position + transform.forward * totalLength;

        for (int i = 0; i < segments; i++)
        {
            float t = (float)i / (segments - 1);
            Vector3 pos = Vector3.Lerp(a, b, t);
            points[i] = pos;
            prevPoints[i] = pos;
        }

        if (lineRenderer)
        {
            lineRenderer.positionCount = segments;
        }

        initialized = true;
    }

    void Update()
    {
        if (!initialized) InitializePoints();

        float dt = Time.deltaTime;
        if (dt <= 0f) return;

        SimulateVerlet(dt);
        for (int i = 0; i < solverIterations; i++) ConstrainSegments();
        if (render && lineRenderer) RenderLine();
    }

    void SimulateVerlet(float dt)
    {
        // Apply Verlet integration to each free point
        for (int i = 0; i < segments; i++)
        {
            // anchored endpoints
            if ((i == 0 && startPoint != null) || (i == segments - 1 && endPoint != null))
            {
                // anchored: set to transform position (supports moving anchors)
                if (i == 0 && startPoint != null) points[0] = startPoint.position;
                if (i == segments - 1 && endPoint != null) points[segments - 1] = endPoint.position;
                prevPoints[i] = points[i]; // reset previous to avoid tiny pulls
                continue;
            }

            Vector3 current = points[i];
            Vector3 prev = prevPoints[i];
            Vector3 velocity = (current - prev) * (1f - damping);
            Vector3 next = current + velocity + gravity * dt * dt;

            prevPoints[i] = current;
            points[i] = next;

            // simple collision resolution per point
            ResolveCollisions(i);
        }
    }

    void ResolveCollisions(int index)
    {
        // find colliders near the point
        Vector3 p = points[index];
        Collider[] hits = Physics.OverlapSphere(p, thickness * 1.1f);

        foreach (Collider c in hits)
        {
            if (c.isTrigger) continue;
            Vector3 closest = c.ClosestPoint(p);
            Vector3 delta = p - closest;
            float dist = delta.magnitude;

            // if inside or too close to collider, push out
            if (dist < Mathf.Epsilon)
            {
                // point exactly inside surface -> push along collider normal approximation
                // approximate by using point vs collider bounds center
                Vector3 dir = (p - c.bounds.center).normalized;
                if (dir == Vector3.zero) dir = Vector3.up;
                points[index] = closest + dir * (thickness * collisionPush);
            }
            else if (dist < thickness)
            {
                Vector3 push = delta.normalized * (thickness - dist) * collisionPush;
                points[index] += push;
            }
        }
    }

    void ConstrainSegments()
    {
        // Enforce distance constraints
        for (int i = 0; i < segments - 1; i++)
        {
            Vector3 pA = points[i];
            Vector3 pB = points[i + 1];

            float curDist = (pA - pB).magnitude;
            float error = curDist - segmentLength;
            if (Mathf.Abs(error) < 1e-6f) continue;

            Vector3 dir = (pA - pB).normalized;

            bool anchorA = (i == 0 && startPoint != null);
            bool anchorB = (i + 1 == segments - 1 && endPoint != null);

            if (anchorA && anchorB)
            {
                // both anchored -> nothing to correct
                continue;
            }
            else if (anchorA)
            {
                // move B only
                points[i + 1] += dir * error;
            }
            else if (anchorB)
            {
                // move A only
                points[i] -= dir * error;
            }
            else
            {
                // split correction
                points[i] -= dir * (error * 0.5f);
                points[i + 1] += dir * (error * 0.5f);
            }
        }

        // re-anchor to moving transforms to keep endpoints exact after constraints
        if (startPoint != null) points[0] = startPoint.position;
        if (endPoint != null) points[segments - 1] = endPoint.position;
    }

    void RenderLine()
    {
        for (int i = 0; i < segments; i++)
            lineRenderer.SetPosition(i, points[i]);
    }

    // optional: draw thickness in editor
    void OnDrawGizmosSelected()
    {
        if (!initialized) return;
        Gizmos.color = Color.yellow;
        for (int i = 0; i < segments; i++)
            Gizmos.DrawWireSphere(points[i], thickness * 0.5f);
    }

    // Public controls
    public void Rebuild()
    {
        InitializePoints();
    }
}
