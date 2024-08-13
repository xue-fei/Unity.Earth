using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using Unity.EditorCoroutines.Editor;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;

public class MapCacheWindow : EditorWindow
{
    static MapCacheWindow mapWindow;
    static List<string> channelStrings = new List<string>();
    static List<string> typeStrings = new List<string>();

    static int channelIndex = 0;
    static int typeIndex = 0;
    static string tempMapPath;
    static int MaxLevel = 19;
    static int MinLevel = 5;
    /// <summary>
    /// level,lat,lon
    /// </summary>
    static string mapUrl = "";

    [MenuItem("工具/地图缓存工具", false, 0)]
    static void Init()
    {
        mapWindow = (MapCacheWindow)EditorWindow.GetWindow(typeof(MapCacheWindow), false, "打包工具", true);
        mapWindow.Show();

        tempMapPath = Application.dataPath + "/../TempMap/";

        channelStrings.Clear();
        channelStrings = Enum.GetNames(typeof(MapChannel)).ToList();

        typeStrings.Clear();
        typeStrings = Enum.GetNames(typeof(MapType)).ToList();
    }

    private void OnGUI()
    {
        EditorGUILayout.Space();
        GUILayout.Label("地图渠道");
        channelIndex = GUILayout.Toolbar(channelIndex, channelStrings.ToArray());
        EditorGUILayout.Space();
        GUILayout.Label("地图类型");
        typeIndex = GUILayout.Toolbar(typeIndex, typeStrings.ToArray());
        EditorGUILayout.Space();

        if (GUILayout.Button("开始缓存"))
        {
            string channelStr = channelStrings[channelIndex];
            string typeStr = typeStrings[typeIndex];
            MapChannel channel = (MapChannel)Enum.Parse(typeof(MapChannel), channelStr);
            MapType type = (MapType)Enum.Parse(typeof(MapType), typeStr);
            switch (channel)
            {
                case MapChannel.ArcGIS:
                    mapUrl = MapUrl.ArcGIS;
                    break;
                case MapChannel.AutoNavi:
                    mapUrl = MapUrl.AutoNavi;
                    if (type == MapType.Satellite)
                    {
                        mapUrl = "http://wprd03.is.autonavi.com/appmaptile?style=6&x={2}&y={1}&z={0}";
                    }
                    if (type == MapType.RoadMap)
                    {
                        mapUrl = "http://wprd03.is.autonavi.com/appmaptile?style=8&x={2}&y={1}&z={0}";
                    }
                    break;
            }
            tempMapPath = tempMapPath + channel.ToString() + "/" + type.ToString() + "/";
            if (!Directory.Exists(tempMapPath))
            {
                Directory.CreateDirectory(tempMapPath);
            }
            EarthStart();
            EditorUtility.ClearProgressBar();
        }
    }

    public void EarthStart()
    {
        double subdivisions;
        double unitlongiAngle;
        double halfSubdivisions;

        for (int level = MinLevel; level <= MaxLevel; level++)
        {
            //赤道细分2的指数倍
            ReturnSubParam(level, out subdivisions, out unitlongiAngle, out halfSubdivisions);
            for (int lat = 0; lat < subdivisions; lat++)
            {
                for (int lon = 0; lon < subdivisions; lon++)
                {
                    EditorUtility.DisplayProgressBar("正在缓存地图level" + level, "lat:" + lat + " lon:" + lon, (lon + 1) / (float)subdivisions);
                    //第一个参数是层级，第二个是纬度，第三个是经度 
                    string url = string.Format(mapUrl, level, lat, lon);
                    if (!Directory.Exists(tempMapPath + level))
                    {
                        Directory.CreateDirectory(tempMapPath + level);
                    }
                    if (!Directory.Exists(tempMapPath + level + "/" + lat))
                    {
                        Directory.CreateDirectory(tempMapPath + level + "/" + lat);
                    }
                    string path = tempMapPath + level + "/" + lat + "/" + lon + ".jpg";
                    path = path.Replace("\\", "/");
                    if (!File.Exists(path))
                    {
                        WebClient webClient = new WebClient();
                        webClient.DownloadFile(url, path);
                    }
                }
            }
        }
    }

    /// <summary>
    /// 返回细分参数
    /// </summary>
    /// <param name="level">层级</param>
    /// <param name="subdivisions">分段</param>
    /// <param name="unitlongiAngle">经度单位角度</param>
    /// <param name="halfSubdivisions">一半的分段</param>
    void ReturnSubParam(int level, out double subdivisions, out double unitlongiAngle, out double halfSubdivisions)
    {
        //细分2的level次方
        subdivisions = Math.Pow(2, level);
        unitlongiAngle = (360.00000000000d / (subdivisions * 1.000000d));
        //Debug.Log(unitlongiAngle);
        halfSubdivisions = subdivisions * 0.500000d;
    }

    IEnumerator GetTexture(string url, Action<Texture2D> action)
    {
        Debug.Log("开始下载：" + url);
        using (var webRequest = UnityWebRequestTexture.GetTexture(url))
        {
            webRequest.certificateHandler = new WebRequestSkipCertificate();
            webRequest.timeout = 5000;
            yield return webRequest.SendWebRequest();
            yield return new WaitForSeconds(0.1f);
            if (webRequest.result == UnityWebRequest.Result.ConnectionError || webRequest.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.Log(webRequest.error);
            }
            else
            {
                Texture2D texture2D = DownloadHandlerTexture.GetContent(webRequest);
                if (action != null)
                {
                    action(texture2D);
                }
                Debug.Log("下载完成：" + url);
            }
        }
    }
}