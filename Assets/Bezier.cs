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
}
