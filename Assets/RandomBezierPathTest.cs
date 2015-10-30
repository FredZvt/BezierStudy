using System;
using UnityEngine;

[ExecuteInEditMode]
public class RandomBezierPathTest : MonoBehaviour
{
    [Header("Control Points")]
    public Transform origin;
    public Transform destination;

    [Header("Settings")]
    public int parts = 5;
    public int resolution = 8;
    public float rndPathHorRange = 2f;
    public float rndPathVertRange = 4f;
    public float rndCtrlPointsHorRange = 1f;
    public float rndCtrlPointsVertRange = 1f;
    public bool parallelStartAndExit = true;

    [Header("Debug Settings")]
    public bool drawCurve = true;
    public Color curveColor = Color.white;
    public bool drawPoints = true;
    [Range(0.01f, 5)]
    public float drawPointRadius = .2f;
    public Color pointsColor = Color.gray;
    public bool drawTangents = true;
    [Range(0.01f, 5)]
    public float drawTangentsSize = .2f;
    public Color tangentsColor = Color.blue;
    public bool drawNormals = true;
    [Range(0.01f, 5)]
    public float drawNormalsSize = .2f;
    public Color normalsColor = Color.green;
    public bool drawBinormals = true;
    [Range(0.01f, 5)]
    public float drawBinormalsSize = .2f;
    public Color binormalsColor = Color.red;

    private Bezier.PathPoint[] path;
    private Vector3 lastOrigin = Vector3.zero;
    private Vector3 lastDestination = Vector3.zero;
    private int lastParts = 0;
    private int lastResolution = 0;
    private float lastRndPathHorRange = 0;
    private float lastRndPathVertRange = 0;
    private float lastRndCtrlPointsHorRange = 0;
    private float lastRndCtrlPointsVertRange = 0;
    private bool lastParallelStartAndExit = false;

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
                lastParts != parts ||
                lastResolution != resolution ||
                lastRndPathHorRange != rndPathHorRange ||
                lastRndPathVertRange != rndPathVertRange ||
                lastRndCtrlPointsHorRange != rndCtrlPointsHorRange ||
                lastRndCtrlPointsVertRange != rndCtrlPointsVertRange ||
                lastParallelStartAndExit != parallelStartAndExit)
            {
                lastOrigin = origin.position;
                lastDestination = destination.position;
                lastParts = parts;
                lastResolution = resolution;
                lastRndPathHorRange = rndPathHorRange;
                lastRndPathVertRange = rndPathVertRange;
                lastRndCtrlPointsHorRange = rndCtrlPointsHorRange;
                lastRndCtrlPointsVertRange = rndCtrlPointsVertRange;
                lastParallelStartAndExit = parallelStartAndExit;

                path = Bezier.GetRandomPath(
                        origin.position, destination.position,
                        parts, resolution,
                        rndPathHorRange, rndPathVertRange,
                        rndCtrlPointsHorRange, rndCtrlPointsVertRange,
                        parallelStartAndExit
                       );
            }
        }
        else
        {
            path = null;
        }
    }

    private void OnDrawGizmos()
    {
        if (origin != null && destination != null && path != null)
        {
            if (drawCurve || drawPoints || drawTangents || drawNormals || drawBinormals)
            {
                var lastPoint = path[0];

                for (var i = 0; i < path.Length; i++)
                {
                    Gizmos.color = curveColor;
                    if (i > 0 && drawCurve)
                        Gizmos.DrawLine(lastPoint.point, path[i].point);

                    Gizmos.color = pointsColor;
                    if (drawPoints)
                        Gizmos.DrawWireSphere(path[i].point, drawPointRadius);

                    Gizmos.color = tangentsColor;
                    if (drawTangents)
                        Gizmos.DrawLine(path[i].point, path[i].point + (path[i].tangent * drawTangentsSize));

                    Gizmos.color = normalsColor;
                    if (drawNormals)
                        Gizmos.DrawLine(path[i].point, path[i].point + (path[i].normal * drawNormalsSize));

                    Gizmos.color = binormalsColor;
                    if (drawBinormals)
                        Gizmos.DrawLine(path[i].point, path[i].point + (path[i].binormal * drawBinormalsSize));

                    lastPoint = path[i];
                }
            }
        }
    }
}

