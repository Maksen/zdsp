using System.Collections.Generic;
using UnityEngine;

namespace CinemaDirector
{
    /// <summary>
    /// The MultiActorTrackGroup maintains a list of Actors that have something in 
    /// common and a set of tracks that act upon those Actors.
    /// </summary>
    [TrackGroupAttribute("MultiActor Track Group", TimelineTrackGenre.MultiActorTrack)]
    public class MultiActorTrackGroup : TrackGroup
    {
        [SerializeField]
        private int activeIndex = 0;

        [SerializeField]
        private bool onlyone = false;

        [SerializeField]
        private List<Transform> actors = new List<Transform>();

        private List<Transform> filterActors = new List<Transform>();
        /// <summary>
        /// The Actors that this TrackGroup is focused on
        /// </summary>
        public List<Transform> Actors
        {
            get
            {
                if(!OnlyOne)
                    return actors;
                else
                {
                    if(filterActors.Count <= 0)
                    {
                        filterActors.Add(actors[ActiveIndex]);
                    }
                    return filterActors;
                }
            }
            set
            {
                actors = value;
            }
        }

        public int ActiveIndex { get { return activeIndex; } set { activeIndex = value; } }

        public bool OnlyOne { get { return onlyone; } set { onlyone = value; } }
    }
}
