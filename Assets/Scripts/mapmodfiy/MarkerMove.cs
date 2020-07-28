using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MarkerMove : MonoBehaviour
{
    // Move time
    public float time = 10;

    private OnlineMapsMarker marker;

    private Vector2 fromPosition;
    private Vector2 toPosition;

    // Relative position (0-1) between from and to
    private float angle = 0.5f;

    // Move direction
    private int direction = 1;

    private void Start()
    {
        OnlineMaps map = OnlineMaps.instance;
        marker = OnlineMapsMarkerManager.CreateItem(map.position);
        fromPosition = map.topLeftPosition;
        toPosition = map.bottomRightPosition;
        Debug.Log(fromPosition);
        Debug.Log(toPosition);
    }

    private void Update()
    {
        angle += Time.deltaTime / time * direction;
        if (angle > 1)
        {
            angle = 2 - angle;
            direction = -1;
        }
        else if (angle < 0)
        {
            angle *= -1;
            direction = 1;
        }

        // Set new position
        marker.position = Vector2.Lerp(fromPosition, toPosition, angle);
        // Marks the map should be redrawn.
        // Map is not redrawn immediately. It will take some time.
        OnlineMaps.instance.Redraw();
    }
}
