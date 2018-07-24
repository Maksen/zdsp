using UnityEngine;
using System.Collections;
using Zealot.Client.Actions;

public class CharColliderDetect : MonoBehaviour {

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
                {
                    ((ClientAuthoDashAttack)action).OnCollide();
                }
            }
        }
        
    }

    private Zealot.Client.Entities.ActorGhost mGhost;
    public void SetGhost(Zealot.Client.Entities.ActorGhost ghost)
    {
        mGhost = ghost;
    }
}
