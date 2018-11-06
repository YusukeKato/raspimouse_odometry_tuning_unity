using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WebSocketSharp;
using WebSocketSharp.Net;

public class PublisherTest : MonoBehaviour {

    [System.Serializable]
    public class RosData1
    {
        public string op;
        public string topic;
        public string type;
    }

    [System.Serializable]
    public class RosData2
    {
        public string op;
        public string topic;
        public Msg msg;
    }

    [System.Serializable]
    public class Msg
    {
        public Header header;
        public Pose pose;
    }


    [System.Serializable]
    public class Pose
    {
        public Vector3 position;
        public Quaternion orientation;
    }

    [System.Serializable]
    public class Header
    {
        public Stamp stamp;
        public string frame_id;
        public int seq;
    }

    [System.Serializable]
    public class Stamp
    {
        public int secs;
        public int nsecs;
    }

    WebSocket ws;
    public Vector3 posi = new Vector3(0, 0, 0);
    public Vector3 rota = new Vector3(0, 0, 0);

    public string ipaddress = "192.168.22.12"; //自分のIPアドレス

    void Start()
    {
        ws = new WebSocket("ws://" + ipaddress + ":9090/");

        ws.OnOpen += (sender, e) =>
        {
            Debug.Log("WebSocket Open");
            RosData1 data = new RosData1();
            data.op = "advertise";
            data.topic = "/pose";
            data.type = "geometry_msgs/PoseStamped";
            string json = JsonUtility.ToJson(data);
            ws.Send(json);
        };

        ws.OnError += (sender, e) =>
        {
            Debug.Log("WebSocket Error Message: " + e.Message);
        };

        ws.OnClose += (sender, e) =>
        {
            Debug.Log("WebSocket Close");
        };

        ws.Connect();
    }

    // Update is called once per frame
    void Update()
    {
        Stamp stamp = new Stamp();
        stamp.secs = 0;
        stamp.nsecs = 0;

        Header header = new Header();
        header.stamp = stamp;
        header.frame_id = "/map";
        header.seq = 10;

        Vector3 p = posi;
        Quaternion q = Quaternion.Euler(rota);

        Vector3 pos = new Vector3(p.z,-p.x,p.y);
        Quaternion ori = new Quaternion(q.z,-q.x,q.y,-q.w);

        Pose pose = new Pose();
        pose.position = pos;
        pose.orientation = ori;

        Msg msg = new Msg();
        msg.header = header;
        msg.pose = pose;

        RosData2 data = new RosData2();
        data.op = "publish";
        data.topic = "/pose";
        data.msg = msg;
        string json = JsonUtility.ToJson(data);
        Debug.Log(json);
        ws.Send(json);
        /*
        string msg2 = "{\"op\": \"publish\"," +
                       "\"topic\": \"/test\"," +
                       "\"msg\": {" +
                                  "\"linear\"  : { \"x\" : " + linear_val + ", \"y\" : 0, \"z\" : 0 }," +
                                  "\"angular\" : { \"x\" : 0, \"y\" : 0, \"z\" : " + angular_val + " } " +
                                 "}" +
                       "}";
        ws.Send(msg2);
        */
    }
}
