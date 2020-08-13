using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using UnityEngine;
using System;
using System.Text;



public class MyTcpClient : MonoBehaviour
{
    class MyObjectState
    {
        public int messageId;
        public int bodyLen;
        public byte[] headData;
        public byte[] bodyData;
    }
    TcpClient client;
    NetworkStream stream;
    void initSocket()
    {
        ConfigFile cf = jsonConf.configFile;
        string[] strs = cf.TcpClientConf.Split(':');
        client = new TcpClient();
        client.BeginConnect(strs[0], int.Parse(strs[1]), new AsyncCallback(ConnectCallback), client);
    }

    void ConnectCallback(IAsyncResult ar)
    {
        TcpClient t = (TcpClient) ar.AsyncState;

        try
        {
            if (t.Connected)
            {
                t.EndConnect(ar);
                Debug.Log("连接tcp服务器成功");
                stream = t.GetStream();
                readHeadFromTcpServer();
            }
            else
            {
                t.EndConnect(ar);
                Debug.Log("连接tcp服务器失败");
            }
        }
        catch (Exception e)
        {

            Debug.Log(e.Message);
        }
    }
    void readHeadFromTcpServer()
    {
        MyObjectState state = new MyObjectState();
        state.headData = new byte[6];
        stream.BeginRead(state.headData, 0, state.headData.Length, new AsyncCallback(asyncReadHeadCallBack), state);
    }
    void asyncReadHeadCallBack(IAsyncResult ar)
    {
        MyObjectState state = (MyObjectState)ar.AsyncState;
        int len = stream.EndRead(ar);
        state.messageId = BitConverter.ToInt16(state.headData, 0);
        state.bodyLen = BitConverter.ToInt32(state.headData, 2);

        Debug.Log("messageId = " + state.messageId);
        Debug.Log(BitConverter.ToString(state.headData));
        Debug.Log("bodyLen = " + state.bodyLen);
        if (ar.IsCompleted)
        {
            readBodyFromTcpServer(state);
        }
    }

    void readBodyFromTcpServer(MyObjectState state)
    {
        state.bodyData = new byte[state.bodyLen];
        stream.BeginRead(state.bodyData, 0, state.bodyLen, new AsyncCallback(asyncReadBodyCallBack), state);
    }
    void asyncReadBodyCallBack(IAsyncResult ar)
    {
        int len = stream.EndRead(ar);
        MyObjectState state = (MyObjectState)ar.AsyncState;
        if (ar.IsCompleted)
        {
            readHeadFromTcpServer();
        }

        //todo
       
        //Debug.Log(Encoding.UTF8.GetString(state.bodyData));
        GpsInfo info = JsonUtility.FromJson<GpsInfo>(Encoding.UTF8.GetString(state.bodyData));
        GPSInfosManager.Insatance.needUpdateMarkerList.Add(new MarkerInfo(info.Lng, info.Lat, info.Id, info.T));
    }
    void sendToTcpServer(byte[] data)
    {
        if (client.Connected)
        {
            stream.Write(data, 0, data.Length);
        }
    }

    void GetPlayBackPositions()
    {

    }
    
    // Start is called before the first frame update
    void Start()
    {
        initSocket();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

public class GpsInfo
{
    public int Id;
    public double Lng;
    public double Lat;
    public string T;
}

public class PlayBackPositions
{
    public int GpsId;
    public string TBegin;
    public string TEnd;
    public GpsInfo[] Positions;


}