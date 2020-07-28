using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Marker3D : MonoBehaviour
{
    /// <summary>
    /// Prefab of 3D marker
    /// </summary>
    public GameObject markerPrefab;

    private void Start()
    {
        // Get instance of OnlineMapsControlBase3D (Texture or Tileset)
        OnlineMapsControlBase3D control = OnlineMapsControlBase3D.instance;
        
        if (control == null)
        {
            Debug.LogError("You must use the 3D control (Texture or Tileset).");
            return;
        }

        // Subscribe to the click event.
        OnlineMapsControlBase.instance.OnMapDoubleClick += OnMapClick;
        
    }

    private void OnMapClick()
    {
        // Get the coordinates under the cursor.
        double lng, lat;
        OnlineMapsControlBase.instance.GetCoords(out lng, out lat);

        // Create 3D marker
        OnlineMapsMarker3D marker3D = OnlineMapsMarker3DManager.CreateItem(lng,lat, markerPrefab);
        marker3D.scale =20f;
        // Specifies that marker should be shown only when zoom from 1 to 10.
        marker3D.range = new OnlineMapsRange(13, 20);
        
    }

}
