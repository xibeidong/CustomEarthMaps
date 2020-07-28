using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GPSInfosManager
{
    private static GPSInfosManager _instance = new GPSInfosManager();
    public Dictionary<int, OnlineMapsMarker> markersDict = new Dictionary<int, OnlineMapsMarker>();
    public List<MarkerInfo> markerList = new List<MarkerInfo>();
    public static GPSInfosManager Insatance
    {
        get
        {
            return _instance;
        }
    }
  

    private void insert2mysql(double lng, double lat, int gpsID, string timeStr)
    {
        string str = string.Format("insert into gpsinfo values({0},{1},{2},'{3}')", gpsID, lng, lat, timeStr);
        int ret = SqlHelper.Insatance.DoInsert(str);
        if (ret == 1)
        {
            Debug.Log("OK: " + str);
        }
    }
    public void praseMessage(byte[] data)
    {
       
        if (data.Length>=44)
        {
            if (data[1] == 0x00 && data[0] == 0xAA) //标志头
            {
                if (data[3] == 0x00 && data[2] == 0xCC) //0xCC 表示是实时定位数据
                {
                    #region GPSID
                    string idStr = string.Empty;
                    idStr += data[4];
                    for (int i = 5; i < 10; i++)
                    {
                        if (data[i] < 10)
                        {
                            idStr += "0" + data[i];
                        }
                        else
                        {
                            idStr += data[i];
                        }
                    }
                    int gpsID = int.Parse(idStr);
                    #endregion
                    double lng =  BitConverter.ToDouble(data, 10);
                    double lat = BitConverter.ToDouble(data, 18);
                    string t = System.DateTime.Now.ToString();

                    Debug.Log($"解析数据 lng={lng},lat={lat},gpsID={gpsID}");

                    insert2mysql(lng, lat, gpsID, t);

                    markerList.Add(new MarkerInfo(lng,lat,gpsID,t));
                   
                   

                   
                }
            }
        }
    }
}

public class MarkerInfo
{
     public  double lng, lat;
    public int gpsID;
        public string t;
    public MarkerInfo(double _lng,double _lat,int _gpsID,string _t)
    {
        lng = _lng;
        lat = _lat;
        t = _t;
        gpsID = _gpsID;
    }
}