using UnityEngine;
using UnityEngine.UI;
using Zealot.Client.Entities;

public class UI_Hero_AddSkillPointsDialog : BaseWindowBehaviour
{
    [SerializeField] Transform iconTransform;
    [SerializeField] GameObject iconPrefab;
    [SerializeField] Text itemName;
    [SerializeField] Text amtText;
    [SerializeField] Text pointsText;

    private GameIcon_MaterialConsumable item;
    private int heroId;

    public void Init(Hero hero)
    {
        PlayerGhost player = GameInfo.gLocalPlayer;
        heroId = hero.HeroId;
        if (item == null)
        {
            GameObject icon = ClientUtils.CreateChild(iconTransform, iconPrefab);
            item = icon.GetComponent<GameIcon_MaterialConsumable>();
        }

        int itemId = hero.HeroJson.upgradeitemid;
        item.Init(itemId, 0);
        itemName.text = item.inventoryItem.JsonObject.localizedname;

        int count = player.clientItemInvCtrl.itemInvData.GetTotalStackCountByItemId((ushort)itemId);
        string avail = count > 999 ? "999+" : count.ToString();
        amtText.text = avail + " / " + hero.HeroJson.upgradeitemcount;

        pointsText.text = hero.SkillPoints.ToString();
    }

    public void OnClickAddSkillPoint()
    {
        RPCFactory.CombatRPC.AddHeroSkillPoint(heroId);
    }
}