using System;
using UnityEngine;

public static class Bezier
{
    // Based on the awesome talk: https://www.youtube.com/watch?v=o9RK6O2kOKo

    public struct CurvePoint
    {
        public Vector3 position;
        public Vector3 tangent;
        public Vector3 normal;
        public Vector3 binormal;
        public Quaternion rotation;
    }
    
    public struct Curve
    {
        public Curve(
            Vector3 start,
            Vector3 startControlPoint,
            Vector3 end,
            Vector3 endControlPoint
            )
        {
            this.start = start;
            this.startControlPoint = startControlPoint;
            this.end = end;
            this.endControlPoint = endControlPoint;
        }

        public Vector3 start;
        public Vector3 startControlPoint;
        public Vector3 end;
        public Vector3 endControlPoint;
    }
    
    public static CurvePoint GetPointInCurve(Curve curve, float time)
    {
        return GetPointInCurve(
            curve.start,
            curve.startControlPoint,
            curve.end,
            curve.endControlPoint,
            time
            );
    }

    public static CurvePoint GetPointInCurve(
        Vector3 start,
        Vector3 startControlPoint,
        Vector3 end,
        Vector3 endControlPoint,
        float time)
    {
        float omt = 1f - time;
        float omt2 = omt * omt;
        float t2 = time * time;

        var point = new CurvePoint();

        point.position =
            start * (omt2 * omt) +
            startControlPoint * (3f * omt2 * time) +
            endControlPoint * (3f * omt * t2) +
            end * (t2 * time);

        point.tangent =
            (start * (-omt2) +
            startControlPoint * (3f * omt2 - 2 * omt) +
            endControlPoint * (-3f * t2 + 2 * time) +
            end * (t2)).normalized;

        point.binormal = Vector3.Cross(Vector3.up, point.tangent).normalized;
        point.normal = Vector3.Cross(point.tangent, point.binormal).normalized;
        point.rotation = Quaternion.LookRotation(point.tangent, point.normal);

        return point;
    }

    public static CurvePoint[] GetPointsInCurve(Curve curve, byte resolution)
    {
        return GetPointsInCurve(
            curve.start,
            curve.startControlPoint,
            curve.end,
            curve.endControlPoint,
            resolution
            );
    }

    public static CurvePoint[] GetPointsInPath(Curve[] curves, byte resolutionPerSegment)
    {
        var pathPoints = new CurvePoint[(resolutionPerSegment * curves.Length) + 1];
        for (var i = 0; i < curves.Length; i++)
        {
            var points = GetPointsInCurve(curves[i], resolutionPerSegment);
            var pointsToGetInThisSegment = resolutionPerSegment;

            if (i == curves.Length - 1)
                pointsToGetInThisSegment++;

            var offset = resolutionPerSegment * i;
            for (var j = 0; j < pointsToGetInThisSegment; j++)
                pathPoints[offset + j] = points[j];
        }
        return pathPoints;
    }

    public static CurvePoint[] GetPointsInCurve(
        Vector3 start,
        Vector3 startControlPoint,
        Vector3 end,
        Vector3 endControlPoint,
        byte resolution)
    {
        if (resolution < 2)
            throw new ArgumentException("Resolution must be greater than two.");

        var path = new CurvePoint[resolution + 1];
        var increment = 1f / resolution;

        for (var i = 0; i < resolution + 1; i++)
            path[i] = GetPointInCurve(start, startControlPoint, end, endControlPoint, i * increment);

        return path;
    }

    public static Curve[] GenerateRandomPath(
        Vector3 origin, Vector3 destination, int segments,
        float rndPathHorRange, float rndPathVertRange,
        float rndCtrlPointsHorRange, float rndCtrlPointsVertRange,
        bool parallelStartAndExit
        )
    {
        var pathPoints = new Vector3[segments + 1];
        var controlPoints = new Vector3[segments][];

        // Divide the whole path into segments and apply noise.
        pathPoints = SplitPath(origin, destination, segments);
        ApplyRandomOffsetToPathNodes(pathPoints, rndPathHorRange, rndPathVertRange);

        // For each segment, divide into control points and apply noise.
        for (var i = 0; i < segments; i++)
        {
            controlPoints[i] = SplitPath(pathPoints[i], pathPoints[i + 1], 3);
            ApplyRandomOffsetToPathNodes(controlPoints[i], rndCtrlPointsHorRange, rndCtrlPointsVertRange);
        }

        // Make shared points simetrical.
        for (var i = 1; i < segments; i++)
        {
            var pointA = controlPoints[i - 1][2];
            var pointB = controlPoints[i - 1][3];
            var pointC = controlPoints[i][1];
            controlPoints[i][1] = AlignThirtPoint(pointA, pointB, pointC);
        }

        // Smooth entrance and exit.
        if (parallelStartAndExit)
        {
            controlPoints[0][1] = InverseAlignThirtPoint(origin, destination, controlPoints[0][1]);
            controlPoints[segments - 1][2] = InverseAlignThirtPoint(destination, origin, controlPoints[segments - 1][2]);
        }

        // Create curves array.
        var curves = new Curve[segments];
        for (var i = 0; i < segments; i++)
        {
            curves[i] = new Curve(
                pathPoints[i],
                controlPoints[i][1],
                pathPoints[i + 1],
                controlPoints[i][2]
            );
        }

        return curves;
    }

    private static Vector3[] SplitPath(Vector3 start, Vector3 end, int numOfParts)
    {
        var path = new Vector3[numOfParts + 1];
        var part = (end - start) / numOfParts;

        for (var i = 0; i < numOfParts + 1; i++)
            path[i] = start + (part * i);

        return path;
    }

    private static void ApplyRandomOffsetToPathNodes(Vector3[] path, float horRange, float vertRange)
    {
        var direction = Quaternion.LookRotation(path[path.Length - 1] - path[0], Vector3.up);

        for (var i = 1; i < path.Length - 1; i++)
            path[i] += direction * GetRandomOffset(horRange, vertRange);
    }

    private static Vector3 AlignThirtPoint(Vector3 pointA, Vector3 pointB, Vector3 pointC)
    {
        var lengthFromBToC = Vector3.Distance(pointB, pointC);
        var vectorBetweenAAndB = pointB - pointA;
        return pointB + (vectorBetweenAAndB.normalized * lengthFromBToC);
    }

    private static Vector3 InverseAlignThirtPoint(Vector3 pointA, Vector3 pointB, Vector3 pointC)
    {
        var lengthFromAToC = Vector3.Distance(pointA, pointC);
        var vectorBetweenAAndB = pointB - pointA;
        return pointA + (vectorBetweenAAndB.normalized * lengthFromAToC);
    }

    private static Vector3 GetRandomOffset(float horRange, float vertRange)
    {
        return new Vector3(
            UnityEngine.Random.Range(-horRange, horRange),
            UnityEngine.Random.Range(-vertRange, vertRange),
            0
        );
    }
    
    public static void DrawPath(
        Curve[] curves,
        CurvePoint[] path,
        bool drawCurve = true, Color? curveColor = null,
        bool drawHandles = true, float handlesRadius = .2f, Color? handlesColor = null, 
        bool drawPoints = true, float pointRadius = .2f, Color? pointsColor = null,
        bool drawTangents = true, float tangentsSize = .2f, Color? tangentsColor = null,
        bool drawNormals = true, float normalsSize = .2f, Color? normalsColor = null,
        bool drawBinormals = true, float binormalsSize = .2f, Color? binormalsColor = null
        )
    {
        var oldGizmosColor = Gizmos.color;

        handlesColor = handlesColor ?? Color.gray;
        curveColor = curveColor ?? Color.white;
        pointsColor = pointsColor ?? Color.gray;
        tangentsColor = tangentsColor ?? Color.blue;
        normalsColor = normalsColor ?? Color.green;
        binormalsColor = binormalsColor ?? Color.red;

        if (curves != null && drawHandles)
        {
            for (var i = 0; i < curves.Length; i++)
            {
                Gizmos.color = handlesColor.Value;
                Gizmos.DrawLine(curves[i].start, curves[i].startControlPoint);
                Gizmos.DrawLine(curves[i].end, curves[i].endControlPoint);
                Gizmos.DrawWireSphere(curves[i].startControlPoint, handlesRadius);
                Gizmos.DrawWireSphere(curves[i].endControlPoint, handlesRadius);
                Gizmos.DrawWireSphere(curves[i].start, handlesRadius);
                Gizmos.DrawWireSphere(curves[i].end, handlesRadius);
            }
        }

        if (path != null)
        {
            if (drawCurve || drawPoints || drawTangents || drawNormals || drawBinormals)
            {
                var lastPoint = path[0];

                for (var i = 0; i < path.Length; i++)
                {
                    Gizmos.color = curveColor.Value;
                    if (i > 0 && drawCurve)
                        Gizmos.DrawLine(lastPoint.position, path[i].position);

                    Gizmos.color = pointsColor.Value;
                    if (drawPoints)
                        Gizmos.DrawWireSphere(path[i].position, pointRadius);

                    Gizmos.color = tangentsColor.Value;
                    if (drawTangents)
                        Gizmos.DrawLine(path[i].position, path[i].position + (path[i].tangent * tangentsSize));

                    Gizmos.color = normalsColor.Value;
                    if (drawNormals)
                        Gizmos.DrawLine(path[i].position, path[i].position + (path[i].normal * normalsSize));

                    Gizmos.color = binormalsColor.Value;
                    if (drawBinormals)
                        Gizmos.DrawLine(path[i].position, path[i].position + (path[i].binormal * binormalsSize));

                    lastPoint = path[i];
                }
            }
        }

        Gizmos.color = oldGizmosColor;
    }
}
