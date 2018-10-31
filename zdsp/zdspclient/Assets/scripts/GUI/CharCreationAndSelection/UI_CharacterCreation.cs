using UnityEngine;
using UnityEngine.UI;
using Zealot.Common;
using Kopio.JsonContracts;
using System.Collections.Generic;
using System.Linq;
using Zealot.Repository;
using System.Collections;

public class LoadedCharcterData
{
    public Gender Gender { get; set; }
    public int HairStyle { get; set; }
    public int HairColor { get; set; }
    public int MakeUp { get; set; }
    public int SkinColor { get; set; }
    public int ClothesStyle { get; set; }
    public string Name { get; set; }

    public LoadedCharcterData(Gender gender, int hairstyle, int haircolor, int makeup, int skincolor, int clothesstyle)
    {
        Gender = gender;
        HairStyle = hairstyle;
        HairColor = haircolor;
        MakeUp = makeup;
        SkinColor = skincolor;
        ClothesStyle = clothesstyle;
        Name = "";
    }

    public string GetAppearanceData()
    {
        return JsonConvertDefaultSetting.SerializeObject(new List<int> () { HairStyle, HairColor, MakeUp, SkinColor }); ;
    }
}

public class UI_CharacterCreation : MonoBehaviour
{
    [SerializeField]
    Toggle MaleToggle;

    [SerializeField]
    Toggle HairStyleToggle;

    [SerializeField]
    Toggle Clothes0Toggle;

    [SerializeField]
    Button Back;

    [SerializeField]
    ToggleGroup CustomizeGroup;

    [SerializeField]
    Transform[] CustomizePages;

    [SerializeField]
    GameObject[] PageToggles;

    [SerializeField]
    GameObject CustomizeData;

    [SerializeField]
    Model_3DAvatar PlayerAvatar;

    [SerializeField]
    CreationCameraController CameraController;

    private Gender mGender;
    private EquipmentInventoryData mEquipmentInventoryData;
    private int mClothesStyle;

    private LoadedCharcterData mLoadedCharcterData;

    private Dictionary<int, AppearanceJson> mHairStyleList;
    private Dictionary<int, AppearanceJson> mHairColorList;
    private Dictionary<int, AppearanceJson> mMakeUpList;
    private Dictionary<int, AppearanceJson> mSkinColorList;

    private bool bInit;
    private List<CharacterCreationData> mOwnedCharacterList;
    private ApperanceType mApperanceType;   
    private int mMaxPage = 6;
    private int mCurrentCustomizeDataCount = 0;
    private int mMaxCustomizeDataPerPage = 15;
    private List<GameObject> mCustomizeObjects;

    public void InitFromLogin()
    {
        bInit = false;
        mOwnedCharacterList = new List<CharacterCreationData>();
        mEquipmentInventoryData = new EquipmentInventoryData();
        mEquipmentInventoryData.InitDefault();
        InitDefaultUI();
        Back.gameObject.SetActive(false);
    }

    public void InitFromSelection(List<CharacterCreationData> characterlist)
    {
        bInit = false;
        mOwnedCharacterList = characterlist;
        mEquipmentInventoryData = new EquipmentInventoryData();
        mEquipmentInventoryData.InitDefault();
        InitDefaultUI();
        Back.gameObject.SetActive(true);
    }

    private void InitDefaultUI()
    {
        GetCustomizeDatas();
       
        mClothesStyle = 0;
        mApperanceType = ApperanceType.HairStyle;

        mCustomizeObjects = new List<GameObject>();
        MaleToggle.isOn = true;
        HairStyleToggle.isOn = true;
        Clothes0Toggle.isOn = true;

        mLoadedCharcterData = new LoadedCharcterData(mGender, mEquipmentInventoryData.GetAppearanceSlot((int)AppearanceSlot.HairStyle), mEquipmentInventoryData.GetAppearanceSlot((int)AppearanceSlot.HairColor), mEquipmentInventoryData.GetAppearanceSlot((int)AppearanceSlot.MakeUp), mEquipmentInventoryData.GetAppearanceSlot((int)AppearanceSlot.SkinColor), mClothesStyle);
        UpdateCharacterModel();
        UpdateCustomizeData();
        bInit = true;
        CameraController.Init();
        CameraController.ChangeCamera(0, mGender);

        StartCoroutine(DisableCutscene());
    }

    private IEnumerator DisableCutscene()
    {
        yield return new WaitForSecondsRealtime(2.0f);

        GameInfo.gCharacterCreationManager.StopCutScene();
    }

    private void GetCustomizeDatas()
    {
        mHairStyleList = CharacterCreationRepo.GetCustomizeDatas(ApperanceType.HairStyle, mGender == Gender.Male ? ApperanceGender.Male : ApperanceGender.Female, new List<int>());
        mHairColorList = CharacterCreationRepo.GetCustomizeDatas(ApperanceType.HairColor, mGender == Gender.Male ? ApperanceGender.Male : ApperanceGender.Female, new List<int>());
        mMakeUpList = CharacterCreationRepo.GetCustomizeDatas(ApperanceType.MakeUp, mGender == Gender.Male ? ApperanceGender.Male : ApperanceGender.Female, new List<int>());
        mSkinColorList = CharacterCreationRepo.GetCustomizeDatas(ApperanceType.SkinColor, mGender == Gender.Male ? ApperanceGender.Male : ApperanceGender.Female, new List<int>());

        mEquipmentInventoryData.SetAppearanceToSlot((int)AppearanceSlot.HairStyle, GetFirstCustomizeData(ApperanceType.HairStyle));
        mEquipmentInventoryData.SetAppearanceToSlot((int)AppearanceSlot.HairColor, GetFirstCustomizeData(ApperanceType.HairColor));
        mEquipmentInventoryData.SetAppearanceToSlot((int)AppearanceSlot.MakeUp, GetFirstCustomizeData(ApperanceType.MakeUp));
        mEquipmentInventoryData.SetAppearanceToSlot((int)AppearanceSlot.SkinColor, GetFirstCustomizeData(ApperanceType.SkinColor));
    }

    private int GetFirstCustomizeData(ApperanceType apperanceType)
    {
        switch(apperanceType)
        {
            case ApperanceType.HairStyle:
                if (mHairStyleList.Count > 0)
                {
                    return mHairStyleList.First().Key;
                }
                return -1;
            case ApperanceType.HairColor:
                if (mHairColorList.Count > 0)
                {
                    return mHairColorList.First().Key;
                }
                return -1;
            case ApperanceType.MakeUp:
                if (mMakeUpList.Count > 0)
                {
                    return mMakeUpList.First().Key;
                }
                return -1;
            case ApperanceType.SkinColor:
                if (mSkinColorList.Count > 0)
                {
                    return mSkinColorList.First().Key;
                }
                return -1;
            default:
                return -1;
        }
    }

    public void OnClickMale(bool value)
    {
        mGender = Gender.Male;
        if (bInit)
        {
            GetCustomizeDatas();
            mCurrentCustomizeDataCount = 0;
            UpdateCustomizeData();
            UpdateCharacterModel();
            CameraController.SetCamera(mGender);
        }
    }

    public void OnClickFemale(bool value)
    {
        mGender = Gender.Female;
        if (bInit)
        {
            GetCustomizeDatas();
            mCurrentCustomizeDataCount = 0;
            UpdateCustomizeData();
            UpdateCharacterModel();
            CameraController.SetCamera(mGender);
        }
    }

    public void OnClickHairStyle(bool value)
    {
        mApperanceType = ApperanceType.HairStyle;
        mCurrentCustomizeDataCount = 0;
        if (bInit)
        {
            CameraController.ChangeCamera(0, mGender);
            UpdateCustomizeData();
        }
    }

    public void OnClickHairColor(bool value)
    {
        mApperanceType = ApperanceType.HairColor;
        mCurrentCustomizeDataCount = 0;
        if (bInit)
        {
            CameraController.ChangeCamera(0, mGender);
            UpdateCustomizeData();
        }
    }

    public void OnClickMakeUp(bool value)
    {
        mApperanceType = ApperanceType.MakeUp;
        mCurrentCustomizeDataCount = 0;
        if (bInit)
        {
            CameraController.ChangeCamera(1, mGender);
            UpdateCustomizeData();
        }
    }

    public void OnClickSkinColor(bool value)
    {
        mApperanceType = ApperanceType.SkinColor;
        mCurrentCustomizeDataCount = 0;
        if (bInit)
        {
            CameraController.ChangeCamera(2, mGender);
            UpdateCustomizeData();
        }
    }

    private void UpdateCustomizeData()
    {
        ClearCustomizeData();

        Dictionary<int, AppearanceJson> customizedatas = GetCustomizeData();
        foreach (KeyValuePair<int, AppearanceJson> entry in customizedatas)
        {
            GameObject customizedata = Instantiate(CustomizeData);
            bool selected = entry.Key == GetSelectedCustomizeData() ? true : false;
            bool iscolor = string.IsNullOrEmpty(entry.Value.iconpath);
            customizedata.GetComponent<UI_CustomizeData>().Init(CustomizeGroup, iscolor ? entry.Value.iconcolor : entry.Value.iconpath, iscolor, entry.Value.partid, selected, this);
            Transform parent = GetCustomizeParent();
            customizedata.transform.SetParent(parent, false);
            mCustomizeObjects.Add(customizedata);
            mCurrentCustomizeDataCount += 1;
        }
        UpdateTogglePage();
    }

    private int GetSelectedCustomizeData()
    {
        switch(mApperanceType)
        {
            case ApperanceType.HairStyle:
                return mEquipmentInventoryData.GetAppearanceSlot((int)AppearanceSlot.HairStyle);
            case ApperanceType.HairColor:
                return mEquipmentInventoryData.GetAppearanceSlot((int)AppearanceSlot.HairColor);
            case ApperanceType.MakeUp:
                return mEquipmentInventoryData.GetAppearanceSlot((int)AppearanceSlot.MakeUp);
            case ApperanceType.SkinColor:
                return mEquipmentInventoryData.GetAppearanceSlot((int)AppearanceSlot.SkinColor);
            default:
                return -1;
        }
    }

    private Dictionary<int, AppearanceJson> GetCustomizeData()
    {
        switch (mApperanceType)
        {
            case ApperanceType.HairStyle:
                return mHairStyleList;
            case ApperanceType.HairColor:
                return mHairColorList;
            case ApperanceType.MakeUp:
                return mMakeUpList;
            case ApperanceType.SkinColor:
                return mSkinColorList;
        }
        return new Dictionary<int, AppearanceJson>();
    }

    private Transform GetCustomizeParent()
    {
        int groupid = mCurrentCustomizeDataCount / mMaxCustomizeDataPerPage;
        return CustomizePages[groupid];
    }

    private void UpdateTogglePage()
    {
        int totalpage = mCurrentCustomizeDataCount / mMaxCustomizeDataPerPage;
        for (int i = 0; i < totalpage; i++)
        {
            CustomizePages[i].gameObject.SetActive(true);
            PageToggles[i].SetActive(true);
        }
        if (totalpage == 0)
        {
            PageToggles[totalpage].SetActive(false);
        }
        for (int i = totalpage + 1; i < mMaxPage; i++)
        {
            CustomizePages[i].gameObject.SetActive(false);
            PageToggles[i].SetActive(false);
        }
        PageToggles[0].GetComponent<Toggle>().isOn = true;
    }

    public void OnSelectedCustomizeData(int partid)
    {
        if (mApperanceType == ApperanceType.HairStyle)
        {
            mLoadedCharcterData.HairStyle = partid;
            mEquipmentInventoryData.SetAppearanceToSlot((int)AppearanceSlot.HairStyle, partid);
        }
        else if (mApperanceType == ApperanceType.HairColor)
        {
            mLoadedCharcterData.HairColor = partid;
            mEquipmentInventoryData.SetAppearanceToSlot((int)AppearanceSlot.HairColor, partid);
        }
        else if (mApperanceType == ApperanceType.MakeUp)
        {
            mLoadedCharcterData.MakeUp = partid;
            mEquipmentInventoryData.SetAppearanceToSlot((int)AppearanceSlot.MakeUp, partid);
        }
        else if (mApperanceType == ApperanceType.SkinColor)
        {
            mLoadedCharcterData.SkinColor = partid;
            mEquipmentInventoryData.SetAppearanceToSlot((int)AppearanceSlot.SkinColor, partid);
        }

        if (bInit)
        {
            UpdateCharacterModel();
        }
    }

    private void UpdateCharacterModel()
    {
        PlayerAvatar.CreationChange(mEquipmentInventoryData, JobType.Newbie, mGender, mClothesStyle, "Cutscene");
    }

    public void OnClickClothes0(bool value)
    {
        mClothesStyle = 0;
        if (bInit)
        {
            CameraController.ChangeCamera(3, mGender);
            UpdateCharacterModel();
        }
    }

    public void OnClickClothes1(bool value)
    {
        mClothesStyle = 1;
        if (bInit)
        {
            CameraController.ChangeCamera(3, mGender);
            UpdateCharacterModel();
        }
    }

    public void OnClickClothes2(bool value)
    {
        mClothesStyle = 2;
        if (bInit)
        {
            CameraController.ChangeCamera(3, mGender);
            UpdateCharacterModel();
        }
    }

    public void OnClickClothes3(bool value)
    {
        mClothesStyle = 3;
        if (bInit)
        {
            CameraController.ChangeCamera(3, mGender);
            UpdateCharacterModel();
        }
    }

    public void OnClickRandomStyle()
    {
        mEquipmentInventoryData.SetAppearanceToSlot((int)AppearanceSlot.HairStyle, GetRandomCustomizeData(ApperanceType.HairStyle));
        mEquipmentInventoryData.SetAppearanceToSlot((int)AppearanceSlot.HairColor, GetRandomCustomizeData(ApperanceType.HairColor));
        mEquipmentInventoryData.SetAppearanceToSlot((int)AppearanceSlot.MakeUp, GetRandomCustomizeData(ApperanceType.MakeUp));
        mEquipmentInventoryData.SetAppearanceToSlot((int)AppearanceSlot.SkinColor, GetRandomCustomizeData(ApperanceType.SkinColor));

        UpdateCharacterModel();
    }

    public int GetRandomCustomizeData(ApperanceType apperanceType)
    {
        Dictionary<int, AppearanceJson> appearancelist = new Dictionary<int, AppearanceJson>();
        switch (apperanceType)
        {
            case ApperanceType.HairStyle:
                appearancelist = mHairStyleList;
                break;
            case ApperanceType.HairColor:
                appearancelist = mHairColorList;
                break;
            case ApperanceType.MakeUp:
                appearancelist = mMakeUpList;
                break;
            case ApperanceType.SkinColor:
                appearancelist = mSkinColorList;
                break;
        }

        System.Random rand = GameUtils.GetRandomGenerator();
        if (appearancelist.Count > 0)
        {
            return appearancelist.ElementAt(rand.Next(0, appearancelist.Count)).Key;
        }

        return -1;
    }

    public void OnClickBack()
    {
        ClearCustomizeData();
        if (mOwnedCharacterList.Count > 0)
        {
            UIManager.OpenWindow(WindowType.CharacterSelection, (window) => window.GetComponent<UI_CharacterSelection>().Init(mOwnedCharacterList));
            UIManager.CloseWindow(WindowType.CharacterCreation);
        }
    }

    public void OnClickConfirm()
    {
        UIManager.OpenDialog(WindowType.DialogCharacterName, (window) => window.GetComponent<UI_CharacterName>().Init(this));
    }

    public void OnConfirmName(string name)
    {
        mLoadedCharcterData.Name = name;
        mLoadedCharcterData.Gender = mGender;
        UIManager.StartHourglass();
        RPCFactory.LobbyRPC.CreateCharacter(mLoadedCharcterData.Name, (byte)mLoadedCharcterData.Gender, mLoadedCharcterData.HairStyle, mLoadedCharcterData.HairColor, mLoadedCharcterData.MakeUp, mLoadedCharcterData.SkinColor);
    }

    private void OnDestroy()
    {
        ClearCustomizeData();
    }

    private void ClearCustomizeData()
    {
        foreach (GameObject obj in mCustomizeObjects)
        {
            Destroy(obj);
        }

        mCustomizeObjects = new List<GameObject>();
    }
}
