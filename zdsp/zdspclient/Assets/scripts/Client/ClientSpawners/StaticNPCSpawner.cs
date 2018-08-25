using Kopio.JsonContracts;
using UnityEngine;
using Zealot.Common.Entities;
using Zealot.Client.Entities;
using Zealot.Entities;
using Zealot.Repository;
using Zealot.Common;

namespace Zealot.ClientSpawners
{
    [AddComponentMenu("Spawners at Client/Static NPC Spawner")]
    public class StaticNPCSpawner : ClientSpawnerBase
    {
        [Tooltip("Archetype Link to gamedb StaticNPC table.")]
        public string archetype;

        [Tooltip("Area Radius")]
        public float radius;

        private StaticClientNPCAlwaysShow mStaticNPC;

        private void Awake()
        {
            gameObject.tag = "EditorOnly";
        }

        public override void Spawn(ClientEntitySystem ces)
        {
            StaticNPCJson staticNPCJson = StaticNPCRepo.GetNPCByArchetype(archetype);
            if (staticNPCJson == null)
            {
                Debug.LogErrorFormat("Fail to spawn Static NPC: {0}", archetype);
                return;
            }

            if (staticNPCJson.interacttype == StaticNPCInteractType.Talk)
            {
                mStaticNPC = ces.SpawnClientEntity<StaticNPCGhost>();
                ((StaticNPCGhost)mStaticNPC).Init(staticNPCJson, transform.position, transform.forward);
            }
            else if (staticNPCJson.interacttype == StaticNPCInteractType.Area)
            {
                mStaticNPC = ces.SpawnClientEntity<StaticAreaGhost>();
                ((StaticAreaGhost)mStaticNPC).Init(staticNPCJson, transform.position, transform.forward, radius);
            }
            else if (staticNPCJson.interacttype == StaticNPCInteractType.Target)
            {
                mStaticNPC = ces.SpawnClientEntity<StaticTargetGhost>();
                ((StaticTargetGhost)mStaticNPC).Init(staticNPCJson, transform.position, transform.forward, radius);
            }
            ++ces.TotalQuestNPCSpawner;
        }

        public void Show(bool show)
        {
            if (mStaticNPC != null)
                mStaticNPC.Show(show);
        }

        public override ServerEntityJson GetJson()
        {
            StaticClientNPCSpawnerJson jsonclass = new StaticClientNPCSpawnerJson();
            GetJson(jsonclass);
            return jsonclass;
        }

        public void GetJson(StaticClientNPCSpawnerJson jsonclass)
        {
            jsonclass.archetype = archetype;
            jsonclass.radius = radius;
            base.GetJson(jsonclass);
        }
    }  
}

