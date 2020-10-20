using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class selection : MonoBehaviour
{
    public GameObject _Selection;

    public void SelectionButton()
    {
        _Selection.SetActive(true);
    }

    public void CloseSelectionButton()
    {
        _Selection.SetActive(false);
    }
}
