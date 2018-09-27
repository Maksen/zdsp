using Kopio.JsonContracts;
using System;
using System.Text;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
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
            int max = GameRepo.ItemFactory.ItemSortTable.Count;
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
        ItemSortJson ItemSortJson { get; set; }
        int SortOrder { get; set; }
        ushort MaxStackCount { get; set; }
        #endregion

        #region serializable properties
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
        #endregion

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
        public ItemSortJson ItemSortJson { get; set; }
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
            ItemSortJson = GameRepo.ItemFactory.GetItemSortById(jsonobject.itemsort);
            SortOrder = ItemSortJson.sortorder;
            MaxStackCount = (ushort)GameRepo.ItemFactory.GetItemMaxStackCount(ItemSortJson.bagtabtype);
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
            ItemID = (ushort)(itemCode & (int)ItemCodeMask.ID);
            StackCount = (ushort)((itemCode >> (int)ItemBits.StackShift) & (int)ItemCodeMask.STACK);
            Bound = (uint)(itemCode & (int)ItemCodeMask.BOUND) > 0 ? true : false;
            newItem = (uint)(itemCode & (int)ItemCodeMask.NEW) > 0 ? true : false;
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

            public uint groupId1;
            public uint effectId1;
            public uint effectValue1;
            public uint effectId2;
            public uint effectValue2;
            public uint groupId2;
            public uint effectId3;
            public uint effectValue3;
            public uint effectId4;
            public uint effectValue4;
            public uint groupId3;
            public uint effectId5;
            public uint effectValue5;
            public uint effectId6;
            public uint effectValue6;
        }
        public EquipmentAttribute equipAttr;
        string equipmentItemCode;

        [JsonProperty(PropertyName = "0")]
        public ushort UpgradeLevel { get; set; }

        [JsonProperty(PropertyName = "1")]
        public ushort ReformStep { get; set; }

        [JsonProperty(PropertyName = "2")]
        public string Selection { get; set; }

        [JsonProperty(PropertyName = "3")]
        public string FusionEffect { get; set; }

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
            if (string.IsNullOrEmpty(FusionEffect))
            {
                FusionEffect = "0|0,0|0,0|0|0,0|0,0|0|0,0|0,0";
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
            equipAttr.reformStep = ReformStep;
            EncodeSelectionForSync();
            EncodeFusionForSync();

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
                // Skipping attribute[3] due to element being used as padding for uneven struct
                // Remove attribute[3] from skipping if another byte is added after newItem
                UpgradeLevel = getUInt16FromBytes(attributes[4], attributes[5]);
                ReformStep = getUInt16FromBytes(attributes[6], attributes[7]);
                DecodeSelectionsToSingle(attributes);
                DecodeFusionToSingle(attributes);
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
        private void EncodeFusionForSync ()
        {
            List<uint> fusionEffects = GetEffectList();

            equipAttr.groupId1         = fusionEffects[0];
            equipAttr.effectId1        = fusionEffects[1];
            equipAttr.effectValue1     = fusionEffects[2];
            equipAttr.effectId2        = fusionEffects[3];
            equipAttr.effectValue2     = fusionEffects[4];
            equipAttr.groupId2         = fusionEffects[5];
            equipAttr.effectId3        = fusionEffects[6];
            equipAttr.effectValue3     = fusionEffects[7];
            equipAttr.effectId4        = fusionEffects[8];
            equipAttr.effectValue4     = fusionEffects[9];
            equipAttr.groupId3         = fusionEffects[10];
            equipAttr.effectId5        = fusionEffects[11];
            equipAttr.effectValue5     = fusionEffects[12];
            equipAttr.effectId6        = fusionEffects[13];
            equipAttr.effectValue6     = fusionEffects[14];
        }
        private void DecodeSelectionsToSingle(byte[] attributes)
        {
            StringBuilder selectionStr = new StringBuilder(DecodeSelection(getUInt16FromBytes(attributes[8], attributes[9])));
            selectionStr.Append("|");
            selectionStr.Append(DecodeSelection(getUInt16FromBytes(attributes[10], attributes[11])));
            selectionStr.Append("|");
            selectionStr.Append(DecodeSelection(getUInt16FromBytes(attributes[12], attributes[13])));
            selectionStr.Append("|");
            selectionStr.Append(DecodeSelection(getUInt16FromBytes(attributes[14], attributes[15])));
            selectionStr.Append("|");
            selectionStr.Append(DecodeSelection(getUInt16FromBytes(attributes[16], attributes[17])));
            selectionStr.Append("|");
            selectionStr.Append(DecodeSelection(getUInt16FromBytes(attributes[18], attributes[19])));
            selectionStr.Append("|");
            selectionStr.Append(DecodeSelection(getUInt16FromBytes(attributes[20], attributes[21])));
            selectionStr.Append("|");
            selectionStr.Append(DecodeSelection(getUInt16FromBytes(attributes[22], attributes[23])));
            selectionStr.Append("|");
            selectionStr.Append(DecodeSelection(getUInt16FromBytes(attributes[24], attributes[25])));
            selectionStr.Append("|");
            selectionStr.Append(DecodeSelection(getUInt16FromBytes(attributes[26], attributes[27])));
            selectionStr.Append("|");
            selectionStr.Append(DecodeSelection(getUInt16FromBytes(attributes[28], attributes[29])));
            selectionStr.Append("|");
            selectionStr.Append(DecodeSelection(getUInt16FromBytes(attributes[30], attributes[31])));
            selectionStr.Append("|");
            selectionStr.Append(DecodeSelection(getUInt16FromBytes(attributes[32], attributes[33])));
            selectionStr.Append("|");
            selectionStr.Append(DecodeSelection(getUInt16FromBytes(attributes[34], attributes[35])));
            selectionStr.Append("|");
            selectionStr.Append(DecodeSelection(getUInt16FromBytes(attributes[36], attributes[37])));
            Selection = selectionStr.ToString();
        }
        private void DecodeFusionToSingle (byte[] attributes)
        {
            StringBuilder fusionStr = new StringBuilder(getUInt32FromBytes(attributes[40], attributes[41], attributes[42], attributes[43]).ToString());
            fusionStr.Append("|");
            fusionStr.Append(DecodeFusionEffect(getUInt32FromBytes(attributes[44], attributes[45], attributes[46], attributes[47]), getUInt32FromBytes(attributes[48], attributes[49], attributes[50], attributes[51])));
            fusionStr.Append("|");
            fusionStr.Append(DecodeFusionEffect(getUInt32FromBytes(attributes[52], attributes[53], attributes[54], attributes[55]), getUInt32FromBytes(attributes[56], attributes[57], attributes[58], attributes[59])));
            fusionStr.Append("|");
            fusionStr.Append((getUInt32FromBytes(attributes[60], attributes[61], attributes[62], attributes[63]).ToString()));
            fusionStr.Append("|");
            fusionStr.Append(DecodeFusionEffect(getUInt32FromBytes(attributes[64], attributes[65], attributes[66], attributes[67]), getUInt32FromBytes(attributes[68], attributes[69], attributes[70], attributes[71])));
            fusionStr.Append("|");
            fusionStr.Append(DecodeFusionEffect(getUInt32FromBytes(attributes[72], attributes[73], attributes[74], attributes[75]), getUInt32FromBytes(attributes[76], attributes[77], attributes[78], attributes[79])));
            fusionStr.Append("|");
            fusionStr.Append(getUInt32FromBytes(attributes[80], attributes[81], attributes[82], attributes[83]).ToString());
            fusionStr.Append("|");
            fusionStr.Append(DecodeFusionEffect(getUInt32FromBytes(attributes[84], attributes[85], attributes[86], attributes[87]), getUInt32FromBytes(attributes[88], attributes[89], attributes[90], attributes[91])));
            fusionStr.Append("|");
            fusionStr.Append(DecodeFusionEffect(getUInt32FromBytes(attributes[92], attributes[93], attributes[94], attributes[95]), getUInt32FromBytes(attributes[96], attributes[97], attributes[98], attributes[99])));
            FusionEffect = fusionStr.ToString();
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

        private List<uint> GetEffectList()
        {
            List<string> decode = FusionEffect.Split(new char[] { '|', ',' }).ToList();
            List<uint> lis = new List<uint>();
            for (int i = 0; i < decode.Count; ++i)
            {
                uint selection = uint.MaxValue;
                if (uint.TryParse(decode[i], out selection))
                {
                    lis.Add(selection);
                }
                else
                {
                    lis.Add(0); lis.Add(0);
                }
            }
            return lis;
        }

        public void CompileSelectionsList(List<ushort> selectionsList)
        {
            StringBuilder selStr = new StringBuilder();
            for(int i = 0; i < selectionsList.Count; ++i)
            {
                ushort sel = selectionsList[i];
                if(sel == ushort.MaxValue)
                {
                    selStr.Append("-");
                }
                else
                {
                    selStr.Append(sel.ToString());
                }

                if(i < selectionsList.Count - 1)
                {
                    selStr.Append("|");
                }
            }

            Selection = selStr.ToString();
        }

        public void CompileFusionEffectsList(string effectsList)
        {
            FusionEffect = effectsList;
        }

        public ushort GetSelectionByReformStep(int reformStep)
        {
            int currStep = reformStep - 1;

            if(currStep > 0)
            {
                return GetSelectionsList()[currStep];
            }

            return ushort.MaxValue;
        }

        public string GetFusionEffect ()
        {
            return FusionEffect;
        }

        public void AddSelection(int reformStep, ushort selection)
        {
            int realStep = reformStep - 1;
            List<ushort> selections = GetSelectionsList();
            selections[realStep] = selection;
            CompileSelectionsList(selections);
        }

        public void RemoveSelection(int reformStep)
        {
            int realStep = reformStep - 1;
            List<ushort> selections = GetSelectionsList();
            selections[realStep] = ushort.MaxValue;
            CompileSelectionsList(selections);
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

        private string DecodeFusionEffect(uint id, uint value)
        {
            StringBuilder bind = new StringBuilder();
            bind.Append(id.ToString());
            bind.Append(",");
            bind.Append(value.ToString());

            return bind.ToString();
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

    public class ElementalStone : ItemBase
    {
        #region GameDB Properties
        public ElementalStoneJson ElementalStoneJson { get; set; }

        public override ItemEncodingType EncodingType { get { return ItemEncodingType.String; } }
        #endregion

        public struct FusionAttribute
        {
            public ushort itemId;
            public byte newItem;
            public uint groupId1;
            public uint effectId1;
            public uint effectValue1;
            public uint effectId2;
            public uint effectValue2;
            public uint groupId2;
            public uint effectId3;
            public uint effectValue3;
            public uint effectId4;
            public uint effectValue4;
            public uint groupId3;
            public uint effectId5;
            public uint effectValue5;
            public uint effectId6;
            public uint effectValue6;
        }
        public FusionAttribute fusionAttr;
        string equipFusionCode;

        [JsonProperty(PropertyName = "0")]
        public string FusionData { get; set; }

        public ElementalStone()
        {
            if (string.IsNullOrEmpty(FusionData))
            {
                FusionData = "0|0,0|0,0|0|0,0|0,0|0|0,0|0,0";
            }
        }

        public override void LoadJson(ItemBaseJson jsonobject)
        {
            base.LoadJson(jsonobject);
            ElementalStoneJson = jsonobject as ElementalStoneJson;
        }

        public override void EncodeItem()
        {
            if (FusionData == "0|0,0|0,0|0|0,0|0,0|0|0,0|0,0")
            {
                FusionData = EquipFusionRepo.RandomGemEffect(ItemID);
            }
            equipFusionCode = EncodeElementalStoneItem();
        }

        public override void InitFromCode(int fusionCode)
        {
            throw new Exception("should not init equipment from integer code");
        }

        public override void InitFromCode(string fusionCode, bool base64encode = false)
        {
            DecodeElementalStoneItem(fusionCode, base64encode);
        }

        public override string GetStrEncodedItemCode(bool base64encode = false)
        {
            return EncodeElementalStoneItem(base64encode);
        }

        public override object GetItemCodeForLocalObj()
        {
            return equipFusionCode;
        }

        string EncodeElementalStoneItem(bool base64encode = false)
        {
            fusionAttr.itemId = ItemID;
            fusionAttr.newItem = (byte)(newItem ? 1 : 0);
            EncodeEffectForSync();

            var arrproperties = getBytes(ref fusionAttr);
            if (base64encode)
                equipFusionCode = Convert.ToBase64String(arrproperties);
            else
                equipFusionCode = Encoding.Unicode.GetString(arrproperties);

            return equipFusionCode;
        }

        void DecodeElementalStoneItem(string propertystr, bool base64encode = false)
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
                DecodeElementalStoneToSingle(attributes);
            }
        }

        void EncodeEffectForSync()
        {
            List<uint> selections = GetEffectList();

            fusionAttr.groupId1 = selections[0];
            fusionAttr.effectId1 = selections[1];
            fusionAttr.effectValue1 = selections[2];
            fusionAttr.effectId2 = selections[3];
            fusionAttr.effectValue2 = selections[4];
            fusionAttr.groupId2 = selections[5];
            fusionAttr.effectId3 = selections[6];
            fusionAttr.effectValue3 = selections[7];
            fusionAttr.effectId4 = selections[8];
            fusionAttr.effectValue4 = selections[9];
            fusionAttr.groupId3 = selections[10];
            fusionAttr.effectId5 = selections[11];
            fusionAttr.effectValue5 = selections[12];
            fusionAttr.effectId6 = selections[13];
            fusionAttr.effectValue6 = selections[14];
        }

        void DecodeElementalStoneToSingle(byte[] decode)
        {
            StringBuilder bind = new StringBuilder(getUInt32FromBytes(decode[4], decode[5], decode[6], decode[7]).ToString());
            bind.Append("|");
            bind.Append(DecodeFusionData(getUInt32FromBytes(decode[8], decode[9], decode[10], decode[11]), getUInt32FromBytes(decode[12], decode[13], decode[14], decode[15])));
            bind.Append("|");
            bind.Append(DecodeFusionData(getUInt32FromBytes(decode[16], decode[17], decode[18], decode[19]), getUInt32FromBytes(decode[20], decode[21], decode[22], decode[23])));
            bind.Append("|");
            bind.Append(getUInt32FromBytes(decode[24], decode[25], decode[26], decode[27]).ToString());
            bind.Append("|");
            bind.Append(DecodeFusionData(getUInt32FromBytes(decode[28], decode[29], decode[30], decode[31]), getUInt32FromBytes(decode[32], decode[33], decode[34], decode[35])));
            bind.Append("|");
            bind.Append(DecodeFusionData(getUInt32FromBytes(decode[36], decode[37], decode[38], decode[39]), getUInt32FromBytes(decode[40], decode[41], decode[42], decode[43])));
            bind.Append("|");
            bind.Append(getUInt32FromBytes(decode[44], decode[45], decode[46], decode[47]).ToString());
            bind.Append("|");
            bind.Append(DecodeFusionData(getUInt32FromBytes(decode[48], decode[49], decode[50], decode[51]), getUInt32FromBytes(decode[52], decode[53], decode[54], decode[55])));
            bind.Append("|");
            bind.Append(DecodeFusionData(getUInt32FromBytes(decode[56], decode[57], decode[58], decode[59]), getUInt32FromBytes(decode[60], decode[61], decode[62], decode[63])));

            FusionData = bind.ToString();
        }

        private List<uint> GetEffectList()
        {
            List<string> decode = FusionData.Split(new char[] { '|', ',' }).ToList();
            List<uint> lis = new List<uint>();
            for (int i = 0; i < decode.Count; ++i)
            {
                uint selection = uint.MaxValue;
                if (uint.TryParse(decode[i], out selection))
                {
                    lis.Add(selection);
                } else
                {
                    lis.Add(selection);
                }
            }
            return lis;
        }

        private string DecodeFusionData(uint id, uint value)
        {
            StringBuilder bind = new StringBuilder();
            bind.Append(id.ToString());
            bind.Append(",");
            bind.Append(value.ToString());

            return bind.ToString();
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

        byte[] getBytes(ref FusionAttribute fa)
        {
            int size = Marshal.SizeOf(fa);
            byte[] arr = new byte[size];
            IntPtr ptr = Marshal.AllocHGlobal(size);
            Marshal.StructureToPtr(fa, ptr, true);
            Marshal.Copy(ptr, arr, 0, size);
            Marshal.FreeHGlobal(ptr);

            return arr;
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
