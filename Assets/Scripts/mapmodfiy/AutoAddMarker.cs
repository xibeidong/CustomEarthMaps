using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AutoAddMarker : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }
    private void updateMarker(double lng, double lat, int gpsID, string timeStr)
    {
        Dictionary<int, OnlineMapsMarker> markersDict = GPSInfosManager.Insatance.markersDict;
        if (markersDict.ContainsKey(gpsID))
        {
            OnlineMapsMarker m = markersDict[gpsID];
            m.SetPosition(lng, lat);
            m.label = gpsID + " " + timeStr;

        }
        else
        {
            OnlineMapsMarker m = OnlineMapsMarkerManager.CreateItem(lng, lat, gpsID + " " + timeStr);
            markersDict.Add(gpsID, m);
        }
    }
    // Update is called once per frame
    private float t = 0;
    void Update()
    {
        if (GPSInfosManager.Insatance.needUpdateMarkerList.Count>0)
        {
            foreach (var item in GPSInfosManager.Insatance.needUpdateMarkerList)
            {
                updateMarker(item.lng, item.lat, item.gpsID, item.t);
            }
            GPSInfosManager.Insatance.needUpdateMarkerList.Clear();
        }

        t += Time.deltaTime;
        if (t>1800) //半小时激活一次mysql，防止休眠
        {
            t = 0;
            SqlHelper.Insatance.ActiveSQL();
        }
        
    }
}
