using Kopio.JsonContracts;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Zealot.Common;
using Zealot.Repository;

public class Achievement_CollectionData : MonoBehaviour
{
    [SerializeField] Toggle toggle;
    [SerializeField] Image rarityImage;
    [SerializeField] Image iconImage;
    [SerializeField] Image frameImage;
    [SerializeField] Image statusImage;
    [SerializeField] Sprite[] statusSprites;
    [SerializeField] string[] frameColorHex;

    private Color[] frameColors = new Color[4];
    private CollectionInfo collectionInfo;

    public void Setup(ToggleGroup toggleGroup)
    {
        toggle.group = toggleGroup;

        for (int i = 0; i < frameColorHex.Length; ++i)
            ColorUtility.TryParseHtmlString(frameColorHex[i], out frameColors[i]);
    }

    public void Init(CollectionInfo info, UnityAction<bool, CollectionObjective> callback, bool selected = false)
    {
        collectionInfo = info;
        SetCollectionData(info.objective);
        SetStatus(info.status);
        SetOnClickCallback(callback);
        SetToggleOn(selected);
    }

    public void UpdateData(CollectionInfo info)
    {
        collectionInfo = info;
        SetCollectionData(info.objective);
        SetStatus(info.status);
    }

    public void SetCollectionData(CollectionObjective obj)
    {
        switch (obj.type)
        {
            case CollectionType.Monster:
                CombatNPCJson combatNPC = obj.targetJsonObject as CombatNPCJson;
                if (combatNPC != null)
                {
                    SetRarity(false);
                    SetIcon(combatNPC.portraitpath);
                    SetFrameColor(true, (int)obj.json.monstertype);
                }
                break;
            case CollectionType.Fashion:
                Equipment equipment = obj.targetJsonObject as Equipment;
                if (equipment != null)
                {
                    SetRarity(true, equipment.EquipmentJson.rarity);
                    SetIcon(equipment.EquipmentJson.iconspritepath);
                    SetFrameColor(false);
                }
                break;
            case CollectionType.Hero:
                HeroJson hero = obj.targetJsonObject as HeroJson;
                if (hero != null)
                {
                    SetRarity(true, (ItemRarity)((int)hero.rarity + 2));
                    SetIcon(hero.smallportraitpath);
                    SetFrameColor(true, 0);
                }
                break;
            case CollectionType.NPC:
                StaticNPCJson staticNPC = obj.targetJsonObject as StaticNPCJson;
                if (staticNPC != null)
                {
                    SetRarity(false);
                    SetIcon(staticNPC.portraitpath);
                    SetFrameColor(true, 0);
                }
                break;
            case CollectionType.Relic:
                Relic relic = obj.targetJsonObject as Relic;
                if (relic != null)
                {
                    SetRarity(true, relic.RelicJson.rarity);
                    SetIcon(relic.RelicJson.iconspritepath);
                    SetFrameColor(false);
                }
                break;
            case CollectionType.DNA:
                DNA dna = obj.targetJsonObject as DNA;
                if (dna != null)
                {
                    SetRarity(true, dna.DNAJson.rarity);
                    SetIcon(dna.DNAJson.iconspritepath);
                    SetFrameColor(false);
                }
                break;
            case CollectionType.Photo:
                break;
        }
    }

    private void SetIcon(string iconpath)
    {
        iconImage.sprite = ClientUtils.LoadIcon(iconpath);
    }

    private void SetRarity(bool show, ItemRarity rarity = ItemRarity.Common)
    {
        if (show)
        {
            rarityImage.gameObject.SetActive(true);
            rarityImage.sprite = LoadQualityIcon(rarity);
        }
        else
            rarityImage.gameObject.SetActive(false);
    }

    private void SetFrameColor(bool show, int index = 0)
    {
        if (show)
        {
            frameImage.gameObject.SetActive(true);
            frameImage.color = frameColors[index];
        }
        else
            frameImage.gameObject.SetActive(false);
    }

    public void SetStatus(CollectStatus status)
    {
        if (status == CollectStatus.Unlocked)
            statusImage.gameObject.SetActive(false);
        else
        {
            statusImage.gameObject.SetActive(true);
            statusImage.sprite = statusSprites[(int)status];
        }
    }

    private Sprite LoadQualityIcon(ItemRarity rarity)
    {
        string path = "";
        switch (rarity)
        {
            case ItemRarity.Common:
                path = "UI_ZDSP_Icons/GameIcon/quality_default_common.tif";
                break;
            case ItemRarity.Uncommon:
                path = "UI_ZDSP_Icons/GameIcon/quality_default_uncommon.tif";
                break;
            case ItemRarity.Rare:
                path = "UI_ZDSP_Icons/GameIcon/quality_default_rare.tif";
                break;
            case ItemRarity.Epic:
                path = "UI_ZDSP_Icons/GameIcon/quality_default_epic.tif";
                break;
            case ItemRarity.Celestial:
                path = "UI_ZDSP_Icons/GameIcon/quality_default_celestial.tif";
                break;
            case ItemRarity.Legendary:
                path = "UI_ZDSP_Icons/GameIcon/quality_default_legendary.tif";
                break;
        }
        return ClientUtils.LoadIcon(path);
    }

    public void SetToggleOn(bool value)
    {
        if (toggle.isOn != value)
            toggle.isOn = value;
    }

    public void SetOnClickCallback(UnityAction<bool, CollectionObjective> action)
    {
        toggle.onValueChanged.RemoveAllListeners();
        if (action != null)
            toggle.onValueChanged.AddListener((isOn) => action(isOn, collectionInfo.objective));
    }

    public CollectionInfo GetCollectionInfo()
    {
        return collectionInfo;
    }

    public void Clear()
    {
        toggle.onValueChanged.RemoveAllListeners();
        toggle.isOn = false;
        collectionInfo = null;
    }
}