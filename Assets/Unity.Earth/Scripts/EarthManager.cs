using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;

public struct DoubleVector3
{
    public DoubleVector3(double my_x, double my_y, double my_z)
    {
        x = my_x;
        y = my_y;
        z = my_z;
    }
    public double x;
    public double y;
    public double z;
}

public class EarthManager : MonoBehaviour
{
    public GameObject earth;
    //地球赤道半径6378137米
    public float EarthRadius = 6378.137f;
    //数据地址
    public string UrlPath = "https://server.arcgisonline.com/ArcGIS/rest/services/World_Imagery/MapServer/tile/";
    public int MaxLevel = 14;
    public int MinLevel = 5;
    //地球赤道周长的一半 赤道周长PI*r = 20037508.3427892
    double halfEarthLong = 20037508.3427892;
    public GameObject PointPro;
    public Material material;
    Dictionary<string, GameObject> mapDic = new Dictionary<string, GameObject>();
    public double latRectify = 3.5e-05;
    public double lonRectify = 3.5e-05;
    public Dictionary<int, GameObject> MapFas = new Dictionary<int, GameObject>();
    private int nowLevel;

    public int NowLevel
    {
        get
        {
            return nowLevel;

        }
        set
        {
            if (!MapFas.ContainsKey(value))
            {
                GameObject obj = GameObject.Find(value.ToString());
                if (obj == null)
                {
                    obj = new GameObject(value.ToString());
                    obj.transform.parent = earth.transform;
                }
                MapFas.Add(value, obj);
            }
            if (value != nowLevel)
            {
                foreach (var item in MapFas)
                {
                    int i = Math.Abs(value - item.Key);
                    if (i > 2)
                    {
                        item.Value.SetActive(false);
                    }
                    else
                    {
                        item.Value.SetActive(true);
                    }
                }
                nowLevel = value;
            }
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        Application.targetFrameRate = 60;
        earth = new GameObject("Earth");
        //UrlPath = "https://map.geoq.cn/arcgis/rest/services/ChinaOnlineCommunity/MapServer/tile/";
        //UrlPath = "http://server.arcgisonline.com/arcgis/rest/services/USA_Topo_Maps/MapServer/tile/";
        //UrlPath = "http://cache1.arcgisonline.cn/arcgis/rest/services/ChinaOnlineStreetPurplishBlue/MapServer/tile/";
        //UrlPath = "file:\\C:\\Users\\XUEFEI\\Downloads\\GIS瓦片地图资源\\GIS瓦片地图资源\\卫星地图\\中国墨卡托标准TMS瓦片";
        EarthStart(MinLevel);
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        CamerPosToMap();
    }

    #region 地球顶点创建
    /// <summary>
    /// 得到经纬点
    /// </summary>
    /// <param name="unitlongiAngle">单位经度角</param>
    /// <param name="halfSubdivisions">一半的细分值</param>
    /// <param name="LatValue">纬度段</param>
    /// <param name="LonValue">经度段</param>
    public Vector3 GetLatLonPinot(double unitlongiAngle, double halfSubdivisions, int LatValue, int LonValue)
    {
        //赤道与本初子午线交点
        Vector3 zeroPoint = new Vector3(EarthRadius, 0, 0);
        //得到经度
        double longiAngle = -unitlongiAngle * LonValue;
        //墨卡托Y值
        double mercatorY = ((halfEarthLong / halfSubdivisions) * (halfSubdivisions - LatValue));//这里把墨卡托的Y值原点重赤道与本初子午线交点移动到左上角

        //  return GetLatitude(zeroPoint, longiAngle, mercatorY);
        return GetLatitude();
        /// <summary>
        /// 得到纬度
        /// </summary>
        /// <param name="zeroPoint">赤道与本初子午线交点</param>
        /// <param name="longiangle">经度角</param>
        /// <param name="mercatorY">墨卡托Y值</param>
        /// <returns></returns>
        // Vector3 GetLatitude(Vector3 zeroPoint, float longiAngle, double mercatorY)

        Vector3 GetLatitude()
        {
            //新建变换矩阵
            Matrix4x4 matRot = new Matrix4x4();
            //莫卡托Y转纬度
            //  double latitudeAngle = (180.000 / Math.PI) * (2 * Math.Atan(Math.Exp(((mercatorY / halfEarthLong) * 180.000) * Math.PI / 180.000)) - (Math.PI / 2));
            // double latitudeAngle = (Mathf.Rad2Deg) * (2 * Math.Atan(Math.Exp(((mercatorY / halfEarthLong) * 180.000) * Mathf.Deg2Rad)) - (Math.PI / 2));
            double latitudeAngle = mercatorTolat(mercatorY);
            Rectify(ref longiAngle, ref latitudeAngle, 1);


            //转四元数
            Quaternion quaternion = Quaternion.Euler(new Vector3(0, float.Parse(longiAngle.ToString()), float.Parse((latitudeAngle).ToString())));
            matRot.SetTRS(Vector3.zero, quaternion, new Vector3(1, 1, 1));
            return matRot.MultiplyPoint3x4(zeroPoint);
        }
    }

    public void EarthStart(int Level)
    {
        float subdivisions;
        double unitlongiAngle;
        double halfSubdivisions;
        //赤道细分2的指数倍
        ReturnSubParam(Level, out subdivisions, out unitlongiAngle, out halfSubdivisions);
        NowLevel = Level;
        for (int i = 0; i < subdivisions; i++)
        {
            for (int j = 0; j < subdivisions; j++)
            {
                // Vector3 point = GetLatLonPinot(unitlongiAngle, halfSubdivisions, j, i);
                ReadMap(unitlongiAngle, halfSubdivisions, j, i, Level);
            }
        }
        // ReadMap(unitlongiAngle, halfSubdivisions, 11, 12, Level);
    }

    /// <summary>
    /// 返回细分参数
    /// </summary>
    /// <param name="Level">层级</param>
    /// <param name="subdivisions">分段</param>
    /// <param name="unitlongiAngle">经度单位角度</param>
    /// <param name="halfSubdivisions">一半的分段</param>
    void ReturnSubParam(int Level, out float subdivisions, out double unitlongiAngle, out double halfSubdivisions)
    {
        subdivisions = (float)Math.Pow(2, Level);
        unitlongiAngle = (360.00000000000f / (subdivisions * 1.000000f));
        //Debug.Log(unitlongiAngle);
        halfSubdivisions = subdivisions * 0.500000F;
    }

    void instancePoint(Vector3 POS, string NAME)
    {
        GameObject _point = Instantiate(PointPro) as GameObject;
        _point.transform.position = POS;
        _point.name = NAME;
    }

    #endregion

    #region 得到地图
    /// <summary>
    /// 得到地图瓦片
    /// </summary>
    /// <param name="unitlongiAngle">纬度</param>
    /// <param name="halfSubdivisions">一半的细分值</param>
    /// <param name="LatValue">纬度段</param>
    /// <param name="LonValue">经度段</param>
    /// <param name="Level">层级</param>
    void ReadMap(int LatValue, int LonValue, int Level)
    {
        float subdivisions;
        double unitlongiAngle;
        double halfSubdivisions;
        ReturnSubParam(Level, out subdivisions, out unitlongiAngle, out halfSubdivisions);
        ReadMap(unitlongiAngle, halfSubdivisions, LatValue, LonValue, Level);
    }

    void ReadMap(double unitlongiAngle, double halfSubdivisions, int LatValue, int LonValue, int Level)
    {
        string mapID = Level + "&" + LatValue + "&" + LonValue;
        if (!mapDic.ContainsKey(mapID))
        {
            //GameObject Fa = GameObject.Find(Level.ToString());
            //if (Fa == null)
            //{
            //    Fa = new GameObject(Level.ToString());
            //}
            GameObject go = new GameObject(mapID);
            go.transform.parent = MapFas[Level].transform;
            mapDic.Add(mapID, go);
            StartCoroutine(getMap());
            IEnumerator getMap()
            {
                //第一个参数是层级，第二个是纬度，第三个是经度 
                string url = UrlPath + "/" + Level + "/" + LatValue + "/" + LonValue + ".jpg";
                //string url = UrlPath + "&x=" + LonValue + "&y=" + LatValue + "&z=" + Level;//https://gac-geo.googlecnapps.cn/maps/vt?lyrs=s
                //url = "http://wprd03.is.autonavi.com/appmaptile?style=6&x="+ LonValue +"&y="+ LatValue +"&z="+ Level;
                //Debug.Log(url);
                using (var webRequest = UnityWebRequestTexture.GetTexture(url))
                {
                    yield return webRequest.SendWebRequest();

                    if (webRequest.result == UnityWebRequest.Result.ConnectionError || webRequest.result == UnityWebRequest.Result.ProtocolError)
                    {
                        Debug.Log(webRequest.error);
                        //StartCoroutine(getMap());
                    }
                    else
                    {
                        Texture2D texture2D = DownloadHandlerTexture.GetContent(webRequest);

                        if (texture2D)
                        {
                            texture2D.wrapMode = TextureWrapMode.Clamp;
                            Material mat = new Material(material);
                            mat.mainTexture = texture2D;
                            mat.renderQueue = 2000 + Level;//调整渲染列队
                            CreatMesh(mat);
                        }
                    }
                }
            }
            // void CreatMesh(string mapID, Material mat)
            Debug.LogWarning("unitlongiAngle:" + unitlongiAngle + " LonValue:" + LonValue + " LatValue:" + LatValue);
            void CreatMesh(Material mat)
            {
                Vector3[] points = new Vector3[4];
                points[0] = GetLatLonPinot(unitlongiAngle, halfSubdivisions, LatValue, LonValue);
                points[1] = GetLatLonPinot(unitlongiAngle, halfSubdivisions, LatValue + 1, LonValue);
                points[2] = GetLatLonPinot(unitlongiAngle, halfSubdivisions, LatValue + 1, LonValue + 1);
                points[3] = GetLatLonPinot(unitlongiAngle, halfSubdivisions, LatValue, LonValue + 1);
                int[] Triangles = new int[6];
                Vector2[] uvs = new Vector2[4];
                uvs[0] = new Vector2(0, 1);
                uvs[1] = new Vector2(0, 0);
                uvs[2] = new Vector2(1, 0);
                uvs[3] = new Vector2(1, 1);
                Triangles[0] = 1;
                Triangles[1] = 0;
                Triangles[2] = 3;
                Triangles[3] = 3;
                Triangles[4] = 2;
                Triangles[5] = 1;
                MeshFilter filter = go.AddComponent<MeshFilter>();
                filter.mesh.vertices = points;
                filter.mesh.triangles = Triangles;
                filter.mesh.uv = uvs;
                filter.mesh.RecalculateNormals();
                filter.mesh.RecalculateBounds();
                MeshRenderer renderer = go.AddComponent<MeshRenderer>();
                renderer.material = mat;
            }
        }
    }
    #endregion

    /// <summary>
    /// 通过相机位置调取地图
    /// </summary>
    void CamerPosToMap()
    {
        Vector3 zeroPoint = new Vector3(EarthRadius, 0, 0);
        Vector3 camerVec = Camera.main.transform.position - Vector3.zero;
        float camDis = camerVec.magnitude;
        double LonAngle = Vector3.Angle(zeroPoint, new Vector3(camerVec.x, 0, camerVec.z));
        if (LonAngle < 0)
        {
            LonAngle = 360 + LonAngle;
        }
        double LatAngle = Vector3.Angle(new Vector3(camerVec.x, 0, camerVec.z), camerVec);
        double LatAngle1 = GetAngle(zeroPoint, new Vector3(EarthRadius, camerVec.y, 0));

        if (camerVec.y < 0)
        {
            LatAngle = -LatAngle;
        }
        Rectify(ref LonAngle, ref LatAngle, 3);
        // Debug.Log(LatAngle);
        NowLevel = (int)((MaxLevel - MinLevel) / Math.Exp((camDis - EarthRadius) * 30 / EarthRadius)) + MinLevel;
        float subdivisions;
        double unitlongiAngle;
        double halfSubdivisions;
        ReturnSubParam(NowLevel, out subdivisions, out unitlongiAngle, out halfSubdivisions);
        int LonValue = (int)subdivisions - (int)(LonAngle / unitlongiAngle);
        // Debug.Log(LonAngle+"&"+ unitlongiAngle);
        int LatValue = GetLatValue(LatAngle, halfSubdivisions);
        // ReadMap(LatValue , LonValue-1 , Level);
        int length = (int)(NowLevel * 0.5f);
        for (int i = -length; i < length; i++)
        {
            for (int j = -length; j < length; j++)
            {
                if (LonValue < subdivisions - 1 && LonValue + i >= 0 && LatValue < subdivisions && LatValue + j >= 0)
                {
                    ReadMap(LatValue + j, LonValue + i, NowLevel);
                }
            }
        }
    }

    int GetLatValue(double LatAngle, double halfSubdivisions)
    {
        double mercatorY = latToMercator(LatAngle);
        int LatValue1 = (int)Math.Ceiling(((mercatorY * halfSubdivisions) / halfEarthLong));
        int LatValue = (int)(halfSubdivisions - (LatAngle * halfSubdivisions / 180));

        // Debug.Log(LatAngle + "&&" + halfSubdivisions + "&&" + LatValue1); 
        return (int)(halfSubdivisions - LatValue1);
    }

    double GetAngle(Vector3 from, Vector3 to)
    {
        double fromMagni = from.magnitude;
        //Debug.Log(fromMagni);
        double toMagni = to.magnitude;
        // Debug.Log(toMagni);
        double fromToMagni = (to - from).magnitude;
        // Debug.Log(fromToMagni);
        double Angle = (Math.Pow(fromMagni, 2) + Math.Pow(toMagni, 2) - Math.Pow(fromToMagni, 2)) / (2 * fromMagni * toMagni);
        return (Math.Acos(Angle) * (180 / Math.PI));
    }

    /// <summary>
    /// 墨卡托转纬度
    /// </summary>
    /// <param name="mercatorY"></param>
    /// <returns></returns>
    double mercatorTolat(double mercatorY)
    {

        double y = mercatorY / halfEarthLong * 180.00000001;

        y = 180.00001F / Math.PI * (2 * Math.Atan(Math.Exp(y * Math.PI / 180.000001F)) - Math.PI * 0.5000001F);

        return y;
    }

    /// <summary>
    /// 纬度转墨卡托
    /// </summary>
    /// <param name="lat"></param>
    /// <returns></returns>
    double latToMercator(double lat)
    {

        double y = Math.Log(Math.Tan((90 + lat) * Math.PI / 360.000000001)) / (Math.PI / 180.0000000001);

        y = y * halfEarthLong / 180.00000000001;

        return y;
    }

    public Vector3 ToLocationPos(float lon, float lat)
    {
        Quaternion quaternion = Quaternion.Euler(lat, 90 - lon, (float)0);
        Vector3 vector = ((Vector3)(quaternion * new Vector3((float)0, (float)0, -EarthRadius)));
        return vector;
    }

    /// <summary>
    /// 根据经纬度计算球面坐标
    /// </summary>
    /// <param name="longitude">经度</param>
    /// <param name="latitude">纬度</param>
    /// <returns></returns>
    public Vector3 GetSphericalCoordinates(double longitude, double latitude)
    {
        latitude = latitude * Mathf.PI / 180D;
        longitude = longitude * Mathf.PI / 180D;
        double x = EarthRadius * Mathf.Cos((float)latitude) * Mathf.Sin((float)longitude);
        double y = EarthRadius * Mathf.Sin((float)latitude);
        double z = -EarthRadius * Mathf.Cos((float)latitude) * Mathf.Cos((float)longitude);
        return new Vector3((float)x, (float)y, (float)z);
    }

    void Rectify(ref double longiAngle, ref double latitudeAngle, int i)
    {
        // Debug.Log("longiAngle0=" + longiAngle);
        longiAngle += (longiAngle * lonRectify) * i;
        //Debug.Log("longiAngle1=" + longiAngle);
        latitudeAngle += (latitudeAngle * latRectify) * i;
    }

    private void OnApplicationQuit()
    {
        Resources.UnloadUnusedAssets();
    }
}