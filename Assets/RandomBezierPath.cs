using UnityEngine;

public class RandomBezierPath
{
    private Vector3 origin;
    private Vector3 destination;

    public int parts = 5;
    public int resolution = 8;
    private float rndPathHorRange = 2f;
    private float rndPathVertRange = 4f;
    private float rndCtrlPointsHorRange = 1f;
    private float rndCtrlPointsVertRange = 1f;

    public Bezier.PathPoint[] pathPoints;
    public Vector3[] pathKeyPoints;
    public Vector3[][] controlPoints;

    public RandomBezierPath(
    Vector3 origin, Vector3 destination,
    int parts, int resolution,
    float rndPathHorRange, float rndPathVertRange,
    float rndCtrlPointsHorRange, float rndCtrlPointsVertRange)
    {
        this.origin = origin;
        this.destination = destination;
        this.parts = parts;
        this.resolution = resolution;
        this.rndPathHorRange = rndPathHorRange;
        this.rndPathVertRange = rndPathVertRange;
        this.rndCtrlPointsHorRange = rndCtrlPointsHorRange;
        this.rndCtrlPointsVertRange = rndCtrlPointsVertRange;

        pathPoints = new Bezier.PathPoint[(resolution * parts) + 1];
        pathKeyPoints = new Vector3[parts + 1];
        controlPoints = new Vector3[parts][];

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

        // Make first and last control points perpendicular to the whole path.
        controlPoints[0][1] = InverseAlignThirtPoint(origin, destination, controlPoints[0][1]);
        controlPoints[parts - 1][2] = InverseAlignThirtPoint(destination, origin, controlPoints[parts - 1][2]);

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
    }

    private Vector3[] SplitPath(Vector3 start, Vector3 end, int numOfParts)
    {
        var path = new Vector3[numOfParts + 1];
        var part = (end - start) / numOfParts;

        for (var i = 0; i < numOfParts + 1; i++)
            path[i] = start + (part * i);

        return path;
    }

    private void ApplyRandomOffsetToPathNodes(Vector3[] path, float horRange, float vertRange)
    {
        var direction = Quaternion.LookRotation(path[path.Length - 1] - path[0], Vector3.up);

        for (var i = 1; i < path.Length - 1; i++)
            path[i] += direction * GetRandomOffset(horRange, vertRange);
    }

    private Vector3 AlignThirtPoint(Vector3 pointA, Vector3 pointB, Vector3 pointC)
    {
        var lengthFromBToC = Vector3.Distance(pointB, pointC);
        var vectorBetweenAAndB = pointB - pointA;
        return pointB + (vectorBetweenAAndB.normalized * lengthFromBToC);
    }

    private Vector3 InverseAlignThirtPoint(Vector3 pointA, Vector3 pointB, Vector3 pointC)
    {
        var lengthFromAToC = Vector3.Distance(pointA, pointC);
        var vectorBetweenAAndB = pointB - pointA;
        return pointA + (vectorBetweenAAndB.normalized * lengthFromAToC);
    }

    private Vector3 GetRandomOffset(float horRange, float vertRange)
    {
        return new Vector3(
            UnityEngine.Random.Range(-horRange, horRange),
            UnityEngine.Random.Range(-vertRange, vertRange),
            0
        );
    }
}
