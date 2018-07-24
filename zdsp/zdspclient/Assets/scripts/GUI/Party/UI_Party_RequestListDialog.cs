using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using Zealot.Common;

public class UI_Party_RequestListDialog : BaseWindowBehaviour
{
    [SerializeField] ScrollRect listScrollRect;
    [SerializeField] Transform dataContentTransform;
    [SerializeField] GameObject requestDataPrefab;
    [SerializeField] ComboBoxA sortComboBox;

    private Dictionary<string, GameObject> requestDataList = new Dictionary<string, GameObject>();

    public override void OnCloseWindow()
    {
        base.OnCloseWindow();
        ClearList();
        sortComboBox.ResetSelected();  // deselect so will call OnSortSelectionChanged next time OnEnable
    }

    public void OnSortSelectionChanged(int index)  // will be called when combobox OnEnable
    {
        RefreshList();
    }

    public void RefreshList()
    {
        ClearList();
        if (GameInfo.gLocalPlayer == null)
            return;

        PartyStatsClient partyStats = GameInfo.gLocalPlayer.PartyStats;
        if (partyStats != null)
        {
            var requestList = partyStats.GetPartyRequestList().Values;
            if (requestList.Count > 0)
            {
                List<PartyRequest> sortedList;
                if (sortComboBox.SelectedIndex == 0) // by level
                    sortedList = requestList.OrderByDescending(x => x.level).ToList();
                else  // by jobtype
                    sortedList = requestList.OrderByDescending(x => (int)x.jobType).ToList();

                for (int i = 0; i < sortedList.Count; i++)
                {
                    GameObject obj = ClientUtils.CreateChild(dataContentTransform, requestDataPrefab);
                    obj.GetComponent<UI_Party_RequestData>().Init(sortedList[i], OnClickProcessRequest);
                    requestDataList.Add(sortedList[i].name, obj);
                }
            }
        }
    }

    public void RemoveRequest(string name)
    {
        if (requestDataList.ContainsKey(name))
        {
            Destroy(requestDataList[name]);
            requestDataList.Remove(name);
        }
    }

    public void OnClickProcessRequest(string name, bool isAccept)
    {
        RPCFactory.CombatRPC.ProcessPartyRequest(name, isAccept);
    }

    private void ClearList()
    {
        requestDataList.Clear();
        ClientUtils.DestroyChildren(dataContentTransform);
        listScrollRect.verticalNormalizedPosition = 1f;
    }

}
