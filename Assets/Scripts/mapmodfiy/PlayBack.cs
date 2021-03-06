﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayBack : MonoBehaviour
{
    private List<PlayBackExecutor> executorList = new List<PlayBackExecutor>();
    private bool needReDraw = false;
   

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