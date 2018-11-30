using UnityEngine;
using Zealot.Client.Actions;
using Zealot.Client.Entities;
using Zealot.Common;
using Zealot.Common.Actions;
using Zealot.Common.Entities;
using Zealot.Common.RPC;
using Zealot.Repository;

public class UnreliableCombatRPC : RPCBase
{
    public UnreliableCombatRPC() :
        base(typeof(UnreliableCombatRPC), OperationCode.UnreliableCombat)
    {
    }
}

public partial class ClientMain : MonoBehaviour
{
    [RPCMethod(RPCCategory.UnreliableCombat, (byte)ServerUnreliableCombatRPCMethods.SideEffectHit)]
    public void SideEffectHit(int targetPID, int sideeffectID)
    {
        //Debug.Log("SideEffectHit Target pid = " + targetPID + " seid = " + sideeffectID);

        Entity entity = mEntitySystem.GetEntityByPID(targetPID);
        if (entity == null)
            return;

        NetEntityGhost ghost = entity as NetEntityGhost;
        Kopio.JsonContracts.SideEffectJson sideeffectJson = Zealot.Repository.SideEffectRepo.GetSideEffect(sideeffectID);
        ghost.PlaySEEffect(sideeffectJson.name); //if already playing the same hit effect, do not replace and let the existing play to completion
    }

    [RPCMethod(RPCCategory.UnreliableCombat, (byte)ServerUnreliableCombatRPCMethods.EntityOnDamage)]
    public void EntityOnDamage(int targetpid, int attackerpid, int resultinfo, int damage, int labels, int skillid)
    {
        Entity defender = mEntitySystem.GetEntityByPID(targetpid);
        if (defender == null)
        {
            LogManager.DebugLog("UpdateHealth: NetEntityGhost pid [" + targetpid + "] does not exist);");
            return;
        }

        ActorGhost defenderghost = (ActorGhost)defender;
        if (defenderghost.HasAnimObj)
        {
            AttackResult res = new AttackResult(targetpid, damage, false, attackerpid);
            res.LabelNum = labels;
            res.AttackInfo = resultinfo;//res is initied with this byte
            BaseClientCastSkill.isCrtical = res.IsCritical;
            if (res.RealDamage > 0 || res.IsEvasion)
            {
                //Debug.LogWarningFormat("queue dmg {0}, dt={1}", res.RealDamage, System.DateTime.Now.ToLongTimeString());
                if (labels > 1)
                {
                    int damagedivided = damage / labels;
                    int firstdamage = damage - damagedivided * (labels - 1);
                    res.RealDamage = firstdamage;
                    GameInfo.gDmgLabelPool.Setup(res, defenderghost.Position);
                    labels--;
                    if (damagedivided > 0)
                    {
                        while (labels > 0)
                        {
                            AttackResult resqueued = new AttackResult(targetpid, damagedivided, false, attackerpid);
                            resqueued.AttackInfo = resultinfo;//res is initied with this byte
                            GameInfo.gDmgLabelPool.Setup(resqueued, defenderghost.Position);
                            labels--;
                        }
                    }
                }
                else
                    GameInfo.gDmgLabelPool.Setup(res, defenderghost.Position);
            }

            //GetHitCommand ghcmd = new GetHitCommand();
            //ghcmd.skillid = skillid;
            //SkillData skill = SkillRepo.GetSkill(skillid);

            if (defenderghost.IsMonster())//monster get hit
            {
                MonsterGhost monsterGhost = (MonsterGhost)defenderghost;
                monsterGhost.SetHeadLabel();
                if (res.IsCritical)
                    defenderghost.PlayEffect("", GameInfo.gLocalPlayer.GetHitCritEffect());

                //if (skill != null)
                //{
                //    if (!defenderghost.IsDying())
                //    {
                //        //NonClientAuthoACGetHit getHitAction = new NonClientAuthoACGetHit(defenderghost, ghcmd);
                //        //Action prev = defenderghost.GetAction();
                //        //if (prev == null || prev.mdbCommand.GetActionType() == ACTIONTYPE.CASTSKILL)
                //        //    prev = new NonClientAuthoACIdle(defenderghost, new IdleActionCommand());
                //        //getHitAction.SetCompleteCallback(() => defenderghost.PerformAction(prev));
                //        //defenderghost.PerformAction(getHitAction);
                //    }
                //    //else
                //    //{
                //    //    monsterGhost.PlayEffect("", skill.skillJson.name + "_gethit");
                //    //    monsterGhost.Flash();
                //    //}
                //}
            }
            else if (defenderghost.IsPlayer())//player get hit
            {
                PlayerGhost player = (PlayerGhost)defenderghost;
                if (defenderghost.IsLocal)
                {
                    //local player on damage interrupt interact action.
                    if (defenderghost.GetAction().mdbCommand.GetActionType() == ACTIONTYPE.INTERACT)
                        defenderghost.PerformAction(new ClientAuthoACIdle(defenderghost, new IdleActionCommand()));

                    GetHitCommand ghcmd = new GetHitCommand();
                    ghcmd.skillid = skillid;
                    SkillData skill = SkillRepo.GetSkill(skillid);

                    if (skill != null)
                    {
                        player.ActionInterupted(true);

                        if (!defenderghost.IsDying())
                        {
                            ClientAuthoACGetHit getHitAction = new ClientAuthoACGetHit(defenderghost, ghcmd);
                            Action prev = defenderghost.GetAction();
                            getHitAction.SetCompleteCallback(() => defenderghost.PerformAction(prev));
                            defenderghost.PerformAction(getHitAction);
                        }
                        //else
                        //    defenderghost.PlayEffect("", skill.skillJson.name + "_gethit");
                    }
                }
                else
                {
                    player.SetHeadLabel();

                    //if (skill != null)
                    //{
                    //    if ((player.IsRecovering || player.IsMoving() || player.IsIdling()) && !defenderghost.IsDying())
                    //    {
                    //        NonClientAuthoACGetHit getHitAction = new NonClientAuthoACGetHit(defenderghost, ghcmd);
                    //        Action prev = defenderghost.GetAction();
                    //        getHitAction.SetCompleteCallback(() => defenderghost.PerformAction(prev));
                    //        defenderghost.PerformAction(getHitAction);
                    //    }
                    //    else
                    //        player.PlayEffect("", skill.skillJson.name + "_gethit");
                    //}
                }
            }
            //Debug.Log("damage: " + ghost.Name + " " + attacktype);
        }

        Entity attacker = mEntitySystem.GetEntityByPID(attackerpid);
        if (attacker == null)
        {
            LogManager.DebugLog("UpdateHealth: NetEntityGhost pid [" + attackerpid + "] does not exist);");
            return;
        }

        ActorGhost attackerGhost = (ActorGhost)attacker;

        if (attackerGhost.HasAnimObj)
        {
            //Turn on label for monster
            if (attackerGhost.IsMonster())
            {
                if (attackerGhost.HeadLabel.mPlayerLabel.LabelType != LabelTypeEnum.HurtMonster &&
                    attackerGhost.HeadLabel.mPlayerLabel.LabelType != LabelTypeEnum.BossMonster)
                    attackerGhost.HeadLabel.mPlayerLabel.SetHurtMonster();
            }
            //else if (attackerGhost.IsPlayer() && attackerGhost.IsLocal == false)
            //{
            //Make sure attacker is not party member or ally or self
            //PlayerGhost hostilePlayer = (PlayerGhost)attackerGhost;
            //bool isEnemyToSelf = CombatUtils.IsEnemy(hostilePlayer, GameInfo.gLocalPlayer);

            //if (isEnemyToSelf && hostilePlayer.HeadLabel.mPlayerLabel.LabelType != LabelTypeEnum.Battleground_Enemy)
            //{
            //    hostilePlayer.HeadLabel.mPlayerLabel.SetBattleGroundEnemy();
            //}
            //else if (!isEnemyToSelf &&
            //         hostilePlayer.HeadLabel.mPlayerLabel.LabelType != LabelTypeEnum.PartyMember &&
            //         hostilePlayer.HeadLabel.mPlayerLabel.LabelType != LabelTypeEnum.Battleground_Ally)
            //{
            //    hostilePlayer.HeadLabel.mPlayerLabel.SetBattleGroundAlly();
            //}

            //}
        }
    }
}