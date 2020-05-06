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

    public string _routeName;

    public RouteData(string routename)
    {
        _anchors = new List<AnchorData>();
        _routeName = routename;
    }
    public void AddAnchor(float x, float y, float z)
    {
        _anchors.Add(new AnchorData(x, y, z));
    }

    public void AddAnchor(Vector3 position)
    {
        _anchors.Add(new AnchorData(position));
    }

    public List<AnchorData> GetAnchors(){
        return _anchors;
    }
}

[System.Serializable]
public class MapData
{
    public List<RouteData> _routes;

    public int Length => _routes.Count;

    public MapData()
    {
        _routes = new List<RouteData>();
    }

    public void AddRoute(string routename)
    {
        _routes.Add(new RouteData(routename));
    }

    public void AddAnchorToRoute(int routeIndex, float x, float y, float z)
    {
        //Debug.Log(routeIndex + " " + x + y + z);
        if (routeIndex < Length)
            _routes[routeIndex].AddAnchor(x, y, z);
        else
            Debug.LogError("MapData AddAnchor Error: routeIndex is out of the range of _routes");
    }

    public void AddAnchorToRoute(int routeIndex, Vector3 position)
    {
        Debug.Log(routeIndex + " " + position.ToString());
        if (routeIndex < Length)
            _routes[routeIndex].AddAnchor(position);
        else
            Debug.LogError("MapData AddAnchor Error: routeIndex is out of the range of _routes");
    }

    public List<string> GetAllRoutesName()
    {
        List<string> names = new List<string>();
        foreach (RouteData route in _routes)
        {
            names.Add(route._routeName);
        }
        return names;
    }

    public List<AnchorData> GetAnchorsByIndex(int index){
        return _routes[index].GetAnchors();
    }
}

public class ARNavModel
{
    private const string _savedFileName = "/ARNavData.json";
    public MapData mapData = new MapData();
    private string _dataPath;

    public int currentRouteIndex;
    public ARNavModel()
    {
        _dataPath = Application.persistentDataPath + _savedFileName;
        currentRouteIndex = 0;
        if (mapData.Length <= 0)
        {
            AddRoute("Default Route");
        }
        CreateTestData();
    }

    void CreateTestData()
    {
        AddRoute("test1");
        AddAnchorToCurrentRouter(0, 0, 0);
        AddAnchorToCurrentRouter(1, 1, 1);
        AddAnchorToCurrentRouter(1, 2, 3);
        AddRoute("test2");
        AddAnchorToCurrentRouter(7, 7, 7);
        AddAnchorToCurrentRouter(6, 1, 6);
        AddAnchorToCurrentRouter(8, 2, 6);
        //Debug.Log(JsonUtility.ToJson(mapData));
        SaveToJSon();
        ReadFromJSon();
        Debug.Log(JsonUtility.ToJson(mapData));
    }

    public void PrintText(string strContent)
    {
        Text kDebugText = GameObject.Find("DebugText").GetComponent<Text>();
        kDebugText.text = strContent;
    }

    public void ChangeCurrentRoute(int routeIndex)
    {
        currentRouteIndex = routeIndex;
    }

    public void AddRoute(string newRouteName)
    {
        mapData.AddRoute(newRouteName);
        currentRouteIndex = mapData.Length - 1;
        Debug.Log("Current Route: " + currentRouteIndex);
    }

    public void AddAnchorToCurrentRouter(Anchor newAnchor)
    {
        mapData.AddAnchorToRoute(currentRouteIndex, newAnchor.transform.position);
    }

    public void AddAnchorToCurrentRouter(float x, float y, float z)
    {
        mapData.AddAnchorToRoute(currentRouteIndex, x, y, z);
    }

    public void AddAnchorToCurrentRouter(Vector3 position)
    {
        mapData.AddAnchorToRoute(currentRouteIndex, position);
    }

    public void SaveToJSon()
    {
        File.WriteAllText(_dataPath, JsonUtility.ToJson(mapData));
        Debug.Log("Data saved to " + _dataPath);
    }

    public List<string> ReadFromJSon()
    {
        mapData = JsonUtility.FromJson<MapData>(File.ReadAllText(_dataPath));
        return mapData.GetAllRoutesName();
    }

    public List<AnchorData> GetAnchorsByRouteIndex(int index){
        return mapData.GetAnchorsByIndex(index);
    }
}
