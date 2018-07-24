using UnityEngine;
using Zealot.Entities;
using Zealot.Repository;
using System.Collections.Generic;
using Kopio.JsonContracts;

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
			if (other.CompareTag("LocalPlayer") && myName != "" && exitName != "")
            {
                int myProgressLvl = GameInfo.gLocalPlayer.GetAccumulatedLevel();
                string[] exitNames = exitName.Split(';');
                if (exitNames.Length == 1)
                {
                    LocationData mPortalExitData;
                    if (PortalInfos.mExits.TryGetValue(exitName, out mPortalExitData))
                    {
                        RealmJson realmJson = RealmRepo.GetPortalExitRealmInfo(mPortalExitData.mLevel);
                        if (realmJson == null)
                            return;
                        if (myProgressLvl < realmJson.reqlvl)
                        {
                            Dictionary<string, string> parameters = new Dictionary<string, string>();
                            parameters.Add("level", realmJson.reqlvl.ToString());
                            UIManager.ShowSystemMessage(GUILocalizationRepo.GetLocalizedSysMsgByName("sys_Portal_LevelTooLow", parameters));
                            return;
                        }
                        RPCFactory.CombatRPC.OnEnterPortal(myName);
                    }
                }
                else
                {
                    for (int index = 0; index < exitNames.Length; index++)
                    {
                        LocationData mPortalExitData;
                        if (PortalInfos.mExits.TryGetValue(exitNames[index], out mPortalExitData))
                        {
                            RealmJson realmJson = RealmRepo.GetPortalExitRealmInfo(mPortalExitData.mLevel);
                            if (realmJson != null && myProgressLvl >= realmJson.reqlvl)
                            {
                                RPCFactory.CombatRPC.OnEnterPortal(myName);
                                return;
                            }
                        }
                    }   
                }
			}
		}
	}
}
