using UnityEngine;
using Zealot.Common.Entities;
using Zealot.Client.Entities;
using Zealot.Entities;
using Zealot.Repository;
using Kopio.JsonContracts;
using Zealot.Common;

namespace Zealot.ClientSpawners
{
    [AddComponentMenu("Spawners at Client/Quest NPC Spawner")]
    public class QuestNPCSpawnerDesc : ClientSpawnerDesc
    {
        [Tooltip("Archetype Link to gamedb StaticNPC table.")]
        public string archetype;

        [Tooltip("Area Radius")]
        public float radius;

        private QuestNPC mQuestNpc;
        private StaticAreaGhost mStaticArea;
        private StaticTargetGhost mStaticTarget;

        public override void Spawn(ClientEntitySystem es)
        {
            StaticNPCJson staticNPCJson = StaticNPCRepo.GetStaticNPCByName(archetype);
            if (staticNPCJson != null)
            {
                Transform t = GetComponent<Transform>();
                if (staticNPCJson.interacttype == StaticNPCInteractType.Talk)
                {
                    mQuestNpc = es.SpawnClientEntity<QuestNPC>();
                    mQuestNpc.Init(archetype, t.position, t.forward);
                }
                else if (staticNPCJson.interacttype == StaticNPCInteractType.Area)
                {
                    mStaticArea = es.SpawnClientEntity<StaticAreaGhost>();
                    mStaticArea.Init(archetype, t.position, t.forward, radius);
                }
                else if (staticNPCJson.interacttype == StaticNPCInteractType.Target)
                {
                    mStaticTarget = es.SpawnClientEntity<StaticTargetGhost>();
                    mStaticTarget.Init(archetype, t.position, t.forward, radius);
                }
                es.TotalQuestNPCSpawner++;
            }
        }

        public void Show(bool show)
        {
            if (mQuestNpc != null)
                mQuestNpc.Show(show);
            else if (mStaticArea != null)
                mStaticArea.Show(show);
            else if (mStaticTarget != null)
                mStaticTarget.Show(show);
        }

        public override ServerEntityJson GetJson()
        {
            QuestNPCSpawnerDescJson jsonclass = new QuestNPCSpawnerDescJson();
            GetJson(jsonclass);
            return jsonclass;
        }

        public void GetJson(QuestNPCSpawnerDescJson jsonclass)
        {
            jsonclass.archetype = archetype;
            base.GetJson(jsonclass);
        }
    }  
}

