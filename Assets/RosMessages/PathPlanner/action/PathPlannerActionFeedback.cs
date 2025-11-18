using System.Collections.Generic;
using Unity.Robotics.ROSTCPConnector.MessageGeneration;
using RosMessageTypes.Std;
using RosMessageTypes.Actionlib;

namespace RosMessageTypes.PathPlanner
{
    public class PathPlannerActionFeedback : ActionFeedback<PathPlannerFeedback>
    {
        public const string k_RosMessageName = "path_planner/PathPlannerActionFeedback";
        public override string RosMessageName => k_RosMessageName;


        public PathPlannerActionFeedback() : base()
        {
            this.feedback = new PathPlannerFeedback();
        }

        public PathPlannerActionFeedback(HeaderMsg header, GoalStatusMsg status, PathPlannerFeedback feedback) : base(header, status)
        {
            this.feedback = feedback;
        }
        public static PathPlannerActionFeedback Deserialize(MessageDeserializer deserializer) => new PathPlannerActionFeedback(deserializer);

        PathPlannerActionFeedback(MessageDeserializer deserializer) : base(deserializer)
        {
            this.feedback = PathPlannerFeedback.Deserialize(deserializer);
        }
        public override void SerializeTo(MessageSerializer serializer)
        {
            serializer.Write(this.header);
            serializer.Write(this.status);
            serializer.Write(this.feedback);
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
