using System;
using UnityEngine;

using Curve = Bezier.Curve;
using CurvePoint = Bezier.CurvePoint;

[ExecuteInEditMode]
public class RandomBezierPathTest : MonoBehaviour
{
    [Header("Control Points")]
    public Transform origin;
    public Transform destination;

    [Header("Settings")]
    public int segments = 5;
    public int resolutionPerSegment = 8;
    public float rndPathHorRange = 2f;
    public float rndPathVertRange = 4f;
    public float rndCtrlPointsHorRange = 1f;
    public float rndCtrlPointsVertRange = 1f;
    public bool parallelStartAndExit = true;

    [Header("Debug Settings")]
    public bool toggleToChange = true;

    public bool drawHandles = true;
    public Color handlesColor = Color.gray;
    public float handlesRadius = .2f;

    public bool drawCurve = true;
    public Color curveColor = Color.white;

    public bool drawPoints = true;
    [Range(0.01f, 5)]
    public float pointRadius = .2f;
    public Color pointsColor = Color.gray;

    public bool drawTangents = true;
    [Range(0.01f, 5)]
    public float tangentsSize = .2f;
    public Color tangentsColor = Color.blue;

    public bool drawNormals = true;
    [Range(0.01f, 5)]
    public float normalsSize = .2f;
    public Color normalsColor = Color.green;

    public bool drawBinormals = true;
    [Range(0.01f, 5)]
    public float binormalsSize = .2f;
    public Color binormalsColor = Color.red;

    private Curve[] curves;
    private CurvePoint[] path;

    private Vector3 lastOrigin = Vector3.zero;
    private Vector3 lastDestination = Vector3.zero;
    private int lastSegments = 0;
    private int lastResolutionPerSegment = 0;
    private float lastRndPathHorRange = 0;
    private float lastRndPathVertRange = 0;
    private float lastRndCtrlPointsHorRange = 0;
    private float lastRndCtrlPointsVertRange = 0;
    private bool lastParallelStartAndExit = false;
    private bool lastToggleToChange = false;

    private void Start()
    {
        if (origin == null)
            throw new Exception("Origin not defined.");

        if (destination == null)
            throw new Exception("Destination not defined.");
    }

    private void Update()
    {
        if (origin != null && destination != null)
        {
            if (lastOrigin != origin.position ||
                lastDestination != destination.position ||
                lastSegments != segments ||
                lastResolutionPerSegment != resolutionPerSegment ||
                lastRndPathHorRange != rndPathHorRange ||
                lastRndPathVertRange != rndPathVertRange ||
                lastRndCtrlPointsHorRange != rndCtrlPointsHorRange ||
                lastRndCtrlPointsVertRange != rndCtrlPointsVertRange ||
                lastParallelStartAndExit != parallelStartAndExit ||
                lastToggleToChange != toggleToChange)
            {
                lastOrigin = origin.position;
                lastDestination = destination.position;
                lastSegments = segments;
                lastResolutionPerSegment = resolutionPerSegment;
                lastRndPathHorRange = rndPathHorRange;
                lastRndPathVertRange = rndPathVertRange;
                lastRndCtrlPointsHorRange = rndCtrlPointsHorRange;
                lastRndCtrlPointsVertRange = rndCtrlPointsVertRange;
                lastParallelStartAndExit = parallelStartAndExit;
                lastToggleToChange = toggleToChange;
                
                curves = 
                    Bezier.GenerateRandomPath(
                        origin.position, destination.position,
                        segments, 
                        rndPathHorRange, rndPathVertRange,
                        rndCtrlPointsHorRange, rndCtrlPointsVertRange,
                        parallelStartAndExit
                    );

                path = Bezier.GetPointsInPath(curves, (byte)resolutionPerSegment);
            }
        }
        else
        {
            path = null;
        }
    }

    private void OnDrawGizmos()
    {
        Bezier.DrawPath(
            curves, path,
            drawCurve, curveColor,
            drawHandles, handlesRadius, handlesColor,
            drawPoints, pointRadius, pointsColor,
            drawTangents, tangentsSize, tangentsColor,
            drawNormals, normalsSize, normalsColor,
            drawBinormals, binormalsSize, binormalsColor
            );
    }
}

