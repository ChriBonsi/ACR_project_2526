using System.Collections.Generic;
using Unity.Robotics.ROSTCPConnector.MessageGeneration;


namespace RosMessageTypes.PathPlanner
{
    public class PathPlannerAction : Action<PathPlannerActionGoal, PathPlannerActionResult, PathPlannerActionFeedback, PathPlannerGoal, PathPlannerResult, PathPlannerFeedback>
    {
        public const string k_RosMessageName = "path_planner/PathPlannerAction";
        public override string RosMessageName => k_RosMessageName;


        public PathPlannerAction() : base()
        {
            this.action_goal = new PathPlannerActionGoal();
            this.action_result = new PathPlannerActionResult();
            this.action_feedback = new PathPlannerActionFeedback();
        }

        public static PathPlannerAction Deserialize(MessageDeserializer deserializer) => new PathPlannerAction(deserializer);

        PathPlannerAction(MessageDeserializer deserializer)
        {
            this.action_goal = PathPlannerActionGoal.Deserialize(deserializer);
            this.action_result = PathPlannerActionResult.Deserialize(deserializer);
            this.action_feedback = PathPlannerActionFeedback.Deserialize(deserializer);
        }

        public override void SerializeTo(MessageSerializer serializer)
        {
            serializer.Write(this.action_goal);
            serializer.Write(this.action_result);
            serializer.Write(this.action_feedback);
        }

    }
}
