using UnityEngine;
using UnityEngine.UI;
using System;
using UnityEngine.Events;
public class RoutePopup
{
    public GameObject _box;
    public Button _confirmBtn;

    public RoutePopup(GameObject obj)
    {
        _box = obj;
        _confirmBtn = _box.transform.Find("Modal").Find("Confirm").gameObject.GetComponent<Button>();
        _confirmBtn.onClick.AddListener(() => { _box.SetActive(false); });
    }

    /// <summary>
    /// Show the alert box
    /// </summary>
    public void show()
    {
        _box.SetActive(true);
    }
};