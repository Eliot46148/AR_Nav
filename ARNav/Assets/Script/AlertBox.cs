using UnityEngine;
using UnityEngine.UI;
using System;
using UnityEngine.Events;
public class AlertBox
{
    public GameObject _box;
    public Text _title;
    public Text _message;
    public Button _confirmBtn;

    public AlertBox(GameObject obj)
    {
        _box = obj;
        _title = _box.transform.Find("Title").gameObject.GetComponent<Text>();
        _message = _box.transform.Find("Message").gameObject.GetComponent<Text>();
        _confirmBtn = _box.transform.Find("Confirm").gameObject.GetComponent<Button>();
        _confirmBtn.onClick.AddListener(() => { _box.SetActive(false); });
    }

    /// <summary>
    /// Set the title and message will display on box
    /// </summary>
    /// <param name="title">Title of box</param>
    /// <param name="message">Message of box</param>
    public void SetInfo(string title, string message)
    {
        _title.text = title;
        _message.text = message;
    }

    /// <summary>
    /// Show the alert box
    /// </summary>
    public void show()
    {
        _box.SetActive(true);
    }
};