using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using Zealot.Entities;
using Zealot.Repository;
using Zealot.Common;

public class HUD_MiniMap : MonoBehaviour
{
    [SerializeField]
    Image mMap;
    [SerializeField]
    RectTransform mMapTransform;
    [SerializeField]
    Text mMapName;
    [SerializeField]
    GameObject mMiniMapIconPrefab;

    [Header("Icon Sprite")]
    [SerializeField]
    Sprite mSprDailyQuest;
    [SerializeField]
    Sprite mSprDestinyQuest;
    [SerializeField]
    Sprite mSprMainQuest;
    [SerializeField]
    Sprite mSprSideQuest;
    [SerializeField]
    Sprite mSprReturnDailyQuest;
    [SerializeField]
    Sprite mSprReturnDestinyQuest;
    [SerializeField]
    Sprite mSprReturnMainQuest;
    [SerializeField]
    Sprite mSprReturnSideQuest;
    [SerializeField]
    Sprite mSprFinishDailyQuest;
    [SerializeField]
    Sprite mSprFinishDestinyQuest;
    [SerializeField]
    Sprite mSprFinishMainQuest;
    [SerializeField]
    Sprite mSprFinishSideQuest;
    [SerializeField]
    Sprite mSprMonster;
    [SerializeField]
    Sprite mSprBoss;
    [SerializeField]
    Sprite mSprMiniBoss;
    [SerializeField]
    Sprite mSprParty;
    [SerializeField]
    Sprite mSprPlayer;
    [SerializeField]
    Sprite mSprShop;
    [SerializeField]
    Sprite mSprRevive;
    [SerializeField]
    Sprite mSprTeleport;

    [Header("Parent GameObjects")]
    [SerializeField]
    GameObject mMMIconParent_Teleport;
    [SerializeField]
    GameObject mMMIconParent_Monster;
    [SerializeField]
    GameObject mMMIconParent_QuestNPC;
    [SerializeField]
    GameObject mMMIconParent_ShopNPC;
    [SerializeField]
    GameObject mMMIconParent_Boss;
    [SerializeField]
    GameObject mMMIconParent_MiniBoss;
    [SerializeField]
    GameObject mMMIconParent_Party;
    [SerializeField]
    GameObject mMMIconParent_Player;
    [SerializeField]
    GameObject mMMIconParent_Revive;

    List<Image> mPlayerIconLst = new List<Image>();
    List<Image> mPartyIconLst = new List<Image>();
    List<Image> mMonsterIconLst = new List<Image>();
    List<Image> mMiniBossIconLst = new List<Image>();
    List<Image> mBossIconLst = new List<Image>();

    Coroutine mMiniMapUpdateCoroutine = null;
    Coroutine mMiniMapIconUpdateCoroutine = null;
    Coroutine mMiniMapStaticIconInitCoroutine = null;
    const float MINIMAP_UPDATE_INTERVAL = 0.1f;
    const float MINIMAPICON_UPDATE_INTERVAL = 1f;

    public void InitMap()
    {
        string lvname = ClientUtils.GetCurrentLevelName();
        Kopio.JsonContracts.LevelJson curLvJson = LevelRepo.GetInfoByName(lvname);

        CleanMiniMap();

        mMapName.text = curLvJson.localizedname;
        mMap.sprite = null;
        HUD_MapController.LoadMapAsync();
        mMiniMapStaticIconInitCoroutine = StartCoroutine(MiniMapInitStaticIconCoroutine());
        mMiniMapUpdateCoroutine = StartCoroutine(MiniMapUpdateCoroutine());
        
    }
    public void OnDestroy()
    {
        CleanMiniMap();
    }

    #region Helper function
    private Image CreateIcon()
    {
        GameObject obj = Instantiate(mMiniMapIconPrefab, Vector3.zero, Quaternion.identity);
        Image img = obj.GetComponent<Image>();

        return img;
    }
    private void SetIcon(IconType type, Image img, bool setParent = true)
    {
        switch (type)
        {
            case IconType.PLAYER:
                img.sprite = mSprPlayer;
                if (setParent)
                    img.gameObject.transform.SetParent(mMMIconParent_Player.transform, false);
                break;
            case IconType.MONSTER:
                img.sprite = mSprMonster;
                if (setParent)
                    img.gameObject.transform.SetParent(mMMIconParent_Monster.transform, false);
                break;
            case IconType.PARTY:
                img.sprite = mSprParty;
                if (setParent)
                    img.gameObject.transform.SetParent(mMMIconParent_Party.transform, false);
                break;
            case IconType.MINIBOSS:
                img.sprite = mSprMiniBoss;
                if (setParent)
                    img.gameObject.transform.SetParent(mMMIconParent_MiniBoss.transform, false);
                break;
            case IconType.BOSS:
                img.sprite = mSprBoss;
                if (setParent)
                    img.gameObject.transform.SetParent(mMMIconParent_Boss.transform, false);
                break;
            case IconType.TELEPORT:
                img.sprite = mSprTeleport;
                if (setParent)
                    img.gameObject.transform.SetParent(mMMIconParent_Teleport.transform, false);
                break;
            case IconType.DAILYQUEST:
                img.sprite = mSprDailyQuest;
                if (setParent)
                    img.gameObject.transform.SetParent(mMMIconParent_QuestNPC.transform, false);
                break;
            case IconType.DESTINYQUEST:
                img.sprite = mSprDestinyQuest;
                if (setParent)
                    img.gameObject.transform.SetParent(mMMIconParent_QuestNPC.transform, false);
                break;
            case IconType.MAINQUEST:
                img.sprite = mSprMainQuest;
                if (setParent)
                    img.gameObject.transform.SetParent(mMMIconParent_QuestNPC.transform, false);
                break;
            case IconType.SIDEQUEST:
                img.sprite = mSprSideQuest;
                if (setParent)
                    img.gameObject.transform.SetParent(mMMIconParent_QuestNPC.transform, false);
                break;
            case IconType.RETURN_DAILYQUEST:
                img.sprite = mSprReturnDailyQuest;
                if (setParent)
                    img.gameObject.transform.SetParent(mMMIconParent_QuestNPC.transform, false);
                break;
            case IconType.RETURN_DESTINYQUEST:
                img.sprite = mSprReturnDestinyQuest;
                if (setParent)
                    img.gameObject.transform.SetParent(mMMIconParent_QuestNPC.transform, false);
                break;
            case IconType.RETURN_MAINQUEST:
                img.sprite = mSprReturnMainQuest;
                if (setParent)
                    img.gameObject.transform.SetParent(mMMIconParent_QuestNPC.transform, false);
                break;
            case IconType.RETURN_SIDEQUEST:
                img.sprite = mSprReturnSideQuest;
                if (setParent)
                    img.gameObject.transform.SetParent(mMMIconParent_QuestNPC.transform, false);
                break;
            case IconType.FINISH_DAILYQUEST:
                img.sprite = mSprFinishDailyQuest;
                if (setParent)
                    img.gameObject.transform.SetParent(mMMIconParent_QuestNPC.transform, false);
                break;
            case IconType.FINISH_DESTINYQUEST:
                img.sprite = mSprFinishDestinyQuest;
                if (setParent)
                    img.gameObject.transform.SetParent(mMMIconParent_QuestNPC.transform, false);
                break;
            case IconType.FINISH_MAINQUEST:
                img.sprite = mSprFinishMainQuest;
                if (setParent)
                    img.gameObject.transform.SetParent(mMMIconParent_QuestNPC.transform, false);
                break;
            case IconType.FINISH_SIDEQUEST:
                img.sprite = mSprFinishSideQuest;
                if (setParent)
                    img.gameObject.transform.SetParent(mMMIconParent_QuestNPC.transform, false);
                break;
            //case IconType.QUEST_COMPLETED:
            //    //img.sprite = mSprCompleteQuest;
            //    img.gameObject.transform.SetParent(mMMIconParent_QuestNPC.transform, false);
            //    break;
            //case IconType.LOCKED_QUEST:
            //    img.sprite = null;
            //    img.gameObject.transform.SetParent(mMMIconParent_QuestNPC.transform, false);
            //    img.gameObject.SetActive(false);
            //    break;
            case IconType.SHOP:
                img.sprite = mSprShop;
                if (setParent)
                    img.gameObject.transform.SetParent(mMMIconParent_ShopNPC.transform, false);
                break;
            case IconType.REVIVE:
                img.sprite = mSprRevive;
                if (setParent)
                    img.gameObject.transform.SetParent(mMMIconParent_Revive.transform, false);
                break;
            case IconType.EMPTY:
                img.sprite = null;
                img.gameObject.SetActive(false);
                if (setParent)
                    img.gameObject.transform.SetParent(mMMIconParent_QuestNPC.transform, false);
                return;
        }

        img.gameObject.SetActive(true);
    }
    private void SetIcon2(StaticMapIconGameObjectPair pair, Image img, bool setParent = true)
    {
        QuestType qt = QuestType.Event;

        if (pair.GetNPCQuestType(ref qt) == false)
        {
            Debug.LogError("HUD_MiniMap.MiniMapUpdateIconCoroutine: Walaoeh!! quest id is invalid");
            return;
        }

        switch (qt)
        {
            case QuestType.Destiny:
                if (pair.hasQuestAvailable())
                    SetIcon(IconType.DESTINYQUEST, img, setParent);
                else if (pair.hasQuestOngoing())
                    SetIcon(IconType.RETURN_DESTINYQUEST, img, setParent);
                else if (pair.hasQuestToSubmit())
                    SetIcon(IconType.FINISH_DESTINYQUEST, img, setParent);
                else
                    SetIcon(IconType.EMPTY, img, setParent);
                break;
            case QuestType.Main:
                if (pair.hasQuestAvailable())
                    SetIcon(IconType.MAINQUEST, img, setParent);
                else if (pair.hasQuestOngoing())
                    SetIcon(IconType.RETURN_MAINQUEST, img, setParent);
                else if (pair.hasQuestToSubmit())
                    SetIcon(IconType.FINISH_MAINQUEST, img, setParent);
                else
                    SetIcon(IconType.EMPTY, img, setParent);
                break;
            case QuestType.Sub:
                if (pair.hasQuestAvailable())
                    SetIcon(IconType.DAILYQUEST, img, setParent);
                else if (pair.hasQuestOngoing())
                    SetIcon(IconType.RETURN_DAILYQUEST, img, setParent);
                else if (pair.hasQuestToSubmit())
                    SetIcon(IconType.FINISH_DAILYQUEST, img, setParent);
                else
                    SetIcon(IconType.EMPTY, img, setParent);
                break;
            case QuestType.Event:
                if (pair.hasQuestAvailable())
                    SetIcon(IconType.SIDEQUEST, img, setParent);
                else if (pair.hasQuestOngoing())
                    SetIcon(IconType.RETURN_SIDEQUEST, img, setParent);
                else if (pair.hasQuestToSubmit())
                    SetIcon(IconType.FINISH_SIDEQUEST, img, setParent);
                else
                    SetIcon(IconType.EMPTY, img, setParent);
                break;
        }
    }
    private void SetIconPos(Image img, Vector3 pos)
    {
        pos.x *= mMap.rectTransform.rect.width * 0.5f;
        pos.y *= mMap.rectTransform.rect.height * 0.5f;
        img.gameObject.transform.localPosition = pos;
    }
    private void DeleteAllChildren(GameObject parent)
    {
        if (parent == null)
            return;

        foreach (Transform child in parent.transform)
        {
            Destroy(child.gameObject);
        }
    }
    private void CleanMiniMap()
    {
        if (mMiniMapUpdateCoroutine != null)
            StopCoroutine(mMiniMapUpdateCoroutine);
        if (mMiniMapIconUpdateCoroutine != null)
            StopCoroutine(mMiniMapIconUpdateCoroutine);

        //Delete all game icons
        DeleteAllChildren(mMMIconParent_Teleport);
        DeleteAllChildren(mMMIconParent_Monster);
        DeleteAllChildren(mMMIconParent_QuestNPC);
        DeleteAllChildren(mMMIconParent_ShopNPC);
        DeleteAllChildren(mMMIconParent_Boss);
        DeleteAllChildren(mMMIconParent_MiniBoss);
        DeleteAllChildren(mMMIconParent_Party);
        DeleteAllChildren(mMMIconParent_Player);
        DeleteAllChildren(mMMIconParent_Revive);

        mPartyIconLst.Clear();
        mMonsterIconLst.Clear();
        mMiniBossIconLst.Clear();
        mBossIconLst.Clear();
        mPlayerIconLst.Clear();
    }
    #endregion

    #region Coroutine function
    IEnumerator MiniMapUpdateCoroutine()
    {
        while (true)
        {
            if (HUD_MapController.isControllerInitialized())
            {
                //Set the map if different map
                if (mMap.sprite != HUD_MapController.mMap)
                    mMap.sprite = HUD_MapController.mMap;

                //displace the map from the player position
                Vector3 invPlayerMapPos = -HUD_MapController.ScalePos_WorldToMap(GameInfo.gLocalPlayer.AnimObj.transform.position);
                invPlayerMapPos.x *= mMap.rectTransform.rect.width * 0.5f;
                invPlayerMapPos.y *= mMap.rectTransform.rect.height * 0.5f;

                mMapTransform.localPosition = invPlayerMapPos;

                //mPlayerIconLst[0] always meant the player itself
                //Update player icon rotation
                if (mPlayerIconLst.Count > 0)
                    mPlayerIconLst[0].transform.localEulerAngles = new Vector3(0f, 0f, -GameInfo.gLocalPlayer.AnimObj.transform.localEulerAngles.y);
            }

            yield return new WaitForSeconds(MINIMAP_UPDATE_INTERVAL);
        }
    }
    IEnumerator MiniMapUpdateIconCoroutine()
    {
        while (true)
        {
            //create icons and set their position according to HUD_MapController
            if (HUD_MapController.isControllerInitialized())
            {
                //Player icon should not move
                /*** Player ***/
                for (int i = 0; i < HUD_MapController.mPlayerPairLst.Count; ++i)
                {
                    if (i >= mPlayerIconLst.Count)
                    {
                        Image icon = CreateIcon();
                        SetIcon(IconType.PLAYER, icon);
                        mPlayerIconLst.Add(icon);
                    }

                    if (GameInfo.gLocalPlayer.AnimObj != HUD_MapController.mPlayerPairLst[i].entity)
                        SetIconPos(mPlayerIconLst[i], HUD_MapController.mPlayerPairLst[i].iconPos);
                    mPlayerIconLst[i].gameObject.SetActive(true);
                }
                for (int i = HUD_MapController.mPlayerPairLst.Count; i < mPlayerIconLst.Count; ++i)
                {
                    mPlayerIconLst[i].gameObject.SetActive(false);
                }

                /*** Party ***/
                for (int i = 0; i < HUD_MapController.mPartyMemPairLst.Count; ++i)
                {
                    if (i >= mPartyIconLst.Count)
                    {
                        Image icon = CreateIcon();
                        SetIcon(IconType.PARTY, icon);
                        mPartyIconLst.Add(icon);
                    }

                    SetIconPos(mPartyIconLst[i], HUD_MapController.mPartyMemPairLst[i].iconPos);
                    mPartyIconLst[i].gameObject.SetActive(true);
                }
                for (int i = HUD_MapController.mPartyMemPairLst.Count; i < mPartyIconLst.Count; ++i)
                {
                    mPartyIconLst[i].gameObject.SetActive(false);
                }

                /*** Monster ***/
                for (int i = 0; i < HUD_MapController.mMonPairLst.Count; ++i)
                {
                    if (i >= mMonsterIconLst.Count)
                    {
                        Image icon = CreateIcon();
                        SetIcon(IconType.MONSTER, icon);
                        mMonsterIconLst.Add(icon);
                    }

                    SetIconPos(mMonsterIconLst[i], HUD_MapController.mMonPairLst[i].iconPos);
                    mMonsterIconLst[i].gameObject.SetActive(true);
                }
                for (int i = HUD_MapController.mMonPairLst.Count; i < mMonsterIconLst.Count; ++i)
                {
                    mMonsterIconLst[i].gameObject.SetActive(false);
                }

                /*** Mini-Boss ***/
                for (int i = 0; i < HUD_MapController.mMiniBossPairLst.Count; ++i)
                {
                    if (i >= mMiniBossIconLst.Count)
                    {
                        Image icon = CreateIcon();
                        SetIcon(IconType.MINIBOSS, icon);
                        mMiniBossIconLst.Add(icon);
                    }

                    SetIconPos(mMiniBossIconLst[i], HUD_MapController.mMiniBossPairLst[i].iconPos);
                    mMiniBossIconLst[i].gameObject.SetActive(true);
                }
                for (int i = HUD_MapController.mMiniBossPairLst.Count; i < mMiniBossIconLst.Count; ++i)
                {
                    mMiniBossIconLst[i].gameObject.SetActive(false);
                }

                /*** Boss ***/
                for (int i = 0; i < HUD_MapController.mBossPairLst.Count; ++i)
                {
                    if (i >= mBossIconLst.Count)
                    {
                        Image icon = CreateIcon();
                        SetIcon(IconType.BOSS, icon);
                        mBossIconLst.Add(icon);
                    }

                    SetIconPos(mBossIconLst[i], HUD_MapController.mBossPairLst[i].iconPos);
                    mBossIconLst[i].gameObject.SetActive(true);
                }
                for (int i = HUD_MapController.mBossPairLst.Count; i < mBossIconLst.Count; ++i)
                {
                    mBossIconLst[i].gameObject.SetActive(false);
                }

                /*** NPC ***/
                //** QuestNPC may become active from ShopNPC **
                //** QuestNPC may become ShopNPC after quest complete **
                //HUD_MapController during update causes mShopNPCPosLst to change its contents(add/remove)
                for (int i = 0; i < HUD_MapController.mShopNPCPosLst.Count; ++i)
                {
                    //if there are more shop NPCs now than when created initially
                    if (i >= mMMIconParent_ShopNPC.transform.childCount)
                    {
                        Image newicon = CreateIcon();
                        SetIcon(IconType.SHOP, newicon); //Parents new icon to mMMIconParent_ShopNPC
                    }

                    Transform child = mMMIconParent_ShopNPC.transform.GetChild(i);
                    Image icon = child.GetComponent<Image>();
                    SetIconPos(icon, HUD_MapController.mShopNPCPosLst[i].iconPos);
                    child.gameObject.SetActive(true);
                }
                for (int i = HUD_MapController.mShopNPCPosLst.Count; i < mMMIconParent_ShopNPC.transform.childCount; ++i)
                {
                    //Turn off extra shopNPC map icons
                    Transform child = mMMIconParent_ShopNPC.transform.GetChild(i);
                    child.gameObject.SetActive(false);
                }
                

                //QuestNPC may become active from nothing
                //QuestNPC may become inactive after quest complete
                //QuestNPC may become ShopNPC after quest complete
                for (int i = 0; i < HUD_MapController.mQuestNPCPosLst.Count; ++i)
                {
                    StaticMapIconGameObjectPair npcIconPair = HUD_MapController.mQuestNPCPosLst[i];

                    //if there are more quest NPCs now than when created initially
                    if (i >= mMMIconParent_QuestNPC.transform.childCount)
                    {
                        Image newicon = CreateIcon();
                        SetIcon2(npcIconPair, newicon); //Parents to mMMIconParent_QuestNPC
                    }
                    
                    //Turn off marker if marker has no need to show
                    if (npcIconPair.hasQuest() == false ||
                        !npcIconPair.hasQuestAvailable() &&
                        !npcIconPair.hasQuestOngoing() &&
                        !npcIconPair.hasQuestToSubmit() ||
                        npcIconPair.hasQuestCompleted())
                    {
                        mMMIconParent_QuestNPC.transform.GetChild(i).gameObject.SetActive(false);
                        continue;
                    }

                    Transform child = mMMIconParent_QuestNPC.transform.GetChild(i);
                    Image icon = child.GetComponent<Image>();
                    SetIcon2(HUD_MapController.mQuestNPCPosLst[i], icon, false);
                    SetIconPos(icon, npcIconPair.iconPos);
                    child.gameObject.SetActive(true);
                }
            }
            for (int i = HUD_MapController.mQuestNPCPosLst.Count; i < mMMIconParent_QuestNPC.transform.childCount; ++i)
            {
                //Turn off extra shopNPC map icons
                Transform child = mMMIconParent_QuestNPC.transform.GetChild(i);
                child.gameObject.SetActive(false);
            }


            yield return new WaitForSeconds(MINIMAPICON_UPDATE_INTERVAL);
        }
    }
    IEnumerator MiniMapInitStaticIconCoroutine()
    {
        //Wait until MapController finish with LoadMapAsync
        while (!HUD_MapController.isControllerInitialized())
        {
            yield return new WaitForSeconds(MINIMAPICON_UPDATE_INTERVAL);
        }

        //Init all static icons that wont get changed at all
        for (int i = 0; i < HUD_MapController.mPortalPosLst.Count; ++i)
        {
            Image icon = CreateIcon();
            SetIcon(IconType.TELEPORT, icon);
            SetIconPos(icon, HUD_MapController.mPortalPosLst[i]);
        }
        for (int i = 0; i < HUD_MapController.mRevivePosLst.Count; ++i)
        {
            Image icon = CreateIcon();
            SetIcon(IconType.REVIVE, icon);
            SetIconPos(icon, HUD_MapController.mRevivePosLst[i]);
        }

        //Start coroutine update once init is done
        mMiniMapIconUpdateCoroutine = StartCoroutine(MiniMapUpdateIconCoroutine());
    }
    #endregion

    /// <summary>
    /// Opens HUD_Map
    /// </summary>
    public void OnClick_MiniMap()
    {
        if (GameInfo.IsDoingTutorialRealm())
        {
            UIManager.ShowSystemMessage(GUILocalizationRepo.GetLocalizedSysMsgByName("ret_UnableToUseFeature", null));
            return;
        }

        GameObject bigmapobj = UIManager.GetWidget(HUDWidgetType.Map);
        HUD_Map bigmap = bigmapobj.GetComponent<HUD_Map>();
        bigmap.OnMiniMapClick();
    }

    public enum IconType
    {
        PLAYER,
        MONSTER,
        PARTY,
        BOSS,
        MINIBOSS,

        DAILYQUEST,
        DESTINYQUEST,
        MAINQUEST,
        SIDEQUEST,
        RETURN_DAILYQUEST,
        RETURN_DESTINYQUEST,
        RETURN_MAINQUEST,
        RETURN_SIDEQUEST,
        FINISH_DAILYQUEST,
        FINISH_DESTINYQUEST,
        FINISH_MAINQUEST,
        FINISH_SIDEQUEST,
        
        SHOP,
        REVIVE,
        TELEPORT,

        EMPTY,
    }
}
