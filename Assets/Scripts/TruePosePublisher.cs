using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using RBS;
using PoseStamped = RBS.Messages.geometry_msgs.PoseStamped;
using Header = RBS.Messages.std_msgs.Header;
using Pose = RBS.Messages.geometry_msgs.Pose;

public class TruePosePublisher : MonoBehaviour
{
    private RBPublisher<PoseStamped> true_pose_pub;

    private Header header;
    private Pose pose;

    // Start is called before the first frame update
    void Awake()
    {
        true_pose_pub = new RBPublisher<PoseStamped>("/true_pose");
    }

    // Update is called once per frame
    void Update()
    {
        if (RBSocket.Instance.IsConnected)
        {
            PoseStamped true_pose = new PoseStamped();
            true_pose.header = header;
            true_pose.pose = pose;
            true_pose_pub.publish(true_pose);
        }
    }

    public void SetHeader(Header data)
    {
        header = data;
    }

    public void SetPose(Pose data)
    {
        pose = data;
    }
}
