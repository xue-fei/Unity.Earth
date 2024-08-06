using UnityEngine;

public class EarthManager : MonoBehaviour
{
    public GameObject earth;
    public Material material;
    public MapLayer mapLayer;

    // Start is called before the first frame update
    void Start()
    {
        //Application.targetFrameRate = 60; 
        earth = new GameObject("Earth");
        mapLayer = earth.AddComponent<MapLayer>();
        mapLayer.Init(earth, MapChannel.ArcGIS, MapType.Satellite, material,100);
        //MapLayer mapLayer1 = earth.AddComponent<MapLayer>();
        //mapLayer1.Init(earth, MapChannel.AutoNavi, MapType.RoadMap, material,500);
    }

    private void OnApplicationQuit()
    {
        //Resources.UnloadUnusedAssets();
    }
}