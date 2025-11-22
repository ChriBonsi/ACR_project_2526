using UnityEngine;
using Unity.Robotics.ROSTCPConnector;
using RosMessageTypes.PathPlanner;
using System.Collections.Generic;
using RosMessageTypes.ObstacleManager;
using RosMessageTypes.RobotManager;

public class Robot : MonoBehaviour
{
    [Header("Robot settings")]
    public int robotId = 1;
    public float moveSpeed = 3f;
    public float perceptionRadius = 1f;
    public float obstacleDistanceThreshold = 1f;
    public string robotType = "default";
    public List<Vector3> destinations = new();
    public bool loop = false;

    [Header("Request")]
    public float endX = 3;
    public float endY = 3;

    protected ROSConnection ros;
    protected Queue<Vector3> pathQueue = new();
    protected bool obstacleDetected = false;
    protected int destinationIndex = 0;
    protected bool isPathRequestPending = false;
    protected bool isPerformingTask = false;
    private float trackerTimer = 0f;
    private const float trackerInterval = 1f;
    private float startX;
    private float startY;
    private List<GameObject> reportedObstacles = new();
    protected GameObject lastAvoided;

    void Start()
    {
        ros = ROSConnection.GetOrCreateInstance();

        ros.Subscribe<PathPlannerResponseMsg>("path_planner/response", ResultCallback);

        if (destinations.Count == 0) SendRequest();

        perceptionRadius *= gameObject.transform.lossyScale.x;
    }

    void Update()
    {
        if (gameObject == null) return;

        SendTrackingData();

        Vector3 target = Vector3.zero;
        bool hasPath = pathQueue.Count > 0;
        if (hasPath) target = pathQueue.Peek();

        if (hasPath) 
        {
            CheckForObstacles(target);
        }

        UpdateTask();

        if (isPerformingTask) return;

        if (!hasPath)
        {
            CheckAndAskForNewPath();
            return;
        }

        if (obstacleDetected) return;
        
        Move(target);
        CheckIfQueuedPointReached(target);
    }

    private void SendTrackingData()
    {
        trackerTimer += Time.deltaTime;
        if (trackerTimer >= trackerInterval)
        {
            var trackerMsg = new RobotManagerTrackerSubscriberMsg()
            {
                robot_id = robotId,
                current_x = gameObject.transform.position.x,
                current_y = gameObject.transform.position.y,
                start_x = startX,
                start_y = startY,
                end_x = endX,
                end_y = endY,
                robot_type = robotType,
                destinations_x = destinations.ConvertAll(v => v.x).ToArray(),
                destinations_y = destinations.ConvertAll(v => v.y).ToArray(),
                move_speed = moveSpeed,
                perception_radius = perceptionRadius,
                obstacle_distance_threshold = obstacleDistanceThreshold,
                loop = loop,
                obstacle_detected = obstacleDetected,
                performing_task = isPerformingTask
            };            
            Debug.Log($"[Robot {robotId}] Sent tracking data.");
            RobotManagerClient.SendTrackingData(trackerMsg);
            trackerTimer = 0f;
        }
    }

    private void CheckAndAskForNewPath()
    {
        if (loop && !isPathRequestPending)
        {
            startX = destinations[destinationIndex].x;
            startY = destinations[destinationIndex].y;
            destinationIndex = (destinationIndex + 1) % destinations.Count;
            endX = destinations[destinationIndex].x;
            endY = destinations[destinationIndex].y;
            SendRequest();
        }
    }

    protected void Move(Vector3 target)
    {
        gameObject.transform.position =
            Vector3.MoveTowards(gameObject.transform.position, target, moveSpeed * Time.deltaTime);
    }

    protected void CheckIfQueuedPointReached(Vector3 target)
    {
        if (Vector3.Distance(gameObject.transform.position, target) < 0.02f)
        {
            pathQueue.Dequeue();
        }
    }

    private void CheckForObstacles(Vector3 target)
    {
        RaycastHit2D[] hits = Physics2D.CircleCastAll(gameObject.transform.position, perceptionRadius, target - gameObject.transform.position,
            Vector3.Distance(gameObject.transform.position, target));
        
        obstacleDetected = false;

        if (hits.Length == 0) return;

        System.Array.Sort(hits, (x, y) => x.distance.CompareTo(y.distance));
        
        foreach (var hit in hits)
        {
            GameObject objectHit = hit.collider != null ? hit.collider.gameObject : null;
            if (objectHit == null || objectHit == gameObject) continue;
            if (hit.distance > obstacleDistanceThreshold) continue;
            if (HandleSpecialObstacle(objectHit)) continue;
            if (obstacleDetected) return;
            ReportObstacle(objectHit);
            obstacleDetected = true;
            return;
        }
    }

    protected void ReportObstacle(GameObject obstacle)
    {
        if (reportedObstacles.Contains(obstacle)) return;
        var req = new ObstacleManagerSubscriberMsg()
        {
            x = obstacle.transform.position.x,
            y = obstacle.transform.position.y,
            type = obstacle.tag
        };
        ros.Publish("obstacle_manager/report_obstacle", req);
        reportedObstacles.Add(obstacle);
        Debug.Log($"[Robot {robotId}] Reported obstacle at ({req.x}, {req.y}) to Obstacle Manager.");
    }

    protected void PerformSideStep(GameObject obstacle)
    {
        if (lastAvoided == obstacle) return;
        lastAvoided = obstacle;

        List<Vector3> oldPath = new(pathQueue);
        pathQueue.Clear();
        
        Vector3 robotPos = transform.position;
        Vector3 obstaclePos = obstacle.transform.position;
        Vector3 toObstacle = obstaclePos - robotPos;
        Vector3 forwardDir = toObstacle.normalized;
        float distToObstacle = toObstacle.magnitude;
        
        Vector3 sideDir = Vector3.Cross(forwardDir, Vector3.forward);
        
        float sideOffset = 0.2f;
        float clearance = obstacle.transform.localScale.x;

        Vector3 pos1 = robotPos + sideDir * sideOffset;
        Vector3 pos2 = pos1 + forwardDir * (distToObstacle + clearance);
        Vector3 pos3 = pos2 - sideDir * sideOffset;

        float reentryProgress = Vector3.Dot(pos3 - robotPos, forwardDir);
         List<Vector3> newPath = new()
        {
            pos1,
            pos2,
            pos3
        };

        foreach (var pt in oldPath)
        {
            if (Vector3.Distance(pt, obstaclePos) < clearance) continue;
            float ptProgress = Vector3.Dot(pt - robotPos, forwardDir);
            if (ptProgress < reentryProgress) continue;
            newPath.Add(pt);
        }
        
        foreach (var p in newPath) pathQueue.Enqueue(p);
        Debug.Log($"[CleanerRobot {robotId}] Side-stepping unattended package at {obstacle.transform.position}.");
    }

    protected virtual bool HandleSpecialObstacle(GameObject objectHit)
    {
        return false;
    }

    protected virtual void UpdateTask()
    {
        
    }

    public void SendRequest()
    {
        Transform transform = gameObject.transform;
        float currentX = transform.position.x;
        float currentY = transform.position.y;
        var req = new PathPlannerRequestMsg()
        {
            robot_id = robotId,
            start_x = currentX,
            start_y = currentY,
            end_x = endX,
            end_y = endY
        };

        ros.Publish("path_planner/request", req);
        isPathRequestPending = true;
        Debug.Log($"[Robot {robotId}] Sent path request: ({currentX},{currentY}), ({endX},{endY})");
    }

    void ResultCallback(PathPlannerResponseMsg res)
    {
        if (res.robot_id != robotId) return;

        if (!res.success)
        {
            Debug.LogWarning($"[Robot {robotId}] Path planning failed.");
            return;
        }

        //pathQueue.Clear();

        for (int i = 0; i < res.path_x.Length; i++)
        {
            Vector3 point = new(
                res.path_x[i],
                res.path_y[i],
                0f
            );
            //if (Vector3.Distance(robot.transform.position, point) < 0.05f) continue;

            pathQueue.Enqueue(point);
        }

        lastAvoided = null;
        isPathRequestPending = false;
        Debug.Log($"[Robot {robotId}] Received path with {res.path_x.Length} points.");
    }
}
