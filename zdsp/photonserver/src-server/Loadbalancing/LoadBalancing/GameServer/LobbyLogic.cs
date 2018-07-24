namespace Photon.LoadBalancing.GameServer
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using UnityEngine;
    using Photon.SocketServer;
    using Kopio.JsonContracts;
    using Zealot.Common;
    using Zealot.Common.RPC;
    using Zealot.Repository;
    using Zealot.Server.Rules;
    using Zealot.Server.Counters;
    using Hive;

    public partial class GameLogic
    {
        [RPCMethod(RPCCategory.Lobby, (byte)ClientLobbyRPCMethods.InsertCharacter)]
        public async Task<OperationResponse> InsertCharacter(byte jobsect, byte talent, byte faction, string charname, GameClientPeer peer)
        {
            string filteredTxt = "";
            if (WordFilterRepo.FilterString(charname, '*', DirtyWordType.GameName, out filteredTxt))
            {
                peer.ZRPC.LobbyRPC.ShowSystemMessage("sys_CharCreation_ForbiddenWord", peer);
                return null;
            }

            if (peer.GetCharacterCount() >= 4)
            {
                peer.ZRPC.LobbyRPC.ShowSystemMessage("ret_CharCreation_CharactersMoreThan3", peer);
                return null;
            }
            jobsect = 1;
            var chardefault = GameRules.NewCharacterData(false, charname, jobsect, faction, talent);
            string strChar = chardefault.SerializeForDB(false);            

            int serverline = GameApplication.Instance.GetMyServerline();
            string userId = peer.mUserId;

            Tuple<Guid?, bool> result = 
                await GameApplication.dbRepository.Character.InsertNewCharacter(userId, charname, serverline, chardefault.JobSect, chardefault.portraitID, chardefault.Faction, strChar);         
            if (!result.Item1.HasValue)
            {
                bool nameExists = result.Item2;
                peer.ZRPC.LobbyRPC.ShowSystemMessage("ret_CharCreation_NameExists", peer);
            }
            else
            {
                //may change
                //byte recommend = (byte)GameApplication.Instance.Leaderboard.GetFactionRecommend();
                byte recommend = 0;
                string charId = result.Item1.ToString();
                ZLogCreateChar(peer, charId, jobsect, faction, recommend);

                await GetCharacters(peer);
                //await EnterGame(charname, peer);
            }
            return null;
        }

        [RPCMethod(RPCCategory.Lobby, (byte)ClientLobbyRPCMethods.GetCharacters)]
        public async Task GetCharacters(GameClientPeer peer)
        {
            // Get from database and return
            List<Dictionary<string, object>> chars = await GameApplication.dbRepository.Character.GetByUserID(peer.mUserId, GameApplication.Instance.GetMyServerline());
            if (chars == null)
                return;

            GetCharactersList chardataList = new GetCharactersList();
            int latestLogoutIndex = 0;
            DateTime latestLogoutDatetime = DateTime.MinValue;
            //try
            //{
                for (int index = 0; index < chars.Count; ++index)
                {
                    Dictionary<string, object> chardata = chars[index];
                    //Find closest logout date time, then pass argu to InitCharacterList
                    DateTime dtlogout = (DateTime)chardata["dtlogout"];
                    if (latestLogoutDatetime < dtlogout)
                    {
                        latestLogoutIndex = index;
                        latestLogoutDatetime = dtlogout;
                    }

                    // Deserializefromdb, Serializeforclient
                    CharacterData cd = CharacterData.DeserializeFromDB((string)chardata["characterdata"]);
                    if (cd != null)
                    {
                        CharacterCreationData ccd = new CharacterCreationData();
                        ccd.Name = cd.Name;
                        ccd.JobSect = cd.JobSect;
                        ccd.ProgressLevel = cd.ProgressLevel;
                        ccd.EquipmentInventory = cd.EquipmentInventory;
                        chardataList.CharList.Add(ccd);
                    }
                }
            //}
            //catch (Exception ex)
            //{
            //    log.Error(ex.ToString());
            //    GameApplication.Instance.RethrowAsyncException(ex);
            //}
            peer.CharacterList = chars;
            peer.ZRPC.LobbyRPC.GetCharactersResult(chardataList.Serialize(), latestLogoutIndex, peer);
        }

        //[RPCMethod(RPCCategory.Lobby, (byte)ClientLobbyRPCMethods.DeleteCharacter)]
        //public async Task DeleteCharacter(string charname, HivePeer peer)
        //{
        //    bool result = await GameApplication.dbRepository.Character.DeleteCharacterByName(charname);
        //    GameClientPeer gp = (peer as GameClientPeer);
        //    gp.ZRPC.LobbyRPC.DeleteCharacterResult(result, charname, peer);
        //    if (result)
        //    {
        //        List<Dictionary<string, object>> charlist = gp.CharacterList;
        //        bool found = false;
        //        int i = 0, charlistCnt = charlist.Count;
        //        for (; i<charlistCnt; ++i)
        //        {
        //            Dictionary<string, object> charinfo = charlist[i];
        //            string name = charinfo["charname"] as string;
        //            if (name == charname)
        //            {
        //                found = true;
        //                break;
        //            }
        //        }
        //        if (found)
        //            charlist.RemoveAt(i);

        //        gp.CharacterList = charlist;
        //    }
        //}

        [RPCMethod(RPCCategory.Lobby, (byte)ClientLobbyRPCMethods.EnterGame)]
        public void EnterGame(string charname, GameClientPeer peer)
        {
            if (peer.mChar != "")
                return; // Already in process.
            //try
            //{
                peer.SetChar(charname);
                CharacterData charData = peer.CharacterData;
                if (charData != null)
                {
                    GameCounters.ExecutionFiberQueue.Increment();
                    GameApplication.Instance.executionFiber.Enqueue(() =>
                    {
                        GameCounters.ExecutionFiberQueue.Decrement();
                        //charData.TrainingRealmDone = true;
                        if (!charData.TrainingRealmDone)
                        {                           
                            //RealmTutorialJson FirstGuideRealmJson = RealmRepo.TutorialRealmJson;
                            //if (FirstGuideRealmJson != null)
                            //{
                            //    LevelJson level = LevelRepo.GetInfoById(FirstGuideRealmJson.level);
                            //    if (level != null)
                            //    {
                            //        // Create the turorial realm using realmid.
                            //        peer.CreateRealm(FirstGuideRealmJson.id, level.unityscene);
                            //        return;
                            //    }
                            //}
                        }
                        //charData.lastlevelid = 9;
                        LevelJson levelInfo = LevelRepo.GetInfoById(charData.lastlevelid);
                        
                        if (levelInfo != null)
                        {
                            string levelName = levelInfo.unityscene;
                            if (RealmRepo.IsWorld(levelName))
                            {
                                float[] lastpos = charData.lastpos;
                                float[] lastforward = charData.lastdirection;
                                peer.TransferToRealmWorld(levelName);
                                peer.mSpawnPos = new Vector3(lastpos[0], lastpos[1], lastpos[2]);
                                peer.mSpawnForward = new Vector3(lastforward[0], 0, lastforward[2]);
                            }
                            else // Character spawn to non-world realm
                            {
                                string roomGuid = charData.roomguid;
                                Room room;
                                GameApplication.Instance.GameCache.TryGetRoomWithoutReference(roomGuid, out room);
                                Game game = room as Game;
                                if (game != null && game.controller != null && game.controller.mRealmController != null && 
                                    game.controller.mRealmController.CanReconnect())
                                {
                                    float[] lastpos = charData.lastpos;
                                    float[] lastforward = charData.lastdirection;
                                    peer.TransferRoom(roomGuid, levelName);
                                    peer.mSpawnPos = new Vector3(lastpos[0], lastpos[1], lastpos[2]);
                                    peer.mSpawnForward = new Vector3(lastforward[0], 0, lastforward[2]);
                                }
                                else
                                    peer.TransferToCity(charData.ProgressLevel);
                            }
                        }
                        else
                            peer.TransferToCity(charData.ProgressLevel);
                    });
                }
            //}
            //catch (Exception ex)
            //{
            //    log.Error(ex.ToString());
            //    GameApplication.Instance.RethrowAsyncException(ex);
            //}
        }
        
        [RPCMethod(RPCCategory.Lobby, (byte)ClientLobbyRPCMethods.GetLevelData)]
        public void GetLevelData(GameClientPeer peer)
        {
            ZRPC.NonCombatRPC.SendString(LevelRepo.GetLevelDataString(), peer);
        }

        //private void ZLogCreateChar(GameClientPeer playerPeer,string charName,byte job,byte factionSelect)
        private void ZLogCreateChar(GameClientPeer playerPeer, string charId, byte job, byte factionSelect, byte recommend)
        {
            
            //玩家ID，職業ID，時間，選擇武林，我們推薦武林，選擇武林是否為推薦武

            string jobString = Enum.GetName(typeof(JobType), job);
            string selectedFactionString = Enum.GetName(typeof(FactionType), factionSelect);
            string recommandedFactionString = Enum.GetName(typeof(FactionType), recommend);

            string message = string.Format(@"jobsect: {0} | selectedFaction: {1} | recommandedFaction: {2} | isRecommanded : {3} | charId : {4}",
                jobString,
                selectedFactionString,
                recommandedFactionString,
                (factionSelect == recommend),
                charId);

            Zealot.Logging.Client.LogClasses.CreateChar createCharLog = new Zealot.Logging.Client.LogClasses.CreateChar();
            createCharLog.userId = playerPeer.mUserId;
            createCharLog.charId = charId;
            createCharLog.message = message;
            createCharLog.jobsect = jobString;
            createCharLog.selectedFaction = selectedFactionString;
            createCharLog.recommandedFaction = recommandedFactionString;//temp
            createCharLog.isRecommanded = (factionSelect == recommend);
            var ignoreAwait = Zealot.Logging.Client.LoggingAgent.Instance.LogAsync(createCharLog);
        }
    }
}
