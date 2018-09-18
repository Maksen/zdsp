using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;
using Zealot.Common.Entities;
using Zealot.Client.Entities;

namespace Zealot.Client.Actions
{
    using Zealot.Common.Actions;

    public class BaseFrozen : Zealot.Common.Actions.Action // base for all client actions
    {
        protected float duration;
        
        public BaseFrozen(Entity entity, ActionCommand cmd) : base(entity, cmd)
        {
        }

        protected override void OnActiveEnter(string prevstate)
        {
            duration = ((FrozenActionCommand)mdbCommand).dur;
            SetTimer((long)duration, OnTimesUp, null);
        }

        protected override void OnActiveUpdate(long dt)
        {
            base.OnActiveUpdate(dt);
        }

        protected virtual void OnTimesUp(object args)
        {
            GotoState("Completed");
        }

        protected override void OnActiveLeave()
        {
            
        }
    }

    public class NonClientAuthoFrozen : BaseFrozen
    {
        public NonClientAuthoFrozen(Entity entity, FrozenActionCommand cmd) : base(entity, cmd)
        {

        }

    }
}
