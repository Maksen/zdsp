using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Zealot.Client.Entities;
using Zealot.Common;
using Zealot.Repository;

public class UI_Hero_AddSkillPointsDialog : MonoBehaviour
{
    [SerializeField] Transform iconTransform;
    [SerializeField] GameObject iconPrefab;
    [SerializeField] Text itemName;
    [SerializeField] Text amtText;
    [SerializeField] Text pointsText;

    private GameIcon_MaterialConsumable item;
    private int heroId;
    private int bindItemId, unbindItemId;
    private bool showSpendConfirmation;

    public void Init(Hero hero, int overrideSkillPts = 0)
    {
        PlayerGhost player = GameInfo.gLocalPlayer;
        heroId = hero.HeroId;
        if (item == null)
        {
            GameObject icon = ClientUtils.CreateChild(iconTransform, iconPrefab);
            item = icon.GetComponent<GameIcon_MaterialConsumable>();
        }

        string itemIds = hero.HeroJson.upgradeitemid;
        if (!string.IsNullOrEmpty(itemIds))
        {
            string[] itemids = itemIds.Split(';');
            if (itemids.Length > 0 && int.TryParse(itemids[0], out bindItemId))
            {
                item.InitWithToolTipView(bindItemId, 1);
                itemName.text = item.InventoryItem.JsonObject.localizedname;

                int bindCount = player.clientItemInvCtrl.itemInvData.GetTotalStackCountByItemId((ushort)bindItemId);
                int unbindCount = 0;
                if (itemids.Length > 1 && int.TryParse(itemids[1], out unbindItemId))
                    unbindCount = player.clientItemInvCtrl.itemInvData.GetTotalStackCountByItemId((ushort)unbindItemId);
                int totalCount = bindCount + unbindCount;
                string avail = totalCount > 999 ? "999+" : totalCount.ToString();
                int requiredItemCount = HeroRepo.GetItemCountForSkillPointByHeroId(hero.HeroId, hero.GetTotalSkillPoints() + 1);
                bool enough = totalCount >= requiredItemCount;
                if (!enough)
                    avail = string.Format("<color=red>{0}</color>", avail);
                string req = requiredItemCount > 0 ? requiredItemCount.ToString() : "-";
                amtText.text = avail + " / " + req;
                showSpendConfirmation = enough && bindCount < requiredItemCount;  // show confirmation whether want to spend unbind
            }
        }

        pointsText.text = overrideSkillPts > 0 ? overrideSkillPts.ToString() : hero.SkillPoints.ToString();
    }

    public void OnClickAddSkillPoint()
    {
        if (showSpendConfirmation)
        {
            IInventoryItem bindItem = item.InventoryItem;
            IInventoryItem unbindItem = GameRepo.ItemFactory.GetInventoryItem(unbindItemId);
            if (bindItem != null && unbindItem != null)
            {
                Dictionary<string, string> param = new Dictionary<string, string>();
                param.Add("bind", bindItem.JsonObject.localizedname);
                param.Add("unbind", unbindItem.JsonObject.localizedname);
                string message = GUILocalizationRepo.GetLocalizedString("hro_confirmUseUnbindToAddSkillPoint", param);
                UIManager.OpenYesNoDialog(message, OnConfirm);
            }
        }
        else
        {
            OnConfirm();
        }
    }

    private void OnConfirm()
    {
        RPCFactory.CombatRPC.AddHeroSkillPoint(heroId);
    }
}
