using UnityEngine;

namespace ViewPlayer
{
    public class TargetModelControl : MainModelControl
    {
        public override void PlayEffect(string animation, string efxname, float duration = -1)
        {
            if (mEffectController)
            {
                Vector3? dir =null;
                var attacker = ViewPlayerModelControl.Instance.GetMainModel();
                if (attacker)
                { 
                    dir = mModel.transform.position - attacker.transform.position;
                    dir.Value.Normalize();
                }
                mEffectController.PlayEffect(animation, efxname, dir, duration);
            }
        }
    }
}
