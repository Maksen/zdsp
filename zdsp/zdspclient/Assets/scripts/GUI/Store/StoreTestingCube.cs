using System.Collections.Generic;
using UnityEngine;
using Zealot.Common;
using Zealot.Repository;

public class StoreTestingCube : MonoBehaviour
{
    public UIShop theshop;
    public int storeid = 1;

    // Use this for initialization
    void Start()
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

        switch (theshop.storetype)
        {
            case NPCStoreInfo.StoreType.Normal:
                {
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

                    ((UIShopSell)theshop).init("CubeTestShop", randlist.ToArray());
                }
                break;

            case NPCStoreInfo.StoreType.Barter:                
                {
                    var randlist = new List<NPCStoreInfo.StandardItem>();

                    int count = Random.Range(5, 15);
                    for (int i = 0; i < count; ++i)
                    {
                        var newitem = new NPCStoreInfo.StandardItem(0, 0, true, 0, NPCStoreInfo.ItemStoreType.Barter, 1, NPCStoreInfo.SoldCurrencyType.Normal, 1, 0.0f, 1, new System.DateTime(), new System.DateTime(), 1, NPCStoreInfo.Frequency.Unlimited);

						IInventoryItem randitem = null;

                        while (randitem == null)
                        {
                            var randid = Random.Range(0, GameRepo.ItemFactory.ItemTable.Count);
                            randitem = GameRepo.ItemFactory.GetInventoryItem(randid);
                        }                        
                        newitem.ItemID = randitem.ItemID;
                        newitem.data = randitem;

                        int randreqcount = Random.Range(1, 4);
                        for (int j = 0; j < randreqcount; ++j)
                        {
                            newitem.required_items.Add(new NPCStoreInfo.BarterReq { StoreID = theshop.id, ItemListID = newitem.ItemListID, ReqItemID = Random.Range(0, 10), ReqItemValue = Random.Range(1, 4) });
                        }

                        randlist.Add(newitem);
                    }

                    ((UIShopBarter)theshop).init("CubeTestShop", randlist.ToArray());
                }
                break;                
        }
    }
    
    private void OnEnable()
    { GetShop(); }

    public bool getshop = false;
    private void GetShop()
    {
        if (RPCFactory.NonCombatRPC != null)
        {

            theshop.RequestShopInfo(storeid);
        }
    }

    public int MoneyGiven = 1000;
    public bool GiveMoney = false;

    // Update is called once per frame
    public bool generaterandom = false;
    void Update()
    {
        if (generaterandom)
        {
            generaterandom = false;
            GenerateRandomItems();
        }

        if (getshop)
        {
            getshop = false;
            GetShop();
        }

        if (GiveMoney)
        {
            GiveMoney = false;
            RPCFactory.CombatRPC.AddCurrency((int)CurrencyType.Money, MoneyGiven);
        }
    }
}
