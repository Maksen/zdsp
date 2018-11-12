namespace Zealot.Server.SideEffects
{
    using System;
    using Zealot.Common;
    using Kopio.JsonContracts;
    using Entities;

    public class RemoveRandomBuffSE : SideEffect
    {
        public RemoveRandomBuffSE(SideEffectJson sideeffectData, SEORIGINID origin, int originID)
            : base(sideeffectData, origin, originID)
        {
            mNeedCaster = false;
        }

        protected override void InitKopioData()
        {
            base.InitKopioData();
            mDuration = 0;
        }

        protected override bool OnApply(int equipid = -1)
        {
            if (mSideeffectData.effecttype == EffectType.Remove_RandomBuff)
            {
                mTarget.RemoveRandomBuff();
            }
            else if (mSideeffectData.effecttype == EffectType.Remove_RandomDebuff)
            {
                mTarget.RemoveRandomBuff(true);
            }
            return true;
        }
    }

    public class RemoveBuffSE : SideEffect
    {
        public RemoveBuffSE(SideEffectJson sideeffectData, SEORIGINID origin, int originID)
            : base(sideeffectData, origin, originID)
        {
            mNeedCaster = false;
        }

        protected override void InitKopioData()
        {
            base.InitKopioData();
            mDuration = 0;
        }

        protected override bool OnApply(int equipid = -1)
        {
            //if (mSideeffectData.effecttype == EffectType.Remove_Buff)
            //{
            //    EffectType setype;
            //    bool valid = false;
            //    setype = CombatUtils.GetStatsEffectTypeByStatsType(mSideeffectData.stat1, true,out valid); 
            //    if (valid) mTarget.RemoveBuff(setype,true);
            //}
            //else if (mSideeffectData.effecttype == EffectType.Remove_DeBuff)
            //{
            //    EffectType setype;
            //    bool valid = false;
            //    setype = CombatUtils.GetStatsEffectTypeByStatsType(mSideeffectData.stat1, false, out valid);
            //    if(valid) mTarget.RemoveBuff(setype, false);
            //}
            return true;
        }
    }
     

    public class DotImmuneSE : SideEffect
    {
        public DotImmuneSE(SideEffectJson sideeffectData, SEORIGINID origin, int originID)
            : base(sideeffectData, origin, originID)
        {
            mNeedCaster = false;
        }


        protected override bool OnApply(int equipid = -1)
        {
            mTarget.PlayerStats.InvincibleDot = true;
            return base.OnApply(equipid);
        }

        protected override void OnRemove()
        {
            mTarget.PlayerStats.InvincibleDot = false;
            base.OnRemove();
        }

        public override bool IsBuff()
        {
            return true;
        }
    }

    public class SilenceBuffSE : SideEffect
    {
        public SilenceBuffSE(SideEffectJson sideeffectData, SEORIGINID origin, int originID)
            : base(sideeffectData, origin, originID)
        {
            mNeedCaster = false;
        }


        protected override bool OnApply(int equipid = -1)
        {
            if (Int32.Parse(mSideeffectData.parameter) == 0)
                mTarget.PlayerStats.Silence = true;
            else
                mTarget.PlayerStats.Silence = true;
            return base.OnApply(equipid);
        }

        protected override void OnRemove()
        {
            if (Int32.Parse(mSideeffectData.parameter) == 0)
                mTarget.PlayerStats.Silence = false;
            else
                mTarget.PlayerStats.Silence = false;
            base.OnRemove();
        }

        public override bool IsControl()
        {
            return true;
        }
    }

    public class DotRemoveSE : SideEffect
    {
        public DotRemoveSE(SideEffectJson sideeffectData, SEORIGINID origin, int originID)
            : base(sideeffectData, origin, originID)
        {
            mNeedCaster = false;
        }

        protected override void InitKopioData()
        {
            base.InitKopioData();
            mDuration = 0;
        }
        protected override bool OnApply(int equipid = -1)
        {
            mTarget.RemoveDot();
            return true;
        }

    }

    public class ControlImmuneSE : SideEffect
    {
        protected int mParam;
        public ControlImmuneSE(SideEffectJson sideeffectData, SEORIGINID origin, int originID)
            : base(sideeffectData, origin, originID)
        {
            mNeedCaster = false;
        }

        protected override void InitKopioData()
        {
            base.InitKopioData();
            mParam = (int)float.Parse(mSideeffectData.parameter);
            //mDuration = 0;
        }
        protected override bool OnApply(int equipid = -1)
        {
            if (base.OnApply(equipid))
            {
                if (mParam == 0)
                {
                    mTarget.PlayerStats.InvincibleCtl = true;
                    mTarget.PlayerStats.InvincibleStatsAtk = true;
                    mTarget.PlayerStats.InvincibleStatsDef = true;
                    mTarget.PlayerStats.InvincibleDmg = true;
                    mTarget.PlayerStats.InvincibleDot = true;
                }
                else if (mParam == 1)
                    mTarget.PlayerStats.InvincibleCtl = true;
                else if (mParam == 2)
                    mTarget.PlayerStats.InvincibleStatsAtk = true;
                else if (mParam == 3)
                    mTarget.PlayerStats.InvincibleStatsDef = true;
                else if (mParam == 4)
                    mTarget.PlayerStats.InvincibleDmg = true;
                else if(mParam ==5)
                {
                    mTarget.PlayerStats.InvincibleStatsAtk = true;
                    mTarget.PlayerStats.InvincibleStatsDef = true;
                }
                return true;
            }
            return false;
            
        }

        protected override void OnRemove()
        {
            if (mParam == 0)
            {
                mTarget.PlayerStats.InvincibleCtl = false;
                mTarget.PlayerStats.InvincibleStatsAtk = false;
                mTarget.PlayerStats.InvincibleStatsDef = false;
                mTarget.PlayerStats.InvincibleDmg = false;
                mTarget.PlayerStats.InvincibleDot = false;
            }
            else if (mParam == 1)
                mTarget.PlayerStats.InvincibleCtl = false;
            else if (mParam == 2)
                mTarget.PlayerStats.InvincibleStatsAtk = false;
            else if (mParam == 3)
                mTarget.PlayerStats.InvincibleStatsDef = false;
            else if (mParam == 4)
                mTarget.PlayerStats.InvincibleDmg = false;
            else if (mParam == 5)
            {
                mTarget.PlayerStats.InvincibleStatsAtk = false;
                mTarget.PlayerStats.InvincibleStatsDef = false;
            }
            base.OnRemove();
        }

        public override bool IsBuff()
        {
            return true;
        }
    }

    public class RemoveAllImmuneSE : SideEffect
    {

        public RemoveAllImmuneSE(SideEffectJson sideeffectData, SEORIGINID origin, int originID)
            : base(sideeffectData, origin, originID)
        {
            mNeedCaster = false;
        }

        protected override void InitKopioData()
        {
            base.InitKopioData();
            mDuration = 0;
        }

        //AllImmune to negative effects are stopped. 
        protected override bool OnApply(int equipid = -1)
        {
            if (mTarget.PlayerStats.InvincibleDmg)
                mTarget.PlayerStats.InvincibleDmg = false;
            if (mTarget.PlayerStats.InvincibleCtl)
                mTarget.PlayerStats.InvincibleCtl = false;
            if (mTarget.PlayerStats.InvincibleDot)
                mTarget.PlayerStats.InvincibleDot = false;
            if (mTarget.PlayerStats.InvincibleStatsAtk)
                mTarget.PlayerStats.InvincibleStatsAtk = false;
            if (mTarget.PlayerStats.InvincibleStatsDef)
                mTarget.PlayerStats.InvincibleStatsDef = false;
            //mTarget.StopSideEffect(EffectType.Control_Immune);//make slot availiable.
            return true;
        }
    }

    public class IncreasePlayerSkillCDSE : SideEffect
    {
        public IncreasePlayerSkillCDSE(SideEffectJson sideeffectData, SEORIGINID origin, int originID)
            : base(sideeffectData, origin, originID)
        {
            mNeedCaster = false;
            mSideeffectData.duration = 0;
        }

        protected override bool OnApply(int equipid = -1)
        {
            if (mTarget.IsPlayer())
            {
                Player player = mTarget as Player;
                int intval  = (int)mSideeffectData.max;//percentage
                player.Slot.ZRPC.CombatRPC.OnIncreaseCooldown("", intval, player.Slot);
            }else if (mTarget.IsAIPlayer())
            {
                AIPlayer player = mTarget as AIPlayer;
                int intval = (int)mSideeffectData.max;//percentage 
                player.IncreaseSkillCD(new int[4] { 0, 1, 2, 3 }, intval);
            }
            return base.OnApply(equipid);
        }
        
    }

    public class BasicAttack_SupressSE : SideEffect
    {
        protected int mAmount = 0;
        public BasicAttack_SupressSE(SideEffectJson sideeffectData, SEORIGINID origin, int originID)
            : base(sideeffectData, origin, originID)
        {
            mNeedCaster = false;
        }

        protected override bool OnApply(int equipid = -1)
        { 
            bool applied = base.OnApply(equipid);
            if (applied)
            {
                mAmount = (int) mSideeffectData.max;//use max will be ok.
                mTarget.SkillPassiveStats.AddToField(Common.Entities.SkillPassiveFieldName.BasicAttack_DamageSupress, mAmount);
            }
            return true;
        }

        protected override void OnRemove()
        {
            base.OnRemove();
            mTarget.SkillPassiveStats.AddToField(Common.Entities.SkillPassiveFieldName.BasicAttack_DamageSupress, -mAmount);
        }

        public override bool IsDeBuff()
        {
            return true;
        }
    }

    public class MaxEvasionSE:SideEffect
    {
        public MaxEvasionSE(SideEffectJson sideeffectData, SEORIGINID origin, int originID)
            : base(sideeffectData, origin, originID)
        {
            mNeedCaster = false;
        } 

        protected override bool OnApply(int equipid = -1)
        {
            if (base.OnApply(equipid))
            {
                mTarget.MaxEvasionChance = true;
                return true;
            }
            return false;
        }

        protected override void OnRemove()
        {
            mTarget.MaxEvasionChance = false;
            base.OnRemove();
        }

        public override bool IsBuff()
        {
            return true;
        }
    }

    public class MaxCriticalSE : SideEffect
    {
        public MaxCriticalSE(SideEffectJson sideeffectData, SEORIGINID origin, int originID)
            : base(sideeffectData, origin, originID)
        {
            mNeedCaster = false;
        }

        protected override bool OnApply(int equipid = -1)
        {
            if (base.OnApply(equipid))
            {
                mTarget.MaxCriticalChance = true;
                return true;
            }
            return false;
        }

        protected override void OnRemove()
        {
            mTarget.MaxCriticalChance = false;
            base.OnRemove();
        }

        public override bool IsBuff()
        {
            return true;
        }
    }

    public class RejSupressSE : SideEffect
    {

        public RejSupressSE(SideEffectJson sideeffectData, SEORIGINID origin, int originID)
            : base(sideeffectData, origin, originID)
        {
            mNeedCaster = false;
        }
         
        protected override bool OnApply(int equipid = -1)
        {
            mTarget.SkillPassiveStats.AddToField(Common.Entities.SkillPassiveFieldName.RejSupress, (int)mSideeffectData.max);
            return base.OnApply(equipid);
        }

        protected override void OnRemove()
        {
            mTarget.SkillPassiveStats.AddToField(Common.Entities.SkillPassiveFieldName.RejSupress, -(int)mSideeffectData.max);
            base.OnRemove();
        }

        public override bool IsDeBuff()
        {
            return true;
        }
    }

}