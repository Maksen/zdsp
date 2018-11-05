using Kopio.JsonContracts;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;
using Zealot.Common;

namespace Zealot.Repository
{
    public abstract class BaseItemFactory
    {
        public Dictionary<int, ItemBaseJson> ItemTable = new Dictionary<int, ItemBaseJson>();
        public Dictionary<int, ItemSortJson> ItemSortTable = new Dictionary<int, ItemSortJson>();
        public Dictionary<int, ItemOriginJson> ItemOriginTable = new Dictionary<int, ItemOriginJson>();
        public Dictionary<int, LinkUIJson> LinkUITable = new Dictionary<int, LinkUIJson>();

        public void InitGameData(GameDBRepo gameData)
        {
            ItemTable.Clear();
            ItemSortTable = gameData.ItemSort;
            ItemOriginTable = gameData.ItemOrigin;
            LinkUITable.Clear();

            foreach (KeyValuePair<int, LinkUIJson> kvp in gameData.LinkUI)
                LinkUITable.Add(kvp.Value.uiid, kvp.Value);      

            foreach (KeyValuePair<int, PotionFoodJson> kvp in gameData.PotionFood)
                ItemTable.Add(kvp.Value.itemid, kvp.Value);

            foreach (KeyValuePair<int, MaterialJson> kvp in gameData.Material)
                ItemTable.Add(kvp.Value.itemid, kvp.Value);

            foreach (KeyValuePair<int, LuckyPickJson> kvp in gameData.LuckyPick)
                ItemTable.Add(kvp.Value.itemid, kvp.Value);

            foreach (KeyValuePair<int, HenshinJson> kvp in gameData.Henshin)
                ItemTable.Add(kvp.Value.itemid, kvp.Value);

            foreach (KeyValuePair<int, FeaturesJson> kvp in gameData.Features)
                ItemTable.Add(kvp.Value.itemid, kvp.Value);
            
            foreach (KeyValuePair<int, EquipmentJson> kvp in gameData.Equipment)
                ItemTable.Add(kvp.Value.itemid, kvp.Value);

            foreach (KeyValuePair<int, DNAJson> kvp in gameData.DNA)
                ItemTable.Add(kvp.Value.itemid, kvp.Value);

            foreach (KeyValuePair<int, RelicJson> kvp in gameData.Relic)
                ItemTable.Add(kvp.Value.itemid, kvp.Value);

            foreach (KeyValuePair<int, QuestItemJson> kvp in gameData.QuestItem)
                ItemTable.Add(kvp.Value.itemid, kvp.Value);

            foreach (KeyValuePair<int, HeroItemJson> kvp in gameData.HeroItem)
                ItemTable.Add(kvp.Value.itemid, kvp.Value);

            foreach (KeyValuePair<int, InstanceItemJson> kvp in gameData.InstanceItem)
                ItemTable.Add(kvp.Value.itemid, kvp.Value);

            foreach (KeyValuePair<int, ElementalStoneJson> kvp in gameData.ElementalStone)
                ItemTable.Add(kvp.Value.itemid, kvp.Value);
        }

        public ItemBaseJson GetItemById(int itemId)
        {
            ItemBaseJson itemBaseJson;
            ItemTable.TryGetValue(itemId, out itemBaseJson);
            return itemBaseJson;
        }

        public IInventoryItem GetInventoryItem(int itemId)
        {
            ItemBaseJson itemjson = GetItemById(itemId);
            if (itemjson != null)
            {
                IInventoryItem retItem;
                switch (itemjson.itemtype)
                {
                    case ItemType.PotionFood:
                        retItem = new PotionFood();
                        break;
                    case ItemType.Material:
                        retItem = new MaterialItem();
                        break;
                    case ItemType.LuckyPick:
                        retItem = new LuckyPick();
                        break;
                    case ItemType.Henshin:
                        retItem = new Henshin();
                        break;
                    case ItemType.Features:
                        retItem = new Features();
                        break;
                    case ItemType.Equipment:
                        retItem = new Equipment();
                        break;
                    case ItemType.DNA:
                        retItem = new DNA();
                        break;
                    case ItemType.Relic:
                        retItem = new Relic();
                        break;
                    case ItemType.QuestItem:
                        retItem = new QuestItem();
                        break;
                    case ItemType.MercenaryItem:
                        retItem = new HeroItem();
                        break;
                    case ItemType.InstanceItem:
                        retItem = new InstanceItem();
                        break;
                    case ItemType.ElementalStone:
                        retItem = new ElementalStone();
                        break;
                    default:
                        return null;
                }

                retItem.LoadJson(itemjson);
                return retItem;
            }
            return null;
        }

        public IInventoryItem GetInventoryItem(JObject jObject)
        {
            int itemId = (int)jObject["id"];
            ItemBaseJson itemjson = GetItemById(itemId);
            if (itemjson != null)
            {
                IInventoryItem retItem;
                switch (itemjson.itemtype)
                {
                    case ItemType.PotionFood:
                        retItem = jObject.ToObject<PotionFood>();
                        break;
                    case ItemType.Material:
                        retItem = jObject.ToObject<MaterialItem>();
                        break;
                    case ItemType.LuckyPick:
                        retItem = jObject.ToObject<LuckyPick>();
                        break;
                    case ItemType.Henshin:
                        retItem = jObject.ToObject<Henshin>();
                        break;
                    case ItemType.Features:
                        retItem = jObject.ToObject<Features>();
                        break;
                    case ItemType.Equipment:
                        retItem = jObject.ToObject<Equipment>();
                        break;
                    case ItemType.DNA:
                        retItem = jObject.ToObject<DNA>();
                        break;
                    case ItemType.Relic:
                        retItem = jObject.ToObject<Relic>();
                        break;
                    case ItemType.QuestItem:
                        retItem = jObject.ToObject<QuestItem>();
                        break;
                    case ItemType.MercenaryItem:
                        retItem = jObject.ToObject<HeroItem>();
                        break;
                    case ItemType.InstanceItem:
                        retItem = jObject.ToObject<InstanceItem>();
                        break;
                    case ItemType.ElementalStone:
                        retItem = jObject.ToObject<ElementalStone>();
                        break;
                    default:
                        return null;
                }

                retItem.LoadJson(itemjson);
                retItem.CalculateItemScore();
                return retItem;
            }
            return null;
        }

        public ItemSortJson GetItemSortById(int id)
        {
            ItemSortJson itemSortJson = null;
            ItemSortTable.TryGetValue(id, out itemSortJson);
            return itemSortJson;
        }

        public LinkUIJson GetLinkUI(int UIId)
        {
            LinkUIJson _ret = null;
            LinkUITable.TryGetValue(UIId, out _ret);
            return null;    
        }

        public List<ItemOriginJson> GetItemOriginJson(int itemId)
        {
            List<ItemOriginJson> _result = new List<ItemOriginJson>();
            ItemBaseJson _itemJson = GetItemById(itemId);
            if (_itemJson != null)
            {
                string _origin = _itemJson.origin;
                if (_origin != "-1" && _origin != "")
                {
                    string[] _originIds = _origin.Split(';');
                    int length = _originIds.Length, _originId;
                    ItemOriginJson itemOriginJson;
                    for (int index = 0; index < length; ++index)
                    {
                        if (int.TryParse(_originIds[index], out _originId) && ItemOriginTable.TryGetValue(_originId, out itemOriginJson))
                            _result.Add(itemOriginJson);
                    }
                }
            }
            return _result;
        }
        public virtual IInventoryItem CreateItemInstance(int itemid) { return null; }
        public virtual IInventoryItem GetItemFromCode(int itemCode) { return null; }
        public virtual IInventoryItem GetItemFromCode(string itemCode, bool base64encode = false) { return null; }
        public virtual IInventoryItem GetItemFromCode(object itemCode)
        {
            if (itemCode is int)
                return GetItemFromCode((int)itemCode);
            else if (itemCode is string)
                return GetItemFromCode((string)itemCode, false);
            else
                return null;
        }
        public virtual IInventoryItem GetItemFromCode(string itemCode, ItemEncodingType type)
        {
            if (type == ItemEncodingType.Integer)
                return GetItemFromCode(int.Parse(itemCode));
            else if (type == ItemEncodingType.String)
                return GetItemFromCode(itemCode, false);
            else
                return null;
        }
    }

    public class ClientItemFactory : BaseItemFactory
    {
        private int GetItemIdFromCode(int itemCode)
        {
            return itemCode & (int)ItemCodeMask.ID;
        }

        /// <summary>
        /// Reads item ID from the first 2 bytes of a string
        /// </summary>
        /// <param name="itemCode">unicode encoded string</param>
        private int GetItemIdFromCode(string itemCode, bool base64encode = false)
        {
            byte[] byteArr = null;
            if (base64encode)
                byteArr = Convert.FromBase64String(itemCode);
            else
                byteArr = Encoding.Unicode.GetBytes(itemCode);
            return BitConverter.ToInt16(byteArr, 0);
        }

        /// <summary>
        /// Decode non-equipment items, uses integer encoding
        /// </summary>
        public override IInventoryItem GetItemFromCode(int itemCode)
        {
            int itemID = GetItemIdFromCode(itemCode);
            IInventoryItem item = base.GetInventoryItem(itemID);
            if (item != null)
                item.InitFromCode(itemCode);
            return item;
        }

        /// <summary>
        /// Decode equipment items, uses string encoding
        /// </summary>
        public override IInventoryItem GetItemFromCode(string itemCode, bool base64encode = false)
        {
            int itemID = GetItemIdFromCode(itemCode, base64encode);
            IInventoryItem item = base.GetInventoryItem(itemID);
            if (item != null)
                item.InitFromCode(itemCode, base64encode);
            return item;
        }
    }
}
