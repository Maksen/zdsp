namespace Zealot.Server.SideEffects
{ 
    using Zealot.Server.Entities; 
    using Kopio.JsonContracts; 
    using UnityEngine;
    using Zealot.Common;
    /// <summary>
    /// inherit from SideEffect class , all the proctime, procchance, property is handled as well.
    /// </summary>
    public abstract class ActionSE : SideEffect
    {           
        public ActionSE(SideEffectJson sideeffectData) : base(sideeffectData)
        {
            
        }
                
        protected override bool OnApply(int equipid = -1)
        {
            StartAction(); 
            return true;  
        }

        protected abstract void StartAction();
         
        
    }


    public class KnockUpSE : ActionSE
    {
        public KnockUpSE(SideEffectJson sideeffectData) : base(sideeffectData)
        {

        }
        protected override void StartAction()
        {
            if (mTarget.IsPlayer())
                return;
            Monster target = mTarget as Monster;
            if (target.mArchetype.monstertype != MonsterType.Normal)
            {
                return;
            }
            var dur = float.Parse(mSideeffectData.parameter) > 0 ? float.Parse(mSideeffectData.parameter) : 2.0f;//ensure kopio values correct
            target.OnKnockedUp(dur);
             
        }
    } 

    public class KnockBackSE:ActionSE
    {
        public KnockBackSE(SideEffectJson sideeffectData) : base(sideeffectData)
        {
        }

        protected override void StartAction()
        {
            if (mTarget.IsPlayer())
                return;
                
            Monster target = mTarget as Monster;
            if (target.mArchetype.monstertype != MonsterType.Normal)
            {
                return;
            }
            Vector3 dir = target.Position - mCaster.Position;
            dir = dir.normalized;
            var dist = float.Parse(mSideeffectData.parameter) > 0 ? float.Parse(mSideeffectData.parameter) : 4.0f;//ensure kopio values correct
            target.OnKnockedBack(target.Position + dir * dist);
            
        }
    }
   
}
