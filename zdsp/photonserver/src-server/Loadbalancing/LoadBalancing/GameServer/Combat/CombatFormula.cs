using Kopio.JsonContracts;
using System;
using Zealot.Common.Entities;
using Zealot.Common;
using System.IO;


namespace Photon.LoadBalancing.GameServer.CombatFormula
{
    public static class Debug
    {
        public class LogObject
        {
            public string m_Log = string.Empty;
            public bool m_IsLogging = true;
        }

        private static string m_exePath = string.Empty;
        private static bool m_IsLogging = false;
        private static System.Collections.Generic.Dictionary<string, LogObject> m_Logs = new System.Collections.Generic.Dictionary<string, LogObject>();

        public static LogObject CreateLog(string name)
        {
            if (m_Logs.ContainsKey(name))
                return null;

            m_exePath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            LogObject log = new LogObject();
            string loc = m_exePath + "\\ZDSP_CustomLogs";

            if (!System.IO.Directory.Exists(loc))
                System.IO.Directory.CreateDirectory(loc);

            log.m_Log = loc + "\\" + name + ".txt";

            if (!File.Exists(log.m_Log))
                File.Create(log.m_Log);

            m_Logs.Add(name, log);

            return log;
        }

        public static void StartLogging()
        {
            m_IsLogging = true;
        }

        public static void StopLogging()
        {
            m_IsLogging = false;
        }

        public static void Log(string key, string logMessage)
        {
            if (m_IsLogging && m_Logs[key].m_IsLogging)
            {
                try
                {
                    using (StreamWriter w = File.AppendText(m_Logs[key].m_Log))
                    {
                        w.Write("{0} {1}  ", DateTime.Now.ToLongTimeString(), DateTime.Now.ToShortDateString());
                        w.WriteLine("Photon - " + key + " [INFO] : {0}", logMessage);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
        }

        public static void ClearLog(string key)
        {
            try
            {
                File.WriteAllText(m_Logs[key].m_Log, string.Empty);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        public static void ClearLog()
        {
            foreach(var log in m_Logs.Values)
            {
                try
                {
                    File.WriteAllText(log.m_Log, string.Empty);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
        }

        public static void LogWarning(string key, string logMessage)
        {
            if (m_IsLogging && m_Logs[key].m_IsLogging)
            {
                try
                {
                    using (StreamWriter w = File.AppendText(m_Logs[key].m_Log))
                    {
                        w.Write("{0} {1}  ", DateTime.Now.ToLongTimeString(), DateTime.Now.ToShortDateString());
                        w.WriteLine("Photon - " + key + " [WARNING] : {0}", logMessage);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
        }

        public static void LogError(string key, string logMessage)
        {
            if (m_IsLogging && m_Logs[key].m_IsLogging)
            {
                try
                {
                    using (StreamWriter w = File.AppendText(m_Logs[key].m_Log))
                    {
                        w.Write("{0} {1}  ", DateTime.Now.ToLongTimeString(), DateTime.Now.ToShortDateString());
                        w.WriteLine("Photon - " + key + " [ERROR] : {0}", logMessage);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
        }

        public static void LogOnly(string key)
        {
            foreach(var iter in m_Logs)
            {
                if (iter.Key == key) continue;
                iter.Value.m_IsLogging = false;
            }
        }
    }

    public static class CombatFormula
    {        
        // Hack //
        public static bool totalCrit = false;

        public static float critRate = 0.0f;

        private static DecisionNode root;
        private static DecisionNode basicAttackTree;
        private static DecisionNode skillAttackTree;

        #region Piliq

        //private static int GetMainTalentPoint(IActor actor)
        //{
        //    return actor.GetTalentPoint(actor.GetTalentType());
        //}
        ///// talent bonus is  (0.2 + floating value ) * attackerDmg , only when positiveTalent case.
        //private static float GetTalentBonus(IActor attacker, IActor defender, bool changeTalent = false, TalentType newTalent = TalentType.None)
        //{
        //    TalentType attackerTalent = attacker.GetTalentType();
        //    if (changeTalent)
        //        attackerTalent = newTalent;
        //    TalentType defendTalent = defender.GetTalentType();
        //    if (attackerTalent == TalentType.None || defendTalent == TalentType.None)
        //    {
        //        return 0f;
        //    }
        //    bool positiveTalent = false;
        //    positiveTalent = (attackerTalent == TalentType.Cloth && defendTalent == TalentType.Stone) || (attackerTalent == TalentType.Scissors && defendTalent == TalentType.Cloth) || (attackerTalent == TalentType.Stone && defendTalent == TalentType.Scissors);
        //    int atttalent = GetMainTalentPoint(attacker);
        //    int deftalent = GetMainTalentPoint(defender);
        //    if (atttalent + deftalent == 0)
        //    {
        //        if (positiveTalent)
        //            return 0.2f;//the base val is 0.2.
        //        else
        //            return 0f;
        //    }
        //    float diff = (atttalent - deftalent) *1.0f /(atttalent + deftalent);//TODO: make sure devider is not 0;

        //    if (attackerTalent == defendTalent)
        //    {
        //        return 0f;
        //    }else if (positiveTalent)
        //    {
        //        int temp = TalentRepo.GetTalentDamagePercent((int) (diff * 100));//db value is without %
        //        temp = Math.Max(0, temp);
        //        float res = 0.2f + temp * 0.0001f;
        //        return res;
        //    }
        //    else
        //    {
        //        return 0f;
        //    }
        //}
        private static int GetCriticalBonus(IActor attacker, IActor defender) {
            int criticalbonus = attacker.GetCriticalDamage() - defender.GetCocriticalDamage();
            if (criticalbonus < 1)
                criticalbonus = 1;
            return criticalbonus;
        }

        private static int GetBasicDamage(IActor attacker, IActor defender, bool ignoreArmor) {
            int A_Attack = attacker.GetAttack();
            float exdamage = attacker.GetExDamage();
            int B_Armor = defender.GetArmor();

            if (ignoreArmor) exdamage = 100;
            int absArmor = Math.Max((int)(B_Armor * (1 - exdamage * 0.01f)), 0);
            double res = Math.Max(1, A_Attack - absArmor);
            int mindmg = attacker.GetMinDmg();
            if (mindmg > res) {
                res = mindmg;//min damage
            }

            return (int)res;
        }

        public static bool IsEvasion(IActor attacker, IActor defender) {
            float percent = 0;
            if (defender.MaxEvasionChance)
                percent = 0.75f;
            else {
                int A_Accuracy = attacker.GetAccuracy();
                int B_Evasion = defender.GetEvasion();
                if (B_Evasion + A_Accuracy == 0)
                    return false;
                int evasionPoint = (int)((B_Evasion - A_Accuracy) * 1.0f / (B_Evasion + A_Accuracy) * 10000);
                evasionPoint = Math.Min(7500, Math.Max(0, evasionPoint));
                percent = (float)(evasionPoint * 0.0001f);
            }

            double evasionRoll = GameUtils.GetRandomGenerator().NextDouble();
            bool isEvasion = evasionRoll <= percent;

            return isEvasion;
        }

        private static bool IsCritical(IActor attacker, IActor defender, int bonus) {
            float percent = 0;
            if (critRate > 0.0f) {
                percent = critRate;
            }
            else {
                if (attacker.MaxCriticalChance) {
                    percent = 0.75f;
                }
                else {
                    int A = attacker.GetCritical();
                    int B = defender.GetCocritical();
                    if (A + B == 0)
                        return false;
                    int criticalPoint = (int)((A - B) * 1.0f / (A + B) * 10000);
                    criticalPoint = Math.Min(7500, Math.Max(0, criticalPoint));
                    percent = (float)(criticalPoint * 0.0001f);
                    percent += bonus * 0.01f;//this is the se bonus value.
                }
            }
            double roll = GameUtils.GetRandomGenerator().NextDouble();

            return roll <= percent;
        }

        private static float CriticalFactor(IActor attacker, IActor defender) {
            int A = attacker.GetCriticalDamage();
            int B = defender.GetCocriticalDamage();
            if (A + B == 0)
                return 1.25f;
            int criticalPoint = (int)((A - B) * 1.0f / (A + B) * 10000);
            criticalPoint = Math.Min(7500, Math.Max(0, criticalPoint));
            float percent = (float)(criticalPoint * 0.0001f);
            return 1.25f + percent;//the base critical point is 125%
        }

        public static AttackResult ComputeDamage(IActor attacker, IActor defender, SideEffectJson sedata, float skillDmgPercentage = 100f, int extradamage = 0,
            bool isDotDamage = false, bool isBasicAttack = false, bool ignoreArmor = false, bool changeTalent = false) {
            Random random = GameUtils.GetRandomGenerator();
            AttackResult res = new AttackResult(0, 0, false, 0);
            res.IsDot = isDotDamage;
            res.IsEvasion = false;//because sideEffect already check if hit in ASCastskill
            ICombatStats A_CombatStats = attacker.CombatStats;
            ICombatStats B_CombatStats = defender.CombatStats;
            float finalDamage = 0;
            if (!isDotDamage) {
                if (sedata.criticaltype == CriticalType.Normal) {
                    res.IsCritical = IsCritical(attacker, defender, 0);//evasion check then critial
                }
                else if (sedata.criticaltype == CriticalType.None) {
                    res.IsCritical = false;
                }
                else {
                    res.IsCritical = IsCritical(attacker, defender, sedata.bonuscriticalchance);
                }
            }
            else {
                res.IsCritical = false;
            }
            if (totalCrit) res.IsCritical = true;
            if (!res.IsEvasion)//only if not evasion
            {
                float factor = 1.0f;
                int basicdamage = GetBasicDamage(attacker, defender, ignoreArmor);
                if (res.IsCritical) {
                    factor = CriticalFactor(attacker, defender);
                    basicdamage += GetCriticalBonus(attacker, defender);
                }
                //float talentfactor = GetTalentBonus(attacker, defender, changeTalent, newTalent);
                //if(talentfactor > 0)
                //{
                //    basicdamage += (int)(attacker.GetAttack() * talentfactor);
                //}
                finalDamage = ((basicdamage + extradamage) * skillDmgPercentage * 0.01f) * factor;
                int absorbValue = (int)defender.CombatStats.GetField(FieldName.AbsorbDamage);
                finalDamage *= (1 - absorbValue * 0.01f);
            }

            int damageInc = (int)attacker.SkillPassiveStats.GetField(SkillPassiveFieldName.All_Damage);
            finalDamage *= (1 + damageInc * 0.01f);

            //2% vaiant for final damage.
            int minDamage = (int)(finalDamage * 0.98f);
            int variant = (int)(finalDamage * 0.02f);
            res.RealDamage = random.Next(0, 2 * variant) + minDamage;
            res.RealDamage = Math.Max(1, res.RealDamage);//min damage is 1;
            return res;
        }

        public static int GetFinalAttack(IActor actor) {
            ICombatStats cbtstats; SkillPassiveCombatStats spvstats;
            cbtstats = actor.CombatStats;
            spvstats = actor.SkillPassiveStats;
            int baseAttack = (int)cbtstats.GetField(FieldName.AttackBase);
            int bonusAttack = (int)cbtstats.GetField(FieldName.AttackBonus) + (int)cbtstats.GetField(FieldName.AttackBonus_NoScore);

            int skpassive = (int)spvstats.GetField(SkillPassiveFieldName.Dot_Buff_Attack) +
                (int)spvstats.GetField(SkillPassiveFieldName.Evasion_Buff_Attack) +
                (int)spvstats.GetField(SkillPassiveFieldName.OnCritical_Buff_Attack) +
                (int)spvstats.GetField(SkillPassiveFieldName.OnDeBuff_Buff_Attack)
                - (int)spvstats.GetField(SkillPassiveFieldName.OnCritical_Debuff_Attack);
            int attack = (int)cbtstats.GetField(FieldName.Attack);
            attack += (int)((baseAttack + bonusAttack) * skpassive * 0.01f);
            if (attack < 0)
                return 0;
            return attack;
        }

        public static int GetFinalArmor(IActor actor) {
            ICombatStats cbtstats; SkillPassiveCombatStats spvstats;
            cbtstats = actor.CombatStats;
            spvstats = actor.SkillPassiveStats;
            int baseAttack = (int)cbtstats.GetField(FieldName.ArmorBase);
            int bonusAttack = (int)cbtstats.GetField(FieldName.ArmorBonus) + (int)cbtstats.GetField(FieldName.ArmorBonus_NoScore);
            int skpassive = (int)spvstats.GetField(SkillPassiveFieldName.Dot_Buff_Armor) +
                (int)spvstats.GetField(SkillPassiveFieldName.Evasion_Buff_Armor) +
                (int)spvstats.GetField(SkillPassiveFieldName.OnCritical_Buff_Armor) +
                (int)spvstats.GetField(SkillPassiveFieldName.OnDeBuff_Buff_Armor)
                - (int)spvstats.GetField(SkillPassiveFieldName.OnCritical_Debuff_Armor);
            int attack = (int)cbtstats.GetField(FieldName.Armor);
            attack += (int)((baseAttack + bonusAttack) * skpassive * 0.01f);
            if (attack < 0)
                return 0;
            return attack;
        }

        public static int GetFinalAccuracy(IActor actor) {
            //ICombatStats cbtstats; SkillPassiveCombatStats spvstats;
            //cbtstats = actor.CombatStats;
            //spvstats = actor.SkillPassiveStats;
            //int baseAttack = (int)cbtstats.GetField(FieldName.AccuracyBase);
            //int bonusAttack = (int)cbtstats.GetField(FieldName.AccuracyBonus) + (int)cbtstats.GetField(FieldName.AccuracyBonus_NoScore);
            //int skpassive = (int)spvstats.GetField(SkillPassiveFieldName.Dot_Buff_Accuracy) +
            //    (int)spvstats.GetField(SkillPassiveFieldName.Evasion_Buff_Accuracy) +
            //    (int)spvstats.GetField(SkillPassiveFieldName.OnCritical_Buff_Accuracy) +
            //    (int)spvstats.GetField(SkillPassiveFieldName.OnDeBuff_Buff_Accuracy)
            //    - (int)spvstats.GetField(SkillPassiveFieldName.OnCritical_Debuff_Accuracy);
            //int attack = (int)cbtstats.GetField(FieldName.Accuracy);
            //attack += (int)((baseAttack + bonusAttack )* skpassive * 0.01f);
            //if (attack < 0)
            //    return 0;
            //return attack;
            return 0;
        }

        public static int GetFinalEvasion(IActor actor) {
            ICombatStats cbtstats; SkillPassiveCombatStats spvstats;
            cbtstats = actor.CombatStats;
            spvstats = actor.SkillPassiveStats;
            int baseAttack = (int)cbtstats.GetField(FieldName.EvasionBase);
            int bonusAttack = (int)cbtstats.GetField(FieldName.EvasionBonus) + (int)cbtstats.GetField(FieldName.EvasionBonus_NoScore);
            int skpassive = (int)spvstats.GetField(SkillPassiveFieldName.Dot_Buff_Evasion) +
                (int)spvstats.GetField(SkillPassiveFieldName.Evasion_Buff_Evasion) +
                (int)spvstats.GetField(SkillPassiveFieldName.OnCritical_Buff_Evasion) +
                (int)spvstats.GetField(SkillPassiveFieldName.OnDeBuff_Buff_Evasion)
                - (int)spvstats.GetField(SkillPassiveFieldName.OnCritical_Debuff_Evasion);
            int attack = (int)cbtstats.GetField(FieldName.Evasion);
            attack += (int)((baseAttack + bonusAttack) * skpassive * 0.01f);
            if (attack < 0)
                return 0;
            return attack;
        }

        public static int GetFinalCritical(IActor actor) {
            ICombatStats cbtstats; SkillPassiveCombatStats spvstats;
            cbtstats = actor.CombatStats;
            spvstats = actor.SkillPassiveStats;
            //ToDO: add skillpassivestats with combatstats.
            int baseAttack = 0;//(int)cbtstats.GetField(FieldName.CriticalBase);
            int bonusAttack = (int)cbtstats.GetField(FieldName.CriticalBonus) + (int)cbtstats.GetField(FieldName.CriticalBonus_NoScore);
            int skpassive = (int)spvstats.GetField(SkillPassiveFieldName.Dot_Buff_Critical) +
                (int)spvstats.GetField(SkillPassiveFieldName.Evasion_Buff_Critical) +
                (int)spvstats.GetField(SkillPassiveFieldName.OnCritical_Buff_Critical) +
                (int)spvstats.GetField(SkillPassiveFieldName.OnDeBuff_Buff_Critical)
                - (int)spvstats.GetField(SkillPassiveFieldName.OnCritical_Debuff_Critical);
            int attack = (int)cbtstats.GetField(FieldName.Critical);
            attack += (int)((baseAttack + bonusAttack) * skpassive * 0.01);
            if (attack < 0)
                return 0;
            return attack;
        }

        public static int GetFinalCriticalDamage(IActor actor) {
            ICombatStats cbtstats; SkillPassiveCombatStats spvstats;
            cbtstats = actor.CombatStats;
            spvstats = actor.SkillPassiveStats;
            int baseAttack = 0;// (int)cbtstats.GetField(FieldName.CriticalDamageBase);
            int bonusAttack = (int)cbtstats.GetField(FieldName.CriticalDamageBonus) + (int)cbtstats.GetField(FieldName.CriticalDamageBonus_NoScore);
            int skpassive = (int)spvstats.GetField(SkillPassiveFieldName.Dot_Buff_CriticalDamage) +
                (int)spvstats.GetField(SkillPassiveFieldName.Evasion_Buff_CriticalDamage) +
                (int)spvstats.GetField(SkillPassiveFieldName.OnCritical_Buff_CriticalDamage) +
                (int)spvstats.GetField(SkillPassiveFieldName.OnDeBuff_Buff_CriticalDamage)
                - (int)spvstats.GetField(SkillPassiveFieldName.OnCritical_Debuff_CriticalDamage);
            int attack = (int)cbtstats.GetField(FieldName.CriticalDamage);
            attack += (int)((baseAttack + bonusAttack) * skpassive * 0.01f);
            if (attack < 0)
                return 0;
            return attack;
        }

        public static int GetFinalCoCritical(IActor actor) {
            ICombatStats cbtstats; SkillPassiveCombatStats spvstats;
            cbtstats = actor.CombatStats;
            spvstats = actor.SkillPassiveStats;
            int baseAttack = 0;// (int)cbtstats.GetField(FieldName.CocriticalBase);
            int bonusAttack = (int)cbtstats.GetField(FieldName.CocriticalBonus) + (int)cbtstats.GetField(FieldName.CocriticalBonus_NoScore);
            int skpassive = (int)spvstats.GetField(SkillPassiveFieldName.Dot_Buff_CoCritical) +
                (int)spvstats.GetField(SkillPassiveFieldName.Evasion_Buff_CoCritical) +
                (int)spvstats.GetField(SkillPassiveFieldName.OnCritical_Buff_CoCritical) +
                (int)spvstats.GetField(SkillPassiveFieldName.OnDeBuff_Buff_CoCritical)
                - (int)spvstats.GetField(SkillPassiveFieldName.OnCritical_Debuff_CoCritical);
            int attack = (int)cbtstats.GetField(FieldName.Cocritical);
            attack += (int)((baseAttack + bonusAttack) * skpassive * 0.01f);
            if (attack < 0)
                return 0;
            return attack;
        }

        public static int GetFinalCoCriticalDamage(IActor actor) {
            //ICombatStats cbtstats; SkillPassiveCombatStats spvstats;
            //cbtstats = actor.CombatStats;
            //spvstats = actor.SkillPassiveStats;
            //int baseAttack = (int)cbtstats.GetField(FieldName.CoCriticalDamageBase);
            //int bonusAttack = (int)cbtstats.GetField(FieldName.CoCriticalDamageBonus)+ (int)cbtstats.GetField(FieldName.CoCriticalDamageBonus_NoScore);
            //int skpassive = (int)spvstats.GetField(SkillPassiveFieldName.Dot_Buff_CoCriticalDamage) +
            //    (int)spvstats.GetField(SkillPassiveFieldName.Evasion_Buff_CoCriticalDamage) +
            //    (int)spvstats.GetField(SkillPassiveFieldName.OnCritical_Buff_CoCriticalDamage) +
            //    (int)spvstats.GetField(SkillPassiveFieldName.OnDeBuff_Buff_CoCriticalDamage)
            //    - (int)spvstats.GetField(SkillPassiveFieldName.OnCritical_Debuff_CoCriticalDamage);
            //int attack = (int)cbtstats.GetField(FieldName.CoCriticalDamage);
            //attack += (int)((baseAttack + bonusAttack )* skpassive * 0.01f);
            //if (attack < 0)
            //    return 0;
            //return attack;
            return 0;
        }

        #endregion Piliq

        public class FIELDNAMEPACKET { 
            public struct BasicInfo {
                public struct ElementType {
                    public Element attacker, defender;
                }
                public struct Racial {
                    public Race attacker, defender;
                }

                public AttackStyle attacker, defender;
                public MainWeaponAttribute weaponAttribute;
                public MonsterType monsterType;
                public ElementType elementInfo;
                public Racial race;
                public bool isDefenderNPC;
            }
            
            public struct Fields {
                public FieldName
                    attackerStrikeType, // attack type of attacker
                    attackerElemeent, // element of attack
                    attackerVSRace, // attack advantage to race
                    attackerVSElement, // attack advantage to element
                    defenderStrikeType, // defender resist to attack type
                    defenderElement, // defender resist to element
                    defenderVSRace; // defender resist to race                  
            }

            public IActor attacker, defender;
            public SideEffectJson sedata;
            public BasicInfo basicsInfo;
            public Fields fieldInfo;

            // Debug Logger
            public string target;
        }

        /// <summary>
        /// 攻擊方基礎傷害
        /// </summary>
        /// <param name="attacker"></param>
        /// <returns></returns>
        public static float BasicDamage(IActor attacker, SideEffectJson se) {
            return attacker.CombatStats.GetField(FieldName.WeaponAttack) + (attacker.CombatStats.GetField(FieldName.WeaponAttack) * (1 + (se.basicskilldamageperc) * 0.01f));
        }

        /// <summary>
        /// 攻擊方保證傷害
        /// </summary>
        /// <param name="attacker"></param>
        /// <param name="attr"></param>
        /// <returns></returns>
        public static float CfmDamage(IActor attacker, MainWeaponAttribute attr) {
            
            switch (attr) {
                case MainWeaponAttribute.Str:
                    return attacker.CombatStats.GetField(FieldName.Strength) * 5 + attacker.CombatStats.GetField(FieldName.Attack);

                case MainWeaponAttribute.Dex:
                    return attacker.CombatStats.GetField(FieldName.Dexterity) * 2 + attacker.CombatStats.GetField(FieldName.Attack);

                case MainWeaponAttribute.Int:
                    return attacker.CombatStats.GetField(FieldName.Intelligence) * 2 + attacker.CombatStats.GetField(FieldName.Attack);

                default:
                    return attacker.CombatStats.GetField(FieldName.Attack);
            }
        }

        /// <summary>
        /// 攻擊方保證傷害
        /// </summary>
        /// <param name="attacker"></param>
        /// <param name="defender"></param>
        /// <returns></returns>
        public static bool IsCritical(FIELDNAMEPACKET packet) {
            float rate = 0;

            if (critRate > 0.0f)
                rate = critRate;   
            else
                rate = packet.attacker.CombatStats.GetField(FieldName.Critical) - packet.defender.CombatStats.GetField(FieldName.Cocritical) * 0.01f;

            float roll = (float)GameUtils.GetRandomGenerator().NextDouble();


            Debug.Log(packet.target, "Critical Roll : " + roll + " to rate of : " + rate);
            return roll < rate;
        }

        public static bool IsEvade(IActor attacker, IActor defender) {
            float rate = attacker.CombatStats.GetField(FieldName.Accuracy) - defender.CombatStats.GetField(FieldName.Evasion); 
            rate = Math.Min(Math.Max(5f, rate), 100f);

            float roll = (float)GameUtils.GetRandomGenerator().NextDouble() * 100f;

            return roll > rate;
        }

        public static bool IsEvade(IActor attacker, IActor defender, string debugKey)
        {
            float rate = attacker.CombatStats.GetField(FieldName.Accuracy) - defender.CombatStats.GetField(FieldName.Evasion);
            rate = Math.Min(Math.Max(5f, rate), 100f);

            float roll = (float)GameUtils.GetRandomGenerator().NextDouble() * 100f;

            Debug.Log(debugKey, "Evade Roll : " + roll + " to rate of : " + rate);

            return roll < rate;
        }

        public static float FinalAttackMod(FIELDNAMEPACKET packet) {
            //攻擊方最終增傷係數 =  ( 1 + 攻擊方攻擊類型增傷％) * ( 1 +攻擊方五行增傷％ ) * ( 1 +攻擊方種族增傷％) * ( 1 + 攻擊方對屬性增傷％ ) * ( 1 + 攻擊方特殊增傷％) 
            float attbonus = 1.0f + packet.attacker.CombatStats.GetField(packet.fieldInfo.attackerStrikeType);
            float elembonus = 1.0f + packet.attacker.CombatStats.GetField(packet.fieldInfo.attackerElemeent);
            float racial = 1.0f + packet.attacker.CombatStats.GetField(packet.fieldInfo.attackerVSRace);
            float elemvsbonus = 1.0f + packet.attacker.CombatStats.GetField(packet.fieldInfo.attackerVSElement);
            float npcbonus = 1;
            if(packet.basicsInfo.monsterType == MonsterType.Boss)
                npcbonus += packet.attacker.CombatStats.GetField(FieldName.VSBossDamage);

            return attbonus * elembonus * racial * elemvsbonus * npcbonus;
        }

        public static float FinalDefendMod(FIELDNAMEPACKET packet) {
            // 防禦方最終防禦係數 = 防禦方攻擊類型減傷％ * 防禦方五行減傷％ * 防禦方種族減傷％ * 防禦方最終減傷％ * ( 1 + 防禦方傷害加深％ )
            float strikedef = packet.defender.CombatStats.GetField(packet.fieldInfo.defenderStrikeType);
            float elemdef = packet.defender.CombatStats.GetField(packet.fieldInfo.defenderElement);
            float racial = packet.defender.CombatStats.GetField(packet.fieldInfo.defenderVSRace);
            float def = packet.defender.CombatStats.GetField(FieldName.DecreaseFinalDamage);
            float amp = 1.0f + packet.defender.CombatStats.GetField(FieldName.AmplifyDamage);

            return 1.0f + (strikedef * elemdef * racial * def * amp);
        }

        public static float FinalDefendModBlock(FIELDNAMEPACKET packet) {
            // 防禦方發動格擋時最終防禦係數 = 防禦方格擋減傷係數 * 防禦方攻擊類型減傷％ * 防禦方五行減傷％ * 防禦方種族減傷％ * 防禦方最終減傷％ * ( 1 + 防禦方傷害加深％ )
            float block = packet.defender.CombatStats.GetField(FieldName.BlockValue) * 0.01f;
            // FinalDefendMod -> 防禦方攻擊類型減傷％ * 防禦方五行減傷％ * 防禦方種族減傷％ * 防禦方最終減傷％ * ( 1 + 防禦方傷害加深％ )
            return block * FinalDefendMod(packet);
        }

        public static float FinalSkillMod(FIELDNAMEPACKET packet) {
            //攻擊方最終技能倍率 = ( 攻擊方技能倍率 + 攻擊方技能倍率增加效果) * ( 1 + 攻擊方技能加成 )
            float skaff = packet.sedata.basicskilldamageperc * 0.01f;
            float enhance = packet.attacker.CombatStats.GetField(FieldName.SkillAffect) * 0.01f;
            //float skaffmod = packet.attacker.CombatStats.GetField(FieldName.SkillAffec)

            return (skaff + enhance) * (1);
        }

        public static bool IsBlock(FIELDNAMEPACKET packet) {
            //do some check that i have no idea is what now
            float roll = (float)GameUtils.GetRandomGenerator().NextDouble();
            float prob = packet.defender.CombatStats.GetField(FieldName.BlockRate) * 0.01f;

            Debug.Log(packet.target, "Block roll : " + roll + " of rate : " + prob);

            return roll < prob;
        }

        public static void GeneratePackage(FIELDNAMEPACKET packet) {
            switch (packet.basicsInfo.attacker) {
                case AttackStyle.Normal:
                    packet.fieldInfo.attackerStrikeType = FieldName.NullDamage;
                    break;
                case AttackStyle.Slice:
                    packet.fieldInfo.attackerStrikeType = FieldName.SliceDamage;
                    break;
                case AttackStyle.Pierce:
                    packet.fieldInfo.attackerStrikeType = FieldName.PierceDamage;
                    break;
                case AttackStyle.Smash:
                    packet.fieldInfo.attackerStrikeType = FieldName.SmashDamage;
                    break;
                case AttackStyle.God:
                    packet.fieldInfo.attackerStrikeType = FieldName.NullDamage; // -> no god type currently...
                    break;
            }

            switch (packet.basicsInfo.defender) {
                case AttackStyle.Normal:
                    packet.fieldInfo.defenderStrikeType = FieldName.NullDefense;
                    break;
                case AttackStyle.Slice:
                    packet.fieldInfo.defenderStrikeType = FieldName.SliceDefense;
                    break;
                case AttackStyle.Pierce:
                    packet.fieldInfo.defenderStrikeType = FieldName.PierceDefense;
                    break;
                case AttackStyle.Smash:
                    packet.fieldInfo.defenderStrikeType = FieldName.SmashDefense;
                    break;
                case AttackStyle.God:
                    packet.fieldInfo.defenderStrikeType = FieldName.NullDefense; // -> no god type currently...
                    break;
            }

            switch (packet.basicsInfo.elementInfo.attacker) {
                case Element.None:
                    packet.fieldInfo.attackerElemeent = FieldName.NullDamage;
                    break;
                case Element.Metal:
                    packet.fieldInfo.attackerElemeent = FieldName.MetalDamage;
                    break;
                case Element.Wood:
                    packet.fieldInfo.attackerElemeent = FieldName.WoodDamage;
                    break;
                case Element.Earth:
                    packet.fieldInfo.attackerElemeent = FieldName.EarthDamage;
                    break;
                case Element.Fire:
                    packet.fieldInfo.attackerElemeent = FieldName.FireDamage;
                    break;
                case Element.Water:
                    packet.fieldInfo.attackerElemeent = FieldName.WaterDamage;
                    break;
            }

            switch (packet.basicsInfo.race.attacker) {
                case Race.Human:
                    packet.fieldInfo.attackerVSRace = FieldName.VSHumanDamage;
                    break;
                case Race.Zombie:
                    packet.fieldInfo.attackerVSRace = FieldName.VSZombieDamage;
                    break;
                case Race.Vampire:
                    packet.fieldInfo.attackerVSRace = FieldName.VSVampireDamage;
                    break;
                case Race.Animal:
                    packet.fieldInfo.attackerVSRace = FieldName.VSBeastDamage;
                    break;
                case Race.Plant:
                    packet.fieldInfo.attackerVSRace = FieldName.VSPlantDamage;
                    break;
            }

            switch (packet.basicsInfo.elementInfo.defender) {
                case Element.None:
                    packet.fieldInfo.attackerVSElement = FieldName.VSNullDamage;
                    break;
                case Element.Metal:
                    packet.fieldInfo.attackerVSElement = FieldName.VSMetalDamage;
                    break;
                case Element.Wood:
                    packet.fieldInfo.attackerVSElement = FieldName.VSWoodDamage;
                    break;
                case Element.Earth:
                    packet.fieldInfo.attackerVSElement = FieldName.VSEarthDamage;
                    break;
                case Element.Water:
                    packet.fieldInfo.attackerVSElement = FieldName.VSWaterDamage;
                    break;
                case Element.Fire:
                    packet.fieldInfo.attackerVSElement = FieldName.VSFireDamage;
                    break;
            }

            switch (packet.basicsInfo.elementInfo.defender) {
                case Element.None:
                    packet.fieldInfo.defenderElement = FieldName.NullDefense;
                    break;
                case Element.Metal:
                    packet.fieldInfo.defenderElement = FieldName.MetalDefense;
                    break;
                case Element.Wood:
                    packet.fieldInfo.defenderElement = FieldName.WoodDefense;
                    break;
                case Element.Earth:
                    packet.fieldInfo.defenderElement = FieldName.EarthDefense;
                    break;
                case Element.Water:
                    packet.fieldInfo.defenderElement = FieldName.WaterDefense;
                    break;
                case Element.Fire:
                    packet.fieldInfo.defenderElement = FieldName.FireDefense;
                    break;

            }

            switch (packet.basicsInfo.race.defender) {
                case Race.Human:
                    packet.fieldInfo.defenderVSRace = FieldName.VSHumanDefense;
                    break;
                case Race.Zombie:
                    packet.fieldInfo.defenderVSRace = FieldName.VSZombieDefense;
                    break;
                case Race.Vampire:
                    packet.fieldInfo.defenderVSRace = FieldName.VSVampireDefense;
                    break;
                case Race.Animal:
                    packet.fieldInfo.defenderVSRace = FieldName.VSBeastDefense;
                    break;
                case Race.Plant:
                    packet.fieldInfo.defenderVSRace = FieldName.VSPlantDefense;
                    break;
            }
            /*
             * defenderElement;
             * defenderVSRace
            */
        }

        /// <summary>
        /// Modified Compute damage to take in element type
        /// </summary>
        /// <param name="attacker"></param>
        /// <param name="defender"></param>
        /// <param name="sedata"></param>
        /// <param name="element"></param>
        /// <param name="skillDmgPercentage"></param>
        /// <param name="extradamage"></param>
        /// <param name="isDotDamage"></param>
        /// <param name="isBasicAttack"></param>
        /// <param name="ignoreArmor"></param>
        /// <returns></returns>
        public static AttackResult ComputeDamage(FIELDNAMEPACKET package, float skillDmgPercentage = 100f, int extradamage = 0,
            bool isDotDamage = false, bool isBasicAttack = false, bool ignoreArmor = false) {

            Debug.Log(package.target, package.attacker.Name + " is currently engaged with " + package.defender.Name);

            string stats = "\n";
            System.Collections.Generic.List<FieldName>[] field = package.attacker.CombatStats.GetAllFields();
            for (int i = 0; i < field.Length; i++)
            {
                System.Collections.Generic.List<FieldName> currentTierNames = field[i];
                foreach (FieldName name in currentTierNames)
                {
                    object val = package.attacker.CombatStats.GetField(name);
                    string desc = name.ToString() + " = " + val.ToString();
                    stats += desc + "\n";
                }
            }
            Photon.LoadBalancing.GameServer.CombatFormula.Debug.ClearLog("Attacker_Stats");
            Photon.LoadBalancing.GameServer.CombatFormula.Debug.Log("Attacker_Stats", "Name : " + package.attacker.Name);
            Photon.LoadBalancing.GameServer.CombatFormula.Debug.Log("Attacker_Stats", stats);

            stats = "\n";
            field = package.defender.CombatStats.GetAllFields();
            for (int i = 0; i < field.Length; i++)
            {
                System.Collections.Generic.List<FieldName> currentTierNames = field[i];
                foreach (FieldName name in currentTierNames)
                {
                    object val = package.defender.CombatStats.GetField(name);
                    string desc = name.ToString() + " = " + val.ToString();
                    stats += desc + "\n";
                }
            }
            Photon.LoadBalancing.GameServer.CombatFormula.Debug.ClearLog("Defender_Stats");
            Photon.LoadBalancing.GameServer.CombatFormula.Debug.Log("Defender_Stats", "Name : " + package.defender.Name);
            Photon.LoadBalancing.GameServer.CombatFormula.Debug.Log("Defender_Stats", stats);

            AttackResult res = new AttackResult(0, 0, false, 0);

            ICombatStats A_CombatStats = package.attacker.CombatStats;
            ICombatStats B_CombatStats = package.defender.CombatStats;
            res.IsDot = isDotDamage;
            res.IsEvasion = false;//because sideEffect already check if hit in ASCastskill
            // check for critical
            if (!isDotDamage) {
                if (package.sedata.criticaltype == CriticalType.None) {
                    res.IsCritical = false;
                }
                else {
                    res.IsCritical = IsCritical(package);
                }
            }
            else {
                res.IsCritical = false;
            }
            if (totalCrit) res.IsCritical = true;

            res.IsBlocked = IsBlock(package);

            CombatEvaluator(isBasicAttack, res, package);

            root.Update();
            res.RealDamage = (int)Math.Round(root.result) + extradamage;
            
            Debug.Log(package.target, "Damage total : " + res.RealDamage.ToString());
            return res;
        }

        public static void CombatEvaluator(bool isBasicAttack, AttackResult res, FIELDNAMEPACKET info) {
            //construct bottom up
            //this is for basic attacks
            if (isBasicAttack) {
                if(basicAttackTree == null) {
                    basicAttackTree = new DecisionNode();
                    //leaf nodes
                    BlockedAttack blocked = new BlockedAttack();
                    BlockCritAttack blockedCrit = new BlockCritAttack();
                    NormalAttack norm = new NormalAttack();
                    NormalCritAttack normcrit = new NormalCritAttack();
                    Missed miss = new Missed();

                    DecisionNode isBlockCrit = new DecisionNode();
                    //isBlockCrit.SetConditionalStatement(() => { return IsBlock(defender); });
                    isBlockCrit.SetTrueNode(blockedCrit);
                    isBlockCrit.SetFalseNode(normcrit);

                    DecisionNode nCritblock = new DecisionNode();
                    //nCritblock.SetConditionalStatement(() => { return IsBlock(defender); });
                    nCritblock.SetTrueNode(blocked);
                    nCritblock.SetFalseNode(norm);

                    DecisionNode nCritHit = new DecisionNode();
                    //nCritHit.SetConditionalStatement(() => { return !res.IsEvasion; });
                    nCritHit.SetTrueNode(nCritblock);
                    nCritHit.SetFalseNode(miss);

                    basicAttackTree.SetTrueNode(isBlockCrit);
                    basicAttackTree.SetFalseNode(nCritHit);
                }
                root = basicAttackTree;

                // set the conditions
                root.SetConditionalStatement(() => { return res.IsCritical; });
                DecisionNode node = root.condition.trueNode;
                node.SetConditionalStatement(() => { return res.IsBlocked; });
                ((LeafNode)(node.condition.trueNode)).InitInfo(info);
                ((LeafNode)(node.condition.falseNode)).InitInfo(info);

                node = root.condition.falseNode;
                node.SetConditionalStatement(() => { return !res.IsEvasion; });

                node = node.condition.trueNode;
                node.SetConditionalStatement(() => { return res.IsBlocked; });
                ((LeafNode)(node.condition.trueNode)).InitInfo(info);
                ((LeafNode)(node.condition.falseNode)).InitInfo(info);
            }
            else {
                if(skillAttackTree == null) {
                    skillAttackTree = new DecisionNode();
                    // leaf nodes
                    SkillHit hit = new SkillHit();
                    SkillBlock block = new SkillBlock();
                    Missed miss = new Missed();

                    DecisionNode isBlocked = new DecisionNode();
                    isBlocked.SetTrueNode(block);
                    isBlocked.SetFalseNode(hit);

                    skillAttackTree.SetTrueNode(isBlocked);
                    skillAttackTree.SetFalseNode(miss);
                }

                root = skillAttackTree;

                // set conditions
                root.SetConditionalStatement(() => { return !res.IsEvasion; });
                ((LeafNode)(root.condition.falseNode)).InitInfo(info);

                DecisionNode node = root.condition.trueNode;
                node.SetConditionalStatement(() => { return res.IsBlocked; });
                ((LeafNode)(node.condition.trueNode)).InitInfo(info);
                ((LeafNode)(node.condition.falseNode)).InitInfo(info);
            }
        }

        public class BlockCritAttack : LeafNode {

            public BlockCritAttack()
            {              
            }

            public override void Update()
            {
                Debug.Log(info.target, "Blocked Crit");

                float basicAtk = BasicDamage(info.attacker, info.sedata);
                float cfmDamage = CfmDamage(info.attacker, info.basicsInfo.weaponAttribute);

                Debug.Log(info.target, "Basic Attack is : " + basicAtk.ToString());
                Debug.Log(info.target, "Comformed Damage is : " + cfmDamage);

                float dmg = 0, mod, defmod, critmod;
                mod = FinalAttackMod(info);
                defmod = FinalDefendModBlock(info);
                critmod = (2 + info.attacker.CombatStats.GetField(FieldName.CriticalDamage));

                //防禦方發動格擋時無屬性普通攻擊暴擊傷害 = 無屬性攻擊方最終暴擊基礎傷害 * 攻擊方暴擊增傷 * 攻擊方最終增傷係數 * 防禦方發動格擋時最終防禦係數
                //防禦方發動格擋時金/木/土/水/火屬性普通攻擊暴擊傷害 = 金/木/土/水/火屬性攻擊方最終暴擊基礎傷害 * 攻擊方暴擊增傷 * 攻擊方最終增傷係數 * 防禦方發動格擋時最終防禦係數
                switch (info.basicsInfo.elementInfo.attacker) {
                    case Element.None:
                        dmg = ((basicAtk * Zealot.Repository.ElementalChartRepo.WeaknessChartQuery(info.basicsInfo.attacker, info.basicsInfo.defender)) + cfmDamage);
                        break;
                    case Element.Metal:
                    case Element.Wood:
                    case Element.Earth:
                    case Element.Water:
                    case Element.Fire:
                        dmg = ((basicAtk * Zealot.Repository.ElementalChartRepo.WeaknessChartQuery(info.basicsInfo.attacker, info.basicsInfo.defender))
                                        + cfmDamage) * Zealot.Repository.ElementalChartRepo.ElementChartQuery(info.basicsInfo.elementInfo.attacker, info.basicsInfo.elementInfo.defender);
                        break;

                }
                Debug.Log(info.target, "Damage is : " + dmg);
                Debug.Log(info.target, "Crit Mod : " + critmod);
                Debug.Log(info.target, "Final Attack Mod : " + mod);
                Debug.Log(info.target, "Defence Mod : " + defmod);
                result = Math.Max(1.0f, (dmg * critmod * mod * defmod));
            }
        }

        public class NormalCritAttack : LeafNode {
            public NormalCritAttack()
            {
            }

            public override void Update()
            {
                Debug.Log(info.target, "Normal Crit Attack");

                float basicAtk = BasicDamage(info.attacker, info.sedata);
                float cfmDamage = CfmDamage(info.attacker, info.basicsInfo.weaponAttribute);

                Debug.Log(info.target, "Basic Attack is : " + basicAtk.ToString());
                Debug.Log(info.target, "Comformed Damage is : " + cfmDamage);

                float dmg = 0, mod, defmod, critmod;
                mod = FinalAttackMod(info);
                defmod = FinalDefendMod(info);
                critmod = (2 + info.attacker.CombatStats.GetField(FieldName.CriticalDamage));

                //無屬性普通攻擊暴擊傷害 = 無屬性攻擊方最終暴擊基礎傷害 * 攻擊方暴擊增傷 * 攻擊方最終增傷係數 * 防禦方最終防禦係數
                //金/木/土/水/火屬性普通攻擊暴擊傷害 = 金/木/土/水/火屬性攻擊方最終暴擊基礎傷害 * 攻擊方暴擊增傷 * 攻擊方最終增傷係數 * 防禦方最終防禦係數
                switch (info.basicsInfo.elementInfo.attacker) {
                    case Element.None:
                        dmg = ((basicAtk * Zealot.Repository.ElementalChartRepo.WeaknessChartQuery(info.basicsInfo.attacker, info.basicsInfo.defender)) + cfmDamage);
                        break;
                    case Element.Metal:
                    case Element.Wood:
                    case Element.Earth:
                    case Element.Water:
                    case Element.Fire:
                        dmg = ((basicAtk * Zealot.Repository.ElementalChartRepo.WeaknessChartQuery(info.basicsInfo.attacker, info.basicsInfo.defender))
                                        + cfmDamage) * Zealot.Repository.ElementalChartRepo.ElementChartQuery(info.basicsInfo.elementInfo.attacker, info.basicsInfo.elementInfo.defender);
                        break;

                }

                Debug.Log(info.target, "Damage is : " + dmg);
                Debug.Log(info.target, "Crit Mod : " + critmod);
                Debug.Log(info.target, "Final Attack Mod : " + mod);
                Debug.Log(info.target, "Defence Mod : " + defmod);

                result = Math.Max(1.0f, dmg * critmod * mod * defmod);
            }
        }

        public class BlockedAttack : LeafNode {
            public BlockedAttack()
            {
            }

            public override void Update()
            {
                Debug.Log(info.target, "Attack Blocked");

                float basicAtk = BasicDamage(info.attacker, info.sedata);
                float cfmDamage = CfmDamage(info.attacker, info.basicsInfo.weaponAttribute);

                Debug.Log(info.target, "Basic Attack is : " + basicAtk.ToString());
                Debug.Log(info.target, "Comformed Damage is : " + cfmDamage);

                float dmg = 0, mod, defmod;
                mod = FinalAttackMod(info);
                defmod = FinalDefendModBlock(info);

                //防禦方發動格擋時無屬性普通攻擊傷害 = 無屬性攻擊方最終基礎傷害 * 攻擊方最終增傷係數 * 防禦方發動格擋時最終防禦係數
                //防禦方發動格擋時金/木/土/水/火屬性普通攻擊傷害 = 金/木/土/水/火屬性攻擊方最終基礎傷害 * 攻擊方最終增傷係數 * 防禦方發動格擋時最終防禦係數
                switch (info.basicsInfo.elementInfo.attacker) {
                    case Element.None:
                        dmg = ((basicAtk * Zealot.Repository.ElementalChartRepo.WeaknessChartQuery(info.basicsInfo.attacker, info.basicsInfo.defender) * info.defender.CombatStats.GetField(FieldName.IgnoreArmor) * 0.01f)
                                        + cfmDamage - (info.defender.CombatStats.GetField(FieldName.Constitution)) * 2);
                        break;
                    case Element.Metal:
                    case Element.Wood:
                    case Element.Earth:
                    case Element.Water:
                    case Element.Fire:
                        dmg = ((basicAtk * Zealot.Repository.ElementalChartRepo.WeaknessChartQuery(info.basicsInfo.attacker, info.basicsInfo.defender) * info.defender.CombatStats.GetField(FieldName.IgnoreArmor) * 0.01f)
                                        + cfmDamage) - (info.defender.CombatStats.GetField(FieldName.Intelligence)) * 2;
                        break;

                }

                Debug.Log(info.target, "Damage is : " + dmg);
                Debug.Log(info.target, "Final Attack Mod : " + mod);
                Debug.Log(info.target, "Defence Mod : " + defmod);

                result = Math.Max(1.0f, dmg * mod * defmod);
            }
        }

        public class NormalAttack : LeafNode {
            public NormalAttack()
            {
            }

            public override void Update()
            {
                Debug.Log(info.target, "Normal Attack");

                float basicAtk = BasicDamage(info.attacker, info.sedata);
                float cfmDamage = CfmDamage(info.attacker, info.basicsInfo.weaponAttribute);

                Debug.Log(info.target, "Basic Attack is : " + basicAtk);
                Debug.Log(info.target, "Comformed Damage is : " + cfmDamage);

                float dmg = 0, mod, defmod;
                mod = FinalAttackMod(info);
                defmod = FinalDefendMod(info);

                float weakness = (info.basicsInfo.isDefenderNPC) ? Zealot.Repository.ElementalChartRepo.WeaknessChartQuery(info.basicsInfo.attacker, info.basicsInfo.defender) : 0.0f;

                // 無屬性普通攻擊傷害 = 無屬性攻擊方最終基礎傷害 * 攻擊方最終增傷係數 * 防禦方最終防禦係數
                // 金 / 木 / 土 / 水 / 火屬性普通攻擊傷害 = 金 / 木 / 土 / 水 / 火屬性攻擊方最終基礎傷害 * 攻擊方最終增傷係數 * 防禦方最終防禦係數
                switch (info.basicsInfo.elementInfo.attacker) {
                    case Element.None:
                        
                        dmg = ((basicAtk * (1.0f + weakness) * (1.0f - info.defender.CombatStats.GetField(FieldName.IgnoreArmor)))
                                        + cfmDamage - (info.defender.CombatStats.GetField(FieldName.Constitution)) * 2);
                        break;
                    case Element.Metal:
                    case Element.Wood:
                    case Element.Earth:
                    case Element.Water:
                    case Element.Fire:
                        dmg = ((basicAtk * (1.0f + weakness) * (1.0f - info.defender.CombatStats.GetField(FieldName.IgnoreArmor)))
                                        + cfmDamage) - (info.defender.CombatStats.GetField(FieldName.Intelligence)) * 2;
                                        
                        break;
                        
                }

                Debug.Log(info.target, "Damage is : " + dmg);
                Debug.Log(info.target, "Final Attack Mod : " + mod);
                Debug.Log(info.target, "Defence Mod : " + defmod);

                result = Math.Max(1.0f, dmg * mod * defmod);
            }
        }

        public class SkillHit : LeafNode {
            public override void Update()
            {
                Debug.Log(info.target, "Skill Hit");

                float basicAtk = BasicDamage(info.attacker, info.sedata);
                float cfmDamage = CfmDamage(info.attacker, info.basicsInfo.weaponAttribute);

                Debug.Log(info.target, "Basic Attack is : " + basicAtk);
                Debug.Log(info.target, "Comformed Damage is : " + cfmDamage);

                float dmg = 0, mod, atkmod, defmod;
                mod = FinalSkillMod(info);
                atkmod = FinalAttackMod(info);
                defmod = FinalDefendMod(info);

                // 無屬性技能攻擊傷害 = 無屬性攻擊方最終基礎傷害 * 攻擊方最終技能倍率 * 攻擊方最終增傷係數 * 防禦方最終防禦係數
                // 金/木/土/水/火屬性技能攻擊傷害 = 金/木/土/水/火屬性攻擊方最終基礎傷害 * 攻擊方最終技能倍率 * 攻擊方最終增傷係數 * 防禦方最終防禦係數
                float weakness = (info.basicsInfo.isDefenderNPC) ? Zealot.Repository.ElementalChartRepo.WeaknessChartQuery(info.basicsInfo.attacker, info.basicsInfo.defender) : 0.0f;
                switch (info.basicsInfo.elementInfo.attacker) {
                    case Element.None:
                        // 無屬性攻擊方最終基礎傷害 = ( ( 攻擊方基礎傷害 * 弱點修正係數 * 防禦方防禦減傷％ ) + 攻擊方保證傷害 - 防禦方減值減傷 )
                        dmg = ((basicAtk * (1.0f + weakness) * (1.0f - info.defender.CombatStats.GetField(FieldName.IgnoreArmor)))
                                        + cfmDamage - (info.defender.CombatStats.GetField(FieldName.Constitution)) * 2);
                        break;
                    case Element.Metal:
                    case Element.Wood:
                    case Element.Earth:
                    case Element.Water:
                    case Element.Fire:
                        // 金/木/土/水/火屬性攻擊方最終基礎傷害 = ( ( 攻擊方基礎傷害 * 弱點修正係數 * 防禦方防禦減傷％ ) + 攻擊方保證傷害 ) - 防禦方五行減值減傷
                        dmg = ((basicAtk * (1.0f + weakness) * (1.0f - info.defender.CombatStats.GetField(FieldName.IgnoreArmor)))
                                         + cfmDamage) - (info.defender.CombatStats.GetField(FieldName.Intelligence)) * 2;
                        break;
                }

                Debug.Log(info.target, "Damage is : " + dmg);
                Debug.Log(info.target, "Final Skill Mod : " + mod);
                Debug.Log(info.target, "Final Attack Mod : " + atkmod);
                Debug.Log(info.target, "Defence Mod : " + defmod);

                result = Math.Max(1.0f, dmg * mod * atkmod * defmod);
            }
        }

        public class SkillBlock : LeafNode {
            public override void Update()
            {

                Debug.Log(info.target, "Skill Blocked");

                float basicAtk = BasicDamage(info.attacker, info.sedata);
                float cfmDamage = CfmDamage(info.attacker, info.basicsInfo.weaponAttribute);

                Debug.Log(info.target, "Basic Attack is : " + basicAtk);
                Debug.Log(info.target, "Comformed Damage is : " + cfmDamage);

                float dmg = 0, mod, atkmod, defmod;
                mod = FinalAttackMod(info);
                atkmod = FinalAttackMod(info);
                defmod = FinalDefendModBlock(info);
                float weakness = (info.basicsInfo.isDefenderNPC) ? Zealot.Repository.ElementalChartRepo.WeaknessChartQuery(info.basicsInfo.attacker, info.basicsInfo.defender) : 0.0f;
                // 防禦方發動格擋時無屬性技能攻擊傷害 = 無屬性攻擊方最終基礎傷害 * 攻擊方最終技能倍率 * 攻擊方最終增傷係數 * 防禦方發動格擋時最終防禦係數
                // 防禦方發動格擋時金/木/土/水/火屬性技能攻擊傷害 = 金/木/土/水/火屬性攻擊方最終基礎傷害 * 攻擊方最終技能倍率 * 攻擊方最終增傷係數 * 防禦方發動格擋時最終防禦係數
                switch (info.basicsInfo.elementInfo.attacker) {
                    case Element.None:
                        // 無屬性攻擊方最終基礎傷害 = ( ( 攻擊方基礎傷害 * 弱點修正係數 * 防禦方防禦減傷％ ) + 攻擊方保證傷害 - 防禦方減值減傷 )
                        dmg = ((basicAtk * (1.0f + weakness) * (1.0f - info.defender.CombatStats.GetField(FieldName.IgnoreArmor)))
                                        + cfmDamage - (info.defender.CombatStats.GetField(FieldName.Constitution)) * 2);
                        break;
                    case Element.Metal:
                    case Element.Wood:
                    case Element.Earth:
                    case Element.Water:
                    case Element.Fire:
                        // 金/木/土/水/火屬性攻擊方最終基礎傷害 = ( ( 攻擊方基礎傷害 * 弱點修正係數 * 防禦方防禦減傷％ ) + 攻擊方保證傷害 ) - 防禦方五行減值減傷
                        dmg = ((basicAtk * (1.0f + weakness) * (1.0f - info.defender.CombatStats.GetField(FieldName.IgnoreArmor)))
                                         + cfmDamage) - (info.defender.CombatStats.GetField(FieldName.Intelligence)) * 2;
                        break;
                }

                Debug.Log(info.target, "Damage is : " + dmg);
                Debug.Log(info.target, "Final Skill Mod : " + mod);
                Debug.Log(info.target, "Final Attack Mod : " + atkmod);
                Debug.Log(info.target, "Defence Mod : " + defmod);

                result = Math.Max(1.0f, dmg * mod * atkmod * defmod);
            }
        }

        public class Missed : LeafNode {

            public override void Update()
            {
                Debug.Log(info.target, "Missed");
            }
        }
    }
}
 