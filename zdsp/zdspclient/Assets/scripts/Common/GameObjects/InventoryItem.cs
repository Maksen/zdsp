using System;
using System.Text;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using Kopio.JsonContracts;
using Zealot.Repository;

namespace Zealot.Common
{
    public enum ItemEncodingType
    {
        Integer = 0,
        String
    }

    enum ItemBits
    {
        ItemID = 15,
        Stack = 15,
        Bound = 1,
        New = 1,

        StackShift = ItemID,
        BoundShift = StackShift + Stack,
        NewShift = BoundShift + New,
    }

    [Flags]
    enum ItemCodeMask
    {
        ID = (1 << ItemBits.ItemID) - 1,
        STACK = (1 << ItemBits.Stack) - 1,
        BOUND = 1 << ItemBits.BoundShift,
        NEW = 1 << ItemBits.NewShift,
    }

    [JsonObject(MemberSerialization = MemberSerialization.OptIn)]
    public class IItemData
    {
        [DefaultValue(0)]
        [JsonProperty(PropertyName = "itemkindid")]
        public ushort ItemKindID { get; set; }

        [DefaultValue(0)]
        [JsonProperty(PropertyName = "itemid")]
        public ushort ItemID { get; set; }

        [DefaultValue(0)]
        [JsonProperty(PropertyName = "dailycharges")]
        public ushort DailyCharges { get; set; }

        [DefaultValue("")]
        //[JsonProperty(PropertyName = "lastusedt")]
        public DateTime LastUsedt { get; set; }

        public IItemData() { }

        public IItemData(ushort itemkindid, ushort itemid, ushort dailycharges, DateTime lastusedt)
        {
            ItemKindID = itemkindid;
            ItemID = itemid;
            DailyCharges = dailycharges;
            LastUsedt = lastusedt;
        }
    }

    [JsonObject(MemberSerialization = MemberSerialization.OptIn)]
    public class ItemKindData
    {
        [JsonProperty]
        public List<IItemData> Slots = new List<IItemData>();

        public void InitDefault()
        {
            int max = GameRepo.ItemFactory.GetItemSortTableLength();
            for (int i = 1; i <= max; ++i)
                Slots.Add(new IItemData((ushort)i, 0, 0, DateTime.MinValue));
        }

        public IItemData GetDataByItemKindId(int id)
        {
            return Slots.FirstOrDefault(o => o.ItemKindID == id);
        }

        public IItemData GetDataByItemId(int id)
        {
            return Slots.FirstOrDefault(o => o.ItemID == id);
        }

        public int GetIndexByItemKindId(int id)
        {
            return Slots.FindIndex(o => o.ItemKindID == id);
        }

        public int GetIndexByItemId(int id)
        {
            return Slots.FindIndex(o => o.ItemID == id);
        }

        public int GetEmptySlot()
        {
            int index = Slots.FindIndex(o => o == null);
            return index == -1 ? Slots.Count : index;
        }

        public void SetDataBySlotId(int slotid, IItemData data)
        {
            if (slotid < Slots.Count)
                Slots[slotid] = data;
            else
                Slots.Add(data);
        }

        public void NewDayReset()
        {
            Slots.ForEach(o => o.DailyCharges = 0);
        }
    }

    [JsonObject(MemberSerialization = MemberSerialization.OptIn)]
    public interface IInventoryItem
    {
        #region GameDB Properties   
        ItemBaseJson JsonObject { get; set; }

        int SortOrder { get; set; }

        ushort MaxStackCount { get; set; }
        #endregion

        [JsonProperty(PropertyName = "id")]
        ushort ItemID { get; set; }

        [DefaultValue(1)]
        [JsonProperty(PropertyName = "stack")]
        int StackCount { get; set; }

        [DefaultValue(false)]
        [JsonProperty(PropertyName = "bound")]
        bool Bound { get; set; }

        [DefaultValue("")]
        [JsonProperty]
        string UID { get; set; }

        //[DefaultValue(0)]
        //int ItemCode { get; set; }

        void LoadJson(ItemBaseJson jsonobject);
        void EncodeItem();

        void InitFromCode(int itemCode);
        void InitFromCode(string itemCode, bool base64encode = false);

        string GetStrEncodedItemCode(bool base64encode = false);
        object GetItemCodeForLocalObj();
        void CalculateItemScore();

        void SetNewItem();
        void ResetNewItem();
        bool IsNew { get; }

        ItemEncodingType EncodingType { get; }
    }

    public abstract class ItemBase : IInventoryItem
    {
        #region GameDB Properties   
        public ItemBaseJson JsonObject { get; set; }
        public int SortOrder { get; set; }
        public ushort MaxStackCount { get; set; }
        #endregion

        public ushort ItemID { get; set; }
        public virtual int StackCount { get; set; }
        public bool Bound { get; set; }
        public string UID { get; set; }
        protected int ItemCode { get; set; }
        protected bool newItem { get; set; }

        public virtual ItemEncodingType EncodingType { get { return ItemEncodingType.Integer; } }

        public ItemBase()
        {
            JsonObject = null;
            StackCount = 1;
            Bound = false;
            UID = "";
        }

        public virtual void LoadJson(ItemBaseJson jsonobject)
        {
            JsonObject = jsonobject;
            ItemID = (ushort)jsonobject.itemid;
            MaxStackCount = (ushort)GameRepo.ItemFactory.GetItemMaxStackCount(jsonobject.bagtype);
            SortOrder = GameRepo.ItemFactory.GetItemOrderById(jsonobject.itemsort);
        }

        public virtual void EncodeItem()
        {
            int stackmask = StackCount << (int)ItemBits.StackShift;
            int boundmask = Bound ? 1 << (int)ItemBits.BoundShift : 0;
            int newmask = IsNew ? 1 << (int)ItemBits.NewShift : 0;
            ItemCode = (newmask | boundmask | stackmask | ItemID);
        }

        #region InitFromCode
        public virtual void InitFromCode(int itemCode)
        {
            StackCount = (ushort)((itemCode >> (int)ItemBits.StackShift) & (int)ItemCodeMask.STACK);
            Bound = (itemCode & (int)ItemCodeMask.BOUND) > 0 ? true : false;
            newItem = (itemCode & (int)ItemCodeMask.NEW) > 0 ? true : false;
            ItemID = (ushort)(itemCode & (uint)ItemCodeMask.ID);
        }

        public virtual void InitFromCode(string itemCode, bool base64encode = false)
        {
            if (base64encode) // Used in chat
            {
                byte[] byteArr = Convert.FromBase64String(itemCode);
                itemCode = BitConverter.ToInt32(byteArr, 0).ToString();
            }
            int code;
            if (int.TryParse(itemCode, out code))
            {
                InitFromCode(code);
            }
        }
        #endregion

        public virtual string GetStrEncodedItemCode(bool base64encode = false)
        {
            //throw new Exception("str item encoding should not be called for non equipment item");
            // NOTE: This is used only in chat!!
            EncodeItem();
            var byteArr = BitConverter.GetBytes(ItemCode);
            if (base64encode)
                return Convert.ToBase64String(byteArr);
            else
                return Encoding.Default.GetString(byteArr);
        }

        public virtual object GetItemCodeForLocalObj() { return ItemCode; }

        public virtual void CalculateItemScore() { return; }

        public virtual void SetNewItem() { newItem = true; }

        public virtual void ResetNewItem() { newItem = false; }

        public bool IsNew { get { return newItem; } }
    }

    #region ZDSP Items
    public class PotionFood : ItemBase
    {
        #region GameDB Properties
        public PotionFoodJson PotionFoodJson { get; set; }
        #endregion

        public override void LoadJson(ItemBaseJson jsonobject)
        {
            base.LoadJson(jsonobject);
            PotionFoodJson = jsonobject as PotionFoodJson;
        }
    }

    public class MaterialItem : ItemBase
    {
        #region GameDB Properties
        public MaterialJson MaterialJson { get; set; }
        #endregion

        public override void LoadJson(ItemBaseJson jsonobject)
        {
            base.LoadJson(jsonobject);
            MaterialJson = jsonobject as MaterialJson;
        }
    }

    public class LuckyPick : ItemBase
    {
        #region GameDB Properties
        public LuckyPickJson LuckyPickJson { get; set; }
        #endregion

        public override void LoadJson(ItemBaseJson jsonobject)
        {
            base.LoadJson(jsonobject);
            LuckyPickJson = jsonobject as LuckyPickJson;
        }
    }

    public class Henshin : ItemBase
    {
        #region GameDB Properties
        public HenshinJson HenshinJson { get; set; }
        #endregion

        public override void LoadJson(ItemBaseJson jsonobject)
        {
            base.LoadJson(jsonobject);
            HenshinJson = jsonobject as HenshinJson;
        }
    }

    public class Features : ItemBase
    {
        #region GameDB Properties
        public FeaturesJson FeaturesJson { get; set; }
        #endregion

        public override void LoadJson(ItemBaseJson jsonobject)
        {
            base.LoadJson(jsonobject);
            FeaturesJson = jsonobject as FeaturesJson;
        }
    }
    
    public class Equipment : ItemBase
    {
        #region GameDB Properties   
        public EquipmentJson EquipmentJson { get; set; }

        public override ItemEncodingType EncodingType { get { return ItemEncodingType.String; } }
        #endregion

        public struct EquipmentAttribute
        {
            public ushort itemId;
            public byte newItem;
            public ushort upgradeLevel;
            public ushort reformStep;
            public ushort selection1;
            public ushort selection2;
            public ushort selection3;
            public ushort selection4;
            public ushort selection5;
            public ushort selection6;
            public ushort selection7;
            public ushort selection8;
            public ushort selection9;
            public ushort selection10;
            public ushort selection11;
            public ushort selection12;
            public ushort selection13;
            public ushort selection14;
            public ushort selection15;
            //public ushort surmountLevel;
            //public ushort basicAbilityType;
            //public uint basicAbilityVal;
        }
        public EquipmentAttribute equipAttr;
        string equipmentItemCode;

        [JsonProperty(PropertyName = "0")]
        public ushort UpgradeLevel { get; set; }

        [JsonProperty(PropertyName = "1")]
        public ushort ReformStep { get; set; }

        [JsonProperty(PropertyName = "2")]
        public string Selection { get; set; }

        //[JsonProperty(PropertyName = "2")]
        //public ushort BasicAttributeType { get; set; }

        //[JsonProperty(PropertyName = "3")]
        //public uint BasicAttributeVal { get; set; }

        //[JsonProperty(PropertyName = "4")]
        //public string GemIDs { get; set; }

        //[JsonProperty(PropertyName = "5")]
        //public string GemAttributes { get; set; }

        public Equipment()
        {
            if (string.IsNullOrEmpty(Selection))
            {
                Selection = "-|-|-|-|-|-|-|-|-|-|-|-|-|-|-";
            }
        }

        public override void LoadJson(ItemBaseJson jsonobject)
        {
            base.LoadJson(jsonobject);
            EquipmentJson = jsonobject as EquipmentJson;
        }

        public override void EncodeItem()
        {
            //base.EncodeItem();
            //ItemCode |= (UpgradeLevel << (int)ItemBits.UpgradeLevelShift);
            equipmentItemCode = EncodeEquipmentItem();
        }

        public override void InitFromCode(int itemCode)
        {
            //base.InitFromCode(itemCode);
            //UpgradeLevel = (byte)((itemCode >> (int)ItemBits.UpgradeLevelShift) & (int)ItemCodeMask.UPGRADELEVEL);
            throw new Exception("should not init equipment from integer code");
        }

        public override void InitFromCode(string itemCode, bool base64encode = false)
        {
            DecodeEquipmentItem(itemCode, base64encode);
        }

        public override string GetStrEncodedItemCode(bool base64encode = false)
        {
            return EncodeEquipmentItem(base64encode);
        }

        public override object GetItemCodeForLocalObj()
        {
            return equipmentItemCode;
        }

        byte PackBoundUpgradelvl(int bound, int upgradelvl)
        {
            return (byte)((bound) | (upgradelvl << 1));
        }

        void UnpackBoundUpgradelvl(byte val, out int a, out int upgradelevel)
        {
            a = val & 00000001;
            upgradelevel = (val) >> 1;
        }

        string EncodeEquipmentItem(bool base64encode = false)
        {
            equipAttr.itemId = ItemID;
            equipAttr.newItem = (byte)(newItem ? 1 : 0);
            equipAttr.upgradeLevel = UpgradeLevel;

            var arrProperties = getBytes(ref equipAttr);
            if (base64encode)
                equipmentItemCode = Convert.ToBase64String(arrProperties);
            else
                equipmentItemCode = Encoding.Unicode.GetString(arrProperties);

            return equipmentItemCode;
        }

        void DecodeEquipmentItem(string propertystr, bool base64encode = false)
        {
            byte[] attributes = null;
            if (base64encode)
                attributes = Convert.FromBase64String(propertystr);
            else
                attributes = Encoding.Unicode.GetBytes(propertystr);

            if (attributes.Length > 0)
            {
                ItemID = getUInt16FromBytes(attributes[0], attributes[1]);
                newItem = attributes[2] > 0;
                UpgradeLevel = getUInt16FromBytes(attributes[3], attributes[4]);
                ReformStep = getUInt16FromBytes(attributes[5], attributes[6]);
                DecodeSelectionsToSingle(attributes);
            }
        }

        private void EncodeSelectionForSync()
        {
            List<ushort> selections = GetSelectionsList();

            equipAttr.selection1    = selections[0];
            equipAttr.selection2    = selections[1];
            equipAttr.selection3    = selections[2];
            equipAttr.selection4    = selections[3];
            equipAttr.selection5    = selections[4];
            equipAttr.selection6    = selections[5];
            equipAttr.selection7    = selections[6];
            equipAttr.selection8    = selections[7];
            equipAttr.selection9    = selections[8];
            equipAttr.selection10   = selections[9];
            equipAttr.selection11   = selections[10];
            equipAttr.selection12   = selections[11];
            equipAttr.selection13   = selections[12];
            equipAttr.selection14   = selections[13];
            equipAttr.selection15   = selections[14];
        }

        private void DecodeSelectionsToSingle(byte[] attributes)
        {
            StringBuilder selectionStr = new StringBuilder(DecodeSelection(getUInt16FromBytes(attributes[7], attributes[8])));
            selectionStr.Append("|");
            selectionStr.Append(DecodeSelection(getUInt16FromBytes(attributes[9], attributes[10])));
            selectionStr.Append("|");
            selectionStr.Append(DecodeSelection(getUInt16FromBytes(attributes[11], attributes[12])));
            selectionStr.Append("|");
            selectionStr.Append(DecodeSelection(getUInt16FromBytes(attributes[13], attributes[14])));
            selectionStr.Append("|");
            selectionStr.Append(DecodeSelection(getUInt16FromBytes(attributes[15], attributes[16])));
            selectionStr.Append("|");
            selectionStr.Append(DecodeSelection(getUInt16FromBytes(attributes[17], attributes[18])));
            selectionStr.Append("|");
            selectionStr.Append(DecodeSelection(getUInt16FromBytes(attributes[19], attributes[20])));
            selectionStr.Append("|");
            selectionStr.Append(DecodeSelection(getUInt16FromBytes(attributes[21], attributes[22])));
            selectionStr.Append("|");
            selectionStr.Append(DecodeSelection(getUInt16FromBytes(attributes[23], attributes[24])));
            selectionStr.Append("|");
            selectionStr.Append(DecodeSelection(getUInt16FromBytes(attributes[25], attributes[26])));
            selectionStr.Append("|");
            selectionStr.Append(DecodeSelection(getUInt16FromBytes(attributes[27], attributes[28])));
            selectionStr.Append("|");
            selectionStr.Append(DecodeSelection(getUInt16FromBytes(attributes[29], attributes[30])));
            selectionStr.Append("|");
            selectionStr.Append(DecodeSelection(getUInt16FromBytes(attributes[31], attributes[32])));
            selectionStr.Append("|");
            selectionStr.Append(DecodeSelection(getUInt16FromBytes(attributes[33], attributes[34])));
            selectionStr.Append("|");
            selectionStr.Append(DecodeSelection(getUInt16FromBytes(attributes[35], attributes[36])));
            Selection = selectionStr.ToString();
        }

        public List<ushort> GetSelectionsList()
        {
            List<string> selectionList = Selection.Split('|').ToList();
            List<ushort> selections = new List<ushort>();
            for(int i = 0; i < selectionList.Count; ++i)
            {
                ushort selection = ushort.MaxValue;
                if(ushort.TryParse(selectionList[i], out selection))
                {
                    selections.Add(selection);
                }
                else
                {
                    selections.Add(ushort.MaxValue);
                }
            }

            return selections;
        }

        private string DecodeSelection(ushort selection)
        {
            string realSel = "";

            if(selection >= 15)
            {
                realSel = "-";
            }
            else
            {
                realSel = selection.ToString();
            }

            return realSel;
        }

        uint getUInt32FromBytes(byte byte1, byte byte2, byte byte3, byte byte4)
        {
            uint result;
            byte[] barr = new byte[4] { byte1, byte2, byte3, byte4 };
            result = BitConverter.ToUInt32(barr, 0);

            return result;
        }

        ushort getUInt16FromBytes(byte byte1, byte byte2)
        {
            ushort result;
            byte[] barr = new byte[2] { byte1, byte2 };
            result = BitConverter.ToUInt16(barr, 0);

            return result;
        }

        byte[] getBytes(ref EquipmentAttribute ea)
        {
            int size = Marshal.SizeOf(ea);
            byte[] arr = new byte[size];
            IntPtr ptr = Marshal.AllocHGlobal(size);
            Marshal.StructureToPtr(ea, ptr, true);
            Marshal.Copy(ptr, arr, 0, size);
            Marshal.FreeHGlobal(ptr);

            return arr;
        }

        EquipmentAttribute fromBytes(byte[] arr)
        {
            EquipmentAttribute str = new EquipmentAttribute();

            int size = Marshal.SizeOf(str);
            IntPtr ptr = Marshal.AllocHGlobal(size);

            Marshal.Copy(arr, 0, ptr, size);

            str = (EquipmentAttribute)Marshal.PtrToStructure(ptr, str.GetType());
            Marshal.FreeHGlobal(ptr);

            return str;
        }

        public string GetEquipmentName()
        {
            return EquipmentJson.localizedname;
        }
    }

    public class DNA : ItemBase
    {
        #region GameDB Properties
        public DNAJson DNAJson { get; set; }
        #endregion

        public override void LoadJson(ItemBaseJson jsonobject)
        {
            base.LoadJson(jsonobject);
            DNAJson = jsonobject as DNAJson;
        }
    }

    public class Relic : ItemBase
    {
        #region GameDB Properties
        public RelicJson RelicJson { get; set; }
        #endregion

        public override void LoadJson(ItemBaseJson jsonobject)
        {
            base.LoadJson(jsonobject);
            RelicJson = jsonobject as RelicJson;
        }
    }

    public class QuestItem : ItemBase
    {
        #region GameDB Properties
        public QuestItemJson QuestItemJson { get; set; }
        #endregion

        public override void LoadJson(ItemBaseJson jsonobject)
        {
            base.LoadJson(jsonobject);
            QuestItemJson = jsonobject as QuestItemJson;
        }
    }

    public class HeroItem : ItemBase
    {
        #region GameDB Properties
        public HeroItemJson HeroItemJson { get; set; }
        #endregion

        public override void LoadJson(ItemBaseJson jsonobject)
        {
            base.LoadJson(jsonobject);
            HeroItemJson = jsonobject as HeroItemJson;
        }
    }

    public class InstanceItem : ItemBase
    {
        #region GameDB Properties
        public InstanceItemJson InstanceItemJson { get; set; }
        #endregion

        public override void LoadJson(ItemBaseJson jsonobject)
        {
            base.LoadJson(jsonobject);
            InstanceItemJson = jsonobject as InstanceItemJson;
        }
    }
    #endregion

    #region Item JsonConverters
    public class DBInventoryItemConverter : CustomCreationConverter<IInventoryItem>
    {
        public override IInventoryItem Create(Type objectType)
        {
            return null;
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null || GameRepo.ItemFactory == null)
                return null;

            JObject jObject = JObject.Load(reader);
            return GameRepo.ItemFactory.GetInventoryItem(jObject);
        }
    }

    public class ClientInventoryItemConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return typeof(IInventoryItem).IsAssignableFrom(objectType);
        }
        
        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null)
                return null;

            JObject jObject = JObject.Load(reader);

            string itemcode = jObject["ic"].ToObject<string>();
            int encodetype = (int)jObject["t"];
            var clientItemFactory = GameRepo.ItemFactory as ClientItemFactory;
            return clientItemFactory.GetItemFromCode(itemcode, (ItemEncodingType)encodetype);
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            IInventoryItem item = value as IInventoryItem;
            item.EncodeItem();

            JObject o = new JObject();
            o.Add("t", (int)item.EncodingType);
            o.Add("ic", JToken.FromObject(item.GetItemCodeForLocalObj()));
            o.WriteTo(writer);
        }
    }
    #endregion
}
