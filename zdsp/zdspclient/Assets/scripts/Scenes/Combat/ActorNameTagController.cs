using UnityEngine;
using Zealot.Common;
using Zealot.Client.Entities;

public enum QuestLabelType
{
    None,
    NewMainQuest,
    NewAdventureQuest,
    NewEventQuest,
    NewSubQuest,
    OngoingMainQuest,
    OngoingAdventureQuest,
    OngoingEventQuest,
    OngoingSubQuest,
    SubmitMainQuest,
    SubmitAdventureQuest,
    SubmitEventQuest,
    SubmitSubQuest,
}

public class ActorNameTagController : MonoBehaviour
{
    public GameObject mPlayerLabelObj = null;
    public HUD_PlayerLabel2 mPlayerLabel = null;
    public GameObject mPlayerLabelExtObj = null;
    public HUD_PlayerLabelExt mPlayerLabelExt = null;
    public GameObject mNpcLabelObj = null;
    public HUD_NpcLabel mNpcLabel = null;
    //Add more stuff here

    //Min scale when label is far away from player's character
    static float mLabelMinScale = 0.75f;
    static float mMaxScaleDist = 20;  //This need to scale on the length of visible distance in worldspace

    #region old
    Camera cam;
	RectTransform GameCanvas;
    #endregion

    void Awake()
    {
        GameCanvas = UIManager.GetHUDGameCanvas();
        cam = Camera.main;
        if (GameCanvas == null)
        {
            Debug.LogError("No canvas found. Check scene settings....");
        }
        else if (cam == null)
        {
            Debug.LogError("No UI camera found. Check scene settings....");
        }
    }

    public void OnDestroy()
    {
        if (mPlayerLabelObj != null)
            Destroy(mPlayerLabelObj);
        if (mPlayerLabelExtObj != null)
            Destroy(mPlayerLabelExtObj);
        if (mNpcLabelObj != null)
            Destroy(mNpcLabelObj);
    }

    // Update is called once per frame
    void LateUpdate ()
    {
        if (!IsControllerCreated())
            return;
        if (mPlayerLabel.gameObject.activeSelf)
            mPlayerLabel.UpdateAchorPos();
        mPlayerLabelExt.UpdateAchorPos();
        if (mNpcLabel != null && mNpcLabel.gameObject.activeSelf)
            mNpcLabel.UpdateAchorPos();
        ScaleLabelByDistance();
    }

    public void CreatePlayerLabel()
    {
        //Create the player label object
        if (mPlayerLabelObj == null && UIManager.UIHierarchy.PlayerLabelPrefab != null)
        {
            mPlayerLabelObj = Instantiate(UIManager.UIHierarchy.PlayerLabelPrefab, Vector3.zero, Quaternion.identity) as GameObject;

            GameObject playerlabelParent = UIManager.GetWidget(HUDWidgetType.PlayerLabel);
            if (playerlabelParent != null)
            {
                mPlayerLabelObj.transform.SetParent(playerlabelParent.transform, false);
                mPlayerLabelObj.transform.SetAsLastSibling();
                mPlayerLabelObj.transform.localPosition = new Vector3(0, 0, 0);
                //mPlayerLabelObj.transform.localScale = new Vector3(1.0f, 1.0f, 1.0f);

                mPlayerLabel = mPlayerLabelObj.GetComponent<HUD_PlayerLabel2>();
                mPlayerLabel.CanvasPosFunc = getCanvasPosition;
            }
        }

        //Create the player label ext object
        if (mPlayerLabelExtObj == null && UIManager.UIHierarchy.PlayerLabelExtPrefab != null)
        {
            mPlayerLabelExtObj = Instantiate(UIManager.UIHierarchy.PlayerLabelExtPrefab, Vector3.zero, Quaternion.identity) as GameObject;

            GameObject playerlabelextParent = UIManager.GetWidget(HUDWidgetType.PlayerLabelExt);
            if (playerlabelextParent != null)
            {
                mPlayerLabelExtObj.transform.SetParent(playerlabelextParent.transform, false);
                mPlayerLabelExtObj.transform.SetAsLastSibling();

                mPlayerLabelExt = mPlayerLabelExtObj.GetComponent<HUD_PlayerLabelExt>();
                mPlayerLabelExt.CanvasPosFunc = getCanvasPosition;
            }
        }
    }

    public void CreateNPCLabel(QuestLabelType labelType)
    {
        if (mNpcLabel != null && mNpcLabelObj != null)
            return;

        GameObject npclabelParent = UIManager.GetWidget(HUDWidgetType.NpcLabel);
        if (npclabelParent != null)
        {
            mNpcLabelObj = Instantiate(UIManager.UIHierarchy.NpcLabelPrefab, Vector3.zero, Quaternion.identity) as GameObject;
            mNpcLabelObj.transform.SetParent(npclabelParent.transform, false);
            mNpcLabelObj.transform.SetAsLastSibling();
            mNpcLabelObj.transform.localPosition = Vector3.zero;

            mNpcLabel = mNpcLabelObj.GetComponent<HUD_NpcLabel>();
            mNpcLabel.CanvasPosFunc = getCanvasPosition;
            UpdateQuestLabel(labelType);
        }
    }

    public void SetLabelOffset_WorldSpace(Vector3 plOffset = new Vector3(), Vector3 pleOffset = new Vector3())
    {
        if (!IsControllerCreated())
            return;

        mPlayerLabel.WorldSpaceOffset = plOffset;
        mPlayerLabelExt.WorldSpaceOffset = pleOffset;

        if (mNpcLabel != null)
            mNpcLabel.WorldSpaceOffset = pleOffset;
    }

    public void SetPlayerLabelByRealm(PlayerGhost pg, bool init=false)
    {
        if (GameInfo.mRealmInfo == null || pg == null)
            return;

        if (init)
        {
            mPlayerLabel.Name = pg.Name;
            SetLabelOffset_WorldSpace();
        }

        if (pg.IsLocal)
        {
            mPlayerLabel.SetPlayer();
            return;
        }

        bool isPartyMember = GameInfo.gLocalPlayer.IsInParty() && GameInfo.gLocalPlayer.PartyStats.IsMember(pg.Name);
        switch (GameInfo.mRealmInfo.type)
        {
            case RealmType.Dungeon:
            case RealmType.World:
                if (isPartyMember)
                {
                    mPlayerLabel.SetPartyMember();
                    //mPlayerLabel.SetBuffDebuff(ag);
                }
                else
                {
                    mPlayerLabel.SetFieldPlayer();
                }
                break;
        }
    }

    public void SetMonsterLabelByRealm(MonsterGhost mg, bool init=false)
    {
        //Do nothing if no realm
        //Do nothing if label is already showing
        if (GameInfo.mRealmInfo == null || mPlayerLabel.LabelType != LabelTypeEnum.Monster || mg == null)
            return;

        if (init)
        {
            mPlayerLabel.Name = mg.mArchetype.localizedname;
            SetLabelOffset_WorldSpace();
        }

        bool isEnemy = CombatUtils.IsEnemy(GameInfo.gLocalPlayer, mg);
        switch (GameInfo.mRealmInfo.type)
        {
            case RealmType.Dungeon:
            //case RealmType.RealmTutorial:
            case RealmType.World:
                switch (mg.mArchetype.monstertype)
                {
                    case MonsterType.Normal:
                        mPlayerLabel.SetHurtMonster();
                        break;
                    case MonsterType.MiniBoss:
                    case MonsterType.Boss:
                        mPlayerLabel.SetBoss();
                        break;
                }
                break;
            default:
                break;
        } // end switch
    }

    public void SetHeroLabel(HeroGhost hg)
    {
        if (hg == null)
            return;

        mPlayerLabel.Name = hg.Name;
    }

    public void Show(bool val)
    {
        if (mPlayerLabelObj != null && mPlayerLabelExtObj != null)
        {
            mPlayerLabelObj.SetActive(val);
            mPlayerLabelExtObj.SetActive(val);
        }
        if (mNpcLabelObj != null)
        {
            mNpcLabelObj.SetActive(val);
        }
    }

    public void OnGhostDied()
    {
        mPlayerLabel.HPf = 0f;
        Show(false);
    }

	private Vector2 getCanvasPosition(Vector2 UIOffset)
	{
        Vector2 ViewportPosition = cam.WorldToViewportPoint(this.transform.position);
        Vector2 WorldObject_ScreenPosition= new Vector2(
			((ViewportPosition.x*GameCanvas.sizeDelta.x)-(GameCanvas.sizeDelta.x*0.5f))+UIOffset.x,
			((ViewportPosition.y*GameCanvas.sizeDelta.y)-(GameCanvas.sizeDelta.y*0.5f))+UIOffset.y);
		return WorldObject_ScreenPosition;
	}

    private Vector2 getCanvasPosition(Vector3 WorldSpaceOffset)
    {
        //float height = GameInfo.gLocalPlayer.CharController.height;

        Vector2 ViewportPosition = cam.WorldToViewportPoint(this.transform.position + WorldSpaceOffset);
        Vector2 WorldObject_ScreenPosition = new Vector2(
            ((ViewportPosition.x * GameCanvas.sizeDelta.x) - (GameCanvas.sizeDelta.x * 0.5f)),
            ((ViewportPosition.y * GameCanvas.sizeDelta.y) - (GameCanvas.sizeDelta.y * 0.5f)));
        return WorldObject_ScreenPosition;
    }

    private void ScaleLabelByDistance()
    {
        var localplayer = GameInfo.gLocalPlayer;
        if (localplayer == null)
            return;
        ActorNameTagController local_antc = localplayer.HeadLabel;
        if (this == local_antc || local_antc == null)
            return;

        float dist = (transform.position - local_antc.transform.position).magnitude;
        dist = Mathf.Min(dist / mMaxScaleDist, 1f);
        float scale = mLabelMinScale + (1 - mLabelMinScale) * (1 - dist);
        Vector3 scaleVec = new Vector3(scale, scale, scale);

        mPlayerLabel.SetScale(scaleVec);
        mPlayerLabelExt.ScaleLabel(scaleVec);
        if (mNpcLabel != null)
            mNpcLabel.ScaleLabel(scaleVec);
    }

    public bool IsControllerCreated()
    {
        return (mPlayerLabelObj != null && mPlayerLabelExtObj != null);
    }

    public void UpdateQuestLabel(QuestLabelType labelType)
    {
        if (mNpcLabel != null)
        {
            switch(labelType)
            {
                case QuestLabelType.None:
                    mNpcLabel.HideAll();
                    break;
                case QuestLabelType.NewMainQuest:
                    mNpcLabel.NewMainQuest();
                    break;
                case QuestLabelType.NewAdventureQuest:
                    mNpcLabel.NewAdventureQuest();
                    break;
                case QuestLabelType.NewEventQuest:
                    mNpcLabel.NewEventQuest();
                    break;
                case QuestLabelType.NewSubQuest:
                    mNpcLabel.NewSubQuest();
                    break;
                case QuestLabelType.OngoingMainQuest:
                    mNpcLabel.OngoingMainQuest();
                    break;
                case QuestLabelType.OngoingAdventureQuest:
                    mNpcLabel.OngoingAdventureQuest();
                    break;
                case QuestLabelType.OngoingEventQuest:
                    mNpcLabel.OngoingEventQuest();
                    break;
                case QuestLabelType.OngoingSubQuest:
                    mNpcLabel.OngoingSubQuest();
                    break;
                case QuestLabelType.SubmitMainQuest:
                    mNpcLabel.RetunMainQuest();
                    break;
                case QuestLabelType.SubmitAdventureQuest:
                    mNpcLabel.RetunAdventureQuest();
                    break;
                case QuestLabelType.SubmitEventQuest:
                    mNpcLabel.RetunEventQuest();
                    break;
                case QuestLabelType.SubmitSubQuest:
                    mNpcLabel.RetunSubQuest();
                    break;
            }
        }
    }
}
