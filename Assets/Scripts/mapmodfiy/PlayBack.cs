using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

public class PlayBack : MonoBehaviour
{
   
    private List<PlayBackExecutor> executorList = new List<PlayBackExecutor>();
    private bool needReDraw = false;
   // Text infoText;
    private void Start()
    {
       
       
    }
    public void DoPlayBack(MyObjectState state)
    {
        Debug.Log("bodyData 开始转 string");
        string jsonStr = Encoding.UTF8.GetString(state.bodyData);
        Debug.Log(jsonStr);

        PlayBackPositions pbs = JsonConvert.DeserializeObject<PlayBackPositions>(jsonStr);

        Debug.Log("json 反序列化完成，得到实例");

        List<OnlineMapsVector2d> route = new List<OnlineMapsVector2d>();

        //Debug.Log("定位点数量："+pbs.Positions.Length);
        if (pbs.Positions!=null)
        {
            for (int i = 0; i < pbs.Positions.Length; i++)
            {

                Vector2 v = new Vector2((float)pbs.Positions[i].Lng, (float)pbs.Positions[i].Lat);
                route.Add(v);
            }
        }
      

        if (route.Count > 1)
        {

            Application.targetFrameRate = 60;//FPS调大，使动画流畅
            CreatOneRoute(new PlayBackExecutor(route, pbs.GpsId + " 轨迹"));

            UIstart u = gameObject.GetComponent<UIstart>();
            u.ClosePlayBackPannelAction(); //关闭路径回放UIPannel
        }
        else
        {
            GameObject go = GameObject.Find("Text_Message");
            if (go != null)
            {
                Text infoText = go.GetComponent<Text>();
                infoText.text = "坐标太少（小于2），无法生成路径";

            }
           
           
        }
    }
    public void CreatOneRoute(PlayBackExecutor exetor)
    {
        executorList.Add(exetor);
       
    }
    
    public void Clear()
    {
        // OnlineMapsMarkerManager.instance.Remove(marker);

        foreach (var item in executorList)
        {
            item.Clear();
        }
         executorList.Clear();
         OnlineMapsDrawingElementManager.instance.RemoveAll();
    }
    // Update is called once per frame
    void Update()
    {
        needReDraw = false;
        foreach (var item in executorList)
        {
            item.update();
            if (item.needReDraw)
            {
                needReDraw = true;
            }
        }
        if (needReDraw)
        {
            OnlineMaps.instance.Redraw();
        }
        //if (!isRun || routeIndex>routeLen-1)
        //{
        //   // OnlineMapsDrawingElementManager.instance.RemoveAll();
        //    return;
        //}

        //angle += Time.deltaTime;

        //if (angle>=perTime)
        //{
        //    angle = 0;
        //    routeIndex++;
        //    if (routeIndex<routeLen)
        //    {
        //        fromPosition = points[routeIndex-1];
        //        toPosition = points[routeIndex];

        //        OnlineMapsDrawingLine route1 = new OnlineMapsDrawingLine(new OnlineMapsVector2d []{ points[routeIndex-1],points[routeIndex], });

        //        route1.color = Color.yellow;
        //        route1.width = 3f;
        //        OnlineMapsDrawingElementManager.AddItem(route1);


        //    }
        //    return;
        //}
        //marker.position = Vector2.Lerp(fromPosition, toPosition, angle/perTime);
        //// Marks the map should be redrawn.
        //// Map is not redrawn immediately. It will take some time.
        //OnlineMaps.instance.Redraw();
    }
}

public class PlayBackExecutor
{
    public List<OnlineMapsVector2d> points;

    public float perTime = 2;
    public bool needReDraw = false;

    public bool isRun = false;

    private float angle = 0;

    private int routeLen = 0;

    private int routeIndex = 0;

    private OnlineMapsMarker marker = null;

    private Vector2 fromPosition;

    private Vector2 toPosition;
    public PlayBackExecutor(List<OnlineMapsVector2d> _points,string label)
    {
        points = _points;
        OnlineMaps.instance.SetPosition(points[0].x, points[0].y);

        routeLen = points.Count;
        fromPosition = points[0];
        toPosition = points[1];
        routeIndex = 1;
        // Draw the route.
        OnlineMapsDrawingLine route = new OnlineMapsDrawingLine(points);
        route.color = Color.yellow;
        route.width = 1;

        OnlineMapsDrawingElementManager.AddItem(route);

        marker = OnlineMapsMarkerManager.CreateItem(points[0], label);

        isRun = true;

    }
    public void update()
    {
        if (!isRun || routeIndex > routeLen - 1)
        {
            needReDraw = false;
            // OnlineMapsDrawingElementManager.instance.RemoveAll();
            return;
        }

        angle += Time.deltaTime;

        if (angle >= perTime)
        {
            angle = 0;
            routeIndex++;
            if (routeIndex < routeLen)
            {
                fromPosition = points[routeIndex - 1];
                toPosition = points[routeIndex];

                OnlineMapsDrawingLine route1 = new OnlineMapsDrawingLine(new OnlineMapsVector2d[] { points[routeIndex - 1], points[routeIndex], });

                route1.color = Color.yellow;
                route1.width = 3f;
                OnlineMapsDrawingElementManager.AddItem(route1);


            }
            return;
        }
        marker.position = Vector2.Lerp(fromPosition, toPosition, angle / perTime);

        needReDraw = true;
        // Marks the map should be redrawn.
        // Map is not redrawn immediately. It will take some time.

       // OnlineMaps.instance.Redraw();
    }
    public void Clear()
    {
        if (marker!=null)
        {
            OnlineMapsMarkerManager.instance.Remove(marker);
        }
        
    }
   
    
}