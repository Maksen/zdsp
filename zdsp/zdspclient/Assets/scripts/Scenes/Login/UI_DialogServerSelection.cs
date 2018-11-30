using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using Zealot.Common;

public class UI_DialogServerSelection : MonoBehaviour
{
    [SerializeField]
    ToggleGroup togglegrpServerList = null;
    [SerializeField]
    GameObject prefabServerData = null;

    Dictionary<int, List<ServerInfo>> serverBuckets = new Dictionary<int, List<ServerInfo>>(); //serverline <- ordered serverinfo by serverId ASC
    int selectedServerLine = -1;
    int selectedServerId = -1;

    void OnEnable()
    {
        selectedServerId = selectedServerLine = -1;
        PopulateGameServers();
    }

    void OnDisable()
    {
        if (GameInfo.gLogin != null)
        {
            ServerInfo serverInfo = GameInfo.gLogin.SelectedServerInfo;
            GameInfo.gUILogin.SetSelectedGameServer((serverInfo != null) ? serverInfo.Id : 0);
        }

        foreach (Transform child in togglegrpServerList.transform)
            Destroy(child.gameObject);
    }

    public void InitServerBuckets(Dictionary<int, ServerInfo> serverInfoList)
    {
        serverBuckets.Clear();
        foreach (var kvp in serverInfoList)
        {
            ServerInfo serverInfo = kvp.Value;
            int serverline = serverInfo.ServerLine;
            if (!serverBuckets.ContainsKey(serverline))
                serverBuckets[serverline] = new List<ServerInfo>();
            serverBuckets[serverline].Add(serverInfo);
        }

        List<int> serverLines = serverBuckets.Keys.ToList();
        int count = serverLines.Count;
        for (int index = 0; index < count; ++index)
        {
            int serverline = serverLines[index];
            serverBuckets[serverline] = serverBuckets[serverline].OrderBy(x => x.Id).ToList(); //reorder each serverline by serverid ASC
        }
    }

    void PopulateGameServers()
    {
        Dictionary<int, ServerLine> serverLineDict = GameInfo.gLogin.ServerLineRefDict;
        Dictionary<int, Toggle> togglesByServerId = new Dictionary<int, Toggle>();
        List<int> serverLines = serverBuckets.Keys.OrderBy(x => x).ToList();
        int serverLineCount = serverLines.Count;
        for (int i = 0; i < serverLineCount; ++i)
        {
            List<ServerInfo> gameServerList = serverBuckets[serverLines[i]];
            int gameServerCount = gameServerList.Count;
            for (int j = 0; j < gameServerCount; ++j)
            {
                ServerInfo gameServer = gameServerList[j];
                GameObject serverData = Instantiate(prefabServerData, togglegrpServerList.transform, false);
                int serverId = gameServer.Id, serverLine = gameServer.ServerLine;
                serverData.GetComponent<ServerData>().Init(gameServer, serverLineDict[serverLine], togglegrpServerList,
                    (isOn) => OnValueChangedServerData(serverLine, serverId, isOn));

                togglesByServerId[serverId] = serverData.GetComponent<Toggle>();
            }
        }

        // Set default toggle
        ServerInfo selectedServerInfo = GameInfo.gLogin.SelectedServerInfo;
        Toggle defaultToggle;
        if (selectedServerInfo != null && togglesByServerId.TryGetValue(selectedServerInfo.Id, out defaultToggle))
            defaultToggle.isOn = true;
        else if (togglesByServerId.Count > 0)
        {
            List<ServerLine> serverLineList = serverLineDict.Values.OrderByDescending(x => x.recommended).ToList();
            int count = serverLineList.Count;
            for (int i = 0; i < count; ++i)
            {
                List<ServerInfo> serverInfoList = null;
                if (serverBuckets.TryGetValue(serverLineList[i].serverLineId, out serverInfoList))
                {
                    int serverid = serverInfoList.OrderBy(x => x.ServerLoad).ToList()[0].Id;
                    togglesByServerId[serverid].isOn = true;
                    break;
                }
            }
        }
    }

    public void OnValueChangedServerData(int serverLine, int serverId, bool ison)
    {
        if (!ison || (selectedServerLine == serverLine && selectedServerId == serverId))
            return;

        ServerInfo serverInfo = serverBuckets[serverLine].Find(x => x.Id == serverId);
        if (serverInfo == null)
            return;

        selectedServerLine = serverLine;
        selectedServerId = serverId;
        GameInfo.gLogin.SelectedServerInfo = serverInfo;
        //Debug.LogFormat("lastEnteredServerInfo.idDisplay: {0}", lastEnteredServerInfo.idDisplay);
    }
}
