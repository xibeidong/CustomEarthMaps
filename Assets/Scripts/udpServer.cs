
using UnityEngine;
using System.Collections;
using System;
using System.Net;
using System.Text;
using System.Net.Sockets;
using System.Threading;

public class udpServer : MonoBehaviour
{
    SocketUdpServer udp_Server;
    private void Start()
    {
        ConfigFile cf = jsonConf.configFile;
        if (cf!=null)
        {
            string str = cf.UdpServer; // "UdpServer": "192.168.10.225:8888",
            string[] strs = str.Split(':');
            init(strs[0],int.Parse(strs[1]));

        }

        
    }

    private void init(string ip ,int port)
    {
        // udp_Server = new SocketUdpServer(new IPEndPoint( IPAddress.Parse( "192.168.10.225"), 8888));
        Debug.Log($"udpserver:{ip}:{port}");
        try
        {
            udp_Server = new SocketUdpServer(new IPEndPoint(IPAddress.Parse(ip), port));
            udp_Server.Start();
        }
        catch (Exception e)
        {

            Debug.Log($"udpserver启动失败：{e.Message} ");
        }
       

    }
    private void OnDisable()
    {
        udp_Server.Stop();
        udp_Server = null;
    }
}

public interface ISocketUdpServer
{
    void Start();
    void Stop();
    int SendData(byte[] data, IPEndPoint remoteEndPoint);

    event ReceiveDataHandler ReceivedDataEvent;
    event ErrorHandler ErrorEvent;
}

public delegate void ReceiveDataHandler(SocketState state);

public delegate void OnlineChangeHandler(int onlines, EndPoint client);

public delegate void ErrorHandler(string error, EndPoint client);

public class SocketUdpServer : ISocketUdpServer
{
    private readonly Socket _udpSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
    private bool _isListening;

    public SocketUdpServer(IPEndPoint localPoint)
    {
        _udpSocket.ReceiveBufferSize = 1024 * 8;
        _udpSocket.Bind(localPoint);
    }

    public Socket GetUdpSocket()
    {
        return _udpSocket;
    }

    public void Start()
    {
        _isListening = true;
        BeginReceive();
    }

    public void Stop()
    {
        _isListening = false;
        _udpSocket.Close();
    }

    public int SendData(byte[] data, IPEndPoint remoteEndPoint)
    {
        return _udpSocket.SendTo(data, remoteEndPoint);
    }

    public event ReceiveDataHandler ReceivedDataEvent;

    public event ErrorHandler ErrorEvent;

    private void BeginReceive()
    {
        if (_isListening)
        {
            SocketState state = new SocketState { Self = _udpSocket };
            _udpSocket.BeginReceiveFrom(state.Buffer, 0, state.Buffer.Length, SocketFlags.None,
                ref state.RemotePoint, ReceiveCallback, state);
        }
    }
    private void UpdateMarker(double lat,double lng,int gpsID,string timeStr)
    {

    }
    private void ReceiveCallback(IAsyncResult ar)
    {
        var state = ar.AsyncState as SocketState;
        try
        {
            if (state != null)
            {
                int receiveLen = state.Self.EndReceiveFrom(ar, ref state.RemotePoint);
                if (receiveLen > 0)
                {
                    byte[] receivedData = new byte[receiveLen];
                    Array.Copy(state.Buffer, 0, receivedData, 0, receiveLen);
                    state.Buffer = receivedData;

                    //string str = System.Text.Encoding.UTF8.GetString(receivedData);
                    //Debug.Log(str);
                    //SqlHelper.Insatance.GetSqlConn();

                    Debug.Log($"接收数据长度：{receivedData.Length}");

                    Debug.Log(BitConverter.ToString(receivedData));
                    GPSInfosManager.Insatance.praseMessage(receivedData);

                    state.ReceivedTime = DateTime.Now;
                    ReceivedDataEvent?.Invoke(state);
                }
            }
        }
        catch (Exception error)
        {
            ErrorEvent?.Invoke(error.Message, state?.RemotePoint);
        }
        finally
        {
            if (state != null) BeginReceive();
        }
    }

}

public class SocketState
{
    public byte[] Buffer = new byte[1024 * 8];
    public Socket Self;
    public EndPoint RemotePoint = new IPEndPoint(IPAddress.Any, 0);
    public DateTime ReceivedTime { get; set; }
}