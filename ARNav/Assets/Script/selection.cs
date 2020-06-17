using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class selection : MonoBehaviour
{

    bool _isSelect;
    float speed = 0.001f;
    Vector3 move;
    GameObject _Selection;
    // Start is called before the first frame update
    void Start()
    {
        _Selection = GameObject.Find("VerticalGroup");
        move = _Selection.transform.position;
        _isSelect = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (_isSelect)
        {
            while (_Selection.transform.position.x < 245)
            {
                move = new Vector3(move.x + speed, move.y, move.z);
                _Selection.transform.position = move;
            }
        }
        else if (!_isSelect)
        {
            while (_Selection.transform.position.x > -245)
            {
                move = new Vector3(move.x - speed, move.y, move.z);
                _Selection.transform.position = move;
            }
        }
    }

    public void SelectionButton()
    {
        _isSelect = true;
    }

    public void CloseSelectionButton()
    {
        _isSelect = false;
    }
}
