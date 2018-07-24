using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Zealot.Common;

public class ShopItem : MonoBehaviour {

    public GameObject on, off;
    public Text itemname, price;
    public CurrencyIcon currencyicon;
    public GameIcon_Base itemicon;
    public OnEnableScript selectionEnabled;

    public NPCStoreInfo.Item itemdata;

    // Use this for initialization
    void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}    
}
