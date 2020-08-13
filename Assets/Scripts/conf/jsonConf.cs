using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class jsonConf 
{
    private static ConfigFile pConf = null;
    // Start is called before the first frame update
    public static ConfigFile configFile
    {
        get
        {
            if (pConf == null)
            {
                string filePath = System.IO.Path.Combine(Application.streamingAssetsPath, "conf.json");
                if (string.IsNullOrEmpty(filePath))
                {
                   
                    return null;
                }
               
                if (File.Exists(filePath))
                {
                    Debug.Log("已经找到配置文件：" + filePath);
                    string text = File.ReadAllText(filePath);

                    pConf = JsonUtility.FromJson<ConfigFile>(text);

                   
          
                    Debug.Log(pConf.MysqlConf);
                }
                else
                {
                    Debug.Log("没找到配置文件：" + filePath);
                }
            }
        
            return pConf;
        }
    }

}

public class ConfigFile
{
    public string MysqlConf;
    public string UdpServer;
    public string TcpClientConf;
    public string MapHttpServer;
}