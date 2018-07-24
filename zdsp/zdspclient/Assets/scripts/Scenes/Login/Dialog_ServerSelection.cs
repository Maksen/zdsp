using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using Zealot.Common;

public class Dialog_ServerSelection : MonoBehaviour
{
    // Editor Linked Gameobjects
    [SerializeField]
    Text txtLastEnteredServer = null;
    [SerializeField]
    ToggleGroup togglegrpServerList = null;
    [SerializeField]
    GameObject prefabServerlistData = null;
    [SerializeField]
    ToggleGroup togglegrpServersData = null;
    [SerializeField]
    GameObject prefabServerData = null;

    Dictionary<int, List<ServerInfo>> serverBuckets = new Dictionary<int, List<ServerInfo>>(); //serverline <- ordered serverinfo by serverid ASC
    Dictionary<int, ServerLine> serverLineMap = new Dictionary<int, ServerLine>(); //serverline <- ServerLine

    bool isInitDefault = false;
    int selectedServerLine = 0;
    int selectedServerId = 0;
	
    void OnEnable()
    {
        string serverName = "";
        var serverInfo = GameInfo.gUILogin.GetLastLogin();
        if (serverInfo != null)
        {
            string world = GameInfo.gUILogin.serverLineDict[serverInfo.serverLine].displayName;
            serverName = string.Format("<color={0}>{1} {2}</color>", ClientUtils.GetServerStatusColor(serverInfo.serverLoad), world, serverInfo.serverName);
        }
        txtLastEnteredServer.text = serverName;
        PopulateServers();
    }

    void OnDisable()
    {
        ServerInfo serverInfo = GameInfo.gLogin.SelectedServerInfo;
        GameInfo.gUILogin.SetSelectedGameServer((serverInfo != null) ? serverInfo.id : 0);

        foreach (Transform child in togglegrpServerList.transform)
            Destroy(child.gameObject);
        foreach (Transform child in togglegrpServersData.transform)
            Destroy(child.gameObject);
        selectedServerLine = 0;
        selectedServerId = 0;
    }

    public void InitServerBuckets(Dictionary<int, ServerInfo> serverInfoList, Dictionary<int, ServerLine> serverLineMap)
    {
        this.serverLineMap = serverLineMap;
        serverBuckets.Clear();
        foreach (var kvp in serverInfoList)
        {
            int serverline = kvp.Value.serverLine;
            if (!serverBuckets.ContainsKey(serverline))
                serverBuckets.Add(serverline, new List<ServerInfo>());
            serverBuckets[serverline].Add(kvp.Value);
        }
        var serverlines = serverBuckets.Keys.ToList();
        for (int index = 0; index < serverlines.Count; index++)
        {
            int serverline = serverlines[index];
            serverBuckets[serverline] = serverBuckets[serverline].OrderBy(x => x.id).ToList(); //reorder each serverline by serverid ASC
        }
    }

    void PopulateServers()
    {
        isInitDefault = true;
        // Instantiate Server List Tabs
        Dictionary<int, Toggle> togglesByServerline = new Dictionary<int, Toggle>();
        foreach (int serverline in serverBuckets.Keys.OrderBy(x => x))
        {
            GameObject serverlistData = Instantiate(prefabServerlistData);
            serverlistData.transform.SetParent(togglegrpServerList.transform, false);
            int local_index = serverline;
            serverlistData.GetComponent<ServerListData>().Init(serverLineMap[serverline], togglegrpServerList, (ison) => OnValueChangedServerList(local_index, ison));
            togglesByServerline.Add(serverline, serverlistData.GetComponent<Toggle>());
        }

        ServerInfo selectedServerInfo = GameInfo.gLogin.SelectedServerInfo;
        if (togglesByServerline.Count > 0)
        {
            if (selectedServerInfo != null && togglesByServerline.ContainsKey(selectedServerInfo.serverLine))
                togglesByServerline[selectedServerInfo.serverLine].isOn = true;
            else
            {
                foreach(var kvp in serverLineMap)
                {
                    if (kvp.Value.recommend && togglesByServerline.ContainsKey(kvp.Key))
                    {
                        togglesByServerline[kvp.Key].isOn = true;
                        break;
                    }
                }
            }
        }
        isInitDefault = false;
    }

    public void OnValueChangedServerList(int serverline, bool ison)
    {
        if (!ison)
            return;
        selectedServerLine = serverline;
        foreach (Transform child in togglegrpServersData.transform)
            Destroy(child.gameObject);
        
        List<ServerInfo> serverInfoList = serverBuckets[serverline];
        if (serverInfoList.Count > 0)
        {
            Dictionary<int, Toggle> togglesByServerId = new Dictionary<int, Toggle>();
            for (int index = 0; index < serverInfoList.Count; index++)
            {
                GameObject serverData = Instantiate(prefabServerData);
                serverData.transform.SetParent(togglegrpServersData.transform, false);
                int serverid = serverInfoList[index].id;
                serverData.GetComponent<ServerData>().Init(serverInfoList[index], togglegrpServersData, (on) => OnValueChangedServersData(serverid, on));
                togglesByServerId.Add(serverid, serverData.GetComponent<Toggle>());
            }
            ServerInfo selectedServerInfo = GameInfo.gLogin.SelectedServerInfo;
            if (selectedServerInfo != null && togglesByServerId.ContainsKey(selectedServerInfo.id))
                togglesByServerId[selectedServerInfo.id].isOn = true;
            else
            {
                int serverid = serverInfoList.OrderBy(x => x.serverLoad).ToList()[0].id;
                togglesByServerId[serverid].isOn = true;
            }
        }
    }

    public void OnValueChangedServersData(int serverid, bool ison)
    {
        if (!ison)
            return;
        selectedServerId = serverid;
        ServerInfo currServerInfo = serverBuckets[selectedServerLine].Find(x => x.id == serverid);
        GameInfo.gLogin.SelectedServerInfo = currServerInfo;
        //Debug.LogFormat("lastEnteredServerInfo.idDisplay: {0}", lastEnteredServerInfo.idDisplay);
        //if (!isInitDefault)
        //    UIManager.CloseDialog(WindowType.DialogServerSelection);
    }
}
