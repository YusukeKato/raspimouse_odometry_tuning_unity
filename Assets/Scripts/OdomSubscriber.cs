using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using RBS;
using Odometry = RBS.Messages.nav_msgs.Odometry;

public class OdomSubscriber : MonoBehaviour
{
    Odometry data;
    Vector3 RobotPosition = Vector3.zero;
    Quaternion RobotRotation = Quaternion.identity;

    private void Awake()
    {
        new RBSubscriber<Odometry>("/odom", Callback);
    }
    void Callback(Odometry msg)
    {
        data = msg;
    }

    private void Update()
    {
        if (data != null)
        {
            // 変換、ROSからUnityの座標系
            RobotPosition = new Vector3((float)-data.pose.pose.position.y, (float)data.pose.pose.position.z, (float)data.pose.pose.position.x);
            RobotRotation = new Quaternion((float)-data.pose.pose.orientation.y, (float)data.pose.pose.orientation.z, (float)data.pose.pose.orientation.x, (float)-data.pose.pose.orientation.w);
        }
    }

    public Vector3 GetRobotPosition()
    {
        return RobotPosition;
    }

    public Quaternion GetRobotRotation()
    {
        return RobotRotation;
    }
}
