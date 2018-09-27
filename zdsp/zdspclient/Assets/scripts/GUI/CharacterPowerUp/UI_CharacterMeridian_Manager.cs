using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Zealot.Client.Entities;
using Kopio.JsonContracts;
using Zealot.Repository;
using Zealot.Common;
using System.Text;

public class UI_CharacterMeridian_Manager : MonoBehaviour
{
    public static UI_CharacterMeridian_Manager mine;

    [SerializeField]
    private GameObject[] meridianLockIcons;
    [SerializeField]
    private Text[] meridianLevelsText;
    [SerializeField]
    private Text[] meridianEffectsText;


    [SerializeField]
    private Text meridianExpText;

    PowerUpController powerUpCtrl;

    [SerializeField]
    private Transform requireMaterialParent;
    [SerializeField]
    private GameObject requireMaterialObj;

    [SerializeField]
    private Button lockedMeridianButton;
    [SerializeField]
    private Button onceMeridianButton;
    [SerializeField]
    private Button multiMeridianButton;
    [SerializeField]
    private Button cancelMeridian;

    string[] typesName = new string[] { "任脈", "衝脈", "陽蹻脈", "陽維脈", "督脈", "帶脈", "陰蹻脈", "陰維脈" };

    double currentExpPercent = 0.0d;

    #region BasicSetting
    void Awake()
    {
        InitAwake();
    }

    void OnEnable()
    {
        InitEnable(0);
    }

    void OnDisable()
    {

    }
    #endregion

    #region Refresh
    void InitAwake()
    {
        mine = this;
        powerUpCtrl = GameInfo.gLocalPlayer.clientPowerUpCtrl;
    }

    void InitEnable(int type)
    {
        RefreshUnlockList();
        RefreshExpList(type);
        RefreshMaterial(type);
    }

    void RefreshUnlockList()
    {
        List<MeridianUnlockListJson> lis = powerUpCtrl.GetMeridianUnlockList();

        StringBuilder st = new StringBuilder();

        for (int i = 0; i < lis.Count; ++i)
        {
            if (lis[i] != null)
            {
                int nowLevel = lis[i].mlrank;
                if (nowLevel == 0)
                {
                    meridianLockIcons[i].SetActive(true);
                    meridianLevelsText[i].text = string.Empty;
                }
                else
                {
                    meridianLockIcons[i].SetActive(false);
                    st = new StringBuilder("LV");
                    st.Append(nowLevel.ToString());
                    meridianLevelsText[i].text = st.ToString();
                }
                meridianLockIcons[i].SetActive((nowLevel == 0) ? true : false);

                meridianLevelsText[i].text = nowLevel.ToString();
                st = new StringBuilder(SideEffectRepo.GetSideEffect(lis[i].effect).localizedname);
                st.Append("+");
                st.Append(lis[i].value.ToString());
                meridianEffectsText[i].text = st.ToString();
            }
        }
    }

    void RefreshExpList(int type)
    {
        int currentExp = powerUpCtrl.GetMeridianExp(type);
        int requireExp = powerUpCtrl.GetMeridianExpJson(type).exp;

        currentExpPercent = System.Math.Round((requireExp == 0) ? 100.0d : (double)currentExp / (double)requireExp * 100, 1);

        StringBuilder st = new StringBuilder(currentExpPercent.ToString());
        st.Append("%");
        meridianExpText.text = st.ToString();
    }

    void RefreshMaterial (int type)
    {
        ClientUtils.DestroyChildren(requireMaterialParent);

        List<ItemInfo> materialList = (currentExpPercent >= 100.0d) ? powerUpCtrl.GetMeridianUnlockMaterial(type) : powerUpCtrl.GetMeridianExpMaterial(type);
        for (int i = 0; i < materialList.Count; ++i)
        {
            GameObject materialObj = ClientUtils.CreateChild(requireMaterialParent, requireMaterialObj);
        }
    }
    #endregion

    #region ClickEvent

    #endregion
}