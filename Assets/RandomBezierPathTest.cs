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

    [Header("Debug Settings")]
    public bool drawHandles = true;
    public Color handleColor = Color.gray;
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

    private RandomBezierPath path;
    private int lastParts = 0;
    private int lastResolution = 0;
    private float lastRndPathHorRange = 0;
    private float lastRndPathVertRange = 0;
    private float lastRndCtrlPointsHorRange = 0;
    private float lastRndCtrlPointsVertRange = 0;

    private void Start()
    {
        if (origin == null)
            throw new Exception("Origin not defined.");

        if (destination == null)
            throw new Exception("Destination not defined.");
    }

    private void Update()
    {
        if (lastParts != parts ||
            lastResolution != resolution ||
            lastRndPathHorRange != rndPathHorRange ||
            lastRndPathVertRange != rndPathVertRange ||
            lastRndCtrlPointsHorRange != rndCtrlPointsHorRange ||
            lastRndCtrlPointsVertRange != rndCtrlPointsVertRange)
        {
            path = new RandomBezierPath(
                    origin.position, destination.position,
                    parts, resolution,
                    rndPathHorRange, rndPathVertRange,
                    rndCtrlPointsHorRange, rndCtrlPointsVertRange
                   );
        }
    }

    private void OnDrawGizmos()
    {
        if (path != null && path.pathKeyPoints != null)
        {
            if (drawHandles)
            {
                for (var i = 0; i < parts; i++)
                {
                    Gizmos.color = handleColor;

                    Gizmos.DrawWireSphere(path.pathKeyPoints[i], 0.5f);
                    Gizmos.DrawWireSphere(path.pathKeyPoints[i + 1], 0.5f);

                    Gizmos.DrawLine(path.pathKeyPoints[i], path.controlPoints[i][1]);
                    Gizmos.DrawLine(path.pathKeyPoints[i + 1], path.controlPoints[i][2]);
                    Gizmos.DrawWireSphere(path.controlPoints[i][1], 0.1f);
                    Gizmos.DrawWireSphere(path.controlPoints[i][2], 0.1f);
                }
            }

            if (drawCurve || drawPoints || drawTangents || drawNormals || drawBinormals)
            {
                var lastPoint = path.pathPoints[0];

                for (var i = 0; i < path.pathPoints.Length; i++)
                {
                    Gizmos.color = curveColor;
                    if (i > 0 && drawCurve)
                        Gizmos.DrawLine(lastPoint.point, path.pathPoints[i].point);

                    Gizmos.color = pointsColor;
                    if (drawPoints)
                        Gizmos.DrawWireSphere(path.pathPoints[i].point, drawPointRadius);

                    Gizmos.color = tangentsColor;
                    if (drawTangents)
                        Gizmos.DrawLine(path.pathPoints[i].point, path.pathPoints[i].point + (path.pathPoints[i].tangent * drawTangentsSize));

                    Gizmos.color = normalsColor;
                    if (drawNormals)
                        Gizmos.DrawLine(path.pathPoints[i].point, path.pathPoints[i].point + (path.pathPoints[i].normal * drawNormalsSize));

                    Gizmos.color = binormalsColor;
                    if (drawBinormals)
                        Gizmos.DrawLine(path.pathPoints[i].point, path.pathPoints[i].point + (path.pathPoints[i].binormal * drawBinormalsSize));

                    lastPoint = path.pathPoints[i];
                }
            }
        }
    }
}

