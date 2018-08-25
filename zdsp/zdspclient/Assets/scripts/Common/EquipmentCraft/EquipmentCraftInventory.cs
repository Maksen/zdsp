using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using Zealot.Common.Entities;

namespace Zealot.Common
{
    public partial class EquipmentCraftInventoryData
    {
        public enum EquipmentSlot
        {
            Sword,
            Blade,
            Lance,
            Hammer,
            Fan,
            Xbow,
            Dagger,
            Sanxian,
            Helm,
            Body,
            Wing,
            Boots,
            Bathrobe,
            Ring,
            Jewelry,
            Accessory,
            MAXSOLTS
        };

        #region serializable properties
        [JsonProperty(PropertyName = "EquipmentCraft")]
        public bool EquipmentCrafted;

        [JsonProperty(PropertyName = "EquipmentSlots")]
        public Dictionary<int, int> EquipmentCraftSlots = new Dictionary<int, int>();
        #endregion

                                                            //totalCraftItem 
        //public const int MAX_EQUIPMENTCRAFTSLOTS_LEN = (int)EquipmentSlot.MAXSOLTS;
        public const int MAX_EQUIPMENTCRAFTSLOTS_LEN = 200;

        public EquipmentCraftInventoryData()
        {
            EquipmentCrafted = false;
            if (EquipmentCraftSlots.Count == 0)
            {
                EquipmentCraftSlots = new Dictionary<int, int>();
            }
        }

        public void InitDefault()
        {
            EquipmentCrafted = false;
            if (EquipmentCraftSlots.Count == 0)
            {
                EquipmentCraftSlots = new Dictionary<int, int>();
            }
        }
    }
}