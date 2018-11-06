using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WebSocketSharp;
using UnityEngine.UI;

public class OdomSbscriber : MonoBehaviour {

    [System.Serializable]
    public class RosData
    {
        public string op;
        public string topic;
    }

    [System.Serializable]
    public class SensorValue
    {
        public string topic;
        public Sensors msg;
        public string op;
    }

    [System.Serializable]
    public class Sensors
    {
        public float right_forward;
        public float left_side;
        public float right_side;
        public float left_forward;
    }

    [System.Serializable]
    public class Odom
    {
        public string topic;
        public Msg msg;
        public string op;
    }

    [System.Serializable]
    public class Msg
    {
        public Twist twist;
        public Header header;
        public Pose pose;
        public string child_frame_id;
    }

    [System.Serializable]
    public class Twist
    {
        public Twist2 twist;
        float[] covariance;
    }

    [System.Serializable]
    public class Twist2
    {
        public Vector3 linear;
        public Vector3 angular;
    }


    [System.Serializable]
    public class Pose
    {
        public Pose2 pose;
        float[] covariance;
    }

    [System.Serializable]
    public class Position
    {
        public float x;
        public float y;
        public float z;
    }

    [System.Serializable]
    public class Orientation
    {
        public float x;
        public float y;
        public float z;
        public float w;
    }

    [System.Serializable]
    public class Pose2
    {
        public Vector3 position;
        public Quaternion orientation;
    }

    [System.Serializable]
    public class Header
    {
        public Stamp stamp;
        public string frame_id;
        public string seq;
    }

    [System.Serializable]
    public class Stamp
    {
        public int secs;
        public int nsecs;
    }

    public Text OdomPosi;
    public Text OdomRota;

    public Vector3 odomPosition;
    public Quaternion odomOrientation;
    public Vector3 odomRotation;

    public List<Vector3> odomPosiArray;
    public List<Quaternion> odomRotaArray;

    //public float[] sensorValue = new float[4];

    WebSocket ws;

    //public Text wsConnectionText;
    bool wsConnection = false;

    private float span = 0.1f;
    private float delta = 0;

    //public Text InputIPAdress;
    public string ipadress = "192.168.22.12";
    //public Text NowIPText;

    //public GameObject SettingPanel;
    private bool flag_SetPanel = false;

    public string topicName = "/odom";

    public GameObject ParentObject;
    public GameObject ArrowPrefab;

    bool testFlag = true;

    void Start()
    {
        odomPosiArray = new List<Vector3>();
        odomPosiArray.Add(new Vector3(0, 0, 0));
        odomRotaArray = new List<Quaternion>();
        odomRotaArray.Add(new Quaternion(0, 0, 0, 0));

        //NowIPText.text = "Now:" + ipadress;
        WsSetting();
        //wsConnectionText.text = "Connect";
        ws.Connect();
    }

    void Update()
    {
        delta += Time.deltaTime;

        OdomPosi.text = odomPosition.x.ToString();
        //OdomRota.text = odomRotation.ToString();

        if(Vector3.Distance(odomPosition, odomPosiArray[odomPosiArray.Count-1]) > 0.03f)
        {
            odomPosiArray.Add(odomPosition);
            odomRotaArray.Add(odomOrientation);
            GameObject arrow = Instantiate(ArrowPrefab);
            arrow.transform.parent = ParentObject.transform;
            arrow.transform.localPosition = odomPosiArray[odomPosiArray.Count - 1];
            arrow.transform.localRotation = odomRotaArray[odomRotaArray.Count - 1];
            arrow.transform.Rotate(0, 180, 0);
        }
        /*
        if(testFlag == true)
        {
            testFlag = false;
            //test, オドメトリを表すarrowの表示する座標が正しいかどうか
            GameObject arrow = Instantiate(ArrowPrefab);
            arrow.transform.parent = ParentObject.transform;
            arrow.transform.localPosition = new Vector3(0, 0, 0.3f);
            arrow.transform.localRotation = new Quaternion(0, 0, 0, 0);
            arrow.transform.Rotate(0, 180, 0);
        }
        */
    }

    void WsSetting()
    {
        ws = new WebSocket("ws://" + ipadress + ":9090/");

        ws.OnOpen += (sender, e) =>
        {
            Debug.Log("WebSocket Open!!");
            RosData data = new RosData();
            data.op = "subscribe";
            data.topic = topicName;
            string json = JsonUtility.ToJson(data);
            ws.Send(json);
        };

        ws.OnError += (sender, e) =>
        {
            Debug.Log("WebSocket Error Message : " + e.Message);
        };

        ws.OnClose += (sender, e) =>
        {
            Debug.Log("Websocket Close");
            RosData data = new RosData();
            data.op = "unsubscribe";
            data.topic = topicName;
            string json = JsonUtility.ToJson(data);
            ws.Send(json);
        };

        ws.OnMessage += (sender, e) =>
        {
            if (delta >= span)
            {
                delta = 0;

                string message = e.Data;
                //Debug.Log(message);

                Odom odomMsg = JsonUtility.FromJson<Odom>(message);
                //Debug.Log(odomMsg.msg.pose.pose.position.x);
                Debug.Log("aaa");
                Debug.Log(odomMsg.msg.pose.pose.orientation.eulerAngles);

                odomPosition = new Vector3(-odomMsg.msg.pose.pose.position.y, odomMsg.msg.pose.pose.position.z, odomMsg.msg.pose.pose.position.x);
                odomOrientation = new Quaternion(-odomMsg.msg.pose.pose.orientation.y, odomMsg.msg.pose.pose.orientation.z, odomMsg.msg.pose.pose.orientation.x, -odomMsg.msg.pose.pose.orientation.w);
                //odomOrientation = odomMsg.msg.pose.pose.orientation;
                odomRotation = odomOrientation.eulerAngles;

                Debug.Log("bbb");
                Debug.Log(odomRotation);

                //Debug.Log(odomPosition);
                //Debug.Log(odomRotation);

                //odomPosition = new Vector3(odomMsg.msg.pose.pose.position.x, odomMsg.msg.pose.pose.position.y, odomMsg.msg.pose.pose.position.z);
                //odomRotation = odomMsg.msg.pose.pose.orientation.eulerAngles;
            }
        };
    }

    public void WsConnection()
    {
        if (wsConnection)
        {
            ws.Close();
            wsConnection = false;
            //wsConnectionText.text = "Connect";
            Debug.Log("Websocket Close ......");
        }
        else if (!wsConnection)
        {
            WsSetting();
            ws.Connect();
            wsConnection = true;
            //wsConnectionText.text = "Close";
            Debug.Log("Websocket Connect!!");
        }
    }

    public void InputOKButton()
    {
        //ipadress = InputIPAdress.text;
        //NowIPText.text = "Now:" + ipadress;
    }

    public void SetPanelButton()
    {
        if (flag_SetPanel)
        {
            //SettingPanel.SetActive(false);
            flag_SetPanel = false;
        }
        else if (!flag_SetPanel)
        {
            //SettingPanel.SetActive(true);
            flag_SetPanel = true;
        }
    }

    private void OnApplicationQuit()
    {
        ws.Close();
        Debug.Log("Websocket Close ......");
    }
}

