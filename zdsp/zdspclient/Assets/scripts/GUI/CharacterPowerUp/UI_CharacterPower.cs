using UnityEngine;

public class UI_CharacterPower : MonoBehaviour
{
    public GameObject[] ToggleObjs;

    void OnEnable()
    {
        InitPowerUp();
    }

    void OnDisable()
    {
        ClosePowerUp();
    }

    void InitPowerUp()
    {
        for (int i = 0; i < ToggleObjs.Length; i++) { ToggleObjs[i].SetActive(true);}
    }

    void ClosePowerUp()
    {
        for (int p = 0; p < ToggleObjs.Length; p++) { ToggleObjs[p].SetActive(false); }
    }

}
