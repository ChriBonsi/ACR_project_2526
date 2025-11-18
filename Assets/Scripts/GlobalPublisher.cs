using UnityEngine;
using Unity.Robotics.ROSTCPConnector;
using RosMessageTypes.PathPlanner;

public class ROSGlobalPublisher : MonoBehaviour
{
    void Start()
    {
        var ros = ROSConnection.GetOrCreateInstance();
        ros.RegisterPublisher<PathPlannerRequestMsg>("path_planner/request");
        // ros.RegisterPublisher<BoolMsg>("path_planner/cancel");
        Debug.Log("Global ROS publishers registered.");
    }
}
