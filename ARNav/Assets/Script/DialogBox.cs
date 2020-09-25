using UnityEngine;
using UnityEngine.UI;
using System;
using UnityEngine.Events;
public class DialogBox
{
    public GameObject _box;
    public Text _title;
    public Text _message;
    public Button _confirmBtn;
    public Button _cancelBtn;
    public bool val;

    public DialogBox(GameObject obj)
    {
        _box = obj;
        _title = _box.transform.Find("Title").gameObject.GetComponent<Text>();
        _message = _box.transform.Find("Message").gameObject.GetComponent<Text>();
        _confirmBtn = _box.transform.Find("Confirm").gameObject.GetComponent<Button>();
        _cancelBtn = _box.transform.Find("Cancel").gameObject.GetComponent<Button>();
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
    /// Show the dialog box
    /// </summary>
    /// <param name="confirmHandler">Confirm button click handler function</param>
    /// <param name="cancelHandler">Cancel button click handler function</param>
    public void Show(UnityAction confirmHandler, UnityAction cancelHandler)
    {
        _box.SetActive(true);
        _confirmBtn.onClick.RemoveAllListeners();
        _cancelBtn.onClick.RemoveAllListeners();
        _confirmBtn.onClick.AddListener(confirmHandler);
        _cancelBtn.onClick.AddListener(cancelHandler);
        _confirmBtn.onClick.AddListener(() => { _box.SetActive(false); });
        _cancelBtn.onClick.AddListener(() => { _box.SetActive(false); });
    }
};