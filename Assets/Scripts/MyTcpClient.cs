using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using UnityEngine;
using System;
using System.Text;
using Newtonsoft.Json;

public class MyTcpClient : MonoBehaviour
{
   
    TcpClient client;
    NetworkStream stream;
    

    Queue<MyObjectState> needResolveMessageQueue = new Queue<MyObjectState>();
    void Start()
    {
       
        initSocket();
    }

    // Update is called once per frame
    void Update()
    {
        if (needResolveMessageQueue.Count>0)
        {
            MyObjectState state = needResolveMessageQueue.Dequeue();
            ResolveBodyData(state);
        }
    }
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
        Debug.Log("bodyLen = " + state.bodyLen + " 即将接收bodyData");
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
        Debug.Log("dataBody 接收完毕，开始解析");

        // ResolveBodyData(state);
        needResolveMessageQueue.Enqueue(state);
       
    }
    void sendToTcpServer(byte[] data)
    {
        if (client.Connected)
        {
            stream.Write(data, 0, data.Length);
        }
    }

    public void SendToTcpServer( int messageId,byte[] bodyData)
    {
        List<byte> dataList = new List<byte>();
        dataList.AddRange(new byte[] 
        {
            (byte)((messageId>>8)&0xff),
            (byte)(messageId&0xff)
        });

        int bodyLen = bodyData.Length;
        dataList.AddRange(new byte[]
        {
            (byte)((bodyLen>>24)&0xff),
            (byte)((bodyLen>>16)&0xff),
            (byte)((bodyLen>>8)&0xff),
            (byte)(bodyLen&0xff)
        });

        dataList.AddRange(bodyData);
        sendToTcpServer(dataList.ToArray());
    }

    void ResolveBodyData(MyObjectState state)
    {
        switch (state.messageId)
        {
            case 97:
                GPSPositioss gp = JsonConvert.DeserializeObject<GPSPositioss>(Encoding.UTF8.GetString(state.bodyData));
                if (gp.Positions!=null)
                {
                    foreach (var item in gp.Positions)
                    {
                        GPSInfosManager.Insatance.needUpdateMarkerList.Add(new MarkerInfo(item.Lng, item.Lat, item.Id, item.T));
                    }
                }
                
                break;
            case 99: //实时定位信息
                GpsInfo info = JsonUtility.FromJson<GpsInfo>(Encoding.UTF8.GetString(state.bodyData));
                GPSInfosManager.Insatance.needUpdateMarkerList.Add(new MarkerInfo(info.Lng, info.Lat, info.Id, info.T));
                break;
            case 101: //Heart
                break;
            case 103://路径回放

                PlayBack mPlayBack = gameObject.GetComponent<PlayBack>();
                if (mPlayBack!=null)
                {
                    Debug.Log("接送到服务器路径回放消息体，开始执行路径回放任务");
                    mPlayBack.DoPlayBack(state);
                }
                else
                {
                    Debug.Log("PlayBack组件is null");
                }
                break;
            default:
                break;
        }


    }
  
    // Start is called before the first frame update
    
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

public class GPSPositioss
{
    public GpsInfo[] Positions;
}

public class MyObjectState
{
    public int messageId; //uint16
    public int bodyLen;
    public byte[] headData; //bodyData的长度，int32
    public byte[] bodyData; //json序列化后的bytes
}