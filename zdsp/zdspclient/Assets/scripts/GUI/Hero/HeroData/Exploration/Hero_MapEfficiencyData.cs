using Kopio.JsonContracts;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using Zealot.Repository;

public class Hero_MapEfficiencyData : MonoBehaviour
{
    [SerializeField] Text headerText;
    [SerializeField] Text bodyText;

    private StringBuilder sb = new StringBuilder();
    private string colon;

    private void Awake()
    {
        colon = GUILocalizationRepo.colon;
    }

    public void Init(Hero hero, ExplorationMapJson mapData)
    {
        headerText.text = string.Format("<color=#CE0000FF>{0}</color>{1}", hero.HeroJson.localizedname,
            GUILocalizationRepo.GetLocalizedString("hro_rewards_multiplier"));

        SetText(GUILocalizationRepo.GetLocalizedString("com_level"), hero.GetExploreLevelEfficiency(mapData) * 100,
            mapData.levelmaxefficiency, true);
        SetText(GUILocalizationRepo.GetLocalizedString("hro_relationship"), hero.GetExploreTrustEfficiency(mapData) * 100,
            mapData.trustmaxefficiency, true);
        SetText(GUILocalizationRepo.GetLocalizedString("hro_terrain_efficiency"), hero.GetExploreTerrainEfficiency(mapData) * 100,
            HeroRepo.GetTerrainMaxEfficiency(mapData.terraintype), false);
        bodyText.text = sb.ToString();
    }

    private void SetText(string text, float value, float max, bool newline)
    {
        string str = string.Format("{0}{1}{2}%  <color=#CE0000FF>( {3}% )</color>", text, colon, value.ToString("0.#"), max);
        if (newline)
            sb.AppendLine(str);
        else
            sb.Append(str);
    }
}