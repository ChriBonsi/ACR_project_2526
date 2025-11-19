using UnityEngine;

public class RobotRange : MonoBehaviour
{
    private GameObject robot;

    [Header("Range Visualization")]
    public bool showRange = true;
    public float rangeRadius = 1f;
    [Range(8, 128)] public int rangeSegments = 64;
    public Color rangeColor = new(0f, 0.6f, 1f, 0.85f);

    private LineRenderer rangeRenderer;
    private RobotPathPlanner planner;

    void Start()
    {
        planner = gameObject.GetComponent<RobotPathPlanner>();
        robot = planner.robot;
        rangeRadius /= Mathf.Max(0.0001f, robot.transform.lossyScale.x);
        if (showRange) EnsureRangeRenderer();
    }

    void Update()
    {
        if (showRange)
            EnsureRangeRenderer();
        else if (rangeRenderer != null)
            rangeRenderer.enabled = false;
    }

    private void EnsureRangeRenderer()
    {
        if (robot == null) return;

        if (rangeRenderer == null)
        {
            var go = new GameObject($"Robot{planner.robotId}_Range");
            go.transform.SetParent(robot.transform, false);

            rangeRenderer = go.AddComponent<LineRenderer>();
            rangeRenderer.useWorldSpace = false;
            rangeRenderer.loop = true;
            rangeRenderer.widthMultiplier = 0.03f;
            rangeRenderer.material = new Material(Shader.Find("Sprites/Default"));
            rangeRenderer.numCornerVertices = 2;
            rangeRenderer.numCapVertices = 2;
        }

        if (rangeRenderer.transform.parent != robot.transform)
            rangeRenderer.transform.SetParent(robot.transform, false);

        rangeRenderer.enabled = true;
        rangeRenderer.startColor = rangeColor;
        rangeRenderer.endColor = rangeColor;

        rangeSegments = Mathf.Clamp(rangeSegments, 8, 256);
        rangeRenderer.positionCount = rangeSegments;

        float angleStep = 2f * Mathf.PI / rangeSegments;
        for (int i = 0; i < rangeSegments; i++)
        {
            float a = i * angleStep;
            float x = Mathf.Cos(a) * rangeRadius;
            float y = Mathf.Sin(a) * rangeRadius;
            rangeRenderer.SetPosition(i, new Vector3(x, y, 0f));
        }
    }
}
