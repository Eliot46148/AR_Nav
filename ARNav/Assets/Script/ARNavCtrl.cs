using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using GoogleARCore;

public class ARNavCtrl : MonoBehaviour
{
    
    // Start is called before the first frame update
    void Start()
    {
       
    }

    // Update is called once per frame
    void Update()
    {
        
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

        m_inputNewPath.gameObject.SetActive(false);
    }
}
