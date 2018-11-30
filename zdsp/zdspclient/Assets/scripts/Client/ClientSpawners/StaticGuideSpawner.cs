using Kopio.JsonContracts;
using UnityEngine;
using Zealot.Common.Entities;
using Zealot.Client.Entities;
using Zealot.Entities;
using Zealot.Repository;
using Zealot.Common;

namespace Zealot.ClientSpawners
{
    [AddComponentMenu("Spawners at Client/Static Guide Spawner")]
    public class StaticGuideSpawner : ClientSpawnerBase
    {
        [Tooltip("Archetype Link to gamedb StaticNPC table.")]
        public string archetype;

        [Tooltip("Area Radius")]
        public float radius;

        private StaticClientNPCAlwaysShow mStaticGuide;

        private void Awake()
        {
            gameObject.tag = "Untagged"; //TODO: remove later
        }

        public override void Spawn(ClientEntitySystem ces)
        {
            StaticGuideJson staticGuideJson = StaticGuideRepo.GetNPCByArchetype(archetype);
            if (staticGuideJson == null)
            {
                Debug.LogErrorFormat("Fail to spawn Static Guide: {0}", archetype);
                return;
            }

            mStaticGuide = ces.SpawnClientEntity<StaticGuideGhost>();
            ((StaticGuideGhost)mStaticGuide).Init(staticGuideJson, transform.position, transform.forward, radius);
            ++ces.TotalQuestNPCSpawner;
        }

        public void Show(bool show)
        {
            if (mStaticGuide != null)
                mStaticGuide.Show(show);
        }

        public override ServerEntityJson GetJson()
        {
            StaticClientGuideSpawnerJson jsonclass = new StaticClientGuideSpawnerJson();
            GetJson(jsonclass);
            return jsonclass;
        }

        public void GetJson(StaticClientGuideSpawnerJson jsonclass)
        {
            jsonclass.archetype = archetype;
            jsonclass.radius = radius;
            base.GetJson(jsonclass);
        }
    }
}
