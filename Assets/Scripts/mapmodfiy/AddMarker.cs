using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AddMarker : MonoBehaviour
{
    private void Start()
    {
        OnlineMaps map = OnlineMaps.instance;

        // Add OnClick events to static markers
        foreach (OnlineMapsMarker marker in OnlineMapsMarkerManager.instance)
        {
            marker.OnClick += OnMarkerClick;
        }

        // Subscribe to the click event.
        OnlineMapsControlBase.instance.OnMapClick += OnMapClick;

    }
    private void OnMapClick()
    {
        // Get the coordinates under the cursor.
        double lng, lat;
        OnlineMapsControlBase.instance.GetCoords(out lng, out lat);

        // Create a label for the marker.
        string label = "Marker " + (OnlineMapsMarkerManager.CountItems + 1);

        // Create a new marker.
        OnlineMapsMarker m = OnlineMapsMarkerManager.CreateItem(lng, lat, label);
       
    }
    private void OnMarkerClick(OnlineMapsMarkerBase marker)
    {
        // Show in console marker label.
       // Debug.Log(marker.label);
        double lng, lat;
        marker.GetPosition(out lng,out lat);
       // Debug.Log($"{lng},{lat}");
       
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            // Screen coordinate of the cursor.
            Vector3 mousePosition = Input.mousePosition;

            Vector2 v2 = new Vector2(mousePosition.x, mousePosition.y);
            // Converts the screen coordinates to geographic.
            // Vector2 mouseGeoLocation = OnlineMapsControlBase.instance.GetCoords(v2);

            double lng, lat;
            OnlineMapsControlBase.instance.GetCoords(v2, out lng, out lat);
            // Showing geographical coordinates in the console.
           // Debug.Log(mousePosition);
          //  Debug.Log($"{lng},{lat}");
        }
      
    }
}
