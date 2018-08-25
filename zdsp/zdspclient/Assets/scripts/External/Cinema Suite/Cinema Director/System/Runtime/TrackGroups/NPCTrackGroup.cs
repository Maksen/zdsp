using UnityEngine;
using Zealot.Client.Entities;

namespace CinemaDirector
{
    /// <summary>
    /// The ActorTrackGroup maintains an Actor and a set of tracks that affect 
    /// that actor over the course of the Cutscene.
    /// </summary>
    [TrackGroupAttribute("NPC Track Group", TimelineTrackGenre.ActorTrack)]
    public class NPCTrackGroup : ActorTrackGroup
    {
        /// <summary>
        /// The Actor that this TrackGroup is focused on.
        /// </summary>
        public override Transform Actor
        {
            get
            {
                return base.Actor;
            }
            set { base.Actor = value; }
        }


        protected StaticNPCGhost _npc;
        public override void Initialize()
        {
            base.Initialize();
            Debug.LogWarning("NPC track group Initialize");
            if (PhotonNetwork.connected)
            {
                Transform oldactor = Actor;
                _npc = GameInfo.gCombat.mEntitySystem.GetQuestNPC(Archetype);
                if (_npc != null)
                {
                    if (oldactor != null)
                    {
                        oldactor.gameObject.SetActive(false);
                        _npc.Position = oldactor.position;
                        _npc.Forward = oldactor.forward;
                    }
                    Actor = _npc.AnimObj.transform;
                    _npc.AnimObj.GetComponent<ActorNameTagController>().enabled = false;
                    _npc.ShowModelOnly(true);

                }
            }
        }

        public override void UpdateTrackGroup(float time, float deltaTime)
        {
            base.UpdateTrackGroup(time, deltaTime);
            if (PhotonNetwork.connected && _npc != null)
            {
                _npc.Position = Actor.position;
                _npc.Forward = Actor.forward;
            }
        }

        public override void Stop()
        {
            base.Stop();
            Debug.LogWarning("---------npc track group stop---------");
            if (PhotonNetwork.connected && _npc != null)
            {
                _npc.ShowModelOnly(true);
                _npc.AnimObj.GetComponent<ActorNameTagController>().enabled = true;

            }
        }
    }
}
