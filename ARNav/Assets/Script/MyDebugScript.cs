using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using GoogleARCore;
using System;
using System.IO;

[System.Serializable]
public class SavedAnchor
{
    public Vector3 m_AnchorPosition;

}

[System.Serializable]
public class SavedData{
    public List<SavedAnchor> data;
}

public class MyDebugScript : MonoBehaviour
{
    public static List<SavedAnchor> m_kAnchors = new List<SavedAnchor>();
    public SavedData savedData = new SavedData();
    public string dataPath;
    // Start is called before the first frame update
    void Start()
    {
        dataPath = Application.persistentDataPath + "/test.json";
        savedData.data = m_kAnchors;
    }

    // Update is called once per frame
    void Update()
    {

    }

    public static void PrintText(string strContent)
    {
        Text kDebugText = GameObject.Find("DebugText").GetComponent<Text>();
        kDebugText.text = strContent;
    }

    public static void AddAnchor(Anchor kNewAnchor)
    {
        SavedAnchor kNewSavedAnchorPosition = new SavedAnchor();
        kNewSavedAnchorPosition.m_AnchorPosition = kNewAnchor.transform.position;
        m_kAnchors.Add(kNewSavedAnchorPosition);
    }

    public void SaveJSon()
    {
        File.WriteAllText(dataPath, JsonUtility.ToJson(savedData));
    }

    public void ReadJSon()
    {
        savedData = JsonUtility.FromJson<SavedData>(File.ReadAllText(dataPath));
    }
}
