using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DrawLine : MonoBehaviour
{


    // private OnlineMapsVector2d[] points;
    private List<OnlineMapsVector2d> points = new List<OnlineMapsVector2d>();
    private int pointIndex = 0;
    private double progress;
    private void Start()
    {
        points.Add(new OnlineMapsVector2d(120.254126, 36.023108));
        points.Add(new OnlineMapsVector2d(120.264126, 36.023108));
        points.Add(new OnlineMapsVector2d(120.254126, 36.033108));
        points.Add(new OnlineMapsVector2d(120.274126, 36.023108));

    
       
        // Draw the route.
        OnlineMapsDrawingLine route = new OnlineMapsDrawingLine(points);
      
        OnlineMapsDrawingElementManager.AddItem(route);

        route.color = Color.yellow;
        route.width = 3;
        //OnlineMapsDrawingElementManager.instance.Remove(route);
    }


}
