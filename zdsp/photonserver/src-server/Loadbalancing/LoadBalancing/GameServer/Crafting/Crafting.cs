using Kopio.JsonContracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Zealot.Common;
using Zealot.Logging.Client.LogClasses;
using Zealot.Repository;
using Zealot.Server.Entities;
using Zealot.Server.Rules;

namespace Photon.LoadBalancing.GameServer.Crafting
{
    public class Crafting
    {
        public enum CraftReturnCode
        {
            SUCCESS,
            FAIL,
            FAIL_ITEM,
            FAIL_MONEY
        }
        Player mPlayer;
        bool mIsEnoughMoney;//this is use for auto craft and level
        public Crafting(Player player)
        {
            mPlayer = player;
            mIsEnoughMoney = false;
        }

        public void AutoCraft(int craftid)
        {
            CraftingJson craftitem = CraftingRepo.GetCraftingJsonById(craftid);
            if (craftitem != null)
            {
                CraftLog sysLog = new CraftLog();
                string beforealluseditemcount = "";
                string afteralluseditemcount = "";
                List<ItemInfo> allcraftitem = new List<ItemInfo>();
                string[] allid = craftitem.itemid.Split(',');
                string[] allcount = craftitem.itemcount.Split(',');
                int craftcount = 0;//this is the highest amount the player can craft with their require item and money

                int costamountforcraft = mPlayer.SecondaryStats.Money / craftitem.cost;
                craftcount = costamountforcraft;
                sysLog.BeforeCraftedItemCount = mPlayer.Slot.mInventory.GetItemStackCountByItemId((ushort)craftitem.crafteditemid);
                for (int i = 0; i < allid.Length; i++)
                {
                    ushort itemid = ushort.Parse(allid[i]);
                    ushort itemcount = ushort.Parse(allcount[i]);
                    int playeritemcount = mPlayer.Slot.mInventory.GetItemStackCountByItemId(itemid);
                    int amount = playeritemcount / itemcount;

                    if (amount < craftcount)
                    {
                        craftcount = amount;
                    }

                    ItemInfo craftinfo = new ItemInfo();
                    craftinfo.itemId = itemid;
                    craftinfo.stackCount = itemcount;
                    allcraftitem.Add(craftinfo);

                    if(i == allid.Length - 1)
                        beforealluseditemcount += playeritemcount;
                    else
                        beforealluseditemcount += playeritemcount + ",";
                }

                sysLog.BeforeAllUsedItemCount = beforealluseditemcount;

                if (craftcount <= 0)//not suppose to happen unless player hack
                    return;

                for(int i=0;i<allcraftitem.Count;i++)
                {
                    allcraftitem[i].stackCount = (ushort)(allcraftitem[i].stackCount * craftcount);
                    int itemcount = mPlayer.Slot.mInventory.GetItemStackCountByItemId(allcraftitem[i].itemId);//the item count inside player bag
                    if (itemcount < allcraftitem[i].stackCount)//check if player have enough item
                    {
                        return;
                    }
                }

                InvRetval result = mPlayer.Slot.mInventory.AddItemsIntoInventory((ushort)craftitem.crafteditemid, craftitem.craftedcount * craftcount, true, "Craft");
                int totalcost = craftitem.cost * craftcount;
                if (result.retCode == InvReturnCode.AddSuccess)
                {
                    sysLog.BeforeCraftedMoney = mPlayer.SecondaryStats.Money;
                    var temp2 = mPlayer.DeductMoney(totalcost, "Craft");//not suppose to fail
                    sysLog.AfterCraftedMoney = mPlayer.SecondaryStats.Money;

                    var temp = mPlayer.Slot.mInventory.UseToolItems(allcraftitem, "Craft");//not suppose to fail
                    for (int i = 0; i < allcraftitem.Count; i++)
                    {
                        int itemcount = mPlayer.Slot.mInventory.GetItemStackCountByItemId(allcraftitem[i].itemId);//the item count inside player bag
                        if (i == allcraftitem.Count - 1)
                            afteralluseditemcount += itemcount;
                        else
                            afteralluseditemcount += itemcount + ",";
                    }
                    sysLog.AfterAllUsedItemCount = afteralluseditemcount;

                  //  mPlayer.Slot.mInventory.AddItemsIntoInventory((ushort)craftitem.crafteditemid, (ushort)(craftitem.craftedcount * craftcount));
                    RareItemNotificationRules.CheckNotification(craftitem.crafteditemid, mPlayer.Name);

                    sysLog.userId = mPlayer.Slot.mUserId;
                    sysLog.charId = mPlayer.Slot.GetCharId();
                    // sysLog.message = sb.ToString();
                    sysLog.CraftedItemId = craftitem.crafteditemid;
                    sysLog.CraftedItemCount = craftitem.craftedcount;
                    sysLog.AllUsedItemId = craftitem.itemid;
                    sysLog.AllUsedItemCount = craftitem.itemcount;
                    sysLog.MoneyUsed = totalcost;
                    sysLog.AfterCraftedItemCount = mPlayer.Slot.mInventory.GetItemStackCountByItemId((ushort)craftitem.crafteditemid);
                    var ignoreAwait = Zealot.Logging.Client.LoggingAgent.Instance.LogAsync(sysLog);
                }
                else if (result.retCode == InvReturnCode.Full)
                {
                    mPlayer.Slot.ZRPC.CombatRPC.Ret_SendSystemMessage("sys_BagNoEnoughSpace", "", false, mPlayer.Slot);
                    return;
                }
            }
        }

        /// <summary>
        /// autolevelitemid is use for auto crafting and level
        /// </summary>
        /// <param name="craftid"></param>
        /// <param name="autolevelitemid"></param>
        /// <param name="addone"></param>
        /// <returns></returns>
        public bool SingleCraft(int craftid,int autolevelitemid = -1,bool minusone = false,bool broadcast = true,bool addtobag = true)
        {
            CraftingJson craftitem = CraftingRepo.GetCraftingJsonById(craftid);
            if(craftitem != null)
            {
                CraftLog sysLog = new CraftLog();
                string beforealluseditemcount = "";
                string afteralluseditemcount = "";
                List<ItemInfo> allcraftitem = new List<ItemInfo>();
                string[] allid = craftitem.itemid.Split(',');
                string[] allcount = craftitem.itemcount.Split(',');

                sysLog.BeforeCraftedItemCount = mPlayer.Slot.mInventory.GetItemStackCountByItemId((ushort)craftitem.crafteditemid);

                for (int i=0;i< allid.Length; i++)
                {
                    ItemInfo craftinfo = new ItemInfo();
                    craftinfo.itemId = ushort.Parse(allid[i]);
                    craftinfo.stackCount = ushort.Parse(allcount[i]);
                    if(minusone == true)
                    {
                        if (craftinfo.itemId == autolevelitemid)
                            craftinfo.stackCount-=1;
                    }

                    int itemcount = mPlayer.Slot.mInventory.GetItemStackCountByItemId(craftinfo.itemId);//the item count inside player bag
                    if (itemcount < craftinfo.stackCount)//check if player have enough item
                    {
                        return false;
                    }
                    else
                    {
                        if (i == allid.Length -1)
                            beforealluseditemcount += itemcount;
                        else
                            beforealluseditemcount += itemcount + ",";
                    }
                    allcraftitem.Add(craftinfo);
                }

                sysLog.BeforeAllUsedItemCount = beforealluseditemcount;

                if (mPlayer.SecondaryStats.Money < craftitem.cost)//check if player have enough money
                {
                    mIsEnoughMoney = false;
                    return false;
                }
                else
                {
                    mIsEnoughMoney = true;
                }

                if (addtobag == true)
                {
                    //if the code enter to here, money and use item will be enough for deduct
                    InvRetval result = mPlayer.Slot.mInventory.AddItemsIntoInventory((ushort)craftitem.crafteditemid, craftitem.craftedcount, true, "Craft");
                    if(result.retCode == InvReturnCode.AddSuccess)
                    {
                        if (broadcast == true)
                            RareItemNotificationRules.CheckNotification(craftitem.crafteditemid, mPlayer.Name);


                        sysLog.userId = mPlayer.Slot.mUserId;
                        sysLog.charId = mPlayer.Slot.GetCharId();
                        // sysLog.message = sb.ToString();
                        sysLog.CraftedItemId = craftitem.crafteditemid;
                        sysLog.CraftedItemCount = craftitem.craftedcount;
                        sysLog.AllUsedItemId = craftitem.itemid;
                        sysLog.AllUsedItemCount = craftitem.itemcount;
                        sysLog.MoneyUsed = craftitem.cost;
                        sysLog.AfterCraftedItemCount = mPlayer.Slot.mInventory.GetItemStackCountByItemId((ushort)craftitem.crafteditemid);
                    }
                    else if(result.retCode == InvReturnCode.Full)
                    {
                        mPlayer.Slot.ZRPC.CombatRPC.Ret_SendSystemMessage("sys_BagNoEnoughSpace", "", false, mPlayer.Slot);
                        return false;
                    }
                }
                sysLog.BeforeCraftedMoney = mPlayer.SecondaryStats.Money;
                mPlayer.DeductMoney(craftitem.cost, "Craft");//this not suppose to fail
                sysLog.AfterCraftedMoney = mPlayer.SecondaryStats.Money;

                mPlayer.Slot.mInventory.UseToolItems(allcraftitem, "Craft");//this not suppose to fail
                for(int i=0;i<allcraftitem.Count;i++)
                {
                    int itemcount = mPlayer.Slot.mInventory.GetItemStackCountByItemId(allcraftitem[i].itemId);//the item count inside player bag
                    if (i == allcraftitem.Count - 1)
                        afteralluseditemcount += itemcount;
                    else
                        afteralluseditemcount += itemcount + ",";
                }
                sysLog.AfterAllUsedItemCount = afteralluseditemcount;
                var ignoreAwait = Zealot.Logging.Client.LoggingAgent.Instance.LogAsync(sysLog);
                return true;
            }
            return false;
        }

        /// <summary>
        /// FAIL_MONEY,FAIL_ITEM and FAIL will both return -1 for the crafteditemid
        /// </summary>
        /// <param name="itemid"></param>
        /// <param name="crafteditemid"></param>
        /// <returns></returns>
        public CraftReturnCode AutoCraftAndLevelItem(int itemid,out int crafteditemid)
        {
            mIsEnoughMoney = true;
            crafteditemid = -1;
            CraftingRepo.CraftedItemInfo craftinfo = CraftingRepo.GetSubIdByItemId(itemid);
            if (craftinfo == null)
                return CraftReturnCode.FAIL;

            List<CraftingJson> allcraft = CraftingRepo.GetAllCraftingByTypeAndSubId(craftinfo.type, craftinfo.SubId);
            int checkitemid = itemid;//this is use to save the crafted item id as it will not be add into bag
            if (allcraft != null)
            {
                bool found = false;
                bool isfirstlevelcraft = false;
                bool firstcraft = true;
                for (int i = 0; i < allcraft.Count; i++)
                {
                    if (found == false)
                    {
                        if (allcraft[i].crafteditemid == itemid)
                        {
                            found = true;
                            isfirstlevelcraft = false;
                        }
                        else
                        {
                            if(i == 0)//only do for 1st loop
                            {
                                string[] allitemid = allcraft[i].itemid.Split(',');
                                for(int j=0;j<allitemid.Length;j++)
                                {
                                    int tempitemid = -1;
                                    if(int.TryParse(allitemid[j],out tempitemid) == true)
                                    {
                                        if(tempitemid == itemid)
                                        {
                                            found = true;
                                            isfirstlevelcraft = true;
                                            break;
                                        }
                                    }
                                }
                            }
                        }
                    }
                    if(found == true)
                    {
                        CraftingJson tempcraftjson = null;
                        if (isfirstlevelcraft == false)
                        {
                            int index = i + 1;
                            if (index >= allcraft.Count)
                                break;
                            tempcraftjson = allcraft[index];
                        }
                        else
                            tempcraftjson = allcraft[i];
                        if (firstcraft == true)
                        {
                           
                            firstcraft = false;
                            if (SingleCraft(tempcraftjson.id, checkitemid, true, false, false) == false)
                            {
                                if (mIsEnoughMoney == false)
                                {
                                    return CraftReturnCode.FAIL_MONEY;
                                }
                                else
                                {
                                    return CraftReturnCode.FAIL_ITEM;
                                }
                            }
                            else
                            {
                                crafteditemid = tempcraftjson.crafteditemid;
                                checkitemid = crafteditemid;
                            }
                        }
                        else
                        {
                            if (SingleCraft(tempcraftjson.id, checkitemid, true, false, false) == false)
                            {
                                break;//when break here, aleast 1 item will be crafted
                            }
                            else
                            {
                                crafteditemid = tempcraftjson.crafteditemid;
                                checkitemid = crafteditemid;
                            }
                        }
                    }
                }
            }

            if(crafteditemid > 0)
            {
                RareItemNotificationRules.CheckNotification(crafteditemid, mPlayer.Name);
            }
            return CraftReturnCode.SUCCESS;
        }
    }
}
