using UnityEngine;
using UnityEngine.UI;

public class UI_ClueDateData : MonoBehaviour
{
    [SerializeField]
    Text Date;

    public void Init(string date)
    {
        Date.text = date;
    }
}
