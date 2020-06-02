using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using GoogleARCore;
using System;
using System.IO;

/// <summary>
/// Anchor's model class
/// </summary>
[System.Serializable]
public class AnchorData
{
    /// <summary>
    /// Position of this anchor
    /// </summary>
    public Vector3 _postion;

    /// <summary>
    /// Constructor of this anchor
    /// </summary>
    public AnchorData(float x, float y, float z)
    {
        _postion = new Vector3(x, y, z);
    }

    public AnchorData(Vector3 position)
    {
        _postion = position;
    }
}

/// <summary>
/// Route's model class
/// </summary>
[System.Serializable]
public class RouteData
{
    public List<AnchorData> _anchors;

    /// <summary>
    /// Amount of _anchors.
    /// </summary>
    public int Length => _anchors.Count;

    /// <summary>
    /// Name of this route.
    /// </summary>
    public string _routeName;

    /// <summary>
    /// Constructor of class RouteData
    /// </summary>
    /// <param name="routename">name of route</param>
    public RouteData(string routename)
    {
        _anchors = new List<AnchorData>();
        _routeName = routename;
    }

    /// <summary>
    /// Add anchor to _anchors
    /// </summary>
    public void AddAnchor(float x, float y, float z)
    {
        _anchors.Add(new AnchorData(x, y, z));
    }

    public void AddAnchor(Vector3 position)
    {
        _anchors.Add(new AnchorData(position));
    }

    /// <summary>
    /// Get this route's anchors data
    /// </summary>
    /// <returns>List of anchors</returns>
    public List<AnchorData> GetAnchors()
    {
        return _anchors;
    }

    /// <summary>
    /// Remove anchor from data by specified index
    /// </summary>
    /// <param name="index">Index of anchor in route</param>
    public void RemoveAnchorByIndex(int index)
    {
        _anchors.RemoveAt(index);
    }
}

/// <summary>
/// Map's model class, contains of routes' data
/// </summary>
[System.Serializable]
public class MapData
{
    /// <summary>
    /// All routes' data
    /// </summary>
    public List<RouteData> _routes;

    /// <summary>
    /// All routes' total amount
    /// </summary>
    public int Length => _routes.Count;

    /// <summary>
    /// Constructor of MapData
    /// </summary>
    public MapData()
    {
        _routes = new List<RouteData>();
    }

    /// <summary>
    /// Add new route to _routes
    /// </summary>
    /// <param name="routename">Name of routes</param>
    public void AddRoute(string routename)
    {
        _routes.Add(new RouteData(routename));
    }

    /// <summary>
    /// Add new anchor data to specified route
    /// </summary>
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

    /// <summary>
    /// Get all routes' data of name
    /// </summary>
    /// <returns>List of names of routes</returns>
    public List<string> GetAllRoutesName()
    {
        List<string> names = new List<string>();
        foreach (RouteData route in _routes)
        {
            names.Add(route._routeName);
        }
        return names;
    }

    /// <summary>
    /// Get anchors in route by specified route index
    /// </summary>
    /// <param name="index">Index of route</param>
    /// <returns>List of anchors</returns>
    public List<AnchorData> GetAnchorsByIndex(int index)
    {
        return _routes[index].GetAnchors();
    }

    /// <summary>
    /// Remove anchor in route by specified route index
    /// </summary>
    /// <param name="index">Index of route</param>
    public void RemoveRouteByIndex(int index)
    {
        _routes.RemoveAt(index);
    }

    /// <summary>
    /// Remove anchor in route by specified indexs of route an anchor
    /// </summary>
    /// <param name="route">Index of route</param>
    /// <param name="anchor">Index of anchor</param>
    public void RemoveAnchorByIndex(int route, int anchor)
    {
        _routes[route].RemoveAnchorByIndex(anchor);
    }
}

/// <summary>
/// AR_Nav's model, be responsible for providing interface of data control
/// </summary>
public class ARNavModel
{
    /// <summary>
    /// Saved file name
    /// </summary>
    private const string _savedFileName = "/ARNavData.json";

    /// <summary>
    /// Data of routes
    /// </summary>    
    public MapData mapData = new MapData();

    /// <summary>
    /// Saved file path
    /// </summary>
    /// <remarks>
    /// Windows: C:/Users/${User Name}/AppData/LocalLow/NTUT/AR Nav/ARNavData.json
    /// Android: /DCIM/data/com.NTUT.ARNav/file
    /// </remarks>
    private string _dataPath; // C:/Users/${User Name}/AppData/LocalLow/NTUT/AR Nav/ARNavData.json

    /// <summary>
    /// Index of current route
    /// </summary>
    public int currentRouteIndex;

    public ARNavModel()
    {
        _dataPath = Application.persistentDataPath + _savedFileName;
        currentRouteIndex = 0;
        if (mapData.Length <= 0)
        {
            AddRoute("Default Route");
        }

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
        SaveToJSon();
        ReadFromJson();
        Debug.Log(JsonUtility.ToJson(mapData));
    }

    public void PrintText(string strContent)
    {
        Text kDebugText = GameObject.Find("DebugText").GetComponent<Text>();
        kDebugText.text = strContent;
    }

    /// <summary>
    /// Change currentRouteIndex
    /// </summary>
    /// <param name="routeIndex">Index of new currentRou</param>
    public void ChangeCurrentRoute(int routeIndex)
    {
        currentRouteIndex = routeIndex;
    }

    /// <summary>
    /// Push new route to data
    /// </summary>
    /// <param name="newRouteName">Name of new route.</param>
    public void AddRoute(string newRouteName)
    {
        Debug.Log(newRouteName);
        mapData.AddRoute(newRouteName);
        currentRouteIndex = mapData.Length - 1;
        Debug.Log("currentRouteIndex: " + currentRouteIndex);
        //Debug.Log("Current Route: " + currentRouteIndex);
    }

    /// <summary>
    /// Add new anchor to current route
    /// </summary>    
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


    /// <summary>
    /// Write data to Json
    /// </summary>
    public void SaveToJSon()
    {
        File.WriteAllText(_dataPath, JsonUtility.ToJson(mapData));
        Debug.Log("Data saved to " + _dataPath);
    }


    /// <summary>
    /// Read data from Json
    /// </summary>
    public List<string> ReadFromJson()
    {
        if (File.Exists(_dataPath))
            mapData = JsonUtility.FromJson<MapData>(File.ReadAllText(_dataPath));
        else
        {
            File.WriteAllText(_dataPath, JsonUtility.ToJson(mapData));
        }
        return mapData.GetAllRoutesName();
    }

    /// <summary>
    /// Get Anchors in route by specified index
    /// </summary>
    /// <param name="index">Index of route</param>
    /// <returns>List of anchors</returns>
    public List<AnchorData> GetAnchorsByRouteIndex(int index)
    {
        return mapData.GetAnchorsByIndex(index);
    }

    /// <summary>
    /// Get Anchors in route by currentRouteIndex
    /// </summary>
    /// <returns>List of anchors</returns>
    public List<AnchorData> GetAnchorsInCurrentRoute()
    {
        return mapData.GetAnchorsByIndex(currentRouteIndex);
    }

    /// <summary>
    /// Remove route by specified index
    /// </summary>
    /// <param name="index">Index of route</param>
    public void RemoveRouteByIndex(int index)
    {
        mapData.RemoveRouteByIndex(index);
    }

    /// <summary>
    /// Remove route by specified indexs of route and anchor.
    /// </summary>
    /// <param name="route">Index of route</param>
    /// <param name="anchor">Index of anchor</param>
    public void RemoveAnchorByIndex(int route, int anchor)
    {
        mapData.RemoveAnchorByIndex(route, anchor);
    }
}
