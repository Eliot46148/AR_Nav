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

public class MyDebugScript : MonoBehaviour
{
    public static List<SavedAnchor> m_kAnchors = new List<SavedAnchor>();
    // Start is called before the first frame update
    void Start()
    {

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

    public static void SaveJSon()
    {
        string saved = JsonUtility.ToJson(m_kAnchors);
        PrintText(saved);

        //SavedAnchor readSave = JsonUtility.FromJson<SavedAnchor>(saved);

        StreamWriter writer = new StreamWriter(Application.persistentDataPath + "/test.json", true);        
        writer.WriteLine(saved);
        writer.Close();

    }

    public static void ReadJSon()
    {
        StreamReader reader = new StreamReader(Application.persistentDataPath + "/test.json");
        while (reader.Peek() >= 0)
        {
            SavedAnchor readSave = JsonUtility.FromJson<SavedAnchor>(reader.ReadLine());
            PrintText(readSave.m_AnchorPosition.ToString());
        }
        reader.Close();
    }
}
