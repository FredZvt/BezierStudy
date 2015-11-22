using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Curve = Bezier.Curve;
using CurvePoint = Bezier.CurvePoint;

public class ContinuousBezierPathTest : MonoBehaviour
{
    [Header("Path Settings")]
    public float segmentLength = 1f;
    public Vector3 segmentDirectionRandomnessRange = new Vector3(2f, 2f, 1f);
    public Vector3 controlPointsDirectionRandomnessRange = new Vector3(2f, 2f, 1f);
    
    [Header("Debug Settings")]
    public byte previewResolution = 8;
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
    
    private Vector3 currentEnd;
    private Vector3? lastEndControlPoint;
    private List<Curve> curves;
    private Curve[] curvesArr;
    private CurvePoint[] path;

    private void Awake()
    {
        curves = new List<Bezier.Curve>();
        currentEnd = transform.position;
    }

    private void Start()
    {
        StartCoroutine(GenerateNewSegment());
    }

    private void Update()
    {

    }

    private IEnumerator GenerateNewSegment()
    {
        while (true)
        {
            // Define new start and end points, linearliy.
            var start = currentEnd;
            var end = currentEnd + (Vector3.forward * segmentLength);

            // Randomly translate the new end point.
            end = ApplyRandom(end, segmentDirectionRandomnessRange);

            // Define new control points, linearly.
            var fullSegmentVec = end - start;
            var thirdOfSegVect = fullSegmentVec / 3;
            var startControlPoint = start + thirdOfSegVect;
            var endControlPoint = start + (thirdOfSegVect * 2);

            // Randomly translate control points.
            startControlPoint = ApplyRandom(startControlPoint, controlPointsDirectionRandomnessRange);
            endControlPoint = ApplyRandom(endControlPoint, controlPointsDirectionRandomnessRange);

            // Smooth curve by aligning the new start control point to the last end control point.
            if (lastEndControlPoint.HasValue)
                startControlPoint = AlignThirtPoint(lastEndControlPoint.Value, start, startControlPoint);

            // Create the new curve.
            var newCurve = new Curve(start, startControlPoint, end, endControlPoint);
            curves.Add(newCurve);
            
            // Update the storages to use in the gizmos preview.
            curvesArr = curves.ToArray();
            path = Bezier.GetPointsInPath(curves.ToArray(), previewResolution);

            // Set the ground for the next path.
            currentEnd = end;
            lastEndControlPoint = endControlPoint;
            yield return new WaitForSeconds(0.3f);
        }
    }
    
    private Vector3 AlignThirtPoint(Vector3 pointA, Vector3 pointB, Vector3 pointC)
    {
        var lengthFromBToC = Vector3.Distance(pointB, pointC);
        var vectorBetweenAAndB = pointB - pointA;
        return pointB + (vectorBetweenAAndB.normalized * lengthFromBToC);
    }

    private Vector3 ApplyRandom(Vector3 original, Vector3 range)
    {
        return original + new Vector3(
            UnityEngine.Random.Range(-range.x, range.x),
            UnityEngine.Random.Range(-range.y, range.y),
            UnityEngine.Random.Range(-range.z, range.z)
        );
    }

    private void OnDrawGizmos()
    {
        if (Application.isPlaying)
        {
            Bezier.DrawPath(
               curvesArr, path,
               drawCurve, curveColor,
               drawHandles, handlesRadius, handlesColor,
               drawPoints, pointRadius, pointsColor,
               drawTangents, tangentsSize, tangentsColor,
               drawNormals, normalsSize, normalsColor,
               drawBinormals, binormalsSize, binormalsColor
               );
        }
    }
}
