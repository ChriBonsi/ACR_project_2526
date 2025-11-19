using UnityEngine;
using Unity.Robotics.ROSTCPConnector;
using RosMessageTypes.PathPlanner;
using System.Collections.Generic;
using System;
using Unity.VisualScripting;
using UnityEditor;
using RosMessageTypes.ObstacleManager;

public class RobotPathPlanner : MonoBehaviour
{
    [Header("Robot settings")]
    public int robotId = 1;
    public GameObject robot;
    public float moveSpeed = 3f;
    public float perceptionRadius = 1f;
    public float obstacleDistanceThreshold = 1f;

    [Header("Request")]
    public int endX = 3;
    public int endY = 3;

    private ROSConnection ros;
    private Queue<Vector3> pathQueue = new();
    private bool isMoving = true;
    private bool obstacleDetected = false;
    private Vector3 lastPosition;

    void Start()
    {
        ros = ROSConnection.GetOrCreateInstance();

        ros.Subscribe<PathPlannerFeedbackMsg>("path_planner/feedback", FeedbackCallback);
        ros.Subscribe<PathPlannerResponseMsg>("path_planner/response", ResultCallback);

        SendRequest();

        perceptionRadius *= robot.transform.lossyScale.x;
    }

    void Update()
    {
        if (robot == null || pathQueue.Count == 0) return;

        //if (!isMoving) return;

        Vector3 target = pathQueue.Peek();

        CheckForObstacles(target);
        
        isMoving = !obstacleDetected;

        if (!isMoving) return;

        robot.transform.position =
            Vector3.MoveTowards(robot.transform.position, target, moveSpeed * Time.deltaTime);

            lastPosition = robot.transform.position;

        if (Vector3.Distance(robot.transform.position, target) < 0.02f)
            pathQueue.Dequeue();
    }

    private void CheckForObstacles(Vector3 target)
    {
        RaycastHit2D[] hits = Physics2D.CircleCastAll(robot.transform.position, perceptionRadius, target - robot.transform.position,
            Vector3.Distance(robot.transform.position, target));

        foreach (var hit in hits)
        {
            if (hit.collider != null && hit.collider.gameObject != robot)
            {
                //Debug.Log(hit.transform.position + " - " + hit.distance);
                if (hit.distance < obstacleDistanceThreshold && !obstacleDetected)
                {
                    /* Debug.LogWarning($"[Robot {robotId}] Obstacle detected! Stopping movement.");
                    Debug.LogWarning("Hit object: " + hit.collider.gameObject.name); */

                    obstacleDetected = true;

                    var req = new ObstacleManagerSubscriberMsg()
                    {
                        x = (int)hit.transform.position.x,
                        y = (int)hit.transform.position.y,
                        type = "obstacle"
                    };
                    ros.Publish("obstacle_manager/report_obstacle", req);
                    Debug.Log($"[Robot {robotId}] Reported obstacle at ({req.x}, {req.y}) to Obstacle Manager.");
                    return;
                }
            }
        }
    }

    public void SendRequest()
    {
        Transform transform = robot.transform;
        int startX = (int)transform.position.x;
        int startY = (int)transform.position.y;
        var req = new PathPlannerRequestMsg()
        {
            robot_id = robotId,
            start_x = startX,
            start_y = startY,
            end_x = endX,
            end_y = endY
        };

        ros.Publish("path_planner/request", req);
        Debug.Log($"[Robot {robotId}] Sent path request: ({startX},{startY}), ({endX},{endY})");
    }

    void FeedbackCallback(PathPlannerFeedbackMsg fb)
    {
        if (fb.robot_id != robotId) return;

        Debug.Log($"[Robot {robotId}] {fb.percent_complete:F1}%");
    }

    void ResultCallback(PathPlannerResponseMsg res)
    {
        if (res.robot_id != robotId) return;

        if (!res.success)
        {
            Debug.LogWarning($"[Robot {robotId}] Path planning failed.");
            return;
        }

        pathQueue.Clear();

        for (int i = 0; i < res.path_x.Length; i++)
        {
            Vector3 point = new(
                res.path_x[i],
                res.path_y[i],
                0f
            );
            pathQueue.Enqueue(point);
        }

        Debug.Log($"[Robot {robotId}] Received path with {res.path_x.Length} points.");
    }
}
