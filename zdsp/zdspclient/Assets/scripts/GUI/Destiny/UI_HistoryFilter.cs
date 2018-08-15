using UnityEngine;
using Zealot.Repository;
using System.Collections.Generic;
using Kopio.JsonContracts;
using System.Linq;
using UnityEngine.UI;

public class UI_HistoryFilter : MonoBehaviour
{
    [SerializeField]
    Transform HeroContent;

    [SerializeField]
    GameObject HeroPageData;

    [SerializeField]
    GameObject HeroData;

    private DestinyClueClientController mDestinyClueController;
    private Dictionary<int, HeroMemoryJson> mHeroMemories;
    private UI_DestinyHistory mParent;
    private int mSelectedHero;

    private List<GameObject> mPageObj;
    private List<GameObject> mHeroObj;

    public void InitFromDestinyUI(DestinyClueClientController controller, UI_DestinyHistory parent)
    {
        mSelectedHero = 0;
        mDestinyClueController = controller;
        mParent = parent;
        mHeroMemories = DestinyClueRepo.GetHeroMemories();
        mHeroMemories = mHeroMemories.OrderBy(o => o.Value.orderno).ToDictionary(o => o.Key, o => o.Value);
        UpdateHero();
    }

    private void UpdateHero()
    {
        Clean();
        mHeroObj = new List<GameObject>();
        mPageObj = new List<GameObject>();

        int herocount = 0;
        GameObject pageobj = null;
        ToggleGroup togglegroup = HeroContent.GetComponent<ToggleGroup>();
        foreach (KeyValuePair<int , HeroMemoryJson> entry in mHeroMemories)
        {
            if (herocount == 0)
            {
                pageobj = Instantiate(HeroPageData);
                pageobj.transform.SetParent(HeroContent, false);
                mPageObj.Add(pageobj);
            }

            GameObject heroobj = Instantiate(HeroData);
            heroobj.GetComponent<UI_DestinyHeroData>().Init(entry.Value.heroid, entry.Value.avatarpath, togglegroup, this);
            heroobj.transform.SetParent(pageobj.transform, false);
            mHeroObj.Add(heroobj);
            herocount += 1;

            if (herocount > 8)
            {
                herocount = 0;
            }
        }
    }

    public void UpdateSelectedHero(int heroid)
    {
        mSelectedHero = heroid;
    }

    private void Clean()
    {
        if (mHeroObj != null)
        {
            foreach(GameObject obj in mHeroObj)
            {
                Destroy(obj);
            }
            mHeroObj = new List<GameObject>();
        }

        if (mPageObj != null)
        {
            foreach (GameObject obj in mPageObj)
            {
                Destroy(obj);
            }
            mPageObj = new List<GameObject>();
        }
    }

    private void OnDisable()
    {
        Clean();
    }

    public void OnClickAll()
    {
        mSelectedHero = 0;
    }

    public void OnClickConfirm()
    {
        UIManager.CloseDialog(WindowType.DialogHistoryFilter);
        mParent.UpdateSelectedHero(mSelectedHero);
    }
}
