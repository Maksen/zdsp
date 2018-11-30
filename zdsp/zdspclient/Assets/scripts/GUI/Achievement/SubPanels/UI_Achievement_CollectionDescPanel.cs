using Kopio.JsonContracts;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Zealot.Common;
using Zealot.Repository;

public class UI_Achievement_CollectionDescPanel : MonoBehaviour
{
    [SerializeField] Toggle toggle;
    [SerializeField] ScrollRect scrollRect;
    [SerializeField] Text objNameText;
    [SerializeField] Text objDescText;

    [Header("Rewards")]
    [SerializeField] Image aexpIconImage;
    [SerializeField] Text aexpAmtText;
    [SerializeField] Transform itemIconSlot;
    [SerializeField] Image rewardImage;
    [SerializeField] Text rewardNameText;
    [SerializeField] Text rewardAmtText;

    [Header("SideEffect")]
    [SerializeField] GameObject sideEffectsObj;
    [SerializeField] GameObject seDataPrefab;
    [SerializeField] GameObject seLeftColumnObj;
    [SerializeField] Transform seLeftSlot;
    [SerializeField] GameObject seRightColumnObj;
    [SerializeField] Transform seRightSlot;

    private List<int> seIds = new List<int>();

    private void Start()
    {
        aexpIconImage.sprite = ClientUtils.LoadCurrencyIcon(CurrencyType.AExp);
    }

    public void Init(CollectionObjective obj, CollectionElement elem)
    {
        objNameText.text = obj.localizedName;

        if (obj.type == CollectionType.Monster || obj.type == CollectionType.Hero || obj.type == CollectionType.NPC || obj.type == CollectionType.Photo)
        {
            if (obj.type == CollectionType.Photo)
            {
                if (elem != null)
                {
                    objDescText.gameObject.SetActive(true);
                    objDescText.text = elem.PhotoDesc;
                }
                else
                    objDescText.gameObject.SetActive(false);
            }
            else
            {
                objDescText.gameObject.SetActive(true);
                objDescText.text = obj.json.localizeddescription;
            }
        }
        else
            objDescText.gameObject.SetActive(false);

        SetReward(obj);

        if (obj.type == CollectionType.Fashion || obj.type == CollectionType.Relic || obj.type == CollectionType.DNA)
        {
            sideEffectsObj.SetActive(true);
            SetSideEffect(obj, elem);
        }
        else
            sideEffectsObj.SetActive(false);
    }

    private void SetReward(BaseAchievementObjective obj)
    {
        aexpAmtText.text = "x" + obj.exp;

        switch (obj.rewardType)
        {
            case AchievementRewardType.None:
                itemIconSlot.parent.gameObject.SetActive(false);
                break;
            case AchievementRewardType.Item:
                rewardImage.gameObject.SetActive(false);
                IInventoryItem item = GameRepo.ItemFactory.GetInventoryItem(obj.rewardId);
                if (item != null)
                {
                    ItemGameIconType iconType = item.ItemSortJson.gameicontype;
                    GameObject iconPrefab = ClientUtils.LoadGameIcon(iconType);
                    GameObject itemIcon = ClientUtils.CreateChild(itemIconSlot, iconPrefab);
                    ClientUtils.InitGameIcon(itemIcon, item, item.ItemID, iconType, obj.rewardCount, false);
                    rewardNameText.text = item.JsonObject.localizedname;
                    rewardAmtText.text = "x" + obj.rewardCount;
                }
                break;
            case AchievementRewardType.Currency:
                rewardImage.gameObject.SetActive(true);
                CurrencyType currencyType = (CurrencyType)obj.rewardId;
                rewardImage.sprite = ClientUtils.LoadCurrencyIcon(currencyType);
                rewardNameText.text = ClientUtils.GetLocalizedCurrencyName(currencyType);
                rewardAmtText.text = "x" + obj.rewardCount;
                break;
            case AchievementRewardType.SideEffect:
                rewardImage.gameObject.SetActive(true);
                SideEffectJson se = SideEffectRepo.GetSideEffect(obj.rewardId);
                if (se != null)
                {
                    rewardImage.sprite = ClientUtils.LoadIcon(obj.rewardIconPath);
                    rewardNameText.text = SideEffectUtils.GetEffectTypeLocalizedName(se.effecttype);
                    rewardAmtText.text = "+" + (se.isrelative ? string.Format("{0}%", se.max) : se.max.ToString());
                }
                break;
        }
    }

    private void SetSideEffect(CollectionObjective obj, CollectionElement elem)
    {
        ClientUtils.DestroyChildren(seLeftSlot);
        ClientUtils.DestroyChildren(seRightSlot);

        IInventoryItem item = GameRepo.ItemFactory.GetInventoryItem(obj.targetId);
        switch (obj.type)
        {
            case CollectionType.Fashion:
                Equipment equipment = item as Equipment;
                ParseSideEffectString(equipment.EquipmentJson.basese);
                SetLeftSideEffects(true);
                break;
            case CollectionType.Relic:
                Relic relic = item as Relic;
                ParseSideEffectString(relic.RelicJson.sockability);
                SetLeftSideEffects(true);
                break;
            case CollectionType.DNA:
                DNA dna = item as DNA;
                ParseSideEffectString(dna.DNAJson.postive);
                SetLeftSideEffects(true);
                ParseSideEffectString(dna.DNAJson.negative);
                SetLeftSideEffects(false);
                seRightColumnObj.SetActive(false);
                break;
        }

        seLeftColumnObj.SetActive(seLeftSlot.childCount > 0);

        // stored se
        if (obj.type == CollectionType.Fashion || obj.type == CollectionType.Relic)
        {
            for (int i = 0; i < obj.storeSEs.Count; i++)
            {
                GameObject go = ClientUtils.CreateChild(seRightSlot, seDataPrefab);
                go.GetComponent<Achievement_SEText>().SetSEText(obj.storeSEs[i], true, elem != null && elem.Stored);
            }
            seRightColumnObj.SetActive(seRightSlot.childCount > 0);
        }

        if (!seLeftColumnObj.activeSelf && !seRightColumnObj.activeSelf)
            sideEffectsObj.SetActive(false);
    }

    private void SetLeftSideEffects(bool isPositive)
    {
        for (int i = 0; i < seIds.Count; ++i)
        {
            SideEffectJson se = SideEffectRepo.GetSideEffect(seIds[i]);
            if (se != null)
            {
                GameObject go = ClientUtils.CreateChild(seLeftSlot, seDataPrefab);
                go.GetComponent<Achievement_SEText>().SetSEText(se, isPositive, true);
            }
        }
    }

    private void ParseSideEffectString(string seStr)
    {
        seIds.Clear();
        if (!string.IsNullOrEmpty(seStr))
        {
            string[] seStrIds = seStr.Split(';');
            for (int i = 0; i < seStrIds.Length; ++i)
            {
                int seid;
                if (int.TryParse(seStrIds[i], out seid) && seid > 0)
                    seIds.Add(seid);
            }
        }
    }

    public void OnToggle(bool isOn)
    {
        if (!isOn)
            scrollRect.verticalNormalizedPosition = 1f;
    }

    public void ClosePanel()
    {
        if (toggle.isOn)
            toggle.isOn = false;
    }

    public void Empty()
    {
        objNameText.text = "";
        objDescText.text = "";
        objDescText.gameObject.SetActive(false);
        aexpAmtText.text = "x0";
        itemIconSlot.parent.gameObject.SetActive(false);
        sideEffectsObj.SetActive(false);
    }
}