using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using System.IO;
using System;
using UnityEngine.UI;

public class MapDown : MonoBehaviour
{
    public InputField input_path;
    public InputField input_leftTopX;
    public InputField input_leftTopY;
    public InputField input_rightBottomX;
    public InputField input_rightBottomY;
    public Button button_down;
    public Button button_retryDown;
    public Text text_log;
    public InputField input_zoomMin;
    public InputField input_zoomMax;
    public InputField input_maxNumRequest;

    private string path = "D:/MapDownCustom";
    private double[] leftTopPos = { 120.1042556762695312, 35.9743881225585938 };
    private double[] rightbottomPos = { 120.2487945556640625, 36.0701751708984375 };
    private int minZoom = 1;
    private int maxZoom = 18;
    private string googleMapServer = "https://mt{rnd0-3}.googleapis.com/vt/lyrs=y&hl=zh-cn&x={x}&y={y}&z={zoom}";

    private int tileCount = 0;
    private int currentCount = 0;
    private int failCount = 0;
    private string startTime = string.Empty;
    private List<Vector3> failList = new List<Vector3>();
    private List<Vector3> tempFailAgainList = new List<Vector3>();
    private bool isRetry = false;
    private int maxRequestnum = 20;

    // Start is called before the first frame update
    void Start()
    {
        //PlayerPrefs.DeleteAll();

        setInputVaule(input_leftTopX, "leftTopX", "120.1042556762695312");
        setInputVaule(input_leftTopY, "leftTopY", "35.9743881225585938");

        setInputVaule(input_rightBottomX, "rightBottomX", "120.2487945556640625");
        setInputVaule(input_rightBottomY, "rightBottomY", "36.0701751708984375");

        setInputVaule(input_zoomMin, "zoomMin", "14");
        setInputVaule(input_zoomMax, "zoomMax", "15");

        setInputVaule(input_maxNumRequest, "maxNumRequest", "20");

        setInputVaule(input_path, "path", "D:/MapDownCustom");
       
       
        button_down.onClick.AddListener(OnClickBeginDown);
        button_retryDown.onClick.AddListener(OnClickRetryDown);
    }

    void setInputVaule(InputField input,string key,string defaultValue)
    {
        string v = PlayerPrefs.GetString(key);
        input.text = string.IsNullOrEmpty(v) ?  defaultValue:v;
    }
    private void OnClickBeginDown()
    {
        isRetry = false;

        path = input_path.text;
        PlayerPrefs.SetString("path", path);

        maxRequestnum = int.Parse(input_maxNumRequest.text);
        PlayerPrefs.SetString("maxNumRequest", input_maxNumRequest.text);

        leftTopPos[0] = Double.Parse(input_leftTopX.text);
        PlayerPrefs.SetString("leftTopX", input_leftTopX.text);
        leftTopPos[1] = Double.Parse(input_leftTopY.text);
        PlayerPrefs.SetString("leftTopY", input_leftTopY.text);

        rightbottomPos[0] = Double.Parse(input_rightBottomX.text);
        PlayerPrefs.SetString("rightBottomX", input_rightBottomX.text);
        rightbottomPos[1] = Double.Parse(input_rightBottomY.text);
        PlayerPrefs.SetString("rightBottomY", input_rightBottomY.text);

        minZoom = int.Parse(input_zoomMin.text);
        PlayerPrefs.SetString("zoomMin", input_zoomMin.text);
        maxZoom = int.Parse(input_zoomMax.text);
        PlayerPrefs.SetString("zoomMax", input_zoomMax.text);
        startTime = System.DateTime.Now.ToString();
        StartCoroutine(DownTilesStart());
       
      //  StartCoroutine(GetTile("https://mt1.googleapis.com/vt/lyrs=y&hl=zh-cn&x=27326&y=12861&z=15"));
    }
  
    private void OnClickRetryDown()
    {
        isRetry = true;
        StartCoroutine(retry());
    }
    IEnumerator retry()
    {
       
        startTime = System.DateTime.Now.ToString();
        failCount = 0;
        tileCount = failList.Count;
        currentCount = 0;
        for (int i = 0; i < failList.Count; i++)
        {
            Vector3 v3 = failList[i];
            yield return StartCoroutine(GetTile((int)v3.x, (int)v3.y,(int)v3.z));

        }

        failList.Clear();
        foreach (var item in tempFailAgainList)
        {
            failList.Add(item);
        }
        tempFailAgainList.Clear();
    }

    IEnumerator autoRetry()
    {
        int numRequest = 0;
        for (int i = failList.Count-1; i >= 0; i--)
        {
            Vector3 v3 = failList[i];
            failCount--;
            failList.Remove(v3);
            if (numRequest>maxRequestnum)
            {
                numRequest = 0;
                yield return StartCoroutine(GetTile((int)v3.x, (int)v3.y, (int)v3.z));
            }
            else
            {
                numRequest++;
                StartCoroutine(GetTile((int)v3.x, (int)v3.y, (int)v3.z));
            }
            
        }
    }

    IEnumerator DownTilesStart()
    {
        int requestNum = 0;
        int[] xy1;
        int[] xy2;
        tileCount = 0;
        currentCount = 0;
        failCount = 0;
        for (int z = minZoom; z <= maxZoom; z++)
        {
            string zoomDir = $"{path}/{z}";
            if (!Directory.Exists(zoomDir))
            {
                Directory.CreateDirectory(zoomDir);
            }
            xy1 = GoogleLonLatToXYZ(leftTopPos[0], leftTopPos[1], z);
            xy2 = GoogleLonLatToXYZ(rightbottomPos[0], rightbottomPos[1], z);
            int beginX = xy1[0];
            int endX = xy2[0];
            int beginY = xy2[1];
            int endY = xy1[1];
            tileCount += (endX - beginX + 1) * (endY - beginY + 1);
            Debug.Log($"beginX={beginX},endX={endX}");
            Debug.Log($"beginY={beginY},endY={endY}");
            Debug.Log("tileCount = " + tileCount);
            for (int dirIndex = beginX; dirIndex <= endX; dirIndex++)
            {
                string dirPath = $"{path}/{z}/{dirIndex}";
                if (!Directory.Exists(dirPath))
                {
                    Directory.CreateDirectory(dirPath);
                }
                for (int pngIndex = beginY; pngIndex <= endY; pngIndex++)
                {
                    if (requestNum>maxRequestnum)
                    {
                        requestNum = 0;
                        yield return StartCoroutine(GetTile(dirIndex, pngIndex, z));
                    }
                    else
                    {
                        requestNum++;
                        StartCoroutine(GetTile(dirIndex, pngIndex, z));
                    }
                  
                }
            }
        }
        yield return StartCoroutine(autoRetry());
        yield return new WaitForSeconds(1);
        text_log.text = text_log.text + "\n任务完成\n"+System.DateTime.Now.ToString();
    }
  

    IEnumerator GetTile(int x,int y,int z)
    {
        int n = UnityEngine.Random.Range(0, 4);//0.1.2.3
        string u = $"https://mt{n}.googleapis.com/vt/lyrs=y&hl=zh-cn&x={x}&y={y}&z={z}";
       // Debug.Log(u);
        using (UnityWebRequest www = UnityWebRequest.Get(u))
        {
            www.timeout = 3;
            yield return www.SendWebRequest();
            if (www.isHttpError || www.isNetworkError)
            {
                Debug.Log(www.error);

                failCount++;

                text_log.text = $"开始时间:{startTime}\n下载层级{z},\n{currentCount}/{tileCount},失败{failCount}\n{www.error}";
                if (isRetry)
                {
                    tempFailAgainList.Add(new Vector3(x, y, z));
                }
                else
                {
                    failList.Add(new Vector3(x, y, z));
                }
               
            }
            else
            {
                ulong len = www.downloadedBytes;
                //Debug.Log("数据长度 = " + len);
                FileStream fs = File.Create($"{path}/{z}/{x}/{y}.png");
                fs.Write(www.downloadHandler.data, 0, (int)len);
                fs.Close();
                currentCount++;
                text_log.text = $"开始时间:{startTime}\n下载层级{z},\n{currentCount}/{tileCount},失败{failCount}";
                
            }

        }
        
        
    }

    // 参考 https://blog.csdn.net/qq_18298439/article/details/93219931
    /**
    * 谷歌下转换经纬度对应的层行列
    *
    * @param lon  经度
    * @param lat  维度
    * @param zoom 在第zoom层进行转换
    * @return
    */
    public static int[] GoogleLonLatToXYZ(double lon, double lat, int zoom)
    {

        double n = Math.Pow(2, zoom);
        double tileX = ((lon + 180) / 360) * n;
        double tileY = (1 - (Math.Log(Math.Tan(AngularToRadian(lat)) + (1 / Math.Cos(AngularToRadian(lat)))) / Math.PI)) / 2 * n;

        int[] xy = new int[2];

        xy[0] = (int)Math.Floor(tileX);
        xy[1] = (int)Math.Floor(tileY);

        return xy;
    }

    /**
     * 层行列转经纬度
     *
     * @param x
     * @param y
     * @param z
     * @return
     */
    public static double[] XYZtoLonlat(int z, int x, int y)
    {

        double n = Math.Pow(2, z);
        double lon = x / n * 360.0 - 180.0;
        double lat = Math.Atan(Math.Sinh(Math.PI * (1 - 2 * y / n)));
        lat = lat * 180.0 / Math.PI;
        double[] lonlat = new double[2];
        lonlat[0] = lon;
        lonlat[1] = lat;
        return lonlat;
    }

    public static double AngularToRadian(double angular)
    {
        return angular / 180 * Math.PI;
    }
}
