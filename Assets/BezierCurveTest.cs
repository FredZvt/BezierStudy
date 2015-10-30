using UnityEngine;

[ExecuteInEditMode]
public class BezierCurveTest : MonoBehaviour
{
    [Header("Control Points")]
    public Transform start;
    public Transform startControlPoint;
    public Transform end;
    public Transform endControlPoint;

    [Header("Settings")]

    [Range(2, 20)]
    public int resolution = 2;

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

    public Vector3? lastStartPosition;
    public Vector3? lastStartControlPointPosition;
    public Vector3? lastEndPosition;
    public Vector3? lastEndControlPointPosition;

    private Bezier.CurvePoint[] path;

    private void Update()
    {
        if (AllTransformsWereBinded() && (LastPositionsWereNotInitialized() || OneOfTheTransformsHasChangedPositionSinceLastLoop()))
        {
            path = Bezier.GetPointsInCurve(start.position, startControlPoint.position, end.position, endControlPoint.position, (byte)resolution);
        }
    }

    private void OnDrawGizmos()
    {
        if (AllTransformsWereBinded() && path != null && path.Length > 0)
        {
            if (drawHandles)
            {
                Gizmos.color = handleColor;
                Gizmos.DrawLine(start.position, startControlPoint.position);
                Gizmos.DrawLine(end.position, endControlPoint.position);
            }

            if (drawCurve || drawPoints || drawTangents || drawNormals || drawBinormals)
            {
                var lastPoint = path[0];

                for (var i = 0; i < path.Length; i++)
                {
                    Gizmos.color = curveColor;
                    if (i > 0 && drawCurve)
                        Gizmos.DrawLine(lastPoint.position, path[i].position);

                    Gizmos.color = pointsColor;
                    if (drawPoints)
                        Gizmos.DrawWireSphere(path[i].position, drawPointRadius);

                    Gizmos.color = tangentsColor;
                    if (drawTangents)
                        Gizmos.DrawLine(path[i].position, path[i].position + (path[i].tangent * drawTangentsSize));

                    Gizmos.color = normalsColor;
                    if (drawNormals)
                        Gizmos.DrawLine(path[i].position, path[i].position + (path[i].normal * drawNormalsSize));

                    Gizmos.color = binormalsColor;
                    if (drawBinormals)
                        Gizmos.DrawLine(path[i].position, path[i].position + (path[i].binormal * drawBinormalsSize));

                    lastPoint = path[i];
                }
            }
        }
    }

    private bool OneOfTheTransformsHasChangedPositionSinceLastLoop()
    {
        return lastStartPosition != start.position ||
               lastStartControlPointPosition != startControlPoint.position ||
               lastEndPosition != end.position ||
               lastEndControlPointPosition != endControlPoint.position;
    }

    private bool LastPositionsWereNotInitialized()
    {
        return lastStartPosition == null || 
               lastStartControlPointPosition == null || 
               lastEndPosition == null || 
               lastEndControlPointPosition == null;
    }

    private bool AllTransformsWereBinded()
    {
        return start != null && 
               startControlPoint != null && 
               end != null && 
               endControlPoint != null;
    }
}

