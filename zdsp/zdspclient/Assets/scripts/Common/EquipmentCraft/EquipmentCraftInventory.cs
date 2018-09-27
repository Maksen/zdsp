using System.Collections.Generic;
using Newtonsoft.Json;

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

        [JsonProperty(PropertyName = "achievementRequireList")]
        public Dictionary<int, int> achievementRequireList = new Dictionary<int, int>();
        #endregion

        public const int MAX_EQUIPMENTACHIEVEMENT = 500;

        public EquipmentCraftInventoryData()
        {
            InitDefault();
        }

        public void InitDefault()
        {
            EquipmentCrafted = false;
            if (achievementRequireList.Count == 0)
            {
                achievementRequireList = new Dictionary<int, int>();
            }
        }
    }
}