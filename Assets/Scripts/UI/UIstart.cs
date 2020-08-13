using MySql.Data.MySqlClient;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class UIstart : MonoBehaviour
{
    
    // Start is called before the first frame update
    
    private Button btn_full = null;
    private Button btn_playback = null;
    private Button btn_play = null;
    private Button btn_canel = null;
    private Button btn_clear = null;
    private InputField input_id = null;
    private InputField input_start_time = null;
    private InputField input_end_time = null;
    private CanvasGroup canvasGroup = null;
    private Dropdown map_choose = null;
    void Start()
    {
        //隐藏 panel_playback
        // GameObject g = GameObject.Find("Panel_playback");
        GameObject g = InitalPrefabs("Prefabs/Panel_playback");
        if (g!=null)
        {
            canvasGroup = g.transform.GetComponent<CanvasGroup>();
            canvasGroup.alpha = 0;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
        }
        else
        {
            Debug.Log("Panel_playback 物体获取失败");
        }

        GameObject mainMenu = InitalPrefabs("Prefabs/MainMenu");
       
        map_choose = GameObject.Find("Dropdown_MapChoose").GetComponent<Dropdown>();
        map_choose.onValueChanged.AddListener(mapChooseValueChange);

        int v = PlayerPrefs.GetInt("mapType");
        map_choose.value = v;
        loadMapWay(v);

        btn_full = GameObject.Find("btn_full").GetComponent<Button>();
        btn_full.onClick.AddListener(OnClickFull);

        btn_playback = GameObject.Find("btn_playback").GetComponent<Button>();
        btn_playback.onClick.AddListener(OnClickPlayback);

        btn_canel = GameObject.Find("btn_cancel").GetComponent<Button>();
        btn_canel.onClick.AddListener(OnClickCanel);

        btn_play = GameObject.Find("btn_play").GetComponent<Button>();
        btn_play.onClick.AddListener(OnclickPlay);

        btn_clear = GameObject.Find("btn_clear").GetComponent<Button>();
        btn_clear.onClick.AddListener(OnClickClear);

        input_id = GameObject.Find("Input_id").GetComponent<InputField>();
        input_id.text = "8888";

        input_start_time = GameObject.Find("Input_start_time").GetComponent<InputField>();
        input_start_time.text = System.DateTime.Now.ToString();

        input_end_time = GameObject.Find("Input_end_time").GetComponent<InputField>();
        input_end_time.text = System.DateTime.Now.ToString();
    }

    private GameObject InitalPrefabs(string path)
    {
        GameObject prefab = Resources.Load(path) as GameObject;
        GameObject g = GameObject.Instantiate<GameObject>(prefab);
        g.transform.parent = transform;
        g.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 0);
        return g;
    }

    private void mapChooseValueChange(int v)
    {
        //Debug.Log("DropDown value = " + v);
        //string str = map_choose.options[v].text;
        //Debug.Log(str);
        loadMapWay(v);
        PlayerPrefs.SetInt("mapType", v);
    }

    private void loadMapWay(int v)
    {
        GameObject g = GameObject.Find("start");
        if (g == null)
        {
            Debug.Log("loadMapWay: 没有找到名称start的物体");
            return;
        }
        if (v==0) //离线地图(自定义地图服务器)
        {
           CreateCustomMapStyle cm =  g.GetComponent<CreateCustomMapStyle>();
            if (cm==null)
            {
                g.AddComponent<CreateCustomMapStyle>();
            }
        }
        else if (v==1)
        {
            CreateCustomMapStyle cm = g.GetComponent<CreateCustomMapStyle>();
            if (cm != null)
            {
                Destroy(cm);
            }
        }
    }
    private void OnClickFull()
    {
        //Debug.Log("点击了关闭按钮");
        //Application.Quit();

        if (Screen.fullScreen == true)
        {
            Screen.SetResolution(800, 600, true);
            Screen.fullScreen = false;  //退出全屏  
        }
        else
        {
            Screen.SetResolution(800, 600, true);
            Screen.fullScreen = true;  //设置成全屏,
        }
       

    }

    private void OnClickClear()
    {
        Application.targetFrameRate = 15;//降低FPS，CPU
        PlayBack pb = gameObject.GetComponent<PlayBack>();
        if (pb!=null)
        {
            pb.Clear();
           // Destroy(pb);
        }
        
    }

    private void OnClickPlayback()
    {
        //OnlineMapsMarker m = OnlineMapsMarkerManager.CreateItem(120.254126, 36.023108, "123 2020/7/25 14:46:32");
        Debug.Log("点击了 路径回访按钮");
        CanvasGroup c = canvasGroup;
        c.alpha = 1;
        c.interactable = true;
        c.blocksRaycasts = true;

        GameObject.Find("Text_Message").GetComponent<Text>().text = "";
    }
    private void OnClickCanel()
    {
       // double[] xy = wgs2gcj(36.0266590118408, 120.190665721893);
        //OnlineMapsMarkerManager.CreateItem(xy[0],xy[1],"123");

        CanvasGroup c = canvasGroup;
        c.alpha = 0;
        c.interactable = false;
        c.blocksRaycasts = false;
    }
    /**
	 * 
	 * @Title: wgs2gcj 
	 * @Description: 84  to  火星坐标系  (GCJ-02)  World  Geodetic  System  ==>  Mars  Geodetic  System 
	 * @param lat
	 * @param lon
	 * @return
	 */
    static double pi = 3.14159265358979324;
    static double a = 6378245.0;
    static double ee = 0.00669342162296594323;
    public static double[] wgs2gcj(double lat, double lon)
    {
        double dLat = transformLat(lon - 105.0, lat - 35.0);
        double dLon = transformLon(lon - 105.0, lat - 35.0);
        double radLat = lat / 180.0 * pi;
        double magic = Math.Sin(radLat);
        magic = 1 - ee * magic * magic;
        double sqrtMagic = Math.Sqrt(magic);
        dLat = (dLat * 180.0) / ((a * (1 - ee)) / (magic * sqrtMagic) * pi);
        dLon = (dLon * 180.0) / (a / sqrtMagic * Math.Cos(radLat) * pi);
        double mgLat = lat + dLat;
        double mgLon = lon + dLon;
        double[] loc = { mgLat, mgLon };
        return loc;
    }

    private static double transformLat(double lat, double lon)
    {
        double ret = -100.0 + 2.0 * lat + 3.0 * lon + 0.2 * lon * lon + 0.1 * lat * lon + 0.2 * Math.Sqrt(Math.Abs(lat));
        ret += (20.0 * Math.Sin(6.0 * lat * pi) + 20.0 * Math.Sin(2.0 * lat * pi)) * 2.0 / 3.0;
        ret += (20.0 * Math.Sin(lon * pi) + 40.0 * Math.Sin(lon / 3.0 * pi)) * 2.0 / 3.0;
        ret += (160.0 * Math.Sin(lon / 12.0 * pi) + 320 * Math.Sin(lon * pi / 30.0)) * 2.0 / 3.0;
        return ret;
    }

    private static double transformLon(double lat, double lon)
    {
        double ret = 300.0 + lat + 2.0 * lon + 0.1 * lat * lat + 0.1 * lat * lon + 0.1 * Math.Sqrt(Math.Abs(lat));
        ret += (20.0 * Math.Sin(6.0 * lat * pi) + 20.0 * Math.Sin(2.0 * lat * pi)) * 2.0 / 3.0;
        ret += (20.0 * Math.Sin(lat * pi) + 40.0 * Math.Sin(lat / 3.0 * pi)) * 2.0 / 3.0;
        ret += (150.0 * Math.Sin(lat / 12.0 * pi) + 300.0 * Math.Sin(lat / 30.0 * pi)) * 2.0 / 3.0;
        return ret;
    }
    private void OnclickPlay()
    {

      //  List<OnlineMapsVector2d> route = new List<OnlineMapsVector2d>();
        // ArrayList markerRouteList = new ArrayList();
        string gpsID = input_id.text;
        string time_start = input_start_time.text;
        string time_end = input_end_time.text;
      
        //MyTcpClient client
        

        

    }
    private void OnclickPlay_bak()
    {
        
        List<OnlineMapsVector2d> route = new List<OnlineMapsVector2d>();
       // ArrayList markerRouteList = new ArrayList();
        string gpsID = input_id.text;
        string time_start = input_start_time.text;
        string time_end = input_end_time.text;
        string sql = $"select * from gpsinfo where gpsid = {gpsID} and t<'{time_end}' and t>'{time_start}' order by t asc";
        MySqlDataReader dr = SqlHelper.Insatance.DoGetReader(sql);
        if (dr!=null)
        {
            while (dr.Read())
            {
                Vector2 v = new Vector2(Convert.ToSingle(dr["lng"].ToString()), Convert.ToSingle(dr["lat"].ToString()));
                route.Add(v);
            }
        }
        dr.Close();
        Debug.Log("路径节点数量："+ route.Count);

        if (route.Count>1)
        {

            PlayBack pb = gameObject.GetComponent<PlayBack>();
            Application.targetFrameRate = 70;//FPS调大，使动画流畅
            pb.CreatOneRoute(new PlayBackExecutor(route,gpsID + " 轨迹"));

            OnClickCanel();
        }
        else
        {
            GameObject.Find("Text_Message").GetComponent<Text>().text = "坐标太少（小于2），无法生成路径";
        }
      
    }
    // Update is called once per frame
    void Update()
    {
        if (Input.GetKey(KeyCode.Escape))
        {
            Screen.fullScreen = false;  //退出全屏         

        }

        //按A全屏
        //if (Input.GetKey(KeyCode.A))
        //{
        //    Screen.SetResolution(1600, 900, true);

        //    Screen.fullScreen = true;  //设置成全屏,
        //}

    }
}
