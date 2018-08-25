using UnityEngine;
using Zealot.Client.Actions;
using Zealot.Client.Entities;

public class CharColliderDetect : MonoBehaviour
{
    private ActorGhost mGhost;

    void OnControllerColliderHit(ControllerColliderHit hit)
    {
        //Rigidbody body = hit.collider.attachedRigidbody;
        //if (body == null || body.isKinematic)
        //    return;
        //test if it is the boxcollider.
        if (hit.moveDirection.y < -0.3F)
            return;
        if(mGhost.GetActionCmd() != null)
        {
            if (mGhost.GetActionCmd().GetType()  ==  typeof (Zealot.Common.Actions.DashAttackCommand))
            {
                ClientAuthoDashAttack action = mGhost.GetAction() as ClientAuthoDashAttack;
                if (action != null)
                    action.OnCollide();
            }
        }
    }

    public void SetGhost(ActorGhost ghost)
    {
        mGhost = ghost;
    }
}
