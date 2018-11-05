using UnityEngine;
using Zealot.Client.Entities;
using Zealot.Spawners;

public class InteractiveUpdater : MonoBehaviour {

    private PlayerGhost player;
    private int pid;
    private bool interrupt;
    private InteractiveTrigger trigger;

    private float cooldown = 0.0f;

    public void Init(PlayerGhost mPlayer, int mPid, bool mInterrupt, InteractiveTrigger mTrigger)
    {
        player = mPlayer;
        pid = mPid;
        interrupt = mInterrupt;
        trigger = mTrigger;
    }

    public void ResetTime()
    {
        cooldown = 0.0f;
    }

    void Update () {
        if (player.IsIdling())
        {
            cooldown += Time.deltaTime;
            if (cooldown >= 0.1f)
            {
                player.InteractiveController.OnActionEnter(pid, interrupt, true, trigger);
                this.enabled = false;
                cooldown = 0.0f;
            }
        }
	}
}
