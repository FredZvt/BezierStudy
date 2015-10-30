using System;
using UnityEngine;

using Curve = Bezier.Curve;
using CurvePoint = Bezier.CurvePoint;

public class RandomBezierPathFollowTest : MonoBehaviour
{
    [Header("Control Points")]
    public Transform origin;
    public Transform destination;

    [Header("Box settings")]
    public Vector3 boxScale = Vector3.one;
    public float speed = 1f;
    public bool lerp = true;
    public float translationLerpSpeed = 1f;
    public float rotationLerpSpeed = 1f;

    [Header("PathSettings")]
    public int segments = 5;
    public float rndPathHorRange = 2f;
    public float rndPathVertRange = 4f;
    public float rndCtrlPointsHorRange = 1f;
    public float rndCtrlPointsVertRange = 1f;
    public bool parallelStartAndExit = true;

    [Header("Debug Settings")]
    public bool debugEnabled = true;
    public int resolutionPerSegment = 8;

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
    private GameObject obj;
    
    private float currPos = 0f;
    private Vector3 targetPosition;
    private Quaternion targetRotation;

    private void Start()
    {
        if (origin == null)
            throw new Exception("Origin not defined.");

        if (destination == null)
            throw new Exception("Destination not defined.");

        obj = GameObject.CreatePrimitive(PrimitiveType.Cube);

        NewRandomPath();

        obj.transform.position = origin.position;
        obj.transform.LookAt(destination);
    }

    private void Update()
    {
        var curveIdx = (int)currPos;
        var timeInCurve = currPos - curveIdx;

        var reset = false;
        if (curveIdx > curves.Length - 1)
        {
            currPos = 0;
            curveIdx = 0;
            NewRandomPath();
            reset = true;
        }

        var point = Bezier.GetPointInCurve(curves[curveIdx], timeInCurve);
        obj.transform.localScale = boxScale;

        if (lerp && !reset)
        {
            targetPosition = point.position;
            targetRotation = point.rotation;

            obj.transform.position = Vector3.Lerp(obj.transform.position, targetPosition, Time.time * translationLerpSpeed);
            obj.transform.rotation = Quaternion.Slerp(obj.transform.rotation, targetRotation, Time.time * rotationLerpSpeed);
        }
        else
        {
            obj.transform.position = point.position;
            obj.transform.rotation = point.rotation;
        }

        currPos += speed * Time.deltaTime;
    }

    private void NewRandomPath()
    {
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

    private void OnDrawGizmos()
    {
        if (Application.isPlaying && debugEnabled)
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

            if (lerp)
            {
                Gizmos.DrawLine(obj.transform.position, targetPosition);
            }
        }
    }
}

