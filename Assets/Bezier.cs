using System;
using UnityEngine;

public static class Bezier
{
    // Based on the awesome talk: https://www.youtube.com/watch?v=o9RK6O2kOKo

    public struct PathPoint
    {
        public Vector3 point;
        public Vector3 tangent;
        public Vector3 normal;
        public Vector3 binormal;
        public Quaternion orientation;
    }

    public static PathPoint[] GetBezierCurve(
        Vector3 start,
        Vector3 startControlPoint,
        Vector3 end,
        Vector3 endControlPoint,
        byte resolution)
    {
        if (resolution < 2)
            throw new ArgumentException("Resolution must be greater than two.");

        var path = new PathPoint[resolution + 1];
        var increment = 1f / resolution;
        for (var i = 0; i < resolution + 1; i++)
            path[i] = GetPoint(start, startControlPoint, end, endControlPoint, i * increment);

        return path;
    }

    public static PathPoint GetPoint(
        Vector3 start,
        Vector3 startControlPoint,
        Vector3 end,
        Vector3 endControlPoint,
        float step)
    {
        float omt = 1f - step;
        float omt2 = omt * omt;
        float t2 = step * step;

        var pathPoint = new PathPoint();

        pathPoint.point =
            start * (omt2 * omt) +
            startControlPoint * (3f * omt2 * step) +
            endControlPoint * (3f * omt * t2) +
            end * (t2 * step);

        pathPoint.tangent =
            (start * (-omt2) +
            startControlPoint * (3f * omt2 - 2 * omt) +
            endControlPoint * (-3f * t2 + 2 * step) +
            end * (t2)).normalized;

        pathPoint.binormal = Vector3.Cross(Vector3.up, pathPoint.tangent).normalized;
        pathPoint.normal = Vector3.Cross(pathPoint.tangent, pathPoint.binormal).normalized;
        pathPoint.orientation = Quaternion.LookRotation(pathPoint.tangent, pathPoint.normal);

        return pathPoint;
    }

    public static PathPoint[] GetRandomPath(
        Vector3 origin, Vector3 destination,
        int parts, int resolution,
        float rndPathHorRange, float rndPathVertRange,
        float rndCtrlPointsHorRange, float rndCtrlPointsVertRange,
        bool parallelStartAndExit
        )
    {
        var pathPoints = new Bezier.PathPoint[(resolution * parts) + 1];
        var pathKeyPoints = new Vector3[parts + 1];
        var controlPoints = new Vector3[parts][];

        // Divide the whole path into subparts and apply noise.
        pathKeyPoints = SplitPath(origin, destination, parts);
        ApplyRandomOffsetToPathNodes(pathKeyPoints, rndPathHorRange, rndPathVertRange);

        // For each part, divide into control points and apply noise.
        for (var i = 0; i < parts; i++)
        {
            controlPoints[i] = SplitPath(pathKeyPoints[i], pathKeyPoints[i + 1], 3);
            ApplyRandomOffsetToPathNodes(controlPoints[i], rndCtrlPointsHorRange, rndCtrlPointsVertRange);
        }

        // Make shared nodes simetrical.
        for (var i = 1; i < parts; i++)
        {
            var pointA = controlPoints[i - 1][2];
            var pointB = controlPoints[i - 1][3];
            var pointC = controlPoints[i][1];
            controlPoints[i][1] = AlignThirtPoint(pointA, pointB, pointC);
        }

        if (parallelStartAndExit)
        {
            controlPoints[0][1] = InverseAlignThirtPoint(origin, destination, controlPoints[0][1]);
            controlPoints[parts - 1][2] = InverseAlignThirtPoint(destination, origin, controlPoints[parts - 1][2]);
        }

        // For each part, calculate bezier curve to compound the final path.
        for (var i = 0; i < parts; i++)
        {
            var partCurvePoints =
                Bezier.GetBezierCurve(
                    controlPoints[i][0],
                    controlPoints[i][1],
                    controlPoints[i][3],
                    controlPoints[i][2],
                    (byte)resolution
                );

            var lastNodeIndexToAdd = (i == parts - 1) ? resolution + 1 : resolution;
            for (var j = 0; j < lastNodeIndexToAdd; j++)
            {
                var offset = resolution * i;
                var finalIdx = offset + j;
                pathPoints[finalIdx] = partCurvePoints[j];
            }
        }

        return pathPoints;
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
}
