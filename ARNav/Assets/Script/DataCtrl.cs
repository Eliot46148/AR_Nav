using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using GoogleARCore;
using System;
using System.IO;

[System.Serializable]
public class AnchorData
{
    public Vector3 _postion;

    public Vector3 Position => _postion;
    public AnchorData(float x, float y, float z)
    {
        _postion = new Vector3(x, y, z);
    }

    public AnchorData(Vector3 position)
    {
        _postion = position;
    }
}

[System.Serializable]
public class RouteData
{
    public List<AnchorData> _anchors;
    public int Length => _anchors.Count;

    public List<AnchorData> Anchors => _anchors;

    public RouteData()
    {
        _anchors = new List<AnchorData>();
    }
    public void AddAnchor(float x, float y, float z)
    {
        _anchors.Add(new AnchorData(x, y, z));
    }

    public void AddAnchor(Vector3 position)
    {
        _anchors.Add(new AnchorData(position));
    }
}

[System.Serializable]
public class MapData
{
    public List<RouteData> _routes;

    public int Length => _routes.Count;

    public List<RouteData> Routes=>_routes;

    public MapData()
    {
        _routes = new List<RouteData>();
    }

    public void AddRoute()
    {
        _routes.Add(new RouteData());
    }

    public void AddAnchor(int routeIndex, float x, float y, float z)
    {
        if (routeIndex < Length)
            _routes[routeIndex].AddAnchor(x, y, z);
        else
            Debug.Log("MapData AddAnchor Error: routeIndex is out of the range of _routes");
    }

    public void AddAnchorToRoute(int routeIndex, Vector3 position)
    {
        Debug.Log(routeIndex+" "+position.ToString());
        if (routeIndex < Length)
            _routes[routeIndex].AddAnchor(position);
        else
            Debug.Log("MapData AddAnchor Error: routeIndex is out of the range of _routes");
    }
}

public class DataCtrl : MonoBehaviour
{
    private const string _savedFileName = "/data.json";
    public MapData mapData = new MapData();
    private string _dataPath;

    private int currentRouteIndex;
    void Start()
    {
        _dataPath = Application.persistentDataPath + _savedFileName;
        currentRouteIndex = 0;
        CreateTestData();
        SaveJSon();
    }

    void CreateTestData()
    {
        mapData.AddRoute();
        mapData.AddAnchorToRoute(0, new Vector3(0, 0, 0));
        mapData.AddAnchorToRoute(0, new Vector3(1, 1, 1));
        mapData.AddAnchorToRoute(0, new Vector3(2, 2, 2));
        mapData.AddRoute();
        mapData.AddAnchorToRoute(1, new Vector3(5, 5, 5));
        mapData.AddAnchorToRoute(1, new Vector3(1, 1, 1));
        mapData.AddAnchorToRoute(1, new Vector3(2, 2, 2));
    }

    public void PrintText(string strContent)
    {
        Text kDebugText = GameObject.Find("DebugText").GetComponent<Text>();
        kDebugText.text = strContent;
    }

    public void AddAnchor(Anchor newAnchor)
    {
        mapData.AddAnchorToRoute(currentRouteIndex, newAnchor.transform.position);
    }

    public void SaveJSon()
    {
        File.WriteAllText(_dataPath, JsonUtility.ToJson(mapData));
        Debug.Log(JsonUtility.ToJson(mapData));
        Debug.Log(mapData.Routes[0].Anchors[0].Position.ToString());
        Debug.Log(mapData.Routes[0].Anchors[1].Position.ToString());
        Debug.Log(mapData.Routes[0].Anchors[2].Position.ToString());
        Debug.Log("Data saved to " + _dataPath);
    }

    public void ReadJSon()
    {
        mapData = JsonUtility.FromJson<MapData>(File.ReadAllText(_dataPath));
    }
}
