using Zealot.Common.RPC;

public partial class ClientMain
{
    [RPCMethod(RPCCategory.Combat, (byte)ServerCombatRPCMethods.TransferRoom)]
    public void TransferRoom(string levelName)
    {
        if (levelName != "lobby")
            UIManager.ShowLoadingScreen(true);
    }

    [RPCMethod(RPCCategory.Combat, (byte)ServerCombatRPCMethods.LoadLevel)]
    public void LoadLevel(string levelName)
    {
        if (levelName == "lobby")
            GameInfo.OnQuitGame();
        else
            GameInfo.OnLevelChanged();
        PhotonNetwork.LoadLevel(levelName);     
    }
}
