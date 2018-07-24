using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using Zealot.Common;
using Zealot.Repository;

public class UI_Party_Join : MonoBehaviour
{
    [SerializeField] ExpanderToggle[] expanderToggles;
    [SerializeField] GameObject expanderSubDataPrefab;
    [SerializeField] ToggleGroup toggleGroup;
    [SerializeField] ComboBoxA levelComboBox;
    [SerializeField] Toggle autoAcceptCheckBox;
    [SerializeField] ScrollRect partyListScrollRect;
    [SerializeField] Transform partyListTransform;
    [SerializeField] GameObject partyInfoPrefab;

    private int selectedLocationId = 0;
    private int selectedMinLevel = 0;
    private bool selectedAutoAccept = false;
    private List<PartyInfo> partyList = new List<PartyInfo>();
    private GameTimer refreshPartyListTimer;

    public void SetupUIElements()
    {
        // setup location list
        GameObject locObj = Instantiate(expanderSubDataPrefab);
        locObj.GetComponent<ExpanderSubData_Party>().Init(0, toggleGroup, OnSelectLocation);  // nearby - id 0
        expanderToggles[0].AddChild(locObj);

        foreach (var location in PartyRepo.locations.Values)
        {
            locObj = Instantiate(expanderSubDataPrefab);
            locObj.GetComponent<ExpanderSubData_Party>().Init(location.locationId, toggleGroup, OnSelectLocation);
            expanderToggles[(int)location.locationType].AddChild(locObj);
        }

        // setup level combobox
        levelComboBox.AddItem(GUILocalizationRepo.GetLocalizedString("pty_alllevels"), "0");  // all levels
        Dictionary<string, string> param = new Dictionary<string, string>();
        string[] levelFilter = GameConstantRepo.GetConstant("PartyLevelFilter").Split(';');
        for (int i = 0; i < levelFilter.Length; i++)
        {
            param["level"] = levelFilter[i];
            levelComboBox.AddItem(GUILocalizationRepo.GetLocalizedString("pty_abovelevel", param), levelFilter[i]);
        }
    }

    public void Init()
    {
        gameObject.SetActive(true);

        expanderToggles[0].GetChild(0).GetComponent<ExpanderSubData_Party>().SetToggleOn(true);  // must toggle child first
        expanderToggles[0].SetToggleOn(true);  // must toggle after child

        RPCFactory.CombatRPC.GetPartyList(0, 0, false, false);
    }

    private void OnDisable()
    {
        toggleGroup.allowSwitchOff = true;
        for (int i = 0; i < expanderToggles.Length; i++)
        {
            ExpanderToggle currToggle = expanderToggles[i];
            currToggle.SetToggleOn(false);
            for (int childIndex = 0; childIndex < currToggle.ChildCount; childIndex++)
                currToggle.GetChild(childIndex).GetComponent<ExpanderSubData_Party>().SetToggleOn(false);
        }
        toggleGroup.allowSwitchOff = false;
        selectedLocationId = 0;
        selectedMinLevel = 0;
        selectedAutoAccept = false;
        autoAcceptCheckBox.isOn = false;
    }

    public void CleanUp()
    {
        partyList.Clear();
        ClearPartyListGameObjects();
        if (refreshPartyListTimer != null)
        {
            if (GameInfo.gCombat != null && GameInfo.gCombat.mTimers != null)
                GameInfo.gCombat.mTimers.StopTimer(refreshPartyListTimer);
            refreshPartyListTimer = null;
        }
    }

    private void ClearPartyListGameObjects()
    {
        ClientUtils.DestroyChildren(partyListTransform);
        partyListScrollRect.verticalNormalizedPosition = 1;
    }

    public void Hide()
    {
        gameObject.SetActive(false);
        CleanUp();
    }

    public void OnSelectLocation(int locId)
    {
        if (selectedLocationId != locId)
        {
            selectedLocationId = locId;
            PopulateList();
        }
    }

    public void OnLevelSelectionChanged(int index)
    {
        int selectedValue;
        int.TryParse(levelComboBox.SelectedValue, out selectedValue);
        if (selectedMinLevel != selectedValue)
        {
            selectedMinLevel = selectedValue;
            PopulateList();
        }
    }

    public void OnToggleAutoAccept(bool isOn)
    {
        if (selectedAutoAccept != isOn)
        {
            selectedAutoAccept = isOn;
            PopulateList();
        }
    }

    public void OnGetPartyList(string result)
    {
        partyList.Clear();
        partyList = JsonConvertDefaultSetting.DeserializeObject<List<PartyInfo>>(result);
        PopulateList();
    }

    private void PopulateList()
    {
        ClearPartyListGameObjects();

        List<PartyInfo> filteredList = new List<PartyInfo>();

        if (selectedLocationId == 0)  // nearby - do not filter locationid
        {
            if (selectedAutoAccept)  // filter auto accept only and not party full
                filteredList = partyList.Where(x => x.minLevel >= selectedMinLevel && x.isAutoAccept
                    && x.memberCount < PartyData.MAX_MEMBERS).ToList();
            else
                filteredList = partyList.Where(x => x.minLevel >= selectedMinLevel).ToList();
        }
        else
        {
            if (selectedAutoAccept)  // filter auto accept only and not party full
                filteredList = partyList.Where(x => x.locationId == selectedLocationId && x.minLevel >= selectedMinLevel
                    && x.isAutoAccept && x.memberCount < PartyData.MAX_MEMBERS).ToList();
            else
                filteredList = partyList.Where(x => x.locationId == selectedLocationId && x.minLevel >= selectedMinLevel).ToList();
        }

        for (int i = 0; i < filteredList.Count; i++)
        {
            GameObject partyObj = ClientUtils.CreateChild(partyListTransform, partyInfoPrefab);
            partyObj.GetComponent<UI_Party_PartyInfo>().Init(this, filteredList[i]);
        }
    }

    public void OnClickCreateParty()
    {
        RPCFactory.CombatRPC.CreateParty();
    }

    public void OnClickJoinParty(int partyId)
    {
        RPCFactory.CombatRPC.JoinParty(partyId);
    }

    public void OnClickRefresh()
    {
        if (refreshPartyListTimer == null)
        {
            RPCFactory.CombatRPC.GetPartyList(selectedLocationId, selectedMinLevel, autoAcceptCheckBox.isOn, true);
            refreshPartyListTimer = GameInfo.gCombat.mTimers.SetTimer(20000, (arg) => { refreshPartyListTimer = null; }, null);
        }
        else
            UIManager.ShowSystemMessage(GUILocalizationRepo.GetLocalizedSysMsgByName("sys_party_RefreshPartyCD"));
    }

    public void OnClickInviteMember()
    {
        UIManager.OpenDialog(WindowType.DialogPartyInvite);
    }
}