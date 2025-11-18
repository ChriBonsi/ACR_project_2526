using System.Collections.Generic;
using Unity.Robotics.ROSTCPConnector.MessageGeneration;
using RosMessageTypes.Std;
using RosMessageTypes.Actionlib;

namespace RosMessageTypes.PathPlanner
{
    public class PathPlannerActionResult : ActionResult<PathPlannerResult>
    {
        public const string k_RosMessageName = "path_planner/PathPlannerActionResult";
        public override string RosMessageName => k_RosMessageName;


        public PathPlannerActionResult() : base()
        {
            this.result = new PathPlannerResult();
        }

        public PathPlannerActionResult(HeaderMsg header, GoalStatusMsg status, PathPlannerResult result) : base(header, status)
        {
            this.result = result;
        }
        public static PathPlannerActionResult Deserialize(MessageDeserializer deserializer) => new PathPlannerActionResult(deserializer);

        PathPlannerActionResult(MessageDeserializer deserializer) : base(deserializer)
        {
            this.result = PathPlannerResult.Deserialize(deserializer);
        }
        public override void SerializeTo(MessageSerializer serializer)
        {
            serializer.Write(this.header);
            serializer.Write(this.status);
            serializer.Write(this.result);
        }


#if UNITY_EDITOR
        [UnityEditor.InitializeOnLoadMethod]
#else
        [UnityEngine.RuntimeInitializeOnLoadMethod]
#endif
        public static void Register()
        {
            MessageRegistry.Register(k_RosMessageName, Deserialize);
        }
    }
}
