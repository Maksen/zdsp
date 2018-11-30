using Kopio.JsonContracts;
using UnityEngine;
using UnityEngine.UI;
using Zealot.Bot;
using Zealot.Client.Entities;
using Zealot.Common;
using Zealot.Entities;
using Zealot.Repository;

public class Achievement_ItemOriginData : MonoBehaviour
{
    [SerializeField] Image iconImage;
    [SerializeField] Text nameText;
    [SerializeField] Text levelText;
    [SerializeField] Text mapNameText;
    [SerializeField] Button button;

    public void Init(ItemOriginType originType, string param)
    {
        switch (originType)
        {
            case ItemOriginType.Monster:
                CombatNPCJson combatNPCJson = CombatNPCRepo.GetNPCByArchetype(param);
                if (combatNPCJson != null)
                {
                    iconImage.sprite = ClientUtils.LoadIcon(combatNPCJson.portraitpath);
                    nameText.text = combatNPCJson.localizedname;
                    levelText.text = GUILocalizationRepo.GetLocalizedString("com_level") + combatNPCJson.level;
                    string destLevel = "";
                    Vector3 destPos = Vector3.zero;
                    if (NPCPosMap.FindNearestMonster(combatNPCJson.archetype, ClientUtils.GetCurrentLevelName(), GameInfo.gLocalPlayer.Position, ref destLevel, ref destPos))
                    {
                        var levelData = LevelRepo.GetInfoByName(destLevel);
                        mapNameText.text = levelData.localizedname;
                        button.onClick.AddListener(() => PathFindToTarget(destLevel, destPos, ReachTargetAction.None));
                    }
                    else
                    {
                        mapNameText.text = "";
                        button.gameObject.SetActive(false);
                    }
                }
                break;
            case ItemOriginType.Item:
                int itemId;
                if (int.TryParse(param, out itemId))
                {
                    IInventoryItem item = GameRepo.ItemFactory.GetInventoryItem(itemId);
                    if (item != null)
                    {
                        iconImage.sprite = ClientUtils.LoadIcon(item.JsonObject.iconspritepath);
                        nameText.text = item.JsonObject.localizedname;
                        levelText.text = "";
                        mapNameText.text = "";
                        button.gameObject.SetActive(false);
                    }
                }
                break;
            case ItemOriginType.NPC:
                StaticNPCJson staticNPCJson = StaticNPCRepo.GetNPCByArchetype(param);
                if (staticNPCJson != null)
                {
                    iconImage.sprite = ClientUtils.LoadIcon(staticNPCJson.portraitpath);
                    nameText.text = staticNPCJson.localizedname;
                    levelText.text = "";
                    string destLevel = "";
                    Vector3 destPos = Vector3.zero;
                    if (NPCPosMap.FindNearestStaticNPC(staticNPCJson.archetype, ClientUtils.GetCurrentLevelName(), GameInfo.gLocalPlayer.Position, ref destLevel, ref destPos))
                    {
                        var levelData = LevelRepo.GetInfoByName(destLevel);
                        mapNameText.text = levelData.localizedname;
                        button.onClick.AddListener(() => PathFindToTarget(destLevel, destPos, ReachTargetAction.NPC_Interact, staticNPCJson.id));
                    }
                    else
                    {
                        mapNameText.text = "";
                        button.gameObject.SetActive(false);
                    }
                }
                break;
            case ItemOriginType.UI:
                int uitype;
                if (int.TryParse(param, out uitype))
                {
                    LinkUIType uIType = (LinkUIType)uitype;
                    iconImage.sprite = null; // todo: jm replace with ui icon?
                    nameText.text = uIType.ToString();  // to localize
                    levelText.text = "";
                    mapNameText.text = "";
                    button.onClick.AddListener(() =>
                    {
                        if (ClientUtils.OpenUIWindowByLinkUI(uIType))
                            UIManager.CloseAllDialogs();
                    });
                }
                break;
            case ItemOriginType.Auction:
                break;
        }
    }

    private void PathFindToTarget(string targetLevel, Vector3 targetPos, ReachTargetAction targetAction, int targetId = -1)
    {
        PlayerGhost player = GameInfo.gLocalPlayer;
        string currentLevel = ClientUtils.GetCurrentLevelName();
        bool foundtarget = true;

        if (currentLevel == targetLevel)
        {
            if (targetAction == ReachTargetAction.NPC_Interact)
                player.ProceedToTarget(targetPos, targetId, CallBackAction.Interact);
            else
                player.PathFindToTarget(targetPos, -1, 0, false, false, null);
        }
        else
        {
            BotController.TheDijkstra.DoRouter(currentLevel, targetLevel, out foundtarget);
            if (foundtarget)
            {
                BotController.DestLevel = targetLevel;
                BotController.DestMapPos = targetPos;
                BotController.DestAction = targetAction;
                BotController.DestArchtypeID = targetId;
                GameInfo.gLocalPlayer.Bot.SeekingWithRouter();
            }
            else
                UIManager.ShowSystemMessage(GUILocalizationRepo.GetLocalizedSysMsgByName("sys_CannotFindTarget"));
        }

        if (foundtarget)
        {
            UIManager.CloseAllDialogs();
            UIManager.CloseAllWindows();
        }
    }
}