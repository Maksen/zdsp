using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zealot.Client.Entities;
using Zealot.Common;

public class TestButtonSkill : MonoBehaviour
{

    public List<UnityEngine.UI.Image> m_Buttons;

    // Use this for initialization
    public void TestCastSkill(int index)
    {
        if (index == 1)
        {
            int targetpid = 0;
            BaseClientEntity target = GameInfo.GetValidSelectedEntity();
            if (target != null && (target.IsPlayer() || target.IsMonster()))
            {
                ActorGhost ghost = (ActorGhost)target;
                if (CombatUtils.IsValidEnemyTarget(GameInfo.gLocalPlayer, ghost)) //Assume won't cast skill on dead entity e.g. resurrect
                    targetpid = ghost.GetPersistentID();
            }

            if (targetpid == 0)
            {
                ActorGhost ghost = GameInfo.gCombat.GetClosestValidEnemy(Zealot.Bot.BotController.MaxQueryRadius);
                if (ghost != null)
                {
                    targetpid = ghost.GetPersistentID();
                    GameInfo.gCombat.OnSelectEntity(ghost);
                    target = ghost;
                }
            }

            if (targetpid != 0)
            {
                PartyFollowTarget.Pause();
                GameInfo.gCombat.CommonCastBasicAttack(targetpid);
            }
        }
        //else if (index == 2)
        //{
        //    int id = GameInfo.gLocalPlayer.SkillStats.EquipedSkill[0];
        //    GameInfo.gCombat.TryCastActiveSkill(id);
        //}
        //else if (index == 3)
        //{
        //    GameInfo.gCombat.TryCastActiveSkill(6);

        //}
        else
        {
            int id = (int)GameInfo.gLocalPlayer.SkillStats.EquippedSkill[index - 2];
            GameInfo.gCombat.TryCastActiveSkill(id);
        }
    }
}
