using UnityEngine;

namespace CinemaDirector
{
    /// <summary>
    /// The ActorTrackGroup maintains an Actor and a set of tracks that affect 
    /// that actor over the course of the Cutscene.
    /// </summary>
    [TrackGroupAttribute("Actor Track Group", TimelineTrackGenre.ActorTrack)]
    public class ActorTrackGroup : TrackGroup
    {
        [SerializeField]
        private Transform actor;
        [SerializeField]
        protected string Archetype;
        [SerializeField]
        private bool hasWeapon;
        /// <summary>
        /// The Actor that this TrackGroup is focused on.
        /// </summary>
        public virtual Transform Actor
        {
            get { return actor; }
            set { actor = value; }
        }
    }
}