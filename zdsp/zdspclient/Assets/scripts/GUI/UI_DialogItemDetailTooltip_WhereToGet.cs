using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using Zealot.Common;
using Zealot.Repository;
using Zealot.Entities;
using Zealot.Bot;

public class UI_DialogItemDetailTooltip_WhereToGet : MonoBehaviour
{
    [SerializeField]
    GameObject mWhereToGetPrefab;
    [SerializeField]
    GameObject mBtnParent;
    [SerializeField]
    UI_DialogItemDetail_WhereToGetSub mSubMenu;

    List<UI_DialogItemDetailTooltip_WhereToGetBtn> mOriginSourceLst = new List<UI_DialogItemDetailTooltip_WhereToGetBtn>();

    public void OnDisable()
    {
        mSubMenu.gameObject.SetActive(false);
    }
    public void Init(IInventoryItem item)
    {
        //Do nothing if item is null
        if (item == null)
            return;

        //Do nothing if origin is invalid or equals -1
        if (item.JsonObject.origin == "-1")
            return;

        for (int i = 0; i < mOriginSourceLst.Count; ++i)
        {
            mOriginSourceLst[i].Clear();
        }

        //Parse origin and create buttons
        string[] difforigin = item.JsonObject.origin.Split(';');
        int originID = -1;
        Kopio.JsonContracts.ItemOriginJson ioj;
        foreach (string s in difforigin)
        {
            if (!int.TryParse(s, out originID))
                continue;
            if (!GameRepo.ItemFactory.ItemOriginTable.TryGetValue(originID, out ioj))
                continue;

            switch (ioj.origintype)
            {
                case ItemOriginType.Monster:
                    ReadOrigin_Monster(ioj.param);
                    break;
                case ItemOriginType.Item:
                    ReadOrigin_Item(ioj.param);
                    break;
                case ItemOriginType.NPC:
                    ReadOrigin_NPC(ioj.param);
                    break;
                case ItemOriginType.UI:
                    break;
                case ItemOriginType.Auction:
                    break;
            }
        }
    }

    private void ReadOrigin_Monster(string monOriginStr)
    {
        if (monOriginStr.Length == 0)
            return;

        string[] archetypeLst = monOriginStr.Split(';');
        UI_DialogItemDetailTooltip_WhereToGetBtn btn = null;
        foreach (string a in archetypeLst)
        {
            Kopio.JsonContracts.CombatNPCJson cnpc = CombatNPCRepo.GetNPCByArchetype(a);
            bool newbtn = CreateButton(out btn);
            btn.Name = cnpc.localizedname;
            btn.LevelName = cnpc.level.ToString();
            btn.MapName = string.Empty;
            btn.Portrait = ClientUtils.LoadIcon(cnpc.portraitpath);
            btn.ButtonAction = () =>
            {
                FindMonster(cnpc.archetype);
                UIManager.CloseAllDialogs();
                UIManager.CloseAllWindows();
            };

            if (newbtn)
                mOriginSourceLst.Add(btn);
        }
    }
    private void ReadOrigin_NPC(string npcOriginStr)
    {
        if (npcOriginStr.Length == 0)
            return;

        //Create one button to enter UI_DialogItemDetail_WhereToGetSub
        string[] archetypeLst = npcOriginStr.Split(';');
        UI_DialogItemDetailTooltip_WhereToGetBtn btn = null;
        Kopio.JsonContracts.StaticNPCJson snpc = StaticNPCRepo.GetNPCByArchetype(archetypeLst[0]);
        if (snpc == null)
            return;

        bool newbtn = CreateButton(out btn);
        btn.Name = "NPC";
        btn.LevelName = string.Empty;
        btn.MapName = string.Empty;
        btn.Portrait = ClientUtils.LoadIcon(snpc.portraitpath);
        btn.ButtonAction = () =>
        {
            //Open up sub menu
            mSubMenu.gameObject.SetActive(true);
            mSubMenu.Init(npcOriginStr);
        };

        if (newbtn)
            mOriginSourceLst.Add(btn);
    }
    private void ReadOrigin_Item(string itemOriginStr)
    {
        if (itemOriginStr.Length == 0)
            return;

        int itemid = -1;
        string[] itemIdLst = itemOriginStr.Split(';');
        UI_DialogItemDetailTooltip_WhereToGetBtn btn = null;
        foreach (string a in itemIdLst)
        {
            if (!int.TryParse(a, out itemid))
                continue;

            Kopio.JsonContracts.ItemBaseJson item = GameRepo.ItemFactory.GetItemById(itemid);
            bool newbtn = CreateButton(out btn);
            btn.Name = item.localizedname;
            btn.LevelName = string.Empty;
            btn.MapName = string.Empty;
            btn.Portrait = ClientUtils.LoadIcon(item.iconspritepath);
            btn.HideButton = true;

            if (newbtn)
                mOriginSourceLst.Add(btn);
        }
    }

    private bool CreateButton(out UI_DialogItemDetailTooltip_WhereToGetBtn btn)
    {
        for (int i = 0; i < mOriginSourceLst.Count; ++i)
        {
            if (!mOriginSourceLst[i].gameObject.GetActive())
            {
                mOriginSourceLst[i].gameObject.SetActive(true);
                btn = mOriginSourceLst[i];
                return false;
            }
        }

        GameObject obj = Instantiate(mWhereToGetPrefab, Vector3.zero, Quaternion.identity);
        UI_DialogItemDetailTooltip_WhereToGetBtn com = obj.GetComponent<UI_DialogItemDetailTooltip_WhereToGetBtn>();
        obj.transform.SetParent(mBtnParent.transform, false);
        btn = com;

        return true;
    }
    private void FindMonster(string archetype)
    {
        if (archetype == string.Empty || GameInfo.gLocalPlayer == null)
            return;

        string destinationLvl = string.Empty;
        Vector3 monPos = Vector3.zero;

        if (!NPCPosMap.FindNearestMonster(archetype, ClientUtils.GetCurrentLevelName(), GameInfo.gLocalPlayer.Position, ref destinationLvl, ref monPos))
        {
            Debug.LogError(string.Format("HUD_MapController.PathFindToNPC: Cannot find npc via archetype={0}", archetype));
            return;
        }

        if (ClientUtils.GetCurrentLevelName() == destinationLvl)
        {
            GameInfo.gLocalPlayer.PathFindToTarget(monPos, -1, 0f, false, false, null);
            return;
        }

        bool npcFound = false;
        BotController.TheDijkstra.DoRouter(ClientUtils.GetCurrentLevelName(), destinationLvl, out npcFound);
        if (!npcFound)
        {
            Debug.LogError(string.Format("UI_DialogItemDetail_WhereToGetSub.FindNPC: Cannot find npc of archetype={0}", archetype));
            return;
        }

        BotController.DestLevel = destinationLvl;
        BotController.DestMapPos = monPos;
        BotController.DestAction = ReachTargetAction.StartBot;
        BotController.DestArchtypeID = CombatNPCRepo.GetNPCByArchetype(archetype).id;
        GameInfo.gLocalPlayer.Bot.SeekingWithRouter();
    }
}
