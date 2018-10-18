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
        [RPCMethod(RPCCategory.Lobby, (byte)ClientLobbyRPCMethods.GetCharacters)]
        public async Task GetCharacters(bool newcharacter, GameClientPeer peer)
        {
            // Get from database and return
            List<Dictionary<string, object>> chars = await GameApplication.dbRepository.Character.GetByUserID(peer.mUserId, GameApplication.Instance.GetMyServerline());
            if (chars == null)
                return;

            GetCharactersList chardataList = new GetCharactersList();
            int latestLogoutIndex = 0;
            DateTime latestLogoutDatetime = DateTime.MinValue;
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
                    ccd.Gender = cd.Gender;
                    ccd.Levelid = cd.lastlevelid;
                    ccd.RemoveCharDT = cd.RemoveCharDT;
                    ccd.ProgressLevel = cd.ProgressLevel;
                    ccd.EquipmentInventory = cd.EquipmentInventory;
                    chardataList.CharList.Add(ccd);
                }
            }
            peer.CharacterList = chars;
            peer.ZRPC.LobbyRPC.GetCharactersResult(chardataList.Serialize(), latestLogoutIndex, newcharacter, peer);
        }

        [RPCMethod(RPCCategory.Lobby, (byte)ClientLobbyRPCMethods.DeleteCharacter)]
        public async Task DeleteCharacter(string charname, GameClientPeer peer)
        {
            CharacterData characterData = null;
            string charid = "";
            foreach (Dictionary<string, object> charinfo in peer.CharacterList)
            {
                if (charname.Equals((string)charinfo["charname"]))
                {
                    string strCharData = (string)charinfo["characterdata"];
                    charid = charinfo["charid"].ToString();
                    characterData = CharacterData.DeserializeFromDB(strCharData);
                    break;
                }
            }

            if (characterData  != null)
            {
                if (string.IsNullOrEmpty(characterData.RemoveCharDT))
                {
                    DateTime endtime = DateTime.Now.AddHours(24);
                    characterData.RemoveCharDT = string.Format("{0:yyyy}.{0:MM}.{0:dd}-{0:HH}:{0:mm}", endtime);
                    peer.SaveCharacterForRemoveCharacter(charid, charname, characterData);
                    peer.ZRPC.LobbyRPC.DeleteCharacterResult(0, charname, characterData.RemoveCharDT, peer);
                }
                else
                {
                    DateTime endDT = DateTime.ParseExact(characterData.RemoveCharDT, "yyyy.MM.dd-HH:mm", null);
                    if (DateTime.Now > endDT)
                    {
                        peer.ZRPC.LobbyRPC.DeleteCharacterResult(1, charname, characterData.RemoveCharDT, peer);
                    }
                    else
                    {
                        bool result = await GameApplication.dbRepository.Character.DeleteCharacterByName(charname);
                        peer.ZRPC.LobbyRPC.DeleteCharacterResult(result ? 2 : 3, charname, characterData.RemoveCharDT, peer);
                        if (result)
                        {
                            List<Dictionary<string, object>> charlist = peer.CharacterList;
                            for (int i = 0; i < charlist.Count; i++)
                            {
                                Dictionary<string, object> charinfo = peer.CharacterList[i];
                                string name = charinfo["charname"] as string;
                                if (name == charname)
                                {
                                    peer.CharacterList.RemoveAt(i);
                                    break;
                                }
                            }
                        }
                    }
                }
            }               
            else
            {
                peer.ZRPC.LobbyRPC.DeleteCharacterResult(3, charname, "", peer);
            }
        }

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

        [RPCMethod(RPCCategory.Lobby, (byte)ClientLobbyRPCMethods.CheckCharacterName)]
        public async Task<OperationResponse> CheckCharacterName(string charname, GameClientPeer peer)
        {
            Tuple<bool, string> result = await GameApplication.dbRepository.Character.IsCharacterExists(charname);
            peer.ZRPC.LobbyRPC.CheckCharacterNameResult(result.Item1, peer);
            return null;
        }

        [RPCMethod(RPCCategory.Lobby, (byte)ClientLobbyRPCMethods.CreateCharacter)]
        public async Task<OperationResponse> CreateCharacter(string charname, byte gender, int hairstyle, int haircolor, int makeup, int skincolor, GameClientPeer peer)
        {
            if (peer.GetCharacterCount() >= 4)
            {
                peer.ZRPC.LobbyRPC.ShowSystemMessage("ret_CharCreation_CharactersMoreThan3", peer);
                return null;
            }

            var chardefault = GameRules.NewCharacterData(false, charname, gender, hairstyle, haircolor, makeup, skincolor);
            string strChar = chardefault.SerializeForDB(false);

            int serverline = GameApplication.Instance.GetMyServerline();
            string userId = peer.mUserId;

            Tuple<Guid?, bool> result =
                await GameApplication.dbRepository.Character.InsertNewCharacter(userId, charname, serverline, chardefault.JobSect, chardefault.portraitID, chardefault.Faction, strChar);
            if (!result.Item1.HasValue)
            {
                peer.ZRPC.LobbyRPC.ShowSystemMessage("charactercreation_failed", peer);
            }
            else
            {
                string charId = result.Item1.ToString();
                ZLogCreateChar(peer, charId, (byte)JobType.Newbie, gender, hairstyle, haircolor, makeup, skincolor);

                await GetCharacters(true, peer);
            }
            return null;
        }

        [RPCMethod(RPCCategory.Lobby, (byte)ClientLobbyRPCMethods.CancelDeleteCharacter)]
        public void CancelDeleteCharacter(string charname, GameClientPeer peer)
        {
            CharacterData characterData = null;
            string charid = "";
            foreach (Dictionary<string, object> charinfo in peer.CharacterList)
            {
                if (charname.Equals((string)charinfo["charname"]))
                {
                    string strCharData = (string)charinfo["characterdata"];
                    charid = charinfo["charid"].ToString();
                    characterData = CharacterData.DeserializeFromDB(strCharData);
                    break;
                }
            }

            if (characterData != null)
            {
                characterData.RemoveCharDT = "";
                peer.SaveCharacterForRemoveCharacter(charid, charname, characterData);
                peer.ZRPC.LobbyRPC.CancelDeleteCharacterResult(true, charname, peer);
            }
            else
            {
                peer.ZRPC.LobbyRPC.CancelDeleteCharacterResult(false, charname, peer);
            }
        }

        private void ZLogCreateChar(GameClientPeer playerPeer, string charId, byte job, byte gender, int hairstyle, int haircolor, int makeup, int skincolor)
        {
            
            //玩家ID，職業ID，時間，外觀

            string jobString = Enum.GetName(typeof(JobType), job);
            string genderString = Enum.GetName(typeof(Gender), gender);

            string message = string.Format(@"jobsect: {0} | selectedGender: {1} | selectedHairStyle: {2} | selectedHairColor: {3} | selectedMakeUp: {4} | selectedSkinColor: {5} | charId : {6}",
                jobString,
                genderString,
                hairstyle,
                haircolor,
                makeup,
                skincolor,
                charId);

            Zealot.Logging.Client.LogClasses.CreateChar createCharLog = new Zealot.Logging.Client.LogClasses.CreateChar();
            createCharLog.userId = playerPeer.mUserId;
            createCharLog.charId = charId;
            createCharLog.message = message;
            createCharLog.jobsect = jobString;
            var ignoreAwait = Zealot.Logging.Client.LoggingAgent.Instance.LogAsync(createCharLog);
        }
    }
}
