using Kopio.JsonContracts;
using System.Collections.Generic;
using UnityEngine;
using Zealot.Common.Entities;
using Zealot.Repository;

namespace Zealot.Common
{
    public static class CombatUtils
    {
        public static readonly float DEFAULT_ACTOR_RADIUS = 0.7f;
        public static long COMBOTHIT_TIMEOUT = 10000;
        public static long DYING_TIME = 6000;
        public static long BASIC_ATTACK_CHAIN_GAP = 400;
        public static long KNOCKBY_BASICATTACK_TIME = 700;
        public static long RECOVER_FROM_HIT_TIME = 1000;
        private static readonly float QUERYRADIUS_ERRORMARGIN = 0.5f;

        //Checks if enemy but enemy may not be valid e.g. enemy is in safezone
        public static bool IsEnemy(Entity entity1, Entity entity2)
        {
            if (entity1 == null || entity2 == null)
                return false;
            if (entity1 == entity2)
                return false;
            if (entity1.IsActor() && entity2.IsActor())
            {
                int team1 = ((IActor)entity1).Team;
                int team2 = ((IActor)entity2).Team;
                if (IsEnemy(team1, team2))
                    return true;
            }
            return false;
        }

        public static bool IsEnemy(int team1, int team2)
        {
            return (team1 == -2 || team2 == -2 || team1 != team2);
        }

        //Check if enemy and target is valid i.e. still alive and not in safezone
        public static bool IsValidEnemyTarget(IActor attacker, IActor target)
        {
            return IsEnemy((Entity)attacker, (Entity)target) && target.IsAlive() && !target.IsInSafeZone();
        }

        //Check if enemy target is no longer valid i.e. no longer alive, has entered safezone, etc
        public static bool IsInvalidTarget(IActor target)
        {
            return target == null || target.IsInvalidTarget();
        }

        /// <summary>
        /// This function is used across server and client, to make sure the query result is consitant between server and client.
        /// </summary>
        /// <param name="origin">this new target system, origin is always the caster</param>
        /// <param name="mTarget"></param>
        /// <param name="targetpos"></param>
        /// <param name="skillgroupJson"></param>
        /// <returns></returns>
        public static List<IActor> QueryTargetsForClientAndServer(IActor origin, IActor mTarget, SkillData skill, Vector3 targetpos, List<Entity> filterGroup = null)
        {
            List<IActor> resultList = new List<IActor>();
            if (skill == null) Debug.LogError("Skill Data is missing");
            float queryradius = skill.skillJson.radius;
            //Threatzone
            Threatzone threatzone = skill.skillgroupJson.threatzone;
            TargetType targetType = skill.skillgroupJson.targettype;
            SkillBehaviour behaviour = skill.skillgroupJson.skillbehavior;
            int maxTargets = skill.skillJson.maxtargets;
            Entity orginEnt = origin as Entity;
            if (skill.skillgroupJson.skilltype == SkillType.BasicAttack)
            {
                threatzone = Threatzone.Single;
            }
            if (threatzone == Threatzone.Single)
            {
                //friendly skill with single just use the casters as target. 
                if ((targetType != TargetType.Enemy))
                {
                    resultList.Add(origin);
                    return resultList;
                }
                if (mTarget != null)
                {
                    bool eligibleTarget = IsCorrectTargetType(origin, mTarget, skill.skillgroupJson.targettype);
                    if (eligibleTarget)
                    {
                        if (GameUtils.InRange(targetpos,
                            ((Entity)mTarget).Position, queryradius + QUERYRADIUS_ERRORMARGIN))
                        {
                            resultList.Add(mTarget);
                            return resultList;
                        }
                    }
                }
            }

            EntitySystem.QueryEntityFilter IsValidEntity = ((queriedEntity) =>
            {
                Entity target = queriedEntity;
                if (target != null)
                {
                    if (target.Destroyed)
                        return false;
                    if (!target.IsActor())
                        return false;
                    if (filterGroup != null && !filterGroup.Contains(target))
                        return false;
                    return IsCorrectTargetType(origin, (IActor)target, skill.skillgroupJson.targettype);
                }
                return false;
            });

            Entity originEntity = ((Entity)origin);
            Vector3 originForward = ((Entity)origin).Forward;
            EntitySystem.QueryEntityFilter IsValidEntity120 = ((queriedEntity) =>
            {
                Entity target = queriedEntity;
                if (target != null)
                {
                    if (target.Destroyed)//|| !target.IsAlive() alive check outside this
                        return false;
                    if (!target.IsActor())
                        return false;
                    if (filterGroup != null && !filterGroup.Contains(target))
                        return false;
                    //check if caster is inside target, then any angle will hit
                    if (GameUtils.InRange(targetpos, target.Position, DEFAULT_ACTOR_RADIUS))
                    {
                        return IsCorrectTargetType(origin, (IActor)target, skill.skillgroupJson.targettype);
                    }

                    Vector3 originToTarget = (target.Position - originEntity.Position).normalized;
                    float dotproduct = Vector3.Dot(originForward, originToTarget);
                    if (dotproduct >= 0.5f) //within 60 deg left & right
                        return IsCorrectTargetType(origin, (IActor)target, skill.skillgroupJson.targettype);
                    else
                        return false;
                }
                return false;
            });

            resultList = new List<IActor>();
            if (threatzone == Threatzone.DegreeArc360 || threatzone == Threatzone.DegreeArc120)
            {
                EntitySystem.QueryEntityFilter filter = threatzone == Threatzone.DegreeArc360 ? IsValidEntity : IsValidEntity120;
                if (maxTargets == 1 && skill.skillgroupJson.skillbehavior != SkillBehaviour.Ground)
                {
                    if (targetType != TargetType.Enemy)
                    {
                        resultList.Add((IActor)origin);
                    }
                    else
                    {
                        Entity target = originEntity.EntitySystem.QueryForClosestEntityInSphere(targetpos, queryradius + QUERYRADIUS_ERRORMARGIN, filter);
                        if (target != null)
                            resultList.Add((IActor)target);
                    }

                }
                else
                {
                    if (targetType != TargetType.Enemy)
                    {
                        maxTargets = maxTargets - 1;
                        resultList.Add(origin); //add self as the filter not return self;
                    }
                    if (SkillBehaviour.Ground == skill.skillgroupJson.skillbehavior)
                    {
                        List<Entity> targets = originEntity.EntitySystem.QueryEntitiesInSphere(targetpos,
                           queryradius + QUERYRADIUS_ERRORMARGIN, filter);
                        int count = targets.Count > maxTargets ? maxTargets : targets.Count;
                        for (int i = 0; i < count; i++)
                            resultList.Add((IActor)targets[i]);
                    }
                    else
                    {
                        List<Entity> targets = originEntity.EntitySystem.QueryEntitiesInSphere(originEntity.Position,
                            queryradius + QUERYRADIUS_ERRORMARGIN, filter);
                        int count = targets.Count > maxTargets ? maxTargets : targets.Count;
                        for (int i = 0; i < count; i++)
                            resultList.Add((IActor)targets[i]);
                    }
                }
            }
            else if (threatzone == Threatzone.LongStream)
            {
                float width = skill.skillJson.radius * 2;
                float range = skill.skillJson.range; //for long threatzone, we add bonus to the range
                if (maxTargets == 1)
                {
                    if (targetType != TargetType.Enemy)
                    {
                        resultList.Add(origin);
                    }
                    else
                    {
                        Entity target = originEntity.EntitySystem.QueryForClosestEntityInRectangleF(targetpos, originForward, range + QUERYRADIUS_ERRORMARGIN, width, IsValidEntity);
                        if (target != null)
                            resultList.Add((IActor)target);
                    }
                }
                else
                {
                    if (targetType != TargetType.Enemy)
                    {
                        maxTargets = maxTargets - 1;
                        resultList.Add(origin); //add self as the filter not return self
                    }

                    List<Entity> targets = originEntity.EntitySystem.QueryEntitiesInRectangleF(targetpos, originForward, range + QUERYRADIUS_ERRORMARGIN, width, IsValidEntity);
                    int count = targets.Count > maxTargets ? maxTargets : targets.Count;
                    for (int i = 0; i < count; i++)
                        resultList.Add((IActor)targets[i]);
                }
            }

            Vector3 vector3 = ((Entity)origin).Position;
#if UNITY_EDITOR
            //Debug.Log("queired count: " + resultList.Count+ " at "+vector3.ToString());
#elif !UNITY_ANDROID
            //System.Diagnostics.Debug.WriteLine("queried count:" + resultList.Count + " at "+vector3.ToString());
#endif
            return resultList;
        }

        public static int QueryNumberOfPlayersInSphere(Entity originEntity, float queryradius)
        {
            EntitySystem.QueryEntityFilter IsValidEntity = ((queriedEntity) =>
            {
                Entity target = queriedEntity;
                if (target != null)
                {
                    if (target.Destroyed)
                        return false;

                    return target.IsPlayer();
                }
                return false;
            });
            List<Entity> targets = originEntity.EntitySystem.QueryEntitiesInSphere(originEntity.Position, queryradius + QUERYRADIUS_ERRORMARGIN, IsValidEntity);
            return targets.Count;
        }

        private static bool IsCorrectTargetType(IActor target, IActor mEntity, TargetType desiredTargetType)
        {
            Entity entity1 = (Entity)mEntity;
            Entity entity2 = (Entity)target;
            bool isSameID = entity1.ID == entity2.ID;
            if (isSameID)
                return false; // myself is not returned;
            if (desiredTargetType == TargetType.Enemy)
                return IsEnemy(entity1, entity2);
            else if (desiredTargetType == TargetType.Friendly)
                return !IsEnemy(entity1, entity2);
            else if (desiredTargetType == TargetType.Party)
                return target.GetParty() != -1 && mEntity.GetParty() != -1 && mEntity.GetParty() == target.GetParty();
            return false;
        }

        private static void ApplySubSkills(ref SkillData skgroup, List<SideEffectJson> subskills, SkillPassiveCombatStats SkillPassiveStats)
        {
            //foreach (SideEffectJson sej in subskills)
            //{
                //float amount = sej.max;//just use max for consistant
                //int intval = (int)sej.max;
                //switch (sej.effecttype)
                //{
                    //    case EffectType.Enhance_SkillCoolDown:
                    //    case EffectType.Enhance_SkillCoolDownRGB: 
                    //        if (sej.isrelative)
                    //            skgroup.skillJson.cooldown *= (1 - amount * 0.01f);
                    //        else
                    //            skgroup.skillJson.cooldown -= amount;
                    //        if (skgroup.skillJson.cooldown < 0) skgroup.skillJson.cooldown = 0;
                    //        break;
                    //    case EffectType.Enhance_SkillArcRange:
                    //        if (skgroup.skillgroupJson.threatzone == Threatzone.DegreeArc120 || skgroup.skillgroupJson.threatzone == Threatzone.DegreeArc360)
                    //        {
                    //            if (sej.isrelative)
                    //                skgroup.skillJson.radius *= (1 + amount * 0.01f);
                    //            else
                    //                skgroup.skillJson.radius += amount;
                    //            break;
                    //        }
                    //        break;
                    //    case EffectType.Enhance_SkillBeamRange:
                    //        if (skgroup.skillgroupJson.threatzone == Threatzone.LongStream)
                    //        {
                    //            if (sej.isrelative)
                    //            {
                    //                skgroup.skillJson.range *= (1 + amount * 0.01f);
                    //                skgroup.skillJson.radius *= (1 + amount * 0.01f);
                    //            }
                    //            else
                    //            {
                    //                skgroup.skillJson.range += amount;
                    //                skgroup.skillJson.radius += amount;
                    //            }
                    //        }
                    //        break;
                    //    case EffectType.Enhance_SkillMaxTargets:
                    //        if (skgroup.skillgroupJson.threatzone != Threatzone.Single)
                    //            skgroup.skillJson.maxtargets += (int)amount;
                    //        break;
                    //    case EffectType.Passive_All_Damage:
                    //        SkillPassiveStats.AddToField(SkillPassiveFieldName.All_Damage, intval);
                    //        break;
                    //    case EffectType.Passive_All_Rej:
                    //        SkillPassiveStats.AddToField(SkillPassiveFieldName.Rej_Increase, intval);
                    //        break;
                    //    case EffectType.Passive_AllCooldown:
                    //        if (sej.parameter == 0)
                    //            SkillPassiveStats.AddToField(SkillPassiveFieldName.All_CD, intval);//done in client
                    //        else if (sej.parameter == 1)
                    //            SkillPassiveStats.AddToField(SkillPassiveFieldName.JobSkill_CD, intval);
                    //        else if (sej.parameter == 2)
                    //            SkillPassiveStats.AddToField(SkillPassiveFieldName.RSkill_CD, intval);
                    //        else if (sej.parameter == 3)
                    //            SkillPassiveStats.AddToField(SkillPassiveFieldName.GSkill_CD, intval);
                    //        else if (sej.parameter == 4)
                    //            SkillPassiveStats.AddToField(SkillPassiveFieldName.BSkill_CD, intval);
                    //        else if (sej.parameter == 5)
                    //            SkillPassiveStats.AddToField(SkillPassiveFieldName.FlashSkill_CD, intval);
                    //        break;

                    //    case EffectType.Enhance_SkillCoolDownFlash:
                    //        SkillPassiveStats.AddToField(SkillPassiveFieldName.FlashSkill_CD, intval);
                    //        break;

                    //    case EffectType.Passive_WhenInDebuff:
                    //        SkillPassiveStats.SkillPassiveOnDebuff.Add(sej);
                    //        break;
                    //    case EffectType.OnEvasion_Buff:
                    //    case EffectType.OnEvasion_DeBuff:
                    //    case EffectType.OnEvasion_Shield:
                    //    case EffectType.OnEvasion_Rejuvenate:
                    //        SkillPassiveStats.SkillPassiveOnEvasion.Add(sej);
                    //        break;
                    //    case EffectType.Passive_WhenInDOT:
                    //        SkillPassiveStats.SkillPassiveOnDot.Add(sej);
                    //        break;
                    //    case EffectType.Others_PotionCooldown:
                    //        SkillPassiveStats.AddToField(SkillPassiveFieldName.Potion_CD, intval);
                    //        break;
                    //    case EffectType.Passive_WhenEquipAncient:
                    //        if (sej.parameter == 0)
                    //            SkillPassiveStats.AddToField(SkillPassiveFieldName.RedSkill_DamagePerAncient, intval);
                    //        else if (sej.parameter == 1)
                    //            SkillPassiveStats.AddToField(SkillPassiveFieldName.GreenSkill_DamagePerAncient, intval);
                    //        else
                    //            SkillPassiveStats.AddToField(SkillPassiveFieldName.BlueSkill_DamagePerAncient, intval);

                    //        break;
                    //    case EffectType.Passive_WhenEquipRare:
                    //        if (sej.parameter == 0)
                    //            SkillPassiveStats.AddToField(SkillPassiveFieldName.RedSkill_DamagePerRare, intval);
                    //        else if (sej.parameter == 1)
                    //            SkillPassiveStats.AddToField(SkillPassiveFieldName.GreenSkill_DamagePerRare, intval);
                    //        else
                    //            SkillPassiveStats.AddToField(SkillPassiveFieldName.BlueSkill_DamagePerRare, intval);
                    //        break;
                    //    case EffectType.Passive_Increase_FlashDur:
                    //        SkillPassiveStats.AddToField(SkillPassiveFieldName.FlashSkill_Dur, intval);
                    //        break;
                    //    case EffectType.Passive_ReduceSkillCD_RGB:
                    //        SkillPassiveStats.AddToField(SkillPassiveFieldName.RGBSkill_CD, intval);
                    //        break;
                    //    case EffectType.BasicAttack_Enhance:
                    //        SkillPassiveStats.AddToField(SkillPassiveFieldName.BasicAttack_DamageEnhance, intval);
                    //        break;
                    //    default:
                    //        break;
                //}
            //}
        }

        public static SkillPassiveFieldName GetSkillPassiveDotFieldByStatsType(ActorStatsType field, bool buff)
        {
            SkillPassiveFieldName resfield;
            buff = true; //currently no debuff fields. 
            switch (field)
            {
                case ActorStatsType.Accuracy:
                    resfield = SkillPassiveFieldName.Dot_Buff_Accuracy;
                    break;
                case ActorStatsType.Armor:
                    resfield = SkillPassiveFieldName.Dot_Buff_Armor;
                    break;
                case ActorStatsType.Attack:
                    resfield = SkillPassiveFieldName.Dot_Buff_Attack;
                    break;
                case ActorStatsType.Cocritical:
                    resfield = SkillPassiveFieldName.Dot_Buff_CoCritical;
                    break;
                case ActorStatsType.CocriticalDamage:
                    resfield = SkillPassiveFieldName.Dot_Buff_CoCriticalDamage;
                    break;
                case ActorStatsType.Critical:
                    resfield = SkillPassiveFieldName.Dot_Buff_Critical;
                    break;
                case ActorStatsType.CriticalDamage:
                    resfield = SkillPassiveFieldName.Dot_Buff_CriticalDamage;
                    break;
                case ActorStatsType.Evasion:
                    resfield = SkillPassiveFieldName.Dot_Buff_Evasion;
                    break;
                case ActorStatsType.None:
                    resfield = SkillPassiveFieldName.None;
                    break;
                default:
                    resfield = SkillPassiveFieldName.None;
                    break;
            }
            return resfield;
        }

        public static SkillPassiveFieldName GetSkillOnDebuffPassiveFieldByStatsType(ActorStatsType field, bool buff)
        {
            SkillPassiveFieldName resfield;
            buff = true;
            switch (field)
            {
                case ActorStatsType.Accuracy:
                    resfield = SkillPassiveFieldName.OnDeBuff_Buff_Accuracy;
                    break;
                case ActorStatsType.Armor:
                    resfield = SkillPassiveFieldName.OnDeBuff_Buff_Armor;
                    break;
                case ActorStatsType.Attack:
                    resfield = SkillPassiveFieldName.OnDeBuff_Buff_Attack;
                    break;
                case ActorStatsType.Critical:
                    resfield = SkillPassiveFieldName.OnDeBuff_Buff_Critical;
                    break;
                case ActorStatsType.Cocritical:
                    resfield = SkillPassiveFieldName.OnDeBuff_Buff_CoCritical;
                    break;
                case ActorStatsType.CriticalDamage:
                    resfield = SkillPassiveFieldName.OnDeBuff_Buff_CriticalDamage;
                    break;
                case ActorStatsType.CocriticalDamage:
                    resfield = SkillPassiveFieldName.OnDeBuff_Buff_CoCriticalDamage;
                    break;
                case ActorStatsType.Evasion:
                    resfield = SkillPassiveFieldName.OnDeBuff_Buff_Evasion;
                    break;
                case ActorStatsType.None:
                default:
                    resfield = SkillPassiveFieldName.None;
                    break;
            }
            return resfield;
        }

        public static SkillPassiveFieldName GetSkillOnCriticalPassiveFieldByStatsType(ActorStatsType field, bool buff)
        {
            SkillPassiveFieldName resfield;
            switch (field)
            {
                case ActorStatsType.Accuracy:
                    resfield = buff ? SkillPassiveFieldName.OnCritical_Buff_Accuracy : SkillPassiveFieldName.OnCritical_Debuff_Accuracy;
                    break;
                case ActorStatsType.Armor:
                    resfield = buff ? SkillPassiveFieldName.OnCritical_Buff_Armor : SkillPassiveFieldName.OnCritical_Debuff_Armor;
                    break;
                case ActorStatsType.Attack:
                    resfield = buff ? SkillPassiveFieldName.OnCritical_Buff_Attack : SkillPassiveFieldName.OnCritical_Debuff_Attack;
                    break;
                case ActorStatsType.Cocritical:
                    resfield = buff ? SkillPassiveFieldName.OnCritical_Buff_CoCritical : SkillPassiveFieldName.OnCritical_Debuff_CoCritical;
                    break;
                case ActorStatsType.CocriticalDamage:
                    resfield = buff ? SkillPassiveFieldName.OnCritical_Buff_CoCriticalDamage : SkillPassiveFieldName.OnCritical_Debuff_CoCriticalDamage;
                    break;
                case ActorStatsType.Critical:
                    resfield = buff ? SkillPassiveFieldName.OnCritical_Buff_Critical : SkillPassiveFieldName.OnCritical_Debuff_Critical;
                    break;
                case ActorStatsType.CriticalDamage:
                    resfield = buff ? SkillPassiveFieldName.OnCritical_Buff_CriticalDamage :
                        SkillPassiveFieldName.OnCritical_Debuff_CriticalDamage;
                    break;
                case ActorStatsType.Evasion:
                    resfield = buff ? SkillPassiveFieldName.OnCritical_Buff_Evasion :
                        SkillPassiveFieldName.OnCritical_Debuff_Evasion;
                    break;
                case ActorStatsType.None:
                default:
                    resfield = SkillPassiveFieldName.None;
                    break;
            }
            return resfield;
        }

        public static SkillPassiveFieldName GetSkillPassiveEvasionFieldByStatsType(ActorStatsType field, bool buff)
        {
            SkillPassiveFieldName resfield;
            buff = true; //currently no debuff fields. 
            switch (field)
            {
                case ActorStatsType.Accuracy:
                    resfield = SkillPassiveFieldName.Evasion_Buff_Accuracy;
                    break;
                case ActorStatsType.Armor:
                    resfield = SkillPassiveFieldName.Evasion_Buff_Armor;
                    break;
                case ActorStatsType.Attack:
                    resfield = SkillPassiveFieldName.Evasion_Buff_Attack;
                    break;
                case ActorStatsType.Cocritical:
                    resfield = SkillPassiveFieldName.Evasion_Buff_CoCritical;
                    break;
                case ActorStatsType.CocriticalDamage:
                    resfield = SkillPassiveFieldName.Evasion_Buff_CoCriticalDamage;
                    break;
                case ActorStatsType.Critical:
                    resfield = SkillPassiveFieldName.Evasion_Buff_Critical;
                    break;
                case ActorStatsType.CriticalDamage:
                    resfield = SkillPassiveFieldName.Evasion_Buff_CriticalDamage;
                    break;
                case ActorStatsType.Evasion:
                    resfield = SkillPassiveFieldName.Evasion_Buff_Evasion;
                    break;
                case ActorStatsType.None:
                default:
                    resfield = SkillPassiveFieldName.None;
                    break;
            }
            return resfield;
        }

        public static EffectType GetStatsEffectTypeByStatsType(ActorStatsType field, bool buff, out bool validType)
        {
            //EffectType resfield = EffectType.Stats_HealthMax ;
            validType = true;
            //switch (field)
            //{
            //    case ActorStatsType.Accuracy:
            //        resfield = buff?EffectType.StatsAttack_Accuracy: EffectType.StatsAttack_Accuracy_Debuff;
            //        break;
            //    case ActorStatsType.Armor:
            //        resfield =buff? EffectType.StatsDefence_Armor:EffectType.StatsDefence_Armor_Debuff;
            //        break;
            //    case ActorStatsType.Attack:
            //        resfield = buff?EffectType.StatsAttack_Attack:EffectType.StatsAttack_Attack_Debuff;
            //        break;
            //    case ActorStatsType.Cocritical:
            //        resfield = buff?EffectType.StatsDefence_CoCritical:EffectType.StatsDefence_CoCritical_Debuff;
            //        break;
            //    case ActorStatsType.CocriticalDamage:
            //        resfield = buff ? EffectType.StatsDefence_CoCriticalDamage:EffectType.StatsDefence_CoCriticalDamage_Debuff;
            //        break;
            //    case ActorStatsType.Critical:
            //        resfield = buff?EffectType.StatsAttack_Critical:EffectType.StatsAttack_Critical_Debuff;
            //        break;
            //    case ActorStatsType.CriticalDamage:
            //        resfield = buff? EffectType.StatsAttack_CriticalDamage:EffectType.StatsAttack_CriticalDamage_Debuff;
            //        break;
            //    case ActorStatsType.Evasion:
            //        resfield = buff ? EffectType.StatsDefence_Evasion:EffectType.StatsDefence_Evasion_Debuff;
            //        break; 
            //    default:
            //        validType = false;
            //        break;
            //}
            //return resfield;
            return EffectType.Trigger_OnNormalAttack;
        }

        //public static void EnhanceMainStatsSE(SideEffectJson mainsej, SideEffectJson subsej)
        //{
        //    switch (subsej.stat1)
        //    {
        //        case ActorStatsType.Accuracy:
        //            if (mainsej.effecttype == EffectType.StatsAttack_Accuracy)
        //            {
        //                mainsej.max *= (1 + subsej.max * 0.01f);
        //                mainsej.min *= (1 + subsej.max * 0.01f);
        //            }
        //            break;
        //        case ActorStatsType.Armor:
        //            if (mainsej.effecttype == EffectType.StatsDefence_Armor)
        //            {
        //                mainsej.max *= (1 + subsej.max * 0.01f);
        //                mainsej.min *= (1 + subsej.max * 0.01f);
        //            }
        //            break;
        //        case ActorStatsType.Attack:
        //            //if (mainsej.effecttype == EffectType.StatsAttack_Attack)
        //            //{
        //            //    mainsej.max *= (1 + subsej.max * 0.01f);
        //            //    mainsej.min *= (1 + subsej.max * 0.01f);
        //            //}
        //            break;
        //        case ActorStatsType.Cocritical:
        //            if (mainsej.effecttype == EffectType.StatsDefence_CoCritical)
        //            {
        //                mainsej.max *= (1 + subsej.max * 0.01f);
        //                mainsej.min *= (1 + subsej.max * 0.01f);
        //            }
        //            break;
        //        case ActorStatsType.CocriticalDamage:
        //            //if (mainsej.effecttype == EffectType.StatsDefence_CoCriticalDamage)
        //            //{
        //            //    mainsej.max *= (1 + subsej.max * 0.01f);
        //            //    mainsej.min *= (1 + subsej.max * 0.01f);
        //            //}
        //            break;
        //        case ActorStatsType.Critical:
        //            if (mainsej.effecttype == EffectType.StatsAttack_Critical)
        //            {
        //                mainsej.max *= (1 + subsej.max * 0.01f);
        //                mainsej.min *= (1 + subsej.max * 0.01f);
        //            }
        //            break;
        //        case ActorStatsType.CriticalDamage:
        //            if (mainsej.effecttype == EffectType.StatsAttack_CriticalDamage)
        //            {
        //                mainsej.max *= (1 + subsej.max * 0.01f);
        //                mainsej.min *= (1 + subsej.max * 0.01f);
        //            }
        //            break;
        //        case ActorStatsType.Evasion:
        //            if (mainsej.effecttype == EffectType.StatsDefence_Evasion)
        //            {
        //                mainsej.max *= (1 + subsej.max * 0.01f);
        //                mainsej.min *= (1 + subsej.max * 0.01f);
        //            }
        //            break;

        //    } 

        //}

        public static void DebugCompoundSkill(SkillData data)
        {
            string str = "";
            SkillDebugInfo info = new SkillDebugInfo();
            info.init(data);
            str = Newtonsoft.Json.JsonConvert.SerializeObject(info);
#if UNITY_EDITOR
            Debug.Log(str);
#elif !UNITY_ANDROID
            System.Diagnostics.Debug.WriteLine(str);
#endif
        }

        public struct SEDebugInfo
        {
            public EffectType setype;
            public float dur;
            public float max;
            public float min;
            public float percentage;
            public float proctime;
        }

        public struct SkillDebugInfo
        {
            public SkillType skilltype;
            public float cooldown;
            public SEDebugInfo[] mainskills;
            public SEDebugInfo[] subskills;

            public void init(SkillData data)
            {
                skilltype = data.skillgroupJson.skilltype;
                cooldown = data.skillJson.cooldown;


                if (data.skills.mTarget != null)
                {
                    mainskills = new SEDebugInfo[data.skills.mTarget.Count];
                    int idx = 0;
                    foreach (SideEffectJson sej in data.skills.mTarget)
                    {
                        SEDebugInfo info = new SEDebugInfo();
                        info.setype = sej.effecttype;
                        info.dur = sej.duration;
                        info.max = sej.max;
                        info.min = sej.min;
                        info.percentage = sej.procchance;
                        mainskills[idx++] = info;
                    }
                }
                if (data.skills.mSelf != null)
                {
                    subskills = new SEDebugInfo[data.skills.mSelf.Count];
                    int idx = 0;
                    foreach (SideEffectJson sej in data.skills.mSelf)
                    {
                        SEDebugInfo info = new SEDebugInfo();
                        info.setype = sej.effecttype;
                        info.dur = sej.duration;
                        info.max = sej.max;
                        info.min = sej.min;
                        info.percentage = sej.procchance;
                        subskills[idx++] = info;

                    }
                }
            }
        }

        /// <summary>
        /// order of additional param :  duration,max,parameter,stat1
        /// </summary>
        /// <param name="setype"></param>
        /// <param name="issub"></param>
        /// <param name="otherParams"></param>
        /// <returns></returns>
        public static SideEffectJson SetupTestSideEffect(int setype, bool issub, string otherParams)
        {
            SideEffectJson sej = new SideEffectJson();
            sej.effecttype = (Zealot.Common.EffectType)setype;
            sej.id = issub ? 66 : 88;
            sej.procchance = 100;
            sej.min = sej.max = 0;
            sej.parameter = "";
            sej.basicskilldamageperc = 100;
            sej.duration = 0;
            sej.interval = 0f;
            if (otherParams != "")
            {
                string[] additional = otherParams.Split(',');
                if (additional.Length > 0)
                    sej.duration = int.Parse(additional[0]);
                if (additional.Length > 1)
                    sej.interval = (int.Parse(additional[1]));
                if (additional.Length > 2)
                    sej.max = sej.min = int.Parse(additional[2]);
                if (additional.Length > 3)
                    sej.parameter = additional[3];
                //if (additional.Length > 4)
                //    sej.stat1 = (Zealot.Common.ActorStatsType)(int.Parse(additional[4]));
                //if (additional.Length > 5)
                //    sej.stat2 = (Zealot.Common.ActorStatsType)(int.Parse(additional[5]));
                //if (additional.Length > 6)
                //    sej.isrelative = bool.Parse(additional[6]);
                //if (additional.Length > 7)
                //    sej.usereferencetable = bool.Parse(additional[7]);
                //if (additional.Length > 8)
                //    sej.step = int.Parse(additional[8]);
                //if (additional.Length > 9)
                //    sej.increase = float.Parse(additional[9]);
                if (additional.Length > 10)
                    sej.persistentafterdeath = bool.Parse(additional[10]);
                //if (additional.Length > 11)
                //    sej.persistentonlogout = bool.Parse(additional[11]);
            }
            return sej;
        }

        public static SideEffectJson ConvertSideEffectForApply(ActorStatsType stat, float dur, float val)
        {
            SideEffectJson sej = new SideEffectJson();
            sej.id = 666;
            sej.duration = dur;
            sej.max = sej.min = val;
            sej.isrelative = true;
            sej.procchance = 100;
            switch (stat)
            {
                case ActorStatsType.Accuracy:
                    sej.effecttype = EffectType.StatsAttack_Accuracy;
                    break;
                case ActorStatsType.Attack:
                    //sej.effecttype = EffectType.StatsAttack_Attack;
                    break;
                case ActorStatsType.Critical:
                    sej.effecttype = EffectType.StatsAttack_Critical;
                    break;
                case ActorStatsType.CriticalDamage:
                    sej.effecttype = EffectType.StatsAttack_CriticalDamage;
                    break;
                case ActorStatsType.Evasion:
                    sej.effecttype = EffectType.StatsDefence_Evasion;
                    break;
                case ActorStatsType.Armor:
                    sej.effecttype = EffectType.StatsDefence_Armor;
                    break;
                case ActorStatsType.Cocritical:
                    sej.effecttype = EffectType.StatsDefence_CoCritical;
                    break;
                case ActorStatsType.CocriticalDamage:
                    //sej.effecttype = EffectType.StatsDefence_CoCriticalDamage;
                    break;
            }
            return sej;
        }

        /// <summary>
        /// skill index : 0 , 1, 2, 3 =>  job,  red, green , blue
        /// </summary>
        /// <param name="skillPassiveStats"></param>
        /// <param name="skillindex"></param>
        /// <param name="cooldown"></param>
        /// <returns></returns>
        public static float GetFinalSkillCooldown(SkillPassiveCombatStats SkillPassiveStats, int skillindex, float cooldown)
        {
            int val_all = (int)SkillPassiveStats.GetField(SkillPassiveFieldName.All_CD);

            cooldown *= (1 - val_all * 0.01f);
            if (skillindex == 0)//0 is JobSkill
            {
                int cd_duction = (int)SkillPassiveStats.GetField(SkillPassiveFieldName.JobSkill_CD);
                cooldown *= (1 - cd_duction * 0.01f);
            }
            else if (skillindex > 0 && skillindex < 4)//1 2 3 is for rgb skill.
            {

                int val2 = (int)SkillPassiveStats.GetField(SkillPassiveFieldName.RGBSkill_CD);
                cooldown *= (1 - val2 * 0.01f);
                if (skillindex == 1)
                {
                    int cd_duction = (int)SkillPassiveStats.GetField(SkillPassiveFieldName.RSkill_CD);
                    cooldown *= (1 - cd_duction * 0.01f);
                }
                else if (skillindex == 2)
                {
                    int cd_duction = (int)SkillPassiveStats.GetField(SkillPassiveFieldName.GSkill_CD);
                    cooldown *= (1 - cd_duction * 0.01f);
                }
                else if (skillindex == 3)
                {
                    int cd_duction = (int)SkillPassiveStats.GetField(SkillPassiveFieldName.BSkill_CD);
                    cooldown *= (1 - cd_duction * 0.01f);
                }
            }
            else if (skillindex == 4) //4 is FlashSkill
            {
                int val2 = (int)SkillPassiveStats.GetField(SkillPassiveFieldName.FlashSkill_CD);
                cooldown *= (1 - val2 * 0.01f);
            }
            if (cooldown < 0)
                cooldown = 0;
            return cooldown;
        }
    }
}
