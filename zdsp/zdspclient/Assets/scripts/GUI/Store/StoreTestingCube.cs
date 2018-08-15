using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zealot.Common;
using Zealot.Repository;

public class StoreTestingCube : MonoBehaviour
{
    public UIShopSell theshop;    

	// Use this for initialization
	void Start ()
    {
        
    }

    static bool initkopio = false;
    void GenerateRandomItems()
    {
        //enumerate legal item types
        NPCStoreInfo.SoldCurrencyType[] currencytypes = new NPCStoreInfo.SoldCurrencyType[] { NPCStoreInfo.SoldCurrencyType.Normal, NPCStoreInfo.SoldCurrencyType.Auction };

        if (!initkopio)
        {
            initkopio = true;
            //GameRepo.InitLocalizerRepo(gameData.text);
            GameRepo.SetItemFactory(new ClientItemFactory());
            GameRepo.InitClient(AssetManager.LoadPiliQGameData());
        }

        var randlist = new List<NPCStoreInfo.StandardItem>();

        int count = Random.Range(5, 15);
        for (int i = 0; i < count; ++i)
        {
            var newitem = new NPCStoreInfo.StandardItem(0, 0, true, 0, NPCStoreInfo.ItemStoreType.Normal, 1, NPCStoreInfo.SoldCurrencyType.Normal, 1, 0.0f, 1, new System.DateTime(), new System.DateTime(), 1, NPCStoreInfo.Frequency.Unlimited);

            IInventoryItem randitem = null;

            while (randitem == null)
            {
                var randid = Random.Range(0, GameRepo.ItemFactory.ItemTable.Count);
                randitem = GameRepo.ItemFactory.GetInventoryItem(randid);
            }
            newitem.SoldValue = randitem.ItemID;
            newitem.ItemID = randitem.ItemID;
            newitem.data = randitem;
            newitem.SoldType = currencytypes[Random.Range(0, currencytypes.Length)];

            randlist.Add(newitem);
        }

        theshop.init("CubeTestShop", randlist, NPCStoreInfo.StoreType.Normal);
    }

    private void OnEnable()
    {
        if (RPCFactory.NonCombatRPC != null)
        {
            var storeid = 1;
            GameInfo.gUIShopSell = theshop;
            GameInfo.gUIShopSell.id = storeid;

            RPCFactory.CombatRPC.AddCurrency((int)CurrencyType.Money, 10000);
            
            RPCFactory.NonCombatRPC.NPCStoreInit(storeid);
            RPCFactory.NonCombatRPC.NPCStoreGetPlayerTransactions(storeid);            
        }
    }

    // Update is called once per frame
    public bool generaterandom = false;
    void Update ()
    {
        if (generaterandom)
        {
            generaterandom = false;
            GenerateRandomItems();            
        }
	}
}
