using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using GoogleARCore;

public class ARNavCtrl : MonoBehaviour
{
    private ARNavModel model;

    private Dropdown pathDropdown;
    private InputField inputNewField;
    public GameObject m_inputNewPath;

    void Start()
    {
        
            InitUI();
            InitModel();
        
    }

    void Update()
    {

    }

    public void ActiveInputNewPathField()
    {
        m_inputNewPath.SetActive(true);
    }

    private void InitUI()
    {
        pathDropdown = GameObject.Find("PathDropdown").GetComponent<Dropdown>();
        inputNewField = m_inputNewPath.GetComponent<InputField>();
    }

    private void InitModel()
    {
        model = new ARNavModel();
        List<string> data = model.ReadFromJSon();
        foreach (string name in data)
        {
            pathDropdown.options.Add(new Dropdown.OptionData(name));
            Debug.Log(name);
        }
    }

    public void OnAddNewPathBtnClick()
    {
        string newPathName = inputNewField.text;

        pathDropdown.options.Add(new Dropdown.OptionData(newPathName));

        m_inputNewPath.gameObject.SetActive(false);
    }
}
