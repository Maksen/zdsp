using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UIWidgets;
using Zealot.Common;
using Zealot.Repository;
using Kopio.JsonContracts;

public class ViewPlayerDialog : MonoBehaviour {

    public GameObject message;
    public GameObject buttonOK;


    void Start()
    {
        
    }

    void Update()
    {

    }

    public void ShowMessage(string msg)
    {
        message.GetComponent<Text>().text = msg;
        this.gameObject.SetActive(true);
        if (buttonOK)
        {
            buttonOK.SetActive(true);
        }
    }

    public void OnButtonOk()
    {
        this.gameObject.SetActive(false);
    }

    void OnDestroy()
    {
        message = null;
        buttonOK = null;
    }
}