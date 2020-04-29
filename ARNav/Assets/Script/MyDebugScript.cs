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

    enum ePATH
    {
        path0 = 0,
        path1 = 1
    }
    ePATH currentPath = ePATH.path0;

    public string dataPath;
    // Start is called before the first frame update
    void Start()
    {
        dataPath = Application.persistentDataPath + "/test.json";
        savedData.data = m_kAnchors;
        ReadJSon();
        for(int i = 0;i< savedData.data.Count;i++)
        {
            Debug.Log(savedData.data[i].m_AnchorPosition);
        }
    }

    // Update is called once per frame
    void Update()
    {

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
        Debug.Log("Json saved");
    }

    public void ReadJSon()
    {
        savedData = JsonUtility.FromJson<SavedData>(File.ReadAllText(dataPath));
        Debug.Log("Json read");
    }

    public static void PrintText(string strContent)
    {
        Text kDebugText = GameObject.Find("DebugText").GetComponent<Text>();
        kDebugText.text = strContent;
    }


    //UI below
    public GameObject m_inputNewPath;
    public void ActiveInputNewPathField()
    {
        m_inputNewPath.SetActive(true);
    }

    public void AddNewPath()
    {
        Dropdown pathDropdown = GameObject.Find("PathDropdown").GetComponent<Dropdown>();
        InputField inputNewField = m_inputNewPath.GetComponent<InputField>();
        string newPathName = inputNewField.text;

        pathDropdown.options.Add(new Dropdown.OptionData(newPathName));
        //pathDropdown.AddOptions(pathDropdown.options);

        m_inputNewPath.gameObject.SetActive(false);
    }
}
