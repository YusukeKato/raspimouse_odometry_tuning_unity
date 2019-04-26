using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using WebSocketSharp;

public class ROSConnector : MonoBehaviour {

    [System.Serializable]
    public class RosData_sub
    {
        public string op;
        public string topic;
    }

    [System.Serializable]
    public class RosData_pub
    {
        public string op;
        public string topic;
        public string type;
    }

    // Odom Message -----------------------------------------------
    [System.Serializable]
    public class ROSData1
    {
        public string topic;
        public Odom msg;
        public string op;
    }

    [System.Serializable]
    public class Odom
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
        public int seq;
    }

    [System.Serializable]
    public class Stamp
    {
        public float secs;
        public float nsecs;
    }

    // PoseStamped Message ----------------------------------------
    [System.Serializable]
    public class ROSData2
    {
        public string op;
        public string topic;
        public PoseStamped msg;
    }

    [System.Serializable]
    public class PoseStamped
    {
        public Header header;
        public PoseMsg pose;
    }

    [System.Serializable]
    public class PoseMsg
    {
        public Vector3 position;
        public Quaternion orientation;
    }

    // Bool Message -----------------------------------------------
    [System.Serializable]
    public class ROSData3
    {
        public string op;
        public string topic;
        public Msg msg;
    }

    [System.Serializable]
    public class Msg
    {
        public int data;
    }


    //-------------------------------------------------------------
    // マーカを認識するスクリプトへアクセス
    public GameObject ImageTarget;

    // 親となるオブジェクト
    public GameObject ParentObject;

    // 軌道を示す矢印オブジェクトのプレハブ
    public GameObject ArrowBluePrefab;
    public GameObject ArrowRedPrefab;
    List<GameObject> ArrowList;
    Vector3 origin_position;
    Quaternion origin_rotation;

    WebSocket ws_sub;
    WebSocket ws_sub2;
    WebSocket ws_pub;
    WebSocket ws_pub2;

    // ROSへ接続しているかどうか
    bool isConnect;

    // キャリブレーションを開始する
    public bool isStart;

    // ROS PC IP Address
    public string initialIpAddress = "192.168.22.12";
    string ipAddress;

    // Topic Name
    string topic_sub = "/odom";
    string topic_sub2 = "/calibr_flag_2";
    string topic_pub = "/true_pose";
    string topic_pub2 = "/calibr_flag";

    // op Name
    string op_sub = "subscribe";
    string op_pub = "advertise";
    string op_p = "publish";
    string op_un = "unadvertise";

    // type Name
    string type_odom = "geometry_msgs/Odometry";
    string type_pose = "geometry_msgs/PoseStamped";
    string type_int = "std_msgs/Int16";

    // 計測したロボットの座標を保存
    public List<Vector3> odomPosiArray;
    public List<Quaternion> odomRotaArray;

    // 周期を決定
    float span_sub = 0.1f;
    float delta_sub = 0;
    float span_pub = 0.03f;
    float delta_pub = 0;

    float span_flagpub = 1f;
    float delta_flagpub = 0;
    // Text
    public Text connectButtonText;
    public Text startButtonText;
    public Text XZText;
    public Text YText;
    public Text IsStartText;
    public Text IsMarkerFoundText;
    public Text CalibrationFlagText;

    // InputField
    public InputField robotAddress;

    // Vuforiaのマーカ認識スクリプトから参照
    bool isMarkerFound;
    Vector3 trueRobotPosition;
    Quaternion trueRobotRotation;

    // サブスクライブしたロボットの姿勢データ
    Vector3 RobotPosition = Vector3.zero;
    Quaternion RobotRotation = Quaternion.identity;

    // arrow生成のための一時的なロボット姿勢データ
    Vector3 RobotPosition_before = Vector3.zero;
    Quaternion RobotRotation_before = Quaternion.identity;
    Vector3 trueRobotPosition_before = Vector3.zero;
    Quaternion trueRobotRotation_before = Quaternion.identity;

    // Unity側で時間を計測
    float unity_time;

    // キャリブレーションのタイミング管理
    int calibr_flag = 0;
    bool publishFlagFlag = false;
    float deltaTimeFlag = 0;
    int flag_pre = 0;
    //-------------------------------------------------------------

    void Start()
    {
        MyInit();
        WsSetting_sub();
        WsSetting_sub2();
        WsSetting_pub();
        WsSetting_pub2();
    }

    // 変数初期化
    void MyInit ()
	{
        unity_time = 0;
        isStart = false;
        isConnect = false;
        isMarkerFound = false;
        odomPosiArray = new List<Vector3>();
        odomPosiArray.Add(new Vector3(0, 0, 0));
        odomRotaArray = new List<Quaternion>();
        odomRotaArray.Add(new Quaternion(0, 0, 0, 0));

        trueRobotPosition = Vector3.zero;
        trueRobotRotation = Quaternion.identity;

        ArrowList = new List<GameObject>();

        origin_position = Vector3.zero;
        origin_rotation = Quaternion.identity;
        ipAddress = initialIpAddress;
        robotAddress.text = ipAddress;

    }

	void Update () {
        // Update 一周期分の時間を足す
        delta_sub += Time.deltaTime;
        delta_pub += Time.deltaTime;

        delta_flagpub += Time.deltaTime;

        if (publishFlagFlag == true && isConnect == true)
        {
            deltaTimeFlag += Time.deltaTime;
            PublishFunc2(1);
        }
        else if (publishFlagFlag == false && isConnect == true)
        {
            PublishFunc2(0);
        }

        if(deltaTimeFlag > 1.0f)
        {
            publishFlagFlag = false;
            deltaTimeFlag = 0;
        }

        //CalibrationFlagText.text = calibr_flag.ToString();
        CalibrationFlagText.text = "Signal";
        //if (calibr_flag == 3)
        //{
        //    flag_pre = 0;
        //    PublishFunc2(0);
        //}
        //else if (calibr_flag == 4)
        //{
        //    PublishFunc2(5);
        //}
        //else if (calibr_flag == 1)
        //{
        //    PublishFunc2(flag_pre);
        //}

        // DefaultTrackableEventHandlerを参照
        isMarkerFound = ImageTarget.GetComponent<CustomTrackableEventHandler>().isFound;

        IsStartText.text = isStart.ToString();
        IsMarkerFoundText.text = isMarkerFound.ToString();

        if(isMarkerFound == true)
        {
            unity_time += Time.deltaTime;
        }
        // publish
        if(isMarkerFound == true && delta_pub >= span_pub)
        {
            delta_pub = 0;
            PublishFunc();
        }
        // arrow生成
        if(isMarkerFound == true)
        {
            ArrowInstantiateTrueOdom();
            ArrowInstantiateOdom();
        }
	}

    // websocketの内容を定義 ----------------------------------------
    void WsSetting_sub()
    {
        ws_sub = new WebSocket("ws://" + ipAddress + ":9090/");

        ws_sub.OnOpen += (sender, e) =>
        {
            Debug.Log("WebSocket Open!!");
            Debug.Log("connecting " + ipAddress);
            RosData_sub data = new RosData_sub();
            data.op = op_sub;
            data.topic = topic_sub;
            string json = JsonUtility.ToJson(data);
            ws_sub.Send(json);
        };

        ws_sub.OnError += (sender, e) =>
        {
            Debug.Log("WebSocket Error Message : " + e.Message);
        };

        ws_sub.OnClose += (sender, e) =>
        {
            Debug.Log("Websocket Close");
            RosData_sub data = new RosData_sub();
            data.op = "un" + op_sub;
            data.topic = topic_sub;
            string json = JsonUtility.ToJson(data);
            ws_sub.Send(json);
        };

        ws_sub.OnMessage += (sender, e) =>
        {
            if (delta_sub >= span_sub)
            {
                delta_sub = 0;
                SubscribeFunc(e.Data);
            }
        };
    }

    void WsSetting_sub2()
    {
        ws_sub2 = new WebSocket("ws://" + ipAddress + ":9090/");

        ws_sub2.OnOpen += (sender, e) =>
        {
            Debug.Log("WebSocket Open!!");
            Debug.Log("connecting " + ipAddress);
            RosData_sub data = new RosData_sub();
            data.op = op_sub;
            data.topic = topic_sub2;
            string json = JsonUtility.ToJson(data);
            ws_sub2.Send(json);
        };

        ws_sub2.OnError += (sender, e) =>
        {
            Debug.Log("WebSocket Error Message : " + e.Message);
        };

        ws_sub2.OnClose += (sender, e) =>
        {
            Debug.Log("Websocket Close");
            RosData_sub data = new RosData_sub();
            data.op = "un" + op_sub;
            data.topic = topic_sub2;
            string json = JsonUtility.ToJson(data);
            ws_sub2.Send(json);
        };

        ws_sub2.OnMessage += (sender, e) =>
        {
            if (delta_sub >= span_sub)
            {
                delta_sub = 0;
                SubscribeFunc2(e.Data);
            }
        };
    }

    void WsSetting_pub()
    {
        ws_pub = new WebSocket("ws://" + ipAddress + ":9090/");

        ws_pub.OnOpen += (sender, e) =>
        {
            Debug.Log("WebSocket Open!!");
            RosData_pub data = new RosData_pub();
            data.op = op_pub;
            data.topic = topic_pub;
            data.type = type_pose;
            string json = JsonUtility.ToJson(data);
            ws_pub.Send(json);
        };

        ws_pub.OnError += (sender, e) =>
        {
            Debug.Log("WebSocket Error Message : " + e.Message);
        };

        ws_pub.OnClose += (sender, e) =>
        {
            Debug.Log("Websocket Close");
            RosData_pub data = new RosData_pub();
            data.op = "un" + op_pub;
            data.topic = topic_pub;
            data.type = type_pose;
            string json = JsonUtility.ToJson(data);
            ws_pub.Send(json);
        };
    }

    void WsSetting_pub2()
    {
        ws_pub2 = new WebSocket("ws://" + ipAddress + ":9090/");

        ws_pub2.OnOpen += (sender, e) =>
        {
            Debug.Log("WebSocket Open!!");
            RosData_pub data = new RosData_pub();
            data.op = op_pub;
            data.topic = topic_pub2;
            data.type = type_int;
            string json = JsonUtility.ToJson(data);
            ws_pub2.Send(json);
        };

        ws_pub2.OnError += (sender, e) =>
        {
            Debug.Log("WebSocket Error Message : " + e.Message);
        };

        ws_pub2.OnClose += (sender, e) =>
        {
            Debug.Log("Websocket Close");
            RosData_pub data = new RosData_pub();
            data.op = "un" + op_pub;
            data.topic = topic_pub2;
            data.type = type_int;
            string json = JsonUtility.ToJson(data);
            ws_pub2.Send(json);
        };
    }


    // 計測したロボットの姿勢に合わせて矢印オブジェクトを生成 --------------
    void ArrowInstantiateTrueOdom()
    {
        if(Vector3.Distance(trueRobotPosition, trueRobotPosition_before) > 0.03f)
        {
            // Arrow生成
            GameObject arrow = Instantiate(ArrowBluePrefab);
            ArrowList.Add(arrow);
            arrow.transform.parent = ParentObject.transform;
            arrow.transform.localPosition = trueRobotPosition;
            arrow.transform.localRotation = trueRobotRotation;
            arrow.transform.Rotate(0, 180f, 0);
            // 一つ前の座標更新
            trueRobotPosition_before = trueRobotPosition;
            trueRobotRotation_before = trueRobotRotation;
        }
    }

    // ROSから送られてきたロボットの姿勢に合わせて矢印オブジェクトを生成 --------------
    void ArrowInstantiateOdom()
    {
        if (Vector3.Distance(RobotPosition, RobotPosition_before) > 0.03f)
        {
            // Arrow生成
            GameObject arrow = Instantiate(ArrowRedPrefab);
            ArrowList.Add(arrow);
            arrow.transform.parent = ParentObject.transform;
            //arrow.transform.localPosition = RobotPosition - origin_position;
            arrow.transform.localPosition = RobotPosition;
            arrow.transform.localRotation = RobotRotation;
            //Vector3 e = origin_rotation.eulerAngles;
            //arrow.transform.Rotate(0, 180f - e.y, 0);
            arrow.transform.Rotate(0, 180.0f, 0);
            // 一つ前の座標更新
            RobotPosition_before = RobotPosition;
            RobotRotation_before = RobotRotation;
        }
    }

    // サブスクライブの内容 -------------------------------------------
    void SubscribeFunc(string message)
    {
        ROSData1 odom = JsonUtility.FromJson<ROSData1>(message);

        // 変換、ROSからUnityの座標系
        RobotPosition = new Vector3(-odom.msg.pose.pose.position.y, odom.msg.pose.pose.position.z, odom.msg.pose.pose.position.x);
        RobotRotation = new Quaternion(-odom.msg.pose.pose.orientation.y, odom.msg.pose.pose.orientation.z, odom.msg.pose.pose.orientation.x, -odom.msg.pose.pose.orientation.w);

        // 配列へ保存
        //odomPosiArray.Add(odomPosition);
        //odomRotaArray.Add(odomOrientation);
    }

    void SubscribeFunc2(string message)
    {
        ROSData3 data = JsonUtility.FromJson<ROSData3>(message);
        calibr_flag = data.msg.data;
        Debug.Log(calibr_flag);
    }

    // パブリッシュの内容 --------------------------------------------
    void PublishFunc()
    {
        // DefaultTrackableEventHandlerを参照
        trueRobotPosition = ImageTarget.transform.localPosition;
        trueRobotRotation = ImageTarget.transform.localRotation;
        Vector3 pt = trueRobotPosition;
        XZText.text = "X:" + pt.x.ToString("F4") + " Z:" + pt.z.ToString("F4");
        Vector3 rt = trueRobotRotation.eulerAngles;
        YText.text = "Y:" + rt.y;

        // message内容作成
        Stamp stamp = new Stamp();
        stamp.secs = unity_time;
        stamp.nsecs = unity_time;

        Header header = new Header();
        header.stamp = stamp;
        header.frame_id = "/map";
        header.seq = 10;

        Vector3 p = trueRobotPosition;
        Quaternion q = trueRobotRotation;

        Vector3 pos = new Vector3(p.z, -p.x, p.y);
        Quaternion ori = new Quaternion(q.z, -q.x, q.y, -q.w);

        PoseMsg pose = new PoseMsg();
        pose.position = pos;
        pose.orientation = ori;

        PoseStamped msg = new PoseStamped();
        msg.header = header;
        msg.pose = pose;

        ROSData2 data = new ROSData2();
        data.op = "publish";
        data.topic = topic_pub;
        data.msg = msg;

        // パースと送信
        string json = JsonUtility.ToJson(data);
        //Debug.Log(json);
        ws_pub.Send(json);
    }

    void PublishFunc2(int cflag)
    {
        Msg msg = new Msg();
        msg.data = cflag;

        ROSData3 d = new ROSData3();
        d.op = "publish";
        d.topic = topic_pub2;
        d.msg = msg;
        // パースと送信
        string json = JsonUtility.ToJson(d);
        //Debug.Log(json);
        ws_pub2.Send(json);
    }

    // アプリケーション終了時------------------------------------------
    private void OnApplicationQuit()
    {
        ws_sub.Close();
        //ws_sub2.Close();
        ws_pub.Close();
        ws_pub2.Close();
    }

    // ボタンから接続と切断を操作---------------------------------------
    public void ConnectButton()
    {
        if (isConnect == true)
        {
            ws_sub.Close();
            //ws_sub2.Close();
            ws_pub.Close();
            ws_pub2.Close();
            isConnect = false;
            connectButtonText.text = "Connect";
            Debug.Log("Websocket Close ......");
        }
        else if (isConnect == false)
        {
            ws_sub.Connect();
            //ws_sub2.Connect();
            ws_pub.Connect();
            ws_pub2.Connect();
            isConnect = true;
            connectButtonText.text = "DisConnect";
            Debug.Log("Websocket Connect!!");
        }
    }
    // ------------------------------------------------------------

    public void StartButton()
    {
        if(isStart == false)
        {
            isStart = true;
            startButtonText.text = "Stop";
        }
        else if(isStart == true)
        {
            isStart = false;
            startButtonText.text = "Start";
        }
    }

    // ------------------------------------------------------------

    public void ArrowResetButton()
    {
        for (int i = 0; i < ArrowList.Count; i++)
        {
            Destroy(ArrowList[i]);
        }

        ArrowList.Clear();

        origin_position = RobotPosition;
        origin_rotation = RobotRotation;
    }

    // ------------------------------------------------------------
    public void CalibrationFlagButton()
    {
        if(calibr_flag == 0)
        {
            //flag_pre = 2;
            //PublishFunc2(2);

            publishFlagFlag = true;
        }
    }

    public void WorldAnchorButton()
    {
        //ワールド座標のアンカーを更新
        ParentObject.transform.position = ImageTarget.transform.position;
        ParentObject.transform.rotation = ImageTarget.transform.rotation;
    }


    // ------------------------------------------------------------
    public void UpdateRobotAddress()
    {
        ipAddress = robotAddress.text;
        Debug.Log("target IP " + ipAddress);
    }
}


