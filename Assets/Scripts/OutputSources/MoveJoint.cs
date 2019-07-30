/*
This message class is generated automatically with 'ServiceMessageGenerator' of ROS#
*/

using Newtonsoft.Json;
using RosSharp.RosBridgeClient.Messages.Geometry;
using RosSharp.RosBridgeClient.Messages.Navigation;
using RosSharp.RosBridgeClient.Messages.Sensor;
using RosSharp.RosBridgeClient.Messages.Standard;
using RosSharp.RosBridgeClient.Messages.Actionlib;

namespace RosSharp.RosBridgeClient.Services {
    public class MoveJointRequest : Message {
        [JsonIgnore]
        public const string RosMessageName = "dsr_msgs/MoveJoint";

        public float[] pos;
        public float vel;
        public float acc;
        public float time;
        public float radius;
        public int mode;
        public int blendType;
        public int syncType;

        public MoveJointRequest() { }

        public MoveJointRequest(float[] _pos, float _vel, float _acc, float _time, float _radius, int _mode, int _blendType, int _syncType) {
            pos = _pos;
            vel = _vel;
            acc = _acc;
            time = _time;
            radius = _radius;
            mode = _mode;
            blendType = _blendType;
            syncType = _syncType;
        }
    }

    public class MoveJointResponse : Message {
        [JsonIgnore]
        public const string RosMessageName = "dsr_msgs/MoveJoint";

        public bool success;
    }
}
