using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zealot.Common;
using Zealot.Repository;
using Kopio.JsonContracts;
using UnityEngine.UI;

public class UI_CharacterPower : MonoBehaviour
{
   // public bool LoadFromGameScene = false;

    public GameObject[] ToggleObjs;

    void OnEnable()
    {
        print("Open PowerUp Panel");
        InitPowerUp();
    }

    void OnDisable()
    {
        print("Close PowerUp Panel");
        ClosePowerUp();
    }

    void InitPowerUp()
    {
        //Active All Toggle
        for (int i = 0; i < ToggleObjs.Length; i++) { ToggleObjs[i].SetActive(true);}
    }

    void ClosePowerUp()
    {
        for (int p = 0; p < ToggleObjs.Length; p++) { ToggleObjs[p].SetActive(false); }
    }

}
