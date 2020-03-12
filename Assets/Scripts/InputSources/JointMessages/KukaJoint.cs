// System
using System;

// Unity Engine
using RosSharp.RosBridgeClient;
using RosSharp.RosBridgeClient.MessageTypes.Std;

namespace NERVV.Samples.Junlab {
    [Serializable]
    public class KukaJoint : Message {
        public const string RosMessageName = "kuka_robot/kuka_all";

        public Header header;
        public double[] angles;
        public double[] xyzs;
        public double[] torques;
        public uint turn;
        public uint status;

        public KukaJoint() { }
        public KukaJoint(
            Header header,
            double[] angles,
            double[] xyzs,
            double[] torques,
            uint turn,
            uint status) : this() {
            this.header = header;
            this.xyzs = xyzs;
            this.torques = torques;
            this.turn = turn;
            this.status = status;
        }
    }
}
