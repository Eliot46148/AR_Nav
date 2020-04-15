using System.Collections;
using System.Collections.Generic;
using System;
using System.IO;
using UnityEngine;

[System.Serializable]
public class test
{
    public string a = "111";
    public string b = "222";

    public string SaveToString()
    {
        return JsonUtility.ToJson(this);
    }
}

[System.Serializable]
public class data
{
    public List<test> data1;
}

public class JsonIO : MonoBehaviour
{
    string dataPath;
    public data test1;
    public data test2;
    // Start is called before the first frame update
    void Start()
    {
        dataPath = Application.persistentDataPath + "/test.json";
        test1 = new data();
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void saveToJson()
    {
        Debug.Log(JsonUtility.ToJson(test1));
        File.WriteAllText(dataPath, JsonUtility.ToJson(test1));
    }

    public void readFromJson()
    {
        Debug.Log(File.ReadAllText(dataPath));
        test2 = JsonUtility.FromJson<data>(File.ReadAllText(dataPath));
    }
}
