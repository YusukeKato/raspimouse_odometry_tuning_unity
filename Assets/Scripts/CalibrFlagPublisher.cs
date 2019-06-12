using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using RBS;
using Int16 = RBS.Messages.std_msgs.Int16;

public class CalibrFlagPublisher : MonoBehaviour
{
    private RBPublisher<Int16> calibr_flag_pub;
    private int flag;
    // Start is called before the first frame update
    void Awake()
    {
        flag = 0;
        calibr_flag_pub = new RBPublisher<Int16>("/calibr_flag");
    }

    // Update is called once per frame
    void Update()
    {
        if (RBSocket.Instance.IsConnected)
        {
            Int16 calibr_flag = new Int16();
            calibr_flag.data = flag;
            calibr_flag_pub.publish(calibr_flag);
        }
    }

    public void SetFlag(int data)
    {
        flag = data;
    }
}
