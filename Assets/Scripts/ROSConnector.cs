using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using RBS;
using System;

public class ROSConnector : MonoBehaviour
{
    //-------------------------------------------------------------
    // Robot Network Settings
    //public string InitialRobotAddress;
    //public string InitialRobotConnectionPort;

    // InputField
    public InputField robotAddress;
    public InputField robotConnectionPort;

    // Text
    public Text connectButtonText;

    // Debug Message Area
    public Text DialogText;

    // Update()内部で一定秒数間隔で処理するための変数
    private float timeleft;
    //-------------------------------------------------------------


    private void Start()
    {
        timeleft = 0;

        // プレイスホルダーの値をTextフィールドにコピー
        robotAddress.text = robotAddress.placeholder.GetComponent<Text>().text;
        robotConnectionPort.text = robotConnectionPort.placeholder.GetComponent<Text>().text;
    }

    private void Update()
    {
        timeleft -= Time.deltaTime;
        if (timeleft <= 0.0)
        {
            // ターゲットに接続できるかpingを定期的に送信して確認
            if (RBSocket.Instance.IsConnected && !RBSocket.Instance.IsConnectable)
            {
                AddDialogMessage("Ping Failed.");
            }
            timeleft = 10.0f;
        }
    }

    // ボタンから接続と切断を操作
    public void ConnectButton()
    {
        if (RBSocket.Instance.IsConnected)
        {
            RBSocket.Instance.Disconnect();
            connectButtonText.text = "Connect";
            AddDialogMessage("Connection Closed.");
        }
        else
        {
            RBSocket.Instance.IPAddress = robotAddress.text;
            RBSocket.Instance.Port = robotConnectionPort.text;
            Debug.Log("Websocket Connecting to " + RBSocket.Instance.IPAddress + " on Port " + RBSocket.Instance.Port);
            RBSocket.Instance.Connect();
            // Websocketが接続できているか確認する
            if (RBSocket.Instance.IsConnected)
            {
                AddDialogMessage("Connection Successful.");
                connectButtonText.text = "Disconnect";
            }
            else
            {
                AddDialogMessage("Connection Error.");
            }

        }
    }

    public void AddDialogMessage(string m)
    {
        string text = "【" + System.DateTime.Now.ToString() + " " + RBSocket.Instance.IPAddress + ":" + RBSocket.Instance.Port + "】" + m + "\n";
        Debug.Log(text);
        DialogText.text = text + DialogText.text;
    }

}
