using UnityEngine;
using ExitGames.Client.Photon;
using Zealot.Common;
using Zealot.Common.RPC;
using System.Collections.Generic;
using Zealot.Repository;
using System.Collections;

public class PlayerInLobby : Photon.MonoBehaviour
{
    private string mCurrentroom = "lobby";
    private int charCount;

    public static bool mFromLobby = false;

    void Awake()
    {
        GameInfo.gLobby = this;
    }

    void OnDestroy()
    {
        GameInfo.gLobby = null;
    }

    public void OnZealotRPCEvent(PhotonNetworkingMessage eventType, EventData eventData)
    {
        switch (eventType)
        {
            case PhotonNetworkingMessage.OnCombatEvent:
                RPCFactory.CombatRPC.OnCommand(this, eventData);
                break;

            case PhotonNetworkingMessage.OnNonCombatEvent:
                RPCFactory.NonCombatRPC.OnCommand(this, eventData);
                break;

            case PhotonNetworkingMessage.OnLobbyEvent:
                RPCFactory.LobbyRPC.OnCommand(this, eventData);
                break;
        }
    }

    [RPCMethod(RPCCategory.Lobby, (byte)ServerLobbyRPCMethods.GetCharactersResult)]
    public void GetCharactersResult(string charlist, int latestLoginIndex)
    {
        GetCharactersList charListData = GetCharactersList.Deserialize(charlist);
        charCount = charListData.CharList.Count;
        //todo: if more than one characters, show character list.
        if (charCount > 0)
        {
            var chardata = charListData.CharList[0];
            GameInfo.mChar = chardata.Name;
#if ZEALOT_DEVELOPMENT && !UNITY_EDITOR
            Zealot.RemoteLog.SetFileID(chardata.Name);
#endif
            EnterGame();
        }
        else
        {
            SceneLoader.Instance.LoadLevel("UI_CreateChar");
        }
        Debug.Log("GetCharactersResult character count is " + charCount);   
	}

    [RPCMethod(RPCCategory.Lobby, (byte)ServerLobbyRPCMethods.DeleteCharacterResult)]
    public void DeleteCharacterResult(bool result, string charname)
    {
        // May be needed
    }

    [RPCMethod(RPCCategory.Lobby, (byte)ServerLobbyRPCMethods.TransferRoom)]
    public void TransferRoom(string levelName)
    {
        //BotCrossLevelVariables.Reset();
        //BotSettings.Init();
        if (!GameInfo.DCReconnectingGameServer || (levelName != "lobby" && ClientUtils.GetCurrentLevelName() != levelName))
            UIManager.ShowLoadingScreen(true);
    }

    [RPCMethod(RPCCategory.Lobby, (byte)ServerLobbyRPCMethods.LoadLevel)]
    public void LoadLevel(string levelName)
    {
        if (levelName == "lobby")
            OnJoinedRoom();
        else
        {
            if (GameInfo.DCReconnectingGameServer)
            {
                if (ClientUtils.GetCurrentLevelName() == levelName)
                {
                    StartCoroutine(DelayShowLoadingScreen_Combat(false, 0.1f));
                }
                else
                {
                    GameInfo.OnLevelChanged();
                    PhotonNetwork.LoadLevel(levelName); // Lobby to combat.
                }
            }
            else 
                PhotonNetwork.LoadLevel(levelName); // Lobby to combat.
        } 
    }

    [RPCMethod(RPCCategory.Lobby, (byte)ServerLobbyRPCMethods.CreateCharacterSuccess)]
    public void CreateCharacterSuccess(string charname)
    {
        // here play the movie before entergame
        GameInfo.mChar = charname;
        EnterGame();
    }

    [RPCMethod(RPCCategory.Lobby, (byte)ServerLobbyRPCMethods.ShowSystemMessage)]
    public void ShowSystemMessage(string ret)
    {
        UIManager.ShowSystemMessage(GUILocalizationRepo.GetLocalizedSysMsgByName(ret, null));
    }

    private string GetCookie()
	{
        return LoginData.Instance.cookieId.ToString();
	}

    public void JoinLobby()
    {
        RPCFactory.SetMainContext(typeof(PlayerInLobby));
        if (GameInfo.gClientState == PiliClientState.Login || GameInfo.DCReconnectingGameServer)
        {
            if (PhotonNetwork.connected)
            {
                Dictionary<byte, object> op = new Dictionary<byte, object>();
                op[ParameterCode.RoomName] = mCurrentroom;
                PhotonNetwork.networkingPeer.OpCustom(OperationCode.JoinGame, op, true);
            }    
        }
        GameInfo.gClientState = PiliClientState.Lobby;
        //if (GameInfo.gClientState == PiliClientState.Login)
        //{       
        //    ClientUtils.PlayMusic(GameSettings.MusicEnabled);
        //}
    }

    private IEnumerator DelayGetCharOnTransferServer()
    {
        yield return new WaitForSeconds(3);
        RPCFactory.LobbyRPC.GetCharacters();
        if (ClientUtils.GetCurrentLevelName() == "lobby")
            StartCoroutine(DelayShowLoadingScreen(true, 0.1f));
    }

    private IEnumerator DelayShowLoadingScreen(bool show, float interval)
    {
        for (int i = 1; i <= 10; i++)
        {
            UIManager.LoadingScreen.SetLoadingScreenProgress(0.1f * i);
            yield return new WaitForSeconds(interval);
        }
        if (show)
            UIManager.ShowLoadingScreen(false);
    }

    private IEnumerator DelayShowLoadingScreen_Combat(bool show, float interval)
    {
        if (GameInfo.TransferingServer)
            yield return DelayShowLoadingScreen(false, 0.1f);
        PhotonNetwork.networkingPeer.NewSceneLoaded();
        GameInfo.OnReconnected();
        yield break;
    }

    public void OnJoinedRoom()
    {
        PhotonNetwork.SetNetworkState(ClientState.Joined);
        GameInfo.gClientState = PiliClientState.Lobby;
        if (GameInfo.TransferingServer)
            StartCoroutine(DelayGetCharOnTransferServer());
        else
            RPCFactory.LobbyRPC.GetCharacters();
    }

    public void InsertCharacter(string charname, byte jobsect, byte talent, byte faction, int initial_skill)
	{
		// Character creation starts here
		RPCFactory.LobbyRPC.InsertCharacter(jobsect, talent, faction, charname);
	}

    //public void DeleteCharacter(string charname)
    //{
    //    RPCFactory.LobbyRPC.DeleteCharacter(charname);
    //}

    public void EnterGame()
    {
        string selectedchar = GameInfo.mChar;
        if (selectedchar != "")
        {
            RPCFactory.LobbyRPC.EnterGame(selectedchar);
            // Used by MyCard builds to retrieve incomplete transactions
            mFromLobby = true;
        }
    }

    public void OnConfirmExitToLogin()
    {
        PhotonNetwork.networkingPeer.Disconnect();
        UIManager.CloseDialog(WindowType.DialogYesNoOk);
    }

    public void OnCancelExitToLogin()
    {
        UIManager.CloseDialog(WindowType.DialogYesNoOk);
    }

    public void OnExitToLoginClick()
    {                
        Dictionary<string, string> parameters = new Dictionary<string, string>();        
        string message = GameUtils.FormatString(GUILocalizationRepo.GetLocalizedString("sys_ConfirmExitToLogin"), parameters);
        UIManager.OpenYesNoDialog(message, OnConfirmExitToLogin, OnCancelExitToLogin);
    }
}
