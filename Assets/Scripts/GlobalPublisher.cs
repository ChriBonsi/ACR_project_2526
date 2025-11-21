using UnityEngine;
using Unity.Robotics.ROSTCPConnector;
using RosMessageTypes.PathPlanner;
using RosMessageTypes.ObstacleManager;
using RosMessageTypes.RobotManager;

public class ROSGlobalPublisher : MonoBehaviour
{
    void Awake()
    {
        var ros = ROSConnection.GetOrCreateInstance();
        ros.RegisterPublisher<PathPlannerRequestMsg>("path_planner/request");
        ros.RegisterPublisher<ObstacleManagerSubscriberMsg>("obstacle_manager/report_obstacle");
        ros.RegisterPublisher<RobotManagerRobotSubscriberMsg>("robot_manager/subscribe_robot");
        ros.RegisterPublisher<RobotManagerTrackerSubscriberMsg>("robot_manager/subscribe_tracker");
        Debug.Log("Global ROS publishers registered.");
    }
}
