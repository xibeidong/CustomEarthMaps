using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FPS : MonoBehaviour
{
    public int fpsTarget = 10;
    public float updateInterval = 0.5f;
    private float lastInterval;
    private int frames = 0;
    private float fps;
    private void Awake()
    {
        //设置帧率
        Application.targetFrameRate = fpsTarget;
        lastInterval = Time.realtimeSinceStartup;
        frames = 0;
    }
    // Start is called before the first frame update
    void Start()
    {
        OnlineMaps.instance.zoomRange = new OnlineMapsRange(10, 18);
        OnlineMaps.instance.positionRange = new OnlineMapsPositionRange(35.505408770431f,119.66170433021f, 36.138976000377f, 120.4307656071f);
    }

    // Update is called once per frame
    void Update()
    {
        ++frames;
        float timeNow = Time.realtimeSinceStartup;
        if (timeNow >= lastInterval + updateInterval)
        {
            fps = frames / (timeNow - lastInterval);
            frames = 0;
            lastInterval = timeNow;
        }
    }

    void OnGUI()
    {
        GUIStyle s = new GUIStyle();
        s.normal.textColor = Color.black;
        s.fontSize = 20;
        GUI.Label(new Rect(10, 10, 100, 30), "FPS:"+ fps.ToString(),s);
    }
   
}
