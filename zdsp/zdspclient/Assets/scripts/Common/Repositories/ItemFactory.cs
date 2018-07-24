using System;
using System.Text;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using Kopio.JsonContracts;
using Zealot.Common;

namespace Zealot.Repository
{
    public abstract class BaseItemFactory
    {
        private Dictionary<int, ItemBaseJson> itemTable = new Dictionary<int, ItemBaseJson>();
        private Dictionary<int, ItemSortJson> itemSortTable = new Dictionary<int, ItemSortJson>();
        private Dictionary<int, ItemOriginJson> itemOriginTable = new Dictionary<int, ItemOriginJson>();
        private Dictionary<int, LinkUIJson> linkUITable = new Dictionary<int, LinkUIJson>();

        public Dictionary<int, ItemBaseJson> ItemTable
        {
            get { return itemTable; }
        }
        public Dictionary<int, ItemSortJson> ItemSortTable
        {
            get { return itemSortTable; }
        }

        public void InitGameData(GameDBRepo gameData)
        {
            linkUITable.Clear();
            itemTable.Clear();
            itemSortTable = gameData.ItemSort;
            itemOriginTable = gameData.ItemOrigin;

            foreach (KeyValuePair<int, LinkUIJson> kvp in gameData.LinkUI)
                linkUITable.Add(kvp.Value.uiid, kvp.Value);      

            foreach (KeyValuePair<int, PotionFoodJson> kvp in gameData.PotionFood)
                itemTable.Add(kvp.Key, kvp.Value);

            foreach (KeyValuePair<int, MaterialJson> kvp in gameData.Material)
                itemTable.Add(kvp.Key, kvp.Value);

            foreach (KeyValuePair<int, LuckyPickJson> kvp in gameData.LuckyPick)
                itemTable.Add(kvp.Key, kvp.Value);

            foreach (KeyValuePair<int, HenshinJson> kvp in gameData.Henshin)
                itemTable.Add(kvp.Key, kvp.Value);

            foreach (KeyValuePair<int, FeaturesJson> kvp in gameData.Features)
                itemTable.Add(kvp.Key, kvp.Value);
            
            foreach (KeyValuePair<int, EquipmentJson> kvp in gameData.Equipment)
                itemTable.Add(kvp.Key, kvp.Value);

            foreach (KeyValuePair<int, DNAJson> kvp in gameData.DNA)
                itemTable.Add(kvp.Key, kvp.Value);

            foreach (KeyValuePair<int, RelicJson> kvp in gameData.Relic)
                itemTable.Add(kvp.Key, kvp.Value);

            foreach (KeyValuePair<int, QuestItemJson> kvp in gameData.QuestItem)
                itemTable.Add(kvp.Key, kvp.Value);

            foreach (KeyValuePair<int, HeroItemJson> kvp in gameData.HeroItem)
                itemTable.Add(kvp.Key, kvp.Value);

            foreach (KeyValuePair<int, InstanceItemJson> kvp in gameData.InstanceItem)
                itemTable.Add(kvp.Key, kvp.Value);        
        }

        public int GetItemMaxStackCount(BagType type)
        {
            return (type != BagType.Equipment) ? GameConstantRepo.ItemMaxStackCount : 1;
        }

        public ItemBaseJson GetItemById(int itemid)
        {
            if (itemTable.ContainsKey(itemid))
                return itemTable[itemid];
            return null;
        }

        public IInventoryItem GetInventoryItem(int itemid)
        {
            ItemBaseJson itemjson = GetItemById(itemid);
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
            int itemid = (int)jObject["id"];
            ItemBaseJson itemjson = GetItemById(itemid);
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
                    default:
                        return null;
                }

                retItem.LoadJson(itemjson);
                retItem.CalculateItemScore();
                return retItem;
            }
            return null;
        }

        public IInventoryItem GetInventoryItemCopy(IInventoryItem invItem)
        {
            invItem.EncodeItem();
            return GetItemFromCode(invItem.GetItemCodeForLocalObj());
        }

        public int GetItemSortTableLength()
        {
            return itemSortTable.Count;
        }

        public int GetItemOrderById(int id)
        {
            if (itemSortTable.ContainsKey(id))
                return itemSortTable[id].sortorder;
            return 0;
        }

        public LinkUIJson GetLinkUI(int uiid)
        {
            LinkUIJson _ret = null;
            linkUITable.TryGetValue(uiid, out _ret);
            return null;    
        }

        public List<ItemOriginJson> GetItemOriginJson(int itemid)
        {
            List<ItemOriginJson> _result = new List<ItemOriginJson>();
            ItemBaseJson _itemJson = GetItemById(itemid);
            if (_itemJson != null)
            {
                string _origin = _itemJson.origin;
                if (_origin != "-1" && _origin != "")
                {
                    string[] _originids = _origin.Split(';');
                    for (int index = 0; index < _originids.Length; index++)
                    {
                        int _originid;
                        if (int.TryParse(_originids[index], out _originid) && itemOriginTable.ContainsKey(_originid))
                            _result.Add(itemOriginTable[_originid]);
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
            return (int)(itemCode & (uint)ItemCodeMask.ID);
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
