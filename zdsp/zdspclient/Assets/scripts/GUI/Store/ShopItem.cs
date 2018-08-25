using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Zealot.Common;

public class ShopItem : MonoBehaviour {

    public GameObject on, off;
    public Text itemname, price;
    public CurrencyIcon currencyicon;
    public Transform itemicon_parent;
    public OnEnableScript selectionEnabled;

    public NPCStoreInfo.StandardItem itemdata;

    // Use this for initialization
    void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}    
}
