﻿using UnityEngine;

public class EarthManager : MonoBehaviour
{
    public GameObject earth;
    public Material material;
    public MapLayer mapLayer;

    // Start is called before the first frame update
    void Start()
    {
        Application.targetFrameRate = 60; 
        earth = new GameObject("Earth");
        mapLayer = earth.AddComponent<MapLayer>();
        mapLayer.Init(earth, MapChannel.AutoNavi, MapType.Satellite, material, 100);
        MapLayer mapLayer1 = earth.AddComponent<MapLayer>();
        mapLayer1.Init(earth, MapChannel.AutoNavi, MapType.RoadMap, material, 50);
    }

    private void OnDestroy()
    {
        Resources.UnloadUnusedAssets();
    }

    private void OnApplicationQuit()
    {
        //Resources.UnloadUnusedAssets();
    }
}