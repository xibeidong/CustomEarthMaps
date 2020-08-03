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
        CanvasGroup c = canvasGroup;
        c.alpha = 0;
        c.interactable = false;
        c.blocksRaycasts = false;
    }

    private void OnclickPlay()
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
