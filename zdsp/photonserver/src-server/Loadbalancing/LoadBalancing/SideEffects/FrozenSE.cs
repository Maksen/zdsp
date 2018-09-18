using Kopio.JsonContracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Zealot.Server.Entities;
using Zealot.Server.SideEffects;
using Zealot.Server.Entities;

class FrozenSE : ControlSE
{
    public FrozenSE(SideEffectJson sideeffectData)
            : base(sideeffectData)
    {
        mNeedCaster = false;
        mControlSEType = ControlSEType.Freeze;
    }

    protected override void OnStart()
    {
        base.OnStart();
        if (IsImmune)
        {
            //mTarget.ControlStats.StunImmuned  = true;
            mTarget.SetImmune(ImmuneSEType.Freeze);
            return;
        }
        mTarget.OnFrozen();
        mTarget.SetControlStatus(ControlSEType.Freeze);
    }

    public override bool IsHot()
    {
        return false;
    }

    protected override bool OnApply(int equipid = -1)
    {
        if (base.OnApply(equipid))
        {
            mTarget.SetControlStatus(Zealot.Server.Entities.ControlSEType.Stun);
            if (mTarget.IsMonster())
            {
                Monster target = mTarget as Monster;
                target.OnFrozen(mDuration);
            }
            return true;
        }

        
        return false;
    }

    protected override void OnInterval()
    {
        base.OnInterval();

        
    }

    protected override void OnRemove()
    {
        mTarget.RemoveControlStatus(Zealot.Server.Entities.ControlSEType.Stun);

        base.OnRemove();
    }
}
