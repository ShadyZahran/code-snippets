using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[ExecuteInEditMode]
[RequireComponent(typeof(LineRenderer))]
public class Orbit : MonoBehaviour
{
    public LineRenderer myLineRenderer;
    [Range(10, 40)]
    [Tooltip("number of segments/points to draw the line renderer.")]
    public int DE_segments = 40;
    [Tooltip("how wide and tall the orbit is")]
    public float DE_yAxis = 15f, DE_xAxis = 25f;
    //[Range(0f,360f)]
    [Tooltip("how the entire orbit is rotated around all axis")]
    public Vector3 DE_allAxisAngle = new Vector3(0f, 0f, 0f);
    Vector3 orbitCenter;
    Vector3[] pointsLocalSpace;
    private void Awake()
    {
        myLineRenderer = GetComponent<LineRenderer>();
        orbitCenter = transform.position;

        CalculateOrbit();
    }

    /// <summary>
    /// Calculates the orbit according to the variables assigned in the inspector
    /// </summary>
    public void CalculateOrbit()
    {
        orbitCenter = transform.position;
        Vector3[] pointsWorldSpace = new Vector3[DE_segments + 1];
        pointsLocalSpace = new Vector3[DE_segments + 1];
        
        
        Quaternion q = Quaternion.Euler(DE_allAxisAngle.x, DE_allAxisAngle.y, DE_allAxisAngle.z);
        for (int i = 0; i < DE_segments; i++)
        {
            float angle = ((float)i / (float)DE_segments) * 360 * Mathf.Deg2Rad;
            float x = Mathf.Sin(angle) * DE_xAxis;
            float y = Mathf.Cos(angle) * DE_yAxis;
            pointsWorldSpace[i] = new Vector3(x, y, 0.0f);
            pointsWorldSpace[i] = q * pointsWorldSpace[i];

            pointsLocalSpace[i] = new Vector3(x, y, 0.0f);
            pointsLocalSpace[i] = q * pointsLocalSpace[i] + orbitCenter;
        }
        pointsWorldSpace[DE_segments] = pointsWorldSpace[0];
        pointsLocalSpace[DE_segments] = pointsLocalSpace[0];
        ApplyOrbit(pointsWorldSpace);
        
    }

    /// <summary>
    /// Assignment the points to the line renderer to be displayed
    /// </summary>
    /// <param name="fPoints"></param>
    private void ApplyOrbit(Vector3[] fPoints)
    {
        myLineRenderer.positionCount = DE_segments + 1;
        myLineRenderer.SetPositions(fPoints);
    }

    public Vector3[] GetPathWorldSpace()
    {
        Vector3[] path = new Vector3[DE_segments + 1];
        myLineRenderer.GetPositions(path);
        return path;
    }

    public Vector3[] GetPathLocalSpace()
    {
        return pointsLocalSpace;
    }

}
