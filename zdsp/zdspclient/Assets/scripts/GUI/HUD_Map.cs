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
    [SerializeField]
    GameObject mMapGO; //only holds pathfind icons
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
    Sprite mIconDailyQuest;
    [SerializeField]
    Sprite mIconDestinyQuest;
    [SerializeField]
    Sprite mIconMainQuest;
    [SerializeField]
    Sprite mIconSideQuest;
    [SerializeField]
    Sprite mIconReturnDailyQuest;
    [SerializeField]
    Sprite mIconReturnDestinyQuest;
    [SerializeField]
    Sprite mIconReturnMainQuest;
    [SerializeField]
    Sprite mIconReturnSideQuest;
    [SerializeField]
    Sprite mIconFinishDailyQuest;
    [SerializeField]
    Sprite mIconFinishDestinyQuest;
    [SerializeField]
    Sprite mIconFinishMainQuest;
    [SerializeField]
    Sprite mIconFinishSideQuest;
    [SerializeField]
    Sprite mIconPlayer;
    [SerializeField]
    Sprite mIconParty;
    [SerializeField]
    Sprite mIconMonster;
    [SerializeField]
    Sprite mIconBoss;
    [SerializeField]
    Sprite mIconMiniBoss;
    [SerializeField]
    Sprite mIconRevive;
    [SerializeField]
    Sprite mIconShop;
    [SerializeField]
    Sprite mIconTeleport;
    #endregion

    #region MapClick
    [Header("MapClick/Pathfind")]
    [SerializeField]
    LineRenderer mDestinationPathLine;
    [SerializeField]
    Image mMapDestination;
    [SerializeField]
    TrailRenderer mPathFindTrail;
    [SerializeField]
    [Range(1f, 10f)]
    float mMapDestinationRotateSpeed = 1f;
    [Range(50f, 500f)]
    [SerializeField]
    float mTrailSpd = 1;
    [SerializeField]
    float mTrailTime = 0.2f;
    int mWPIdx;
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
    List<Vector3> mPathFindWPLst = new List<Vector3>();

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
            Transform t = GameInfo.gLocalPlayer.AnimObj.transform;
            mPlayerIconLst[0].transform.localEulerAngles = new Vector3(0f, 0f, -t.localEulerAngles.y);
            SetIconPos(mPlayerIconLst[0], HUD_MapController.ScalePos_WorldToMap(t.position));
        }

        if (mMapGO.GetActive())
        {
            mMapDestination.transform.Rotate(new Vector3(0f, 0f, 1f), mMapDestinationRotateSpeed);

            //Animate trail renderer obj to tranverse through the waypoints
            //Slowly move from mPathFindWPLst[0] to mPathFindWPLst[Count-1]
            if (mWPIdx < mPathFindWPLst.Count)
            {
                float dtspd = mTrailSpd * Time.deltaTime;
                mPathFindTrail.time = mTrailTime;
                mPathFindTrail.transform.localPosition = Vector3.MoveTowards(mPathFindTrail.transform.localPosition,
                                                                             mPathFindWPLst[mWPIdx],
                                                                             dtspd);
                Vector3 vec = mPathFindWPLst[mWPIdx] - mPathFindTrail.transform.localPosition;
                if (vec.sqrMagnitude < 1f)
                    mWPIdx++;
            }
            else
            {
                mWPIdx = 0;
                mPathFindTrail.time = 0.0001f;
                mPathFindTrail.transform.localPosition = mPathFindWPLst[0];
            }
            //ParticleSystem.EmitParams mEmitParam = new ParticleSystem.EmitParams();
            //mEmitParam.position = new Vector3(mPSPosX, 0f, 0f);
            //mEmitParam.velocity = mPSVel;
            //mPathFindPS.Emit(mEmitParam, mPSEmitAmt);
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
            case IconType.DAILYQUEST:
                img.sprite = mIconDailyQuest;
                img.gameObject.transform.SetParent(mQuestNPCGO.transform, false);
                break;
            case IconType.DESTINYQUEST:
                img.sprite = mIconDestinyQuest;
                img.gameObject.transform.SetParent(mQuestNPCGO.transform, false);
                break;
            case IconType.MAINQUEST:
                img.sprite = mIconMainQuest;
                img.gameObject.transform.SetParent(mQuestNPCGO.transform, false);
                break;
            case IconType.SIDEQUEST:
                img.sprite = mIconSideQuest;
                img.gameObject.transform.SetParent(mQuestNPCGO.transform, false);
                break;
            case IconType.RETURN_DAILYQUEST:
                img.sprite = mIconReturnDailyQuest;
                img.gameObject.transform.SetParent(mQuestNPCGO.transform, false);
                break;
            case IconType.RETURN_DESTINYQUEST:
                img.sprite = mIconReturnDestinyQuest;
                img.gameObject.transform.SetParent(mQuestNPCGO.transform, false);
                break;
            case IconType.RETURN_MAINQUEST:
                img.sprite = mIconReturnMainQuest;
                img.gameObject.transform.SetParent(mQuestNPCGO.transform, false);
                break;
            case IconType.RETURN_SIDEQUEST:
                img.sprite = mIconReturnSideQuest;
                img.gameObject.transform.SetParent(mQuestNPCGO.transform, false);
                break;
            case IconType.FINISH_DAILYQUEST:
                img.sprite = mIconFinishDailyQuest;
                img.gameObject.transform.SetParent(mQuestNPCGO.transform, false);
                break;
            case IconType.FINISH_DESTINYQUEST:
                img.sprite = mIconFinishDestinyQuest;
                img.gameObject.transform.SetParent(mQuestNPCGO.transform, false);
                break;
            case IconType.FINISH_MAINQUEST:
                img.sprite = mIconFinishMainQuest;
                img.gameObject.transform.SetParent(mQuestNPCGO.transform, false);
                break;
            case IconType.FINISH_SIDEQUEST:
                img.sprite = mIconFinishSideQuest;
                img.gameObject.transform.SetParent(mQuestNPCGO.transform, false);
                break;
            //case IconType.QUEST_COMPLETED:
            //    img.sprite = mIconQuestComplete;
            //    img.gameObject.transform.SetParent(mQuestNPCGO.transform, false);
            //    break;
            //case IconType.LOCKED_QUEST:
            //    img.sprite = null;
            //    img.gameObject.transform.SetParent(mQuestNPCGO.transform, false);
            //    img.gameObject.SetActive(false);
            //    break;
            case IconType.SHOP:
                img.sprite = mIconShop;
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
    private void SetIcon2(StaticMapIconGameObjectPair pair, Image img)
    {
        QuestType qt = QuestType.Main;
        if (pair.GetNPCQuestType(ref qt) == false)
        {
            Debug.LogError("HUD_MiniMap.SetIcon2: Walaoeh!! quest id is invalid");
            return;
        }

        switch (qt)
        {
            case QuestType.Destiny:
                if (pair.hasQuestAvailable())
                    SetIcon(IconType.DESTINYQUEST, img);
                else if (pair.hasQuestOngoing())
                    SetIcon(IconType.RETURN_DESTINYQUEST, img);
                else if (pair.hasQuestToSubmit())
                    SetIcon(IconType.FINISH_DESTINYQUEST, img);
                break;
            case QuestType.Main:
                if (pair.hasQuestAvailable())
                    SetIcon(IconType.MAINQUEST, img);
                else if (pair.hasQuestOngoing())
                    SetIcon(IconType.RETURN_MAINQUEST, img);
                else if (pair.hasQuestToSubmit())
                    SetIcon(IconType.FINISH_MAINQUEST, img);
                break;
            case QuestType.Sub:
                if (pair.hasQuestAvailable())
                    SetIcon(IconType.DAILYQUEST, img);
                else if (pair.hasQuestOngoing())
                    SetIcon(IconType.RETURN_DAILYQUEST, img);
                else if (pair.hasQuestToSubmit())
                    SetIcon(IconType.FINISH_DAILYQUEST, img);
                break;
            case QuestType.Event:
                if (pair.hasQuestAvailable())
                    SetIcon(IconType.SIDEQUEST, img);
                else if (pair.hasQuestOngoing())
                    SetIcon(IconType.RETURN_SIDEQUEST, img);
                else if (pair.hasQuestToSubmit())
                    SetIcon(IconType.FINISH_SIDEQUEST, img);
                break;
        }
    }
    private void SetIconPos(Image img, Vector3 pos)
    {
        img.gameObject.transform.localPosition = ScaleMapCoordinates(ref pos);
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
    private Vector2 NormalizeMapCoordinates(ref Vector2 pos)
    {
        pos.x /= mImgMap.rectTransform.rect.width * 0.5f;
        pos.y /= mImgMap.rectTransform.rect.height * 0.5f;
        return pos;
    }
    private Vector3 ScaleMapCoordinates(ref Vector3 pos)
    {
        pos.x *= mImgMap.rectTransform.rect.width * 0.5f;
        pos.y *= mImgMap.rectTransform.rect.height * 0.5f;
        return pos;
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
        //QuestNPC - create new map icon loop
        for (int i = 0; i < HUD_MapController.mQuestNPCPosLst.Count; ++i)
        {
            //if there is a new quest NPC as told by HUD_MapController
            //Create a new icon in the map
            if (i >= mQuestNPCIconLst.Count)
            {
                Image icon = CreateIcon();
                mQuestNPCIconLst.Add(icon);
            }

            //Set its label, pos and turn it on when expander is ON
            SetIcon2(HUD_MapController.mQuestNPCPosLst[i], mQuestNPCIconLst[i]);
            SetIconPos(mQuestNPCIconLst[i], HUD_MapController.mQuestNPCPosLst[i].iconPos);
            mQuestNPCIconLst[i].gameObject.SetActive(mNPCExpanderTg.isOn);
        }
        //QuestNPC - hide unused map icon loop
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
        //Turn on Map click parent
        mMapGO.SetActive(true);
        mMapDestination.gameObject.transform.localPosition = pos; //Set map destination icon pos

        NormalizeMapCoordinates(ref pos);
        Vector3 mapPos = new Vector3(pos.x, 0f, pos.y);
        OnMapClick(mapPos);
    }
    public void OnMapClick(Vector3 pos)
    {
        Vector3 worldPos = HUD_MapController.ScalePos_MapToWorld(pos);
        GameInfo.gLocalPlayer.PathFindToTarget(worldPos, -1, 0f, false, false, OnMapClickReachDestination);

        //Drawing path that the player will take on the map
        List<Vector3> wpLst = PathFinder.GetWayPoints();
        if (wpLst == null || wpLst.Count == 0)
            return;
        mPathFindWPLst.Clear();
        for (int i = 0; i < wpLst.Count; ++i)
        {
            Vector3 mappos = HUD_MapController.ScalePos_WorldToMap(wpLst[i]);
            ScaleMapCoordinates(ref mappos);
            mappos.z = -1;
            mPathFindWPLst.Add(mappos);
        }
        //mDestinationPathLine.positionCount = mPathFindWPLst.Count;
        //mDestinationPathLine.SetPositions(mPathFindWPLst.ToArray());

        //Set trail renderer to follow mPathFindWPLst
        mPathFindTrail.transform.localPosition = mPathFindWPLst[0];
        mWPIdx = 0;

        //Debug.Log(string.Format("Map Vec2 pos: {0}, {1}", pos.x, pos.z));
        //Debug.Log(string.Format("Map Vec3 pos: {0}, {1}, {2}", worldPos.x, worldPos.y, worldPos.z));
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
    private void OnMapClickReachDestination()
    {
        mDestinationPathLine.positionCount = 0;
        mPathFindWPLst.Clear();

        mMapGO.SetActive(false);
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

        MAPDESTINATION,

        EMPTY,
    }
}
