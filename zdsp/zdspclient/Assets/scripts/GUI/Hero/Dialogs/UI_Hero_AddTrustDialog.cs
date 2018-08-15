using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using Zealot.Client.Entities;
using Zealot.Common;

public class UI_Hero_AddTrustDialog : BaseWindowBehaviour
{
    [SerializeField] Transform dataTransform;
    [SerializeField] GameObject dataPrefab;
    [SerializeField] ScrollRect scrollRect;
    [SerializeField] Text trustLevelText;

    private int heroId;
    private Dictionary<int, int> usableItemsList = new Dictionary<int, int>();
    private Dictionary<int, Hero_GiftItemData> dataList = new Dictionary<int, Hero_GiftItemData>();

    public void Init(Hero hero)
    {
        PlayerGhost player = GameInfo.gLocalPlayer;
        heroId = hero.HeroId;
        trustLevelText.text = hero.TrustLevel.ToString();

        var giftItems = player.clientItemInvCtrl.itemInvData.FindItemByItemType(ItemType.MercenaryItem).Values.Where(CanUseItem);
        foreach (var item in giftItems)
        {
            if (usableItemsList.ContainsKey(item.ItemID))
                usableItemsList[item.ItemID] += item.StackCount;
            else
                usableItemsList[item.ItemID] = item.StackCount;
        }

        foreach (var item in usableItemsList)
        {
            GameObject obj = ClientUtils.CreateChild(dataTransform, dataPrefab);
            Hero_GiftItemData itemData = obj.GetComponent<Hero_GiftItemData>();
            itemData.Init(item.Key, item.Value, OnSendGiftItem);
            dataList.Add(item.Key, itemData);
        }
    }

    public void UpdateList(Hero hero)
    {
        PlayerGhost player = GameInfo.gLocalPlayer;
        trustLevelText.text = hero.TrustLevel.ToString();

        int itemIdToRemove = 0;
        foreach (var item in usableItemsList)
        {
            int currStackCount = player.clientItemInvCtrl.itemInvData.GetTotalStackCountByItemId((ushort)item.Key);
            if (currStackCount == 0)
                itemIdToRemove = item.Key;
            else if (currStackCount > 0 && currStackCount != item.Value)
                dataList[item.Key].OnUsed(currStackCount);
        }

        if (itemIdToRemove > 0)
        {
            usableItemsList.Remove(itemIdToRemove);
            Destroy(dataList[itemIdToRemove].gameObject);
            dataList.Remove(itemIdToRemove);
        }
    }

    private void OnSendGiftItem(int itemId)
    {
        RPCFactory.CombatRPC.AddHeroTrust(heroId, itemId);
    }

    private bool CanUseItem(IInventoryItem item)
    {
        HeroItem heroItem = item as HeroItem;
        if (heroItem != null && heroItem.HeroItemJson.heroitemtype == HeroItemType.Gift)
        {
            string[] heroids = heroItem.HeroItemJson.heroid.Split(';');
            return heroids.Contains(heroId.ToString());
        }
        return false;
    }

    public override void OnCloseWindow()
    {
        base.OnCloseWindow();
        usableItemsList.Clear();
        ClientUtils.DestroyChildren(dataTransform);
        dataList.Clear();
        scrollRect.verticalNormalizedPosition = 1f;
    }
}