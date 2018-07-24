using Zealot.Common;
using Zealot.Repository;
using Zealot.Server.Entities;

namespace Photon.LoadBalancing.GameServer
{
    public class WardrobeController
    {
        private Player player;

        public WardrobeController(Player player)
        {
            this.player = player;
        }

        public void Init()
        {
            player.PlayerSynStats.MountID = player.Slot.CharacterData.MountID;

            if (player.Slot.CharacterData.MountID < 0)
                player.PlayerSynStats.MoveSpeed = 15;
        }

        public void Save()
        {
            player.Slot.CharacterData.MountID = player.PlayerSynStats.MountID;
        }

        private void ReturnCode(WardrobeRetCode code)
        {
            player.Slot.ZRPC.CombatRPC.Ret_OnUpdateWardrobe((byte)code, player.Slot);
        }

        public void EquipFashion(int item_id)
        {
            //var requirement = FashionRequirement.GetByItemID(item_id);
            //if (requirement == null || requirement.CanEquip((JobType)player.PlayerSynStats.jobsect) == false)
            {
                ReturnCode(WardrobeRetCode.Failed);
                return;
            }

            if (!player.Slot.mInventory.HasItem((ushort)item_id, 1))
            {
                ReturnCode(WardrobeRetCode.Failed);
                return;
            }

            /*switch (requirement.itemData.prefabpathtype)
            {
                case PrefabPathType.Mount:
                    player.PlayerSynStats.MountID = -item_id;
                    player.PlayerSynStats.moveSpeed = 15;
                    break;
            }*/

            ReturnCode(WardrobeRetCode.EquipSuccess);
        }

        public void UnequipFashion(int item_id)
        {
            //var requirement = FashionRequirement.GetByItemID(item_id);
            //if (requirement == null)
            {
                ReturnCode(WardrobeRetCode.Failed);
                return;
            }

            /*switch (requirement.itemData.prefabpathtype)
            {
                case PrefabPathType.Mount:
                    player.PlayerSynStats.MountID = 0;
                    player.PlayerSynStats.moveSpeed = 6;
                    break;
            }*/

            ReturnCode(WardrobeRetCode.UnequipSuccess);
        }
    }
}
