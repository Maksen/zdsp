using UnityEngine;
using Zealot.Client.Entities;
using Zealot.Common.Actions;
using Zealot.Common.Entities;

namespace Zealot.Client.Actions
{
    /// <summary>
    /// This action is only used by HeroGhost
    /// </summary>
    public class ClientAuthoACSummon : Action
    {
        private long timeElapsed;
        private long summonDuration;
        private HeroGhost ghost;

        public ClientAuthoACSummon(Entity entity, ActionCommand cmd) : base(entity, cmd)
        {
        }

        //Do not SetAction. Server side do not perform this action
        protected override void OnActiveEnter(string prevstate)
        {
            base.OnActiveEnter(prevstate);

            ghost = mEntity as HeroGhost;
            summonDuration = (long)(ghost.mHeroJson.summonduration * 1000);
            timeElapsed = 0;
            string animationName = ghost.mHeroJson.summonaction;
            string effectName = ghost.mHeroJson.name;

            // need to play without crossfade so that the animation will play immediately
            ghost.PlayEffect(animationName, effectName, null, -1, null, null, false);
        }

        protected override void OnActiveUpdate(long dt)
        {
            base.OnActiveUpdate(dt);

            timeElapsed += dt;
            if (timeElapsed >= summonDuration)
            {
                GotoState("Completed");
            }
        }

        protected override void OnCompleteEnter(string prevstate)
        {
            NetEntityGhost ghost = (NetEntityGhost)mEntity;
            IdleActionCommand cmd = new IdleActionCommand();
            ghost.PerformAction(new ClientAuthoACIdle(ghost, cmd));

            base.OnCompleteEnter(prevstate);
        }
    }
}