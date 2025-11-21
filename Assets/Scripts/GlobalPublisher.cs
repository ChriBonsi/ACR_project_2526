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
        ros.RegisterPublisher<RobotManagerSubscriberMsg>("robot_manager/subscribe_robot"); // changed type
        Debug.Log("Global ROS publishers registered.");
    }
}
