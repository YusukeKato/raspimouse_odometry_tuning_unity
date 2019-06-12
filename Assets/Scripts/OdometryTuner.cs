using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class OdometryTuner : MonoBehaviour
{
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

    // ROSへ接続しているかどうか
    bool isConnected;

    // キャリブレーションを開始する
    public bool isTuning;

    // 計測したロボットの座標を保存
    public List<Vector3> odomPosiArray;
    public List<Quaternion> odomRotaArray;

    // 周期を決定
    //float span_sub = 0.1f;
    float delta_sub = 0;
    float span_pub = 0.03f;
    float delta_pub = 0;

    //float span_flagpub = 1f;
    float delta_flagpub = 0;
    // Text
    public Text connectButtonText;
    public Text startButtonText;
    public Text XZText;
    public Text YText;
    public Text IsStartText;
    public Text IsMarkerFoundText;
    public Text CalibrationFlagText;

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
    //int flag_pre = 0;

    // ROSのトピックをやりとり・デバッグログを表示するためのゲームオブジェクト
    public GameObject RosConnectorGameObject;

    //-------------------------------------------------------------


    void Start()
    {
        // 各変数の初期化
        unity_time = 0;
        isTuning = false;
        isConnected = false;
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
    }


    void Update()
    {
        // Update 一周期分の時間を足す
        delta_sub += Time.deltaTime;
        delta_pub += Time.deltaTime;

        delta_flagpub += Time.deltaTime;

        RobotPosition = RosConnectorGameObject.GetComponent<OdomSubscriber>().GetRobotPosition();
        RobotRotation = RosConnectorGameObject.GetComponent<OdomSubscriber>().GetRobotRotation();

        if (publishFlagFlag == true && isConnected == true)
        {
            deltaTimeFlag += Time.deltaTime;
            RosConnectorGameObject.GetComponent<CalibrFlagPublisher>().SetFlag(1);
        }
        else if (publishFlagFlag == false && isConnected == true)
        {
            RosConnectorGameObject.GetComponent<CalibrFlagPublisher>().SetFlag(0);
        }

        if (deltaTimeFlag > 1.0f)
        {
            publishFlagFlag = false;
            deltaTimeFlag = 0;
        }

        CalibrationFlagText.text = "Signal";

        // DefaultTrackableEventHandlerを参照
        isMarkerFound = ImageTarget.GetComponent<CustomTrackableEventHandler>().isFound;

        IsStartText.text = isTuning.ToString();
        IsMarkerFoundText.text = isMarkerFound.ToString();

        if (isMarkerFound == true)
        {
            unity_time += Time.deltaTime;
        }
        // publish
        if (isMarkerFound == true && delta_pub >= span_pub)
        {
            delta_pub = 0;
            PublishRobotPose();
        }
        // arrow生成
        if (isMarkerFound == true)
        {
            ArrowInstantiateTrueOdom();
            ArrowInstantiateOdom();
        }
    }

    // 計測したロボットの姿勢に合わせて矢印オブジェクトを生成 --------------
    void ArrowInstantiateTrueOdom()
    {
        if (Vector3.Distance(trueRobotPosition, trueRobotPosition_before) > 0.03f)
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

    // パブリッシュの内容 --------------------------------------------
    void PublishRobotPose()
    {
        // DefaultTrackableEventHandlerを参照
        trueRobotPosition = ImageTarget.transform.localPosition;
        trueRobotRotation = ImageTarget.transform.localRotation;

        // GUI
        XZText.text = "X:" + trueRobotPosition.x.ToString("F4") + " Z:" + trueRobotPosition.z.ToString("F4");
        YText.text = "Y:" + trueRobotRotation.eulerAngles.y;

        // ROSトピックのデータ
        RBS.Messages.std_msgs.Header header = new RBS.Messages.std_msgs.Header();
        header.stamp.secs = (uint)unity_time;
        header.stamp.nsecs = (uint)unity_time;
        header.frame_id = "/map";
        header.seq = 10;

        RBS.Messages.geometry_msgs.Point pos = new RBS.Messages.geometry_msgs.Point
        {
            x = trueRobotPosition.z,
            y = -trueRobotPosition.x,
            z = trueRobotPosition.y
        };
        RBS.Messages.geometry_msgs.Quaternion ori = new RBS.Messages.geometry_msgs.Quaternion
        {
            x = trueRobotRotation.z,
            y = -trueRobotRotation.x,
            z = trueRobotRotation.y,
            w = -trueRobotRotation.w
        };
        RBS.Messages.geometry_msgs.Pose pose = new RBS.Messages.geometry_msgs.Pose
        {
            position = pos,
            orientation = ori
        };

        RosConnectorGameObject.GetComponent<TruePosePublisher>().SetHeader(header);
        RosConnectorGameObject.GetComponent<TruePosePublisher>().SetPose(pose);

    }

    // アプリケーション終了時------------------------------------------
    private void OnApplicationQuit()
    {
    }

    // ------------------------------------------------------------

    public void StartButton()
    {
        if (isTuning)
        {
            isTuning = false;
            startButtonText.text = "Start";
            RosConnectorGameObject.GetComponent<ROSConnector>().AddDialogMessage("STOP");
        }
        else
        {
            isTuning = true;
            startButtonText.text = "Stop";
            RosConnectorGameObject.GetComponent<ROSConnector>().AddDialogMessage("START");
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
        RosConnectorGameObject.GetComponent<ROSConnector>().AddDialogMessage("RESET Arrow");
    }

    // ------------------------------------------------------------
    public void CalibrationFlagButton()
    {
        if (calibr_flag == 0)
        {
            publishFlagFlag = true;
        }
        RosConnectorGameObject.GetComponent<ROSConnector>().AddDialogMessage("Calibrating");
    }

    public void WorldAnchorButton()
    {
        //ワールド座標のアンカーを更新
        ParentObject.transform.position = ImageTarget.transform.position;
        ParentObject.transform.rotation = ImageTarget.transform.rotation;
        RosConnectorGameObject.GetComponent<ROSConnector>().AddDialogMessage("RESET World Anchor");
    }

}




