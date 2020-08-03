using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreateCustomMapStyle : MonoBehaviour
{
    //string uu = "https://mt{rnd0-3}.googleapis.com/vt/lyrs=y&hl={lng}&x={x}&y={y}&z={zoom}";
   
    
    public string style1 = "http://127.0.0.1:8001/roadmap?{zoom}{x}{y}";
    //public string style1 = "https://127.0.0.1:8000/v4/mapbox.satellite/{zoom}/{x}/{y}.png?access_token=";
    public string style2 = "https://a.tiles.mapbox.com/v4/mapbox.streets/{zoom}/{x}/{y}.png?access_token=";
    public string mapboxAccessToken;

    private bool useFirstStyle = true;

    private void OnGUI()
    {
        //if (GUILayout.Button("Change Style"))
        //{
        //    useFirstStyle = !useFirstStyle;

        //    // Switch map type
        //    OnlineMaps.instance.mapType = "myprovider.style1";// + (useFirstStyle ? "1" : "2");
        //}
    }

    private void Start()
    {

        // style1 = @"http://127.0.0.1:8001/roadmap?{zoom}{x}{y}";

        ConfigFile cf = jsonConf.configFile;
        if (cf != null)
        {
            style1 = cf.MapHttpServer;
        }
        Debug.Log(style1);
        OnlineMapsProvider.MapType mType = new OnlineMapsProvider.MapType("style1");
        mType.urlWithLabels = style1;
        // Create a new provider
        var provider = OnlineMapsProvider.Create("myprovider");
        provider.AppendTypes(mType);
        //provider .AppendTypes(
        //    // Create a new map types
        //    new OnlineMapsProvider.MapType("style1") { urlWithLabels = style1 + mapboxAccessToken, }
        //   // new OnlineMapsProvider.MapType("style2") { urlWithLabels = style2 + mapboxAccessToken, }
        //);

        // Select map type
        OnlineMaps.instance.mapType = "myprovider.style1";
    }
}

