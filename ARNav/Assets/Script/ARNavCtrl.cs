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
    public GameObject m_arrowObject;

    private int currentRouteIndex;

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
        }
        currentRouteIndex = data.Count - 1;
        currentRouteIndex = pathDropdown.value;
    }

    public void OnAddNewPathBtnClick()
    {
        string newPathName = inputNewField.text;
        pathDropdown.options.Add(new Dropdown.OptionData(newPathName));
        model.AddRoute(newPathName);
        currentRouteIndex =pathDropdown.options.Count;
        m_inputNewPath.gameObject.SetActive(false);
    }

    public void OnDeletePathBtnClick(){
        if(currentRouteIndex>0){
            model.RemoveRouteByIndex(currentRouteIndex);
            pathDropdown.options.RemoveAt(currentRouteIndex);
            currentRouteIndex--;
            pathDropdown.value = currentRouteIndex;
        }
    }

    public void OnPathDropDownChange()
    {
        currentRouteIndex = pathDropdown.value;
        Debug.Log(currentRouteIndex);
    }

    public void CreateArrow(Transform newAnchorTransform)
    {
        float minArrowDistance = 0.05f;//錨點間能放入箭頭的最小距離

        List<AnchorData> anchors = model.mapData._routes[0]._anchors;//錨點的list
        AnchorData lastAnchor = anchors[anchors.Count - 1];//最後一個錨點

        Vector3 distance = newAnchorTransform.position - lastAnchor._postion;//最後一個錨點與新錨點的距離

        if (distance.magnitude > minArrowDistance)//若距離大於最小值
        {
            for (int i = 0; i < distance.magnitude / minArrowDistance; i++)//根據距離放置箭頭
            {
                //放置箭頭，每次的位置是「最後一個錨點的位置朝向新錨點」的方向+最小距離。箭頭的角度旋轉到這個方向
                GameObject arrow = GameObject.Instantiate(m_arrowObject, Vector3.zero, Quaternion.identity) as GameObject;

                GameObject temp = new GameObject();
                temp.transform.position = anchors[anchors.Count - 1]._postion;
                temp.transform.LookAt(newAnchorTransform);
                arrow.transform.rotation = temp.transform.rotation;
                GameObject.Destroy(temp);
            }
        }
    }
}
