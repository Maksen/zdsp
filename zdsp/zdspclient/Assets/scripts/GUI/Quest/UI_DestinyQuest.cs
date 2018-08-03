using Kopio.JsonContracts;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Zealot.Common;
using Zealot.Repository;

public class UI_DestinyQuest : MonoBehaviour
{
    [SerializeField]
    GameObject DescriptionObj;

    [SerializeField]
    Text Description;

    [SerializeField]
    Text HeroName;

    [SerializeField]
    Text QuestName;

    [SerializeField]
    Text Country;

    [SerializeField]
    Transform HeroesContent;

    [SerializeField]
    GameObject HeroData;

    [SerializeField]
    Transform FlowChartContent;

    [SerializeField]
    GameObject FlowChartData;

    [SerializeField]
    GameObject Marker;

    [SerializeField]
    GameObject[] MapCountry;

    [SerializeField]
    Transform[] MapCameraPos;

    [SerializeField]
    UI_MoveToObj HologramMap;

    private QuestClientController mQuestController;
    private int mSelectedHero;
    private int mSelectedDestiny;
    private Dictionary<int, List<QuestDestinyJson>> mDestinyDataByColumn;
    private List<GameObject> mHeroes;
    private Dictionary<int, GameObject> mDestinies;
    private GameObject mActivedMapCountry;

    private void OnEnable()
    {
        if (GameInfo.gLocalPlayer == null)
        {
            return;
        }

        mQuestController = GameInfo.gLocalPlayer.QuestController;
        mDestinyDataByColumn = new Dictionary<int, List<QuestDestinyJson>>();
        mHeroes = new List<GameObject>();
        mDestinies = new Dictionary<int, GameObject>();
        CurrentQuestData questData = mQuestController.GetLatestQuestData(QuestType.Destiny);
        if (questData != null)
        {
            QuestDestinyJson destinyJson = QuestRepo.GetDestinyByQuestId(questData.QuestId);
            mSelectedHero = destinyJson == null ? -1 : destinyJson.groupid;
            mSelectedDestiny = destinyJson == null ? -1 : destinyJson.id;
        }
        else
        {
            mSelectedHero = -1;
            mSelectedDestiny = -1;
        }
        UpdateHeroesList();
        UpdateDestinyData();
        UpdateDestinyList();
        UpdateMapCountry();
    }

    private void OnDisable()
    {
        ClearHeroes();
        ClearDestinies();
    }

    private void UpdateHeroesList()
    {
        ClearHeroes();
        List<int> group = QuestRepo.GetDestinyGroup();
        ToggleGroup toggleGroup = HeroesContent.GetComponent<ToggleGroup>();
        foreach (int groupid in group)
        {
            QuestDestinyJson destinyJson = QuestRepo.GetDestinyByGroupId(groupid);
            if (destinyJson != null)
            {
                GameObject newhero = Instantiate(HeroData);
                newhero.GetComponent<UI_HeroData>().Init(destinyJson.path, toggleGroup, this, groupid, groupid == mSelectedHero);
                newhero.transform.SetParent(HeroesContent, false);
                mHeroes.Add(newhero);
            }
        }
    }

    private void ClearHeroes()
    {
        if (mHeroes != null)
        {
            foreach (GameObject hero in mHeroes)
            {
                Destroy(hero);
            }
        }
    }

    private void UpdateDestinyList()
    {
        if (mSelectedHero != -1)
        {
            List<QuestDestinyJson> destinyJsons = QuestRepo.GetDestinyListByGroupId(mSelectedHero);
            foreach (QuestDestinyJson destinyJson in destinyJsons)
            {
                if (!mDestinyDataByColumn.ContainsKey(destinyJson.uicolumn))
                {
                    mDestinyDataByColumn.Add(destinyJson.uicolumn, new List<QuestDestinyJson>());
                }
                mDestinyDataByColumn[destinyJson.uicolumn].Add(destinyJson);
            }
        }

        ClearDestinies();
        ToggleGroup toggleGroup = FlowChartContent.GetComponent<ToggleGroup>();
        foreach (KeyValuePair<int, List<QuestDestinyJson>> entry in mDestinyDataByColumn)
        {
            GameObject newDestiny = Instantiate(FlowChartData);
            newDestiny.GetComponent<UI_ChartData>().Init(entry.Value, toggleGroup, this, mQuestController);
            newDestiny.transform.SetParent(FlowChartContent, false);
            mDestinies.Add(entry.Key, newDestiny);
        }

        foreach (KeyValuePair<int, List<QuestDestinyJson>> entry in mDestinyDataByColumn)
        {
            UI_ChartData chartData = mDestinies[entry.Key].GetComponent<UI_ChartData>();
            chartData.GenerateLine();
        }
    }

    private void UpdateDestinyData()
    {
        DescriptionObj.SetActive(mSelectedDestiny == -1 ? false : true);
        Marker.SetActive(mSelectedDestiny == -1 ? false : true);
        if (mSelectedDestiny != -1)
        {
            QuestDestinyJson destinyJson = QuestRepo.GetDestinyById(mSelectedDestiny);
            if (destinyJson != null)
            {
                Marker.transform.localPosition = new Vector3(destinyJson.posx, destinyJson.posy, 0);
                HeroName.text = destinyJson.name;
                Country.text = GetCountryName(destinyJson.type);
                QuestJson questJson = QuestRepo.GetQuestByID(destinyJson.questid);
                if (questJson != null)
                {
                    Description.text = questJson.description;
                    QuestName.text = questJson.questname;
                }
                else
                {
                    Description.text = "";
                    QuestName.text = "";
                }
            }
        }
    }

    private string GetCountryName(QuestDestinyType type)
    {
        switch(type)
        {
            case QuestDestinyType.Qing:
                return GUILocalizationRepo.GetLocalizedString("quest_qing");
            case QuestDestinyType.Wei:
                return GUILocalizationRepo.GetLocalizedString("quest_wei");
            case QuestDestinyType.Zhao:
                return GUILocalizationRepo.GetLocalizedString("quest_zhao");
            case QuestDestinyType.Yan:
                return GUILocalizationRepo.GetLocalizedString("quest_yan");
            case QuestDestinyType.Qi:
                return GUILocalizationRepo.GetLocalizedString("quest_qi");
            case QuestDestinyType.Chu:
                return GUILocalizationRepo.GetLocalizedString("quest_chu");
            case QuestDestinyType.Han:
                return GUILocalizationRepo.GetLocalizedString("quest_han");
            default:
                return "";
        }
    }

    public void GetPoint(int startcolumn, int startrow, ChartDirection startdirection, int endcolumn, int endrow, ChartDirection enddirection, out Vector2 startpoint, out Vector2 endpoint)
    {
        startpoint = Vector2.zero;
        endpoint = Vector2.zero;
        Debug.Log(startcolumn + ":" + startrow);
        if (mDestinies.ContainsKey(startcolumn))
        {
            UI_ChartData chartData = mDestinies[startcolumn].GetComponent<UI_ChartData>();
            startpoint = chartData.GetPoint(startrow, startdirection);
        }

        if (mDestinies.ContainsKey(endcolumn))
        {
            UI_ChartData chartData = mDestinies[endcolumn].GetComponent<UI_ChartData>();
            Vector2 ep = chartData.GetPoint(endrow, enddirection);
            int gap = 0; 
            float gapsize = 0;
            if (endcolumn > startcolumn)
            {
                gap = endcolumn - startcolumn;
                gapsize = (gap - 1) * chartData.GetComponent<RectTransform>().rect.width;
            }
            else if (endcolumn < startcolumn)
            {
                gap = startcolumn - endcolumn;
                gapsize = (gap - 1) * chartData.GetComponent<RectTransform>().rect.width;
            }
            endpoint = new Vector2(gap * ep.x * -1 + gapsize, ep.y - startpoint.y);
        }
    }

    private void ClearDestinies()
    {
        if (mDestinies != null)
        {
            foreach (KeyValuePair<int, GameObject> destiny in mDestinies)
            {
                destiny.Value.GetComponent<UI_ChartData>().ClearLineObject();
                Destroy(destiny.Value);
            }
            mDestinies.Clear();
        }
    }

    public void OnHeroChanged(int heroid)
    {
        mSelectedHero = heroid;
        mDestinyDataByColumn = new Dictionary<int, List<QuestDestinyJson>>();
        if (mSelectedHero != -1)
        {
            UpdateDestinyId();
            UpdateDestinyList();
            UpdateDestinyData();
        }
        else
        {
            ClearDestinies();
        }
    }

    private void UpdateDestinyId()
    {
        List<QuestDestinyJson> destinyJsons = QuestRepo.GetDestinyListByGroupId(mSelectedHero);
        foreach(QuestDestinyJson destinyJson in destinyJsons)
        {
            if (mQuestController.IsQuestOngoing(QuestType.Destiny, destinyJson.questid))
            {
                mSelectedDestiny = destinyJson.id;
            }
        }
    }

    public void OnDestinyChanged(int destinyid)
    {
        mSelectedDestiny = destinyid;
        UpdateDestinyData();
        UpdateMapCountry();
    }

    public int GetSelectedDestinyId()
    {
        return mSelectedDestiny;
    }

    public GameObject GetMapCountry(QuestDestinyType type)
    {
        switch(type)
        {
            case QuestDestinyType.Qing:
                return MapCountry[0];
            case QuestDestinyType.Wei:
                return MapCountry[1];
            case QuestDestinyType.Zhao:
                return MapCountry[3];
            case QuestDestinyType.Yan:
                return MapCountry[4];
            case QuestDestinyType.Qi:
                return MapCountry[5];
            case QuestDestinyType.Chu:
                return MapCountry[6];
            case QuestDestinyType.Han:
                return MapCountry[7];
        }
        return null;
    }

    public Transform GetMapTransform(QuestDestinyType type)
    {
        switch (type)
        {
            case QuestDestinyType.Qing:
                return MapCameraPos[0];
            case QuestDestinyType.Wei:
                return MapCameraPos[1];
            case QuestDestinyType.Zhao:
                return MapCameraPos[3];
            case QuestDestinyType.Yan:
                return MapCameraPos[4];
            case QuestDestinyType.Qi:
                return MapCameraPos[5];
            case QuestDestinyType.Chu:
                return MapCameraPos[6];
            case QuestDestinyType.Han:
                return MapCameraPos[7];
        }
        return null;
    }

    private void UpdateMapCountry()
    {
        QuestDestinyJson destinyJson = QuestRepo.GetDestinyById(mSelectedDestiny);
        if (destinyJson != null)
        {
            if (mActivedMapCountry != null)
            {
                mActivedMapCountry.SetActive(false);
                mActivedMapCountry = null;
            }

            mActivedMapCountry = GetMapCountry(destinyJson.type);
            if (mActivedMapCountry != null)
            {
                Transform cameratransform = GetMapTransform(destinyJson.type);
                if (cameratransform != null)
                {
                    HologramMap.MoveTo(cameratransform);
                }
                mActivedMapCountry.SetActive(true);
            }
        }
    }
}
