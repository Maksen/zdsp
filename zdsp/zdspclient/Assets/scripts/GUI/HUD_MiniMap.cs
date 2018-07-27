using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class HUD_MiniMap : MonoBehaviour
{
    [SerializeField]
    Text mMapName;
    [SerializeField]
    GameObject mMiniMapIconPrefab;

    [Header("Icon Sprite")]
    [SerializeField]
    Sprite mSprTeleport;
    [SerializeField]
    Sprite mSprMonster;
    [SerializeField]
    Sprite mSprHasQuest;
    [SerializeField]
    Sprite mSprCompleteQuest;
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

    [Header("Parent GameObjects")]
    [SerializeField]
    GameObject mMMIconParent_Teleport;
    [SerializeField]
    GameObject mMMIconParent_Monster;
    [SerializeField]
    GameObject mMMIconParent_QuestNPC;
    [SerializeField]
    GameObject mMMIconParent_Boss;
    [SerializeField]
    GameObject mMMIconParent_MiniBoss;
    [SerializeField]
    GameObject mMMIconParent_Party;
    [SerializeField]
    GameObject mMMIconParent_Player;
    

    const int MAX_ICON = 128;
    List<Image> mMiniMapIconLst = new List<Image>(MAX_ICON);

    public void InitMap()
    {
        var realm = GameInfo.mRealmInfo;
        if (realm == null)
            return;

        mMapName.text = realm.localizedname;
    }

    public int AddIcon(IconType type, Vector3 worldpos)
    {
        //if (GameInfo.mRealmInfo == null)
        //    return -1;

        //Get a icon index that is not in use
        int idx = FindFreeIconSlot();

        //if cannot find one, create one
        if (idx < 0)
        {
            GameObject obj = Instantiate(mMiniMapIconPrefab, Vector3.zero, Quaternion.identity);
            Image img = obj.GetComponent<Image>();

            //Set the icon sprite using type
            SetIcon(type, img);

            //Calculate and set its position on the map, given its world pos
            Vector2 miniMapPos = Vector2.zero;
            obj.transform.localPosition = new Vector3(miniMapPos.x, miniMapPos.y, 0f);

            mMiniMapIconLst.Add(img);
            idx = mMiniMapIconLst.Count - 1;
        }

        return idx;
    }

    public void DeleteIcon(int index)
    {
        if (index >= mMiniMapIconLst.Count || mMiniMapIconLst[index] == null)
            return;

        mMiniMapIconLst[index].sprite = null;
        mMiniMapIconLst[index].gameObject.SetActive(false);
    }

    public void ChangeIconSprite(int index, IconType type)
    {
        if (index >= mMiniMapIconLst.Count || mMiniMapIconLst[index] == null)
            return;

        SetIcon(type, mMiniMapIconLst[index]);
    }

    public void ChangeIconPosition(int index, Vector3 newpos)
    {
        if (index >= mMiniMapIconLst.Count || mMiniMapIconLst[index] == null)
            return;

        mMiniMapIconLst[index].transform.position = newpos;
    }

    private int FindFreeIconSlot()
    {
        for (int i = 0; i < mMiniMapIconLst.Count; ++i)
        {
            if (mMiniMapIconLst[i] != null && mMiniMapIconLst[i].sprite == null)
                return i;
        }

        return -1;
    }

    private void SetIcon(IconType type, Image img)
    {
        switch (type)
        {
            case IconType.PLAYER:
                img.sprite = mSprPlayer;
                img.gameObject.transform.SetParent(mMMIconParent_Player.transform, false);
                break;
            case IconType.MONSTER:
                img.sprite = mSprMonster;
                img.gameObject.transform.SetParent(mMMIconParent_Monster.transform, false);
                break;
            case IconType.PARTY:
                img.sprite = mSprParty;
                img.gameObject.transform.SetParent(mMMIconParent_Party.transform, false);
                break;
            case IconType.MINIBOSS:
                img.sprite = mSprMiniBoss;
                img.gameObject.transform.SetParent(mMMIconParent_MiniBoss.transform, false);
                break;
            case IconType.BOSS:
                img.sprite = mSprBoss;
                img.gameObject.transform.SetParent(mMMIconParent_Boss.transform, false);
                break;
            case IconType.TELEPORT:
                img.sprite = mSprTeleport;
                img.gameObject.transform.SetParent(mMMIconParent_Teleport.transform, false);
                break;
            case IconType.HAS_QUEST:
                img.sprite = mSprHasQuest;
                img.gameObject.transform.SetParent(mMMIconParent_QuestNPC.transform, false);
                break;
            case IconType.QUEST_COMPLETED:
                img.sprite = mSprCompleteQuest;
                img.gameObject.transform.SetParent(mMMIconParent_QuestNPC.transform, false);
                break;
            case IconType.SHOP:
                //img.sprite = mSpr
                img.gameObject.transform.SetParent(mMMIconParent_QuestNPC.transform, false);
                break;
            case IconType.REVIVE:
                img.sprite = mSprRevive;
                img.gameObject.transform.SetParent(mMMIconParent_Party.transform, false);
                break;
            case IconType.EMPTY:
                img.sprite = null;
                img.gameObject.SetActive(false);
                return;
        }

        img.gameObject.SetActive(true);
    }

    public void OnClick_MiniMap()
    {
        GameObject obj = UIManager.GetWidget(HUDWidgetType.Map);
        obj.SetActive(!obj.GetActive());
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
        SHOP,
        REVIVE,

        EMPTY,
    }
}
