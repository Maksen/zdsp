using Kopio.JsonContracts;
using System.Collections.Generic;
using UnityEngine;
using Zealot.Entities;
using Zealot.Repository;

namespace Zealot.Spawners
{
    [AddComponentMenu("Spawners at Server/PortalEntry")]
    public class PortalEntry : ServerEntity {
        //public bool activeOnStartup = true;
        [Tooltip("ExitName must be exist and unique")]
        public string myName = "";
        public string exitName = "";
        //public bool partyTeleport;
        //public DetectionArea detectionArea;

        public override ServerEntityJson GetJson()
        {
            PortalEntryJson jsonclass = new PortalEntryJson ();
            GetJson (jsonclass);
            return jsonclass;
        }
        
        public void GetJson(PortalEntryJson jsonclass)
        {
            //jsonclass.activeOnStartup = activeOnStartup;
            jsonclass.myName = myName;
            jsonclass.exitName = exitName;
            //jsonclass.partyTeleport = partyTeleport;
            //jsonclass.detectionArea = detectionArea;
            base.GetJson (jsonclass);
        }

        public void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("LocalPlayer") && !string.IsNullOrEmpty(myName) && !string.IsNullOrEmpty(exitName))
            {
                int myProgressLvl = GameInfo.gLocalPlayer.GetAccumulatedLevel();
                string[] exitNames = exitName.Split(';');
                int numOfExits = exitNames.Length;
                if (numOfExits == 1)
                {
                    LocationData mPortalExitData;
                    if (PortalInfos.mExits.TryGetValue(exitName, out mPortalExitData))
                    {
                        if (mPortalExitData.mLevel != ClientUtils.GetCurrentLevelName())
                        {
                            RealmJson realmJson = RealmRepo.GetPortalExitRealmInfo(mPortalExitData.mLevel);
                            if (realmJson == null)
                                return;
                            else if (myProgressLvl < realmJson.reqlvl)
                            {
                                Dictionary<string, string> parameters = new Dictionary<string, string>();
                                parameters.Add("level", realmJson.reqlvl.ToString());
                                UIManager.ShowSystemMessage(GUILocalizationRepo.GetLocalizedSysMsgByName("sys_Portal_LevelTooLow", parameters));
                                return;
                            }
                        }
                        RPCFactory.CombatRPC.OnEnterPortal(myName);
                    }
                }
                else
                {
                    bool canEnter = false;
                    string currentLevelName = ClientUtils.GetCurrentLevelName();
                    for (int index = 0; index < numOfExits; ++index)
                    {
                        LocationData mPortalExitData;
                        if (PortalInfos.mExits.TryGetValue(exitNames[index], out mPortalExitData))
                        {
                            if (mPortalExitData.mLevel == currentLevelName)
                            {
                                canEnter = true;
                                break;
                            }
                            RealmJson realmJson = RealmRepo.GetPortalExitRealmInfo(mPortalExitData.mLevel);
                            if (realmJson != null && myProgressLvl >= realmJson.reqlvl)
                            {
                                canEnter = true;
                                break;
                            }
                        }
                    }
                    if (canEnter)
                        RPCFactory.CombatRPC.OnEnterPortal(myName);
                }
            }
        }
    }
}
