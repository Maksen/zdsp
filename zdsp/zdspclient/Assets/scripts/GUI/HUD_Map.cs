using Kopio.JsonContracts;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using Zealot.Common;
using Zealot.Entities;
using Zealot.Repository;

public class HUD_Map : MonoBehaviour
{
    #region +++ Game Objects +++
    [Header("Game Object Shortcut")]
    [SerializeField]
    Text mTxtMapName;
    [SerializeField]
    Text mTxtChannelName;
    [SerializeField]
    Image mImgMap;
    [SerializeField]
    ToggleGroup mTGExpander;
    [SerializeField]
    Toggle mNPCExpanderTg;
    [SerializeField]
    Toggle mMonsterExpanderTg;
    [SerializeField]
    Button mBtnWorldMap;
    [SerializeField]
    Button mBtnClose;
    [SerializeField]
    MapPointerEvent mMapPosPicker;
    #endregion
    #region +++ Parent Game Objects +++
    [Header("Parents")]
    [SerializeField]
    GameObject mExpanderNPCGO;
    [SerializeField]
    GameObject mExpanderMonsterGO;
    [SerializeField]
    GameObject mTeleportGO;
    [SerializeField]
    GameObject mReviveGO;
    [SerializeField]
    GameObject mMonsterGO;
    [SerializeField]
    GameObject mQuestNPCGO;
    [SerializeField]
    GameObject mShopNPCGO;
    [SerializeField]
    GameObject mBossGO;
    [SerializeField]
    GameObject mMiniBossGO;
    [SerializeField]
    GameObject mPartyGO;
    [SerializeField]
    GameObject mPlayerGO;
    #endregion
    #region +++ Prefab +++
    [Header("Prefab")]
    [SerializeField]
    GameObject mExpanderPrefab;
    [SerializeField]
    GameObject mMapIconPrefab;
    #endregion
    #region +++ Map Icon Texture +++
    [Header("Icons")]
    [SerializeField]
    Sprite mIconMonster;
    [SerializeField]
    Sprite mIconQuest;
    [SerializeField]
    Sprite mIconShop;
    [SerializeField]
    Sprite mIconTeleport;
    [SerializeField]
    Sprite mIconPlayer;
    [SerializeField]
    Sprite mIconParty;
    [SerializeField]
    Sprite mIconBoss;
    [SerializeField]
    Sprite mIconMiniBoss;
    [SerializeField]
    Sprite mIconRevive;
    [SerializeField]
    Sprite mIconQuestComplete;
    #endregion

    MapInfoJson mMapInfo = null;
    List<Image> mPlayerIconLst = new List<Image>();
    List<Image> mPartyIconLst = new List<Image>();
    List<Image> mMonsterIconLst = new List<Image>();
    List<Image> mMiniBossIconLst = new List<Image>();
    List<Image> mBossIconLst = new List<Image>();
    List<Image> mQuestNPCIconLst = new List<Image>();
    List<Image> mShopNPCIconLst = new List<Image>();
    string mLevelName;

    Coroutine mMapCloseCoroutine = null;
    Coroutine mMapUpdateCoroutine = null;
    const float MAP_UPDATEPOS_INTERVAL = 1f;

    private void Awake()
    {
        LoadMap();
    }
    private void OnEnable()
    {
        //Load map if map changed
        if (string.Compare(mLevelName, ClientUtils.GetCurrentLevelName()) != 0)
        {
            if (!LoadMap())
                return;
        }

        //Ensure player, monster, miniboss, boss, party members icon position are up-to-date
        if (mMapUpdateCoroutine != null)
            StopCoroutine(mMapUpdateCoroutine);
        mMapUpdateCoroutine = StartCoroutine(MapIconPosUpdateCoroutine());
    }
    private void OnDisable()
    {
        if (mMapUpdateCoroutine != null)
            StopCoroutine(mMapUpdateCoroutine);
        mMapUpdateCoroutine = null;
    }
    private void LateUpdate()
    {
        if (mPlayerIconLst.Count > 0)
        {
            //mPlayerIconLst[0] always meant the player itself
            //Update player icon rotation
            mPlayerIconLst[0].transform.localEulerAngles = new Vector3(0f, 0f, -GameInfo.gLocalPlayer.AnimObj.transform.localEulerAngles.y);
            SetIconPos(mPlayerIconLst[0], HUD_MapController.ScalePos_WorldToMap(GameInfo.gLocalPlayer.AnimObj.transform.position));
        }
    }

    private bool LoadMap()
    {
        //Retrieve all info about the level
        mLevelName = ClientUtils.GetCurrentLevelName();
        LevelJson curLvJson = LevelRepo.GetInfoByName(ClientUtils.GetCurrentLevelName());
        LevelInfo lvinfo = LevelReader.GetLevel(mLevelName);
        if (lvinfo == null)
        {
            Debug.LogError("HUD_Map.LoadMap: Walaoeh! LevelReader.GetLevel return null!");
            return false;
        }

        //Set basic data
        mTxtMapName.text = curLvJson.localizedname;
        mTxtChannelName.text = "Test Channel #n"; //Waiting for channel to be implemented
        mImgMap.sprite = HUD_MapController.mMap;
        mMapPosPicker.OnDown += OnMapClick;

        //Create all expander
        CleanAllExpander();
        LoadMapExpander(lvinfo);

        //delete and create all map icon
        CleanAllMapIcons();
        LoadStaticMapIcon();
        LoadMapIcon();

        return true;
    }
    private void LoadMapIcon()
    {
        for (int i = 0; i < HUD_MapController.mPlayerPairLst.Count; ++i)
        {
            if (i >= mPlayerIconLst.Count)
            {
                Image icon = CreateIcon();
                SetIcon(IconType.PLAYER, icon);
                mPlayerIconLst.Add(icon);
            }

            SetIconPos(mPlayerIconLst[i], HUD_MapController.mPlayerPairLst[i].iconPos);
        }
        for (int i = 0; i < HUD_MapController.mPartyMemPairLst.Count; ++i)
        {
            if (i >= mPartyIconLst.Count)
            {
                Image icon = CreateIcon();
                SetIcon(IconType.PARTY, icon);
                mPartyIconLst.Add(icon);
            }

            SetIconPos(mPartyIconLst[i], HUD_MapController.mPartyMemPairLst[i].iconPos);
        }
        for (int i = 0; i < HUD_MapController.mMonPairLst.Count; ++i)
        {
            if (i >= mMonsterIconLst.Count)
            {
                Image icon = CreateIcon();
                SetIcon(IconType.MONSTER, icon);
                mMonsterIconLst.Add(icon);
            }

            SetIconPos(mMonsterIconLst[i], HUD_MapController.mMonPairLst[i].iconPos);
        }
        for (int i = 0; i < HUD_MapController.mBossPairLst.Count; ++i)
        {
            if (i >= mBossIconLst.Count)
            {
                Image icon = CreateIcon();
                SetIcon(IconType.BOSS, icon);
                mBossIconLst.Add(icon);
            }

            SetIconPos(mBossIconLst[i], HUD_MapController.mBossPairLst[i].iconPos);
        }
    }
    private void LoadStaticMapIcon()
    {
        //Portal
        for (int i = 0; i < HUD_MapController.mPortalPosLst.Count; ++i)
        {
            Image icon = CreateIcon();
            SetIcon(IconType.TELEPORT, icon);
            SetIconPos(icon, HUD_MapController.mPortalPosLst[i]);
        }
        //Revive
        for (int i = 0; i < HUD_MapController.mRevivePosLst.Count; ++i)
        {
            Image icon = CreateIcon();
            SetIcon(IconType.REVIVE, icon);
            SetIconPos(icon, HUD_MapController.mRevivePosLst[i]);
        }
    }
    private void LoadMapExpander(LevelInfo lvinfo)
    {
        if (lvinfo == null)
            return;

        //NPC
        Dictionary<int, ServerEntityJson> npcDic;
        if (lvinfo.mEntities.TryGetValue("StaticClientNPCSpawnerJson", out npcDic))
        {
            foreach (StaticClientNPCSpawnerJson snpc in npcDic.Values)
            {
                if (!snpc.ShowInMap)
                    continue;

                //Check if archetype is valid
                StaticNPCJson npcJson = StaticNPCRepo.GetNPCByArchetype(snpc.archetype);
                if (npcJson == null)
                    continue;

                GameObject obj = Instantiate(mExpanderPrefab, Vector3.zero, Quaternion.identity);
                obj.transform.SetParent(mExpanderNPCGO.transform, false);

                //Attach ToggleGroup
                Toggle tg = obj.GetComponent<Toggle>();
                if (tg == null)
                {
                    Debug.LogError("HUD_MAP.LoadMapExpander: Walao, Cannot find Toggle component in ExpanderSubData");
                    continue;
                }
                tg.group = this.mTGExpander;
                tg.onValueChanged.AddListener((toggleOn) =>
                {
                    if (toggleOn)
                        OnMapExpanderClick(snpc.position);
                });

                //Find the text component in the children, there should be only 1
                Text txt = obj.GetComponentInChildren<Text>();
                if (txt == null)
                {
                    Debug.LogError("HUD_MAP.LoadMapExpander: Cannot find Text component in ExpanderSubData");
                    continue;
                }

                txt.text = npcJson.localizedname;
            }
        }

        //Monster
        Dictionary<int, ServerEntityJson> monDic;
        if (lvinfo.mEntities.TryGetValue("MonsterSpawnerJson", out monDic))
        {
            foreach (MonsterSpawnerJson ms in monDic.Values)
            {
                if (!ms.ShowInMap)
                    continue;

                //Check if monster archetype is valid
                CombatNPCJson monJson = CombatNPCRepo.GetNPCByArchetype(ms.archetype);
                if (monJson == null)
                    continue;

                GameObject obj = Instantiate(mExpanderPrefab, Vector3.zero, Quaternion.identity);
                obj.transform.SetParent(mExpanderMonsterGO.transform, false);

                //Attach ToggleGroup
                Toggle tg = obj.GetComponent<Toggle>();
                if (tg == null)
                {
                    Debug.LogError("HUD_MAP.LoadMapExpander: Walao, Cannot find Toggle component in ExpanderSubData");
                    continue;
                }
                tg.group = this.mTGExpander;
                tg.onValueChanged.AddListener((toggleOn)=>
                {
                    if (toggleOn)
                        OnMapExpanderClick(ms.position);
                });

                //Find the text component in the children, there should be only 1
                Text txt = obj.GetComponentInChildren<Text>();
                if (txt == null)
                {
                    Debug.LogError("HUD_MAP.LoadMapExpander: Cannot find Text component in ExpanderSubData");
                    continue;
                }

                txt.text = monJson.localizedname;
            }
        }
    }

    #region Button Toggle function
    public void OnClick_Close()
    {
        if (mMapCloseCoroutine == null)
            StartCoroutine(MapCloseCouroutine());
    }
    public void OnClick_OpenWorldMap()
    {
        UIManager.OpenWindow(WindowType.WorldMap);
    }
    public void OnToggleNPCExpander(bool isON)
    {
        foreach (Transform child in mQuestNPCGO.transform)
        {
            child.gameObject.SetActive(isON);
        }
        foreach (Transform child in mShopNPCGO.transform)
        {
            child.gameObject.SetActive(isON);
        }
    }
    public void OnToggleMonsterExpander(bool isON)
    {
        foreach (Transform child in mMonsterGO.transform)
        {
            child.gameObject.SetActive(isON);
        }
    }
    #endregion

    #region Helper function
    private Image CreateIcon()
    {
        GameObject obj = Instantiate(mMapIconPrefab, Vector3.zero, Quaternion.identity);
        Image img = obj.GetComponent<Image>();

        return img;
    }
    private void SetIcon(IconType type, Image img)
    {
        switch (type)
        {
            case IconType.PLAYER:
                img.sprite = mIconPlayer;
                img.gameObject.transform.SetParent(mPlayerGO.transform, false);
                break;
            case IconType.MONSTER:
                img.sprite = mIconMonster;
                img.gameObject.transform.SetParent(mMonsterGO.transform, false);
                break;
            case IconType.PARTY:
                img.sprite = mIconParty;
                img.gameObject.transform.SetParent(mPartyGO.transform, false);
                break;
            case IconType.MINIBOSS:
                img.sprite = mIconMiniBoss;
                img.gameObject.transform.SetParent(mMiniBossGO.transform, false);
                break;
            case IconType.BOSS:
                img.sprite = mIconBoss;
                img.gameObject.transform.SetParent(mBossGO.transform, false);
                break;
            case IconType.TELEPORT:
                img.sprite = mIconTeleport;
                img.gameObject.transform.SetParent(mTeleportGO.transform, false);
                break;
            case IconType.HAS_QUEST:
                img.sprite = mIconQuest;
                img.gameObject.transform.SetParent(mQuestNPCGO.transform, false);
                break;
            case IconType.QUEST_COMPLETED:
                img.sprite = mIconQuestComplete;
                img.gameObject.transform.SetParent(mQuestNPCGO.transform, false);
                break;
            case IconType.LOCKED_QUEST:
                img.sprite = null;
                img.gameObject.transform.SetParent(mQuestNPCGO.transform, false);
                img.gameObject.SetActive(false);
                break;
            case IconType.SHOP:
                //img.sprite = mSpr
                img.gameObject.transform.SetParent(mShopNPCGO.transform, false);
                break;
            case IconType.REVIVE:
                img.sprite = mIconRevive;
                img.gameObject.transform.SetParent(mReviveGO.transform, false);
                break;
            case IconType.EMPTY:
                img.sprite = null;
                img.gameObject.SetActive(false);
                return;
        }

        img.gameObject.SetActive(true);
    }
    private void SetIconPos(Image img, Vector3 pos)
    {
        img.gameObject.transform.localPosition = pos;
    }
    private void GetPartyMemberNames(List<string> nameLst)
    {
        nameLst.Clear();
        if (GameInfo.gLocalPlayer.IsInParty())
        {
            Dictionary<string, PartyMember> pmLst = GameInfo.gLocalPlayer.PartyStats.GetPartyMemberList();
            foreach (PartyMember mem in pmLst.Values)
            {
                if (mem.IsHero())
                    continue;

                nameLst.Add(mem.GetName());
            }
        }
    }
    private void CleanAllMapIcons()
    {
        foreach (Transform child in mTeleportGO.transform)
        {
            Destroy(child.gameObject);
        }
        foreach (Transform child in mReviveGO.transform)
        {
            Destroy(child.gameObject);
        }
        foreach (Transform child in mMonsterGO.transform)
        {
            Destroy(child.gameObject);
        }
        foreach (Transform child in mQuestNPCGO.transform)
        {
            Destroy(child.gameObject);
        }
        foreach (Transform child in mShopNPCGO.transform)
        {
            Destroy(child.gameObject);
        }
        foreach (Transform child in mBossGO.transform)
        {
            Destroy(child.gameObject);
        }
        foreach (Transform child in mMiniBossGO.transform)
        {
            Destroy(child.gameObject);
        }
        foreach (Transform child in mPartyGO.transform)
        {
            Destroy(child.gameObject);
        }
        foreach (Transform child in mPlayerGO.transform)
        {
            Destroy(child.gameObject);
        }

        mPlayerIconLst.Clear();
        mPartyIconLst.Clear();
        mMonsterIconLst.Clear();
        mMiniBossIconLst.Clear();
        mBossIconLst.Clear();
    }
    private void CleanAllExpander()
    {
        foreach (Transform child in mExpanderMonsterGO.transform)
        {
            Destroy(child.gameObject);
        }
        foreach (Transform child in mExpanderNPCGO.transform)
        {
            Destroy(child.gameObject);
        }
    }
    #endregion

    #region Coroutine
    IEnumerator MapIconPosUpdateCoroutine()
    {
        //Ensure player, monster, miniboss, boss, party members icon position are up-to-date
        while (true)
        {
            UpdateAddRemoveMapIcon();

            //Update position every 1 sec
            yield return new WaitForSeconds(MAP_UPDATEPOS_INTERVAL);
        }
    }
    IEnumerator MapCloseCouroutine()
    {
        yield return new WaitForSeconds(0.75f);
        this.gameObject.SetActive(false);
        mMapCloseCoroutine = null;
    }
    private void UpdateAddRemoveMapIcon()
    {
        //Player
        for (int i = 0; i < HUD_MapController.mPlayerPairLst.Count; ++i)
        {
            if (i >= mPlayerIconLst.Count)
            {
                Image icon = CreateIcon();
                SetIcon(IconType.PLAYER, icon);
                mPlayerIconLst.Add(icon);
            }

            SetIconPos(mPlayerIconLst[i], HUD_MapController.mPlayerPairLst[i].iconPos);
            mPlayerIconLst[i].gameObject.SetActive(true);
        }
        for (int i = HUD_MapController.mPlayerPairLst.Count; i < mPlayerIconLst.Count; ++i)
        {
            mPlayerIconLst[i].gameObject.SetActive(false);
        }
        //Party
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
        //Monster
        for (int i = 0; i < HUD_MapController.mMonPairLst.Count; ++i)
        {
            if (i >= mMonsterIconLst.Count)
            {
                Image icon = CreateIcon();
                SetIcon(IconType.MONSTER, icon);
                mMonsterIconLst.Add(icon);
            }

            SetIconPos(mMonsterIconLst[i], HUD_MapController.mMonPairLst[i].iconPos);
            mMonsterIconLst[i].gameObject.SetActive(mMonsterExpanderTg.isOn);
        }
        for (int i = HUD_MapController.mMonPairLst.Count; i < mMonsterIconLst.Count; ++i)
        {
            mMonsterIconLst[i].gameObject.SetActive(false);
        }
        //Miniboss
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
        //Boss
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
        //QuestNPC
        for (int i = 0; i < HUD_MapController.mQuestNPCPosLst.Count; ++i)
        {
            if (i >= mQuestNPCIconLst.Count)
            {
                Image icon = CreateIcon();
                if (HUD_MapController.mQuestNPCPosLst[i].hasQuestAvailable())
                    SetIcon(IconType.HAS_QUEST, icon);
                else if (HUD_MapController.mQuestNPCPosLst[i].hasQuestToSubmit())
                    SetIcon(IconType.QUEST_COMPLETED, icon);
                else if (HUD_MapController.mQuestNPCPosLst[i].hasQuest())
                    SetIcon(IconType.LOCKED_QUEST, icon);
                mQuestNPCIconLst.Add(icon);
            }

            SetIconPos(mQuestNPCIconLst[i], HUD_MapController.mQuestNPCPosLst[i].iconPos);
            mQuestNPCIconLst[i].gameObject.SetActive(mNPCExpanderTg.isOn);
        }
        for (int i = HUD_MapController.mQuestNPCPosLst.Count; i < mQuestNPCIconLst.Count; ++i)
        {
            mQuestNPCIconLst[i].gameObject.SetActive(false);
        }
        //ShopNPC
        for (int i = 0; i < HUD_MapController.mShopNPCPosLst.Count; ++i)
        {
            if (i >= mShopNPCIconLst.Count)
            {
                Image icon = CreateIcon();
                SetIcon(IconType.SHOP, icon);
                mShopNPCIconLst.Add(icon);
            }

            SetIconPos(mShopNPCIconLst[i], HUD_MapController.mShopNPCPosLst[i].iconPos);
            mShopNPCIconLst[i].gameObject.SetActive(mNPCExpanderTg.isOn);
        }
        for (int i = HUD_MapController.mShopNPCPosLst.Count; i < mShopNPCIconLst.Count; ++i)
        {
            mShopNPCIconLst[i].gameObject.SetActive(false);
        }
    }
    #endregion

    public void OnMapClick(Vector2 pos)
    {
        Vector3 mapPos = new Vector3(pos.x, 0f, pos.y);
        OnMapClick(mapPos);
    }
    public void OnMapClick(Vector3 pos)
    {
        //SetIconPos(mPlayerIconLst[0], pos);
        Vector3 worldPos = HUD_MapController.ScalePos_MapToWorld(pos);
        GameInfo.gLocalPlayer.PathFindToTarget(worldPos, -1, 0f, false, false, null);
    }
    public void OnMapExpanderClick(Vector3 pos)
    {
        GameInfo.gLocalPlayer.PathFindToTarget(pos, -1, 0f, false, false, null);
    }
    /// <summary>
    /// Meant for clicking minimap 2nd time to close map
    /// </summary>
    public void OnMiniMapClick()
    {
        if (this.gameObject.GetActive() == false)
            this.gameObject.SetActive(true);
        else
        {
            mBtnClose.onClick.Invoke();
            //if (mMapCloseCoroutine == null)
            //    StartCoroutine(MapCloseCouroutine());
        }
    }

    public enum IconType
    {
        PLAYER,
        MONSTER,
        PARTY,
        BOSS,
        MINIBOSS,

        TELEPORT,
        HAS_QUEST,
        QUEST_COMPLETED,
        LOCKED_QUEST,
        SHOP,
        REVIVE,

        EMPTY,
    }
}
