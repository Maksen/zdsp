using Kopio.JsonContracts;
using System;
using System.Collections.Generic;
using Zealot.Repository;

namespace Zealot.Common.Entities
{
    //Calculations only required for playercombatstats
    //MonsterCombatStats are predetermined
    public enum FieldName : int
    {
        //CoCriticalDamage, //10 map to reduce 0.1% of ciritcal damage
        //CoCriticalDamageBonus, //10 map to reduce 0.1% of ciritcal damage
        //CoCriticalDamagePercBonus,
        //CoCriticalDamageBonus_NoScore, //10 map to reduce 0.1% of ciritcal damage
        //CoCriticalDamagePercBonus_NoScore,
        //CoCriticalDamageBase,

        //TalentPointStone,
        //TalentPointCloth,
        //TalentPointScissors,
        CombatScore,

        // Aditional Stuff
        // Basic Stuff

        Strength,
        StrengthBonus,
        StrengthPercBonus,
        StrengthBonus_NoScore,
        StrengthPercBonus_NoScore,
        StrengthBase,

        Agility,
        AgilityBonus,
        AgilityPercBonus,
        AgilityBonus_NoScore,
        AgilityPercBonus_NoScore,
        AgilityBase,

        Dexterity,
        DexterityBonus,
        DexterityPercBonus,
        DexterityBonus_NoScore,
        DexterityPercBonus_NoScore,
        DexterityBase,

        Constitution,
        ConstitutionBonus,
        ConstitutionPercBonus,
        ConstitutionBonus_NoScore,
        ConstitutionPercBonus_NoScore,
        ConstitutionBase,

        Intelligence,
        IntelligenceBonus,
        IntelligencePercBonus,
        IntelligenceBonus_NoScore,
        IntelligencePercBonus_NoScore,
        IntelligenceBase,

        // Attack speed
        AttackSpeed,
        AttackSpeedBase,
        AttackSpeedBuff,
        AttackSpeedDebuff,

        // Skill Cast Speed
        CastSpeed,
        CastSpeedBase,
        CastSpeedBuff,
        CastSpeedDebuff,

        // Move Speed
        MoveSpeed,
        MoveSpeedBase,
        MoveSpeedBuff,
        MoveSpeedDebuff,

        //Exp Bonus
        ExpBonus,

        // Elements
        ElementWeapon,
        ElementSideEffect,
        Element,

        // Mana Reduction
        ManaReduceBonus,
        ManaReducePercBonus,

        // Skill Affect bonus
        SkillAffect,

        // Healing doob
        Healing,
        HealingRejuvenation,
        HealingPoint,
        HealingEffect,
        HealingBuff,
        HealingDebuff,

        HealthPotion,
        ManaPotion,

        //MaxHP
        Health,
        HealthMax,
        HealthBonus,
        HealthPercBonus,
        HealthBonus_NoScore,
        HealthPercBonus_NoScore,
        HealthBase,

        //HP Regen
        HealthRegen,
        HealthRegenBonus,
        HealthRegenPercBonus,
        HealthRegenBonus_NoScore,
        HealthRegenPercBonus_NoScore,

        Mana,
        ManaMax,
        ManaBonus,
        ManaPercBonus,
        ManaBonus_NoScore,
        ManaPercBonus_NoScore,
        ManaBase,

        //Mana Regen
        ManaRegen,
        ManaRegenBonus,
        ManaRegenPercBonus,
        ManaRegenBonus_NoScore,
        ManaRegenPercBonus_NoScore,

        // Skills and Equipment based
        EnergyShield,

        WeaponAttack,
        WeaponAttackBonus,
        WeaponAttackPercBonus,
        //WeaponAttackBonus_NoScore,
        //WeaponAttackPercBonus_NoScore,
        WeaponAttackBase,

        Attack,
        AttackBonus,
        AttackPercBonus,
        AttackBonus_NoScore,
        AttackPercBonus_NoScore,
        AttackBase,

        Armor,
        ArmorItem,
        ArmorBonus,
        ArmorPercBonus,
        ArmorBonus_NoScore,
        ArmorPercBonus_NoScore,
        ArmorBase,

        IgnoreArmor,
        IgnoreArmorBonus,
        IgnoreArmorBase,

        BlockRate,
        BlockValue,
        BlockValueBonus,
        BlockValuePercBonus,
        BlockValueBonus_NoScore,
        BlockValuePercBonus_NoScore,

        AbsorbDamage,

        Accuracy,
        AccuracyBonus,
        AccuracyPercBonus,
        AccuracyBonus_NoScore,
        AccuracyPercBonus_NoScore,
        AccuracyBase,

        Evasion, //10 map to 0.1% of evasion
        EvasionBonus, //10 map to 0.1% of evasion
        EvasionPercBonus,
        EvasionBonus_NoScore, //10 map to 0.1% of evasion
        EvasionPercBonus_NoScore,
        EvasionBase,

        Critical,   //10 map to 0.1% chance of critical
        CriticalBonus,   //10 map to 0.1% chance of critical
        CriticalPercBonus,
        CriticalBonus_NoScore,   //10 map to 0.1% chance of critical
        CriticalPercBonus_NoScore,
        CriticalBase,

        Cocritical, //10 map to reduce 0.1% chance of ciritical
        CocriticalBonus, //10 map to reduce 0.1% chance of ciritical
        CocriticalPercBonus,
        CocriticalBonus_NoScore, //10 map to reduce 0.1% chance of ciritical
        CocriticalPercBonus_NoScore,
        CocriticalBase,

        CriticalDamage, //10 map to 0.1% of critical damage
        CriticalDamageBonus, //10 map to 0.1% of critical damage
        //CriticalDamagePercBonus,
        CriticalDamageBonus_NoScore, //10 map to 0.1% of critical damage
        CriticalDamagePercBonus_NoScore,
        CriticalDamageBase,

        // Increase Damage buffs

        SmashDamage,
        SmashDamageBonus,
        SmashDamagePercBonus,
        SmashDamageBonus_NoScore,
        SmashDamagePercBonus_NoScore,

        SliceDamage,
        SliceDamageBonus,
        SliceDamagePercBonus,
        SliceDamageBonus_NoScore,
        SliceDamagePercBonus_NoScore,

        PierceDamage,
        PierceDamageBonus,
        PierceDamagePercBonus,
        PierceDamageBonus_NoScore,
        PierceDamagePercBonus_NoScore,

        // Increase Defense Buff

        SmashDefense,
        SmashDefenseBonus,
        SmashDefensePercBonus,
        SmashDefenseBonus_NoScore,
        SmashDefensePercBonus_NoScore,

        SliceDefense,
        SliceDefenseBonus,
        SliceDefensePercBonus,
        SliceDefenseBonus_NoScore,
        SliceDefensePercBonus_NoScore,

        PierceDefense,
        PierceDefenseBonus,
        PierceDefensePercBonus,
        PierceDefenseBonus_NoScore,
        PierceDefensePercBonus_NoScore,

        // Elemental stuff
        // Increase Damage Buffs

        NullDamage,
        NullDamageBonus,
        NullDamagePercBonus,
        NullDamageBonus_NoScore,
        NullDamagePercBonus_NoScore,

        MetalDamage,
        MetalDamageBonus,
        MetalDamagePercBonus,
        MetalDamageBonus_NoScore,
        MetalDamagePercBonus_NoScore,

        WoodDamage,
        WoodDamageBonus,
        WoodDamagePercBonus,
        WoodDamageBonus_NoScore,
        WoodDamagePercBonus_NoScore,

        EarthDamage,
        EarthDamageBonus,
        EarthDamagePercBonus,
        EarthDamageBonus_NoScore,
        EarthDamagePercBonus_NoScore,

        WaterDamage,
        WaterDamageBonus,
        WaterDamagePercBonus,
        WaterDamageBonus_NoScore,
        WaterDamagePercBonus_NoScore,

        FireDamage,
        FireDamageBonus,
        FireDamagePercBonus,
        FireDamageBonus_NoScore,
        FireDamagePercBonus_NoScore,

        // Increase Defense Buff

        NullDefense,
        NullDefenseBonus,
        NullDefensePercBonus,
        NullDefenseBonus_NoScore,
        NullDefensePercBonus_NoScore,

        MetalDefense,
        MetalDefenseBonus,
        MetalDefensePercBonus,
        MetalDefenseBonus_NoScore,
        MetalDefensePercBonus_NoScore,

        WoodDefense,
        WoodDefenseBonus,
        WoodDefensePercBonus,
        WoodDefenseBonus_NoScore,
        WoodDefensePercBonus_NoScore,

        EarthDefense,
        EarthDefenseBonus,
        EarthDefensePercBonus,
        EarthDefenseBonus_NoScore,
        EarthDefensePercBonus_NoScore,

        WaterDefense,
        WaterDefenseBonus,
        WaterDefensePercBonus,
        WaterDefenseBonus_NoScore,
        WaterDefensePercBonus_NoScore,

        FireDefense,
        FireDefenseBonus,
        FireDefensePercBonus,
        FireDefenseBonus_NoScore,
        FireDefensePercBonus_NoScore,

        // Affinity bonus
        // Racial Damage

        VSHumanDamage,
        VSHumanDamageBonus,
        VSHumanDamagePercBonus,
        VSHumanDamageBonus_NoScore,
        VSHumanDamagePercBonus_NoScore,

        VSZombieDamage,
        VSZombieDamageBonus,
        VSZombieDamagePercBonus,
        VSZombieDamageBonus_NoScore,
        VSZombieDamagePercBonus_NoScore,

        VSVampireDamage,
        VSVampireDamageBonus,
        VSVampireDamagePercBonus,
        VSVampireDamageBonus_NoScore,
        VSVampireDamagePercBonus_NoScore,

        VSBeastDamage,
        VSBeastDamageBonus,
        VSBeastDamagePercBonus,
        VSBeastDamageBonus_NoScore,
        VSBeastDamagePercBonus_NoScore,

        VSPlantDamage,
        VSPlantDamageBonus,
        VSPlantDamagePercBonus,
        VSPlantDamageBonus_NoScore,
        VSPlantDamagePercBonus_NoScore,

        //Racial Defense

        VSHumanDefense,
        VSHumanDefenseBonus,
        VSHumanDefensePercBonus,
        VSHumanDefenseBonus_NoScore,
        VSHumanDefensePercBonus_NoScore,

        VSZombieDefense,
        VSZombieDefenseBonus,
        VSZombieDefensePercBonus,
        VSZombieDefenseBonus_NoScore,
        VSZombieDefensePercBonus_NoScore,

        VSVampireDefense,
        VSVampireDefenseBonus,
        VSVampireDefensePercBonus,
        VSVampireDefenseBonus_NoScore,
        VSVampireDefensePercBonus_NoScore,

        VSBeastDefense,
        VSBeastDefenseBonus,
        VSBeastDefensePercBonus,
        VSBeastDefenseBonus_NoScore,
        VSBeastDefensePercBonus_NoScore,

        VSPlantDefense,
        VSPlantDefenseBonus,
        VSPlantDefensePercBonus,
        VSPlantDefenseBonus_NoScore,
        VSPlantDefensePercBonus_NoScore,

        // Elemental Damage

        VSNullDamage,
        VSNullPercDamage,

        VSMetalDamage,
        VSMetalPercDamage,

        VSWoodDamage,
        VSWoodPercDamage,

        VSEarthDamage,
        VSEarthPercDamage,

        VSWaterDamage,
        VSWaterPercDamage,

        VSFireDamage,
        VSFirePercDamage,

        IncreaseFinalDamage,
        IncreaseFinalDamageBonus,
        DecreaseFinalDamage,
        DecreaseFinalDamageBonus,

        VSBossDamage,
        VSBossDamageBonus,
        // Aditional Stuff

        AbsorbDamageBonus,

        LastField //For determining length, not a field
    }

    public interface ICombatStats
    {
        float GetField(FieldName fieldname);

        void SetField(FieldName fieldname, float newval);

        void AddToField(FieldName fieldname, float newval);

        void ComputeAll();

        List<FieldName>[] GetAllFields();

        int GetRandomAttack();
    }

    public abstract class CombatStats : ICombatStats
    {
        protected abstract class CombatStatsField
        {
            protected float fieldValue;
            protected FieldName[] children;
            public bool Dirty { get; set; }

            public float GetValue()
            {
                return fieldValue;
            }

            public void SetValue(float newval)
            {
                fieldValue = newval;
            }

            public void AddValue(float newval)
            {
                fieldValue += newval;
            }

            public virtual void Compute(CombatStatsField[] fields, LocalCombatStats localCombatStats = null, ActorSynStats actorSynStats = null)
            {
            }

            public FieldName[] GetChildren()
            {
                return children;
            }
        }

        protected CombatStatsField[] mFields;
        protected List<FieldName>[] mTierFieldNames;
        protected LocalCombatStats mLocalCombatStats;
        protected ActorSynStats mActorSynStats;
        protected IActor mActor;

        public virtual float GetField(FieldName fieldname)
        {
            return mFields[(int)fieldname].GetValue();
        }      

        public virtual void SetField(FieldName fieldname, float newval)
        {
            CombatStatsField field = mFields[(int)fieldname];
            field.SetValue(newval);
            SetDirty(field);
        }

        public virtual void AddToField(FieldName fieldname, float newval)
        {
            CombatStatsField field = mFields[(int)fieldname];
            field.AddValue(newval);
            SetDirty(field);
        }

        private void SetDirty(CombatStatsField field)
        {
            field.Dirty = true;
            FieldName[] children = field.GetChildren();
            for (int i = 0; i < children.Length; i++)
            {
                FieldName name = children[i];
                CombatStatsField childfield = mFields[(int)name];
                if (!childfield.Dirty) //set if not dirty yet and recurse
                {
                    SetDirty(childfield);
                }
            }
        }

        public bool SuppressComputeAll { get; set; }

        public virtual void ComputeAll()
        {
            if (SuppressComputeAll)
                return;

            for (int i = 0; i < mTierFieldNames.Length; i++)
            {
                List<FieldName> currentTierNames = mTierFieldNames[i];
                foreach (FieldName name in currentTierNames)
                {
                    CombatStatsField field = mFields[(int)name];
                    if (field.Dirty)
                    {
                        field.Compute(mFields, mLocalCombatStats, mActorSynStats);
                        field.Dirty = false;
                    }
                }
            }
            if (mActor != null)
            {
                mActor.UpdateLocalSkillPassiveStats();
                mActor.OnComputeCombatStats();
            }
        }

        public virtual List<FieldName>[] GetAllFields()
        {
            return mTierFieldNames;
        }

        public virtual int GetRandomAttack() //For Damage Computation
        {
            int attack = (int)GetField(FieldName.Attack);

            return attack;
        }
    }

    public class PlayerCombatStats : CombatStats
    {
        public static readonly int STRENGTH_MAX = 32767, STRENGTH_MIN = 0;
        public static readonly int AGILITY_MAX = 32767, AGILITY_MIN = 0;
        public static readonly int CONSTITUTION_MAX = 32767, CONSTITUTION_MIN = 0;
        public static readonly int BASICHEALTHMAX_MAX = 4500000, BASICHEALTHMAX_MIN = 1;
        public static readonly float HEALTHMAXPERCENT_MAX = 2.5f, HEALTHMAXPERCENT_MIN = 0.6f;
        public static readonly float ATTACKPERCENT_MAX = 2.45f, ATTACKPERCENT_MIN = 0.6f;
        public static readonly int ATTACK_MAX = 1200000, ATTACK_MIN = 1;
        public static readonly float ARMORPERCENT_MAX = 2.2f, ARMORPERCENT_MIN = 0.6f;
        public static readonly int ARMOR_MAX = 720000, ARMOR_MIN = 0;
        public static readonly float ACCURACYPERCENT_MAX = 2.35f, ACCURACYPERCENT_MIN = 1.0f;
        public static readonly int ACCURACY_MAX = 50000, ACCURACY_MIN = 1;
        public static readonly float EVASIONPERCENT_MAX = 2.35f, EVASIONPERCENT_MIN = 1.0f;
        public static readonly int EVASION_MAX = 50000, EVASION_MIN = 1;
        public static readonly float EXPPERCENT_MAX = 5.0f, EXPPERCENT_MIN = 1.0f;
        public static readonly float IGNOREAMORPERCENT_MAX = 0.2f, IGNOREAMORPERCENT_MIN = 0.0f;
        public static readonly float CRITICALPERCENT_MAX = 0.65f, CRITICALPERCENT_MIN = 0.0f;
        public static readonly float COCRITICALPERCENT_MAX = 0.65f, COCRITICALPERCENT_MIN = 0.0f;
        public static readonly float CRITICALPERFECTPERCENT_MAX = 0.7f, CRITICALPERFECTPERCENT_MIN = 0.0f;
        public static readonly float COCRITICALPERFECTPERCENT_MAX = 0.7f, COCRITICALPERFECTPERCENT_MIN = 0.0f;
        public static readonly float CRITICALDOUBLEPERCENT_MAX = 0.45f, CRITICALDOUBLEPERCENT_MIN = 0.0f;
        public static readonly float COCRITICALDOUBLEPERCENT_MAX = 0.45f, COCRITICALDOUBLEPERCENT_MIN = 0.0f;
        public static readonly int RECOVERONHIT_MAX = 10000, RECOVERONHIT_MIN = 0;
        public static readonly int SKILLDAMAGE_MAX = 25000, SKILLDAMAGE_MIN = 0;
        public static readonly int SKILLDAMAGEREDUCE_MAX = 20000, SKILLDAMAGEREDUCE_MIN = 0;
        public static readonly float AMPLIFYDAMAGEPERCENT_MAX = 1.7f, AMPLIFYDAMAGEPERCENT_MIN = 0.0f;
        public static readonly float ABSORBDAMAGEPERCENT_MAX = 1.5f, ABSORBDAMAGEPERCENT_MIN = 0.0f;
        public static readonly int SPSKILLDAMAGE_MAX = 200000, SPSKILLDAMAGE_MIN = 0;
        public static readonly int SPSKILLDAMAGEREDUCE_MAX = 200000, SPSKILLDAMAGEREDUCE_MIN = 0;

        

        private class AbsorbDamageBonusField : CombatStatsField
        {
            public AbsorbDamageBonusField()
            {
                fieldValue = 0;
                children = new FieldName[] { FieldName.AbsorbDamage };
            }
        }

        private class SimpleField : CombatStatsField
        {
            public SimpleField()
            {
                fieldValue = 0;
                children = new FieldName[] { };
            }
        }

        // Aditional Stats
        // Basics
        private class StrengthBase : CombatStatsField
        {
            public StrengthBase()
            {
                fieldValue = 0;
                children = new FieldName[] { FieldName.Strength, FieldName.CombatScore };
            }
        }

        private class StrengthBonus : CombatStatsField
        {
            public StrengthBonus()
            {
                fieldValue = 0;
                children = new FieldName[] { FieldName.Strength };
            }
        }

        private class StrengthPercBonus : CombatStatsField
        {
            public StrengthPercBonus()
            {
                fieldValue = 0;
                children = new FieldName[] { FieldName.Strength };
            }
        }

        private class StrengthBonus_NoScore : CombatStatsField
        {
            public StrengthBonus_NoScore()
            {
                fieldValue = 0;
                children = new FieldName[] { FieldName.Strength };
            }
        }

        private class StrengthPercBonus_NoScore : CombatStatsField
        {
            public StrengthPercBonus_NoScore()
            {
                fieldValue = 0;
                children = new FieldName[] { FieldName.Strength };
            }
        }

        private class Strength : CombatStatsField
        {
            public Strength()
            {
                fieldValue = 0;
                children = new FieldName[] { FieldName.CombatScore };
            }

            public override void Compute(CombatStatsField[] fields, LocalCombatStats localCombatStats, ActorSynStats actorSynStats)
            {
                float str = fields[(int)FieldName.StrengthBase].GetValue() + fields[(int)FieldName.StrengthBonus].GetValue();
                float str_mod = fields[(int)FieldName.StrengthPercBonus].GetValue() * 0.01f;
                fieldValue = (int)(str * (1 + str_mod));
                if (localCombatStats != null)
                {
                    str -= (int)fields[(int)FieldName.StrengthBonus_NoScore].GetValue();
                    str_mod -= (int)fields[(int)FieldName.StrengthPercBonus_NoScore].GetValue() * 0.01f;
                    localCombatStats.Strength = (int)(str * (1 + str_mod));
                }
            }
        }

        private class AgilityBase : CombatStatsField
        {
            public AgilityBase()
            {
                fieldValue = 0;
                children = new FieldName[] { FieldName.Agility, FieldName.CombatScore };
            }
        }

        private class AgilityBonus : CombatStatsField
        {
            public AgilityBonus()
            {
                fieldValue = 0;
                children = new FieldName[] { FieldName.Agility };
            }
        }

        private class AgilityPercBonus : CombatStatsField
        {
            public AgilityPercBonus()
            {
                fieldValue = 0;
                children = new FieldName[] { FieldName.Agility };
            }
        }

        private class AgilityBonus_NoScore : CombatStatsField
        {
            public AgilityBonus_NoScore()
            {
                fieldValue = 0;
                children = new FieldName[] { FieldName.Agility };
            }
        }

        private class AgilityPercBonus_NoScore : CombatStatsField
        {
            public AgilityPercBonus_NoScore()
            {
                fieldValue = 0;
                children = new FieldName[] { FieldName.Agility };
            }
        }

        private class Agility : CombatStatsField
        {
            public Agility()
            {
                fieldValue = 0;
                children = new FieldName[] { FieldName.CombatScore };
            }

            public override void Compute(CombatStatsField[] fields, LocalCombatStats localCombatStats, ActorSynStats actorSynStats)
            {
                float agi = fields[(int)FieldName.AgilityBase].GetValue() + fields[(int)FieldName.AgilityBonus].GetValue();
                float agi_mod = fields[(int)FieldName.AgilityPercBonus].GetValue() * 0.01f;
                fieldValue = (int)(agi * (1 + agi_mod));

                if (localCombatStats != null)
                {
                    agi -= (int)fields[(int)FieldName.AgilityBonus_NoScore].GetValue();
                    agi_mod -= (int)fields[(int)FieldName.AgilityPercBonus_NoScore].GetValue() * 0.01f;
                    localCombatStats.Agility = (int)(agi * (1 + agi_mod));
                }
            }
        }

        private class DexterityBase : CombatStatsField
        {
            public DexterityBase()
            {
                fieldValue = 0;
                children = new FieldName[] { FieldName.Dexterity, FieldName.CombatScore };
            }
        }

        private class DexterityBonus : CombatStatsField
        {
            public DexterityBonus()
            {
                fieldValue = 0;
                children = new FieldName[] { FieldName.Dexterity };
            }
        }

        private class DexterityPercBonus : CombatStatsField
        {
            public DexterityPercBonus()
            {
                fieldValue = 0;
                children = new FieldName[] { FieldName.Dexterity };
            }
        }

        private class DexterityBonus_NoScore : CombatStatsField
        {
            public DexterityBonus_NoScore()
            {
                fieldValue = 0;
                children = new FieldName[] { FieldName.Dexterity };
            }
        }

        private class DexterityPercBonus_NoScore : CombatStatsField
        {
            public DexterityPercBonus_NoScore()
            {
                fieldValue = 0;
                children = new FieldName[] { FieldName.Dexterity };
            }
        }

        private class Dexterity : CombatStatsField
        {
            public Dexterity()
            {
                fieldValue = 0;
                children = new FieldName[] { FieldName.CombatScore };
            }

            public override void Compute(CombatStatsField[] fields, LocalCombatStats localCombatStats, ActorSynStats actorSynStats)
            {
                float dex = fields[(int)FieldName.DexterityBase].GetValue() + fields[(int)FieldName.DexterityBonus].GetValue();
                float dex_mod = fields[(int)FieldName.DexterityPercBonus].GetValue() * 0.01f;
                fieldValue = (int)(dex * (1 + dex_mod));

                if (localCombatStats != null)
                {
                    dex -= fields[(int)FieldName.DexterityBonus_NoScore].GetValue();
                    dex_mod -= fields[(int)FieldName.DexterityPercBonus_NoScore].GetValue() * 0.01f;
                    localCombatStats.Dexterity = (int)(dex * (1 + dex_mod));
                }
            }
        }

        private class ConstitutionBase : CombatStatsField
        {
            public ConstitutionBase()
            {
                fieldValue = 0;
                children = new FieldName[] { FieldName.Constitution, FieldName.CombatScore };
            }
        }

        private class ConstitutionBonus : CombatStatsField
        {
            public ConstitutionBonus()
            {
                fieldValue = 0;
                children = new FieldName[] { FieldName.Constitution };
            }
        }

        private class ConstitutionPercBonus : CombatStatsField
        {
            public ConstitutionPercBonus()
            {
                fieldValue = 0;
                children = new FieldName[] { FieldName.Constitution };
            }
        }

        private class ConstitutionBonus_NoScore : CombatStatsField
        {
            public ConstitutionBonus_NoScore()
            {
                fieldValue = 0;
                children = new FieldName[] { FieldName.Constitution };
            }
        }

        private class ConstitutionPercBonus_NoScore : CombatStatsField
        {
            public ConstitutionPercBonus_NoScore()
            {
                fieldValue = 0;
                children = new FieldName[] { FieldName.Constitution };
            }
        }

        private class Constitution : CombatStatsField
        {
            public Constitution()
            {
                fieldValue = 0;
                children = new FieldName[] { FieldName.CombatScore };
            }

            public override void Compute(CombatStatsField[] fields, LocalCombatStats localCombatStats, ActorSynStats actorSynStats)
            {
                float cons = fields[(int)FieldName.ConstitutionBase].GetValue() + fields[(int)FieldName.ConstitutionBonus].GetValue();
                float cons_mod = fields[(int)FieldName.ConstitutionPercBonus].GetValue() * 0.01f;
                fieldValue = (int)(cons * (1 + cons_mod));

                if (localCombatStats != null)
                {
                    cons -= fields[(int)FieldName.ConstitutionBonus_NoScore].GetValue();
                    cons_mod = fields[(int)FieldName.ConstitutionPercBonus_NoScore].GetValue() * 0.01f;
                    localCombatStats.Constitution = (int)(cons * (1 + cons_mod));
                }
            }
        }

        private class IntelligenceBase : CombatStatsField
        {
            public IntelligenceBase()
            {
                fieldValue = 0;
                children = new FieldName[] { FieldName.Intelligence, FieldName.CombatScore };
            }
        }

        private class IntelligenceBonus : CombatStatsField
        {
            public IntelligenceBonus()
            {
                fieldValue = 0;
                children = new FieldName[] { FieldName.Intelligence };
            }
        }

        private class IntelligencePercBonus : CombatStatsField
        {
            public IntelligencePercBonus()
            {
                fieldValue = 0;
                children = new FieldName[] { FieldName.Intelligence, FieldName.ManaRegen };
            }
        }

        private class IntelligenceBonus_NoScore : CombatStatsField
        {
            public IntelligenceBonus_NoScore()
            {
                fieldValue = 0;
                children = new FieldName[] { FieldName.Intelligence };
            }
        }

        private class IntelligencePercBonus_NoScore : CombatStatsField
        {
            public IntelligencePercBonus_NoScore()
            {
                fieldValue = 0;
                children = new FieldName[] { FieldName.Intelligence };
            }
        }

        private class Intelligence : CombatStatsField
        {
            public Intelligence()
            {
                fieldValue = 0;
                children = new FieldName[] { FieldName.CombatScore, FieldName.ManaRegen };
            }

            public override void Compute(CombatStatsField[] fields, LocalCombatStats localCombatStats, ActorSynStats actorSynStats)
            {
                float intel = fields[(int)FieldName.IntelligenceBase].GetValue() + fields[(int)FieldName.IntelligenceBonus].GetValue();
                float intel_mod = fields[(int)FieldName.IntelligencePercBonus].GetValue() * 0.01f;
                fieldValue = (int)(intel * (1 + intel_mod));

                if (localCombatStats != null)
                {
                    intel -= fields[(int)FieldName.IntelligenceBonus_NoScore].GetValue();
                    intel_mod -= fields[(int)FieldName.IntelligencePercBonus_NoScore].GetValue();
                    localCombatStats.Intelligence = (int)(intel * (1 + intel_mod));
                }
            }
        }

        private class AttackSpeedBase : CombatStatsField
        {
            public AttackSpeedBase()
            {
                fieldValue = 100;
                children = new FieldName[] { };
            }
        }

        private class AttackSpeedBuff : CombatStatsField
        {
            public AttackSpeedBuff()
            {
                fieldValue = 0;
                children = new FieldName[] { FieldName.AttackSpeed };
            }
        }

        private class AttackSpeedDebuff : CombatStatsField
        {
            public AttackSpeedDebuff()
            {
                fieldValue = 0;
                children = new FieldName[] { FieldName.AttackSpeedDebuff };
            }
        }

        private class AttackSpeed : CombatStatsField
        {
            public AttackSpeed()
            {
                fieldValue = 0;
                children = new FieldName[] { };
            }

            public override void Compute(CombatStatsField[] fields, LocalCombatStats localCombatStats, ActorSynStats actorSynStats)
            {
                float agi = fields[(int)FieldName.Agility].GetValue() * 0.01f;
                float buff = fields[(int)FieldName.AttackSpeedBuff].GetValue();
                float debuff = fields[(int)FieldName.AttackSpeedDebuff].GetValue();
                fieldValue = fields[(int)FieldName.AttackSpeedBase].GetValue() + agi + buff - debuff;
                fieldValue = Math.Min(400, Math.Max(fieldValue, 50));

                if (actorSynStats != null)
                {
                    actorSynStats.rtReduction = fieldValue * 0.01f;
                }
            }
        }

        private class CastSpeedBase : CombatStatsField
        {
            public CastSpeedBase()
            {
                fieldValue = 100;
                children = new FieldName[] { FieldName.CastSpeed };
            }
        }

        private class CastSpeedBuff : CombatStatsField
        {
            public CastSpeedBuff()
            {
                fieldValue = 0;
                children = new FieldName[] { FieldName.CastSpeedBuff };
            }
        }

        private class CastSpeedDebuff : CombatStatsField
        {
            public CastSpeedDebuff()
            {
                fieldValue = 0;
                children = new FieldName[] { FieldName.CastSpeedDebuff };
            }
        }

        private class CastSpeed : CombatStatsField
        {
            public CastSpeed()
            {
                fieldValue = 0;
                children = new FieldName[] { };
            }

            public override void Compute(CombatStatsField[] fields, LocalCombatStats localCombatStats, ActorSynStats actorSynStats)
            {
                float dex = fields[(int)FieldName.Dexterity].GetValue() + 0.01f;
                float buff = fields[(int)FieldName.CastSpeedBuff].GetValue();
                float debuff = fields[(int)FieldName.CastSpeedDebuff].GetValue();
                fieldValue = fields[(int)FieldName.CastSpeedBase].GetValue() + dex + buff - debuff;
                fieldValue = Math.Min(1000, Math.Max(100, fieldValue));
            }
        }

        private class ExpBonus : CombatStatsField
        {
            public ExpBonus()
            {
                fieldValue = 0;
                children = new FieldName[] { };
            }
        }

        private class MoveSpeedBase : CombatStatsField
        {
            public MoveSpeedBase()
            {
                fieldValue = 6;
                children = new FieldName[] { FieldName.MoveSpeed };
            }
        }

        private class MoveSpeedBuff : CombatStatsField
        {
            public MoveSpeedBuff()
            {
                fieldValue = 0;
                children = new FieldName[] { FieldName.MoveSpeed };
            }
        }

        private class MoveSpeedDebuff : CombatStatsField
        {
            public MoveSpeedDebuff()
            {
                fieldValue = 0;
                children = new FieldName[] { FieldName.MoveSpeed };
            }
        }

        private class MoveSpeed : CombatStatsField
        {
            public MoveSpeed()
            {
                fieldValue = 0;
                children = new FieldName[] { };
            }

            public override void Compute(CombatStatsField[] fields, LocalCombatStats localCombatStats, ActorSynStats actorSynStats)
            {
                float buff = fields[(int)FieldName.MoveSpeedBuff].GetValue();
                float debuff = fields[(int)FieldName.MoveSpeedDebuff].GetValue();
                fieldValue = fields[(int)FieldName.MoveSpeedBase].GetValue() * (1 + buff * 0.01f - debuff * 0.01f);

                fieldValue = Math.Min(18, Math.Max(0, fieldValue));

                if(actorSynStats != null)
                {
                    actorSynStats.MoveSpeed = fieldValue;
                }
            }
        }

        private class ElementWeapon : CombatStatsField
        {
            public ElementWeapon()
            {
                fieldValue = -1;
                children = new FieldName[] { FieldName.Element };
            }
        }

        private class ElementSideEffect : CombatStatsField
        {
            public ElementSideEffect()
            {
                fieldValue = -1;
                children = new FieldName[] { FieldName.Element };
            }
        }

        private class Element : CombatStatsField
        {
            public Element()
            {
                fieldValue = 0;
                children = new FieldName[] { };
            }

            public override void Compute(CombatStatsField[] fields, LocalCombatStats localCombatStats, ActorSynStats actorSynStats)
            {
                fieldValue = fields[(int)FieldName.ElementSideEffect].GetValue() >= 0 ?
                    fields[(int)FieldName.ElementSideEffect].GetValue() : fields[(int)FieldName.ElementWeapon].GetValue();
            }
        }

        private class HealingRejuvenation : CombatStatsField
        {
            public HealingRejuvenation()
            {
                fieldValue = 0;
                children = new FieldName[] { FieldName.Healing };
            }
        }

        private class HealingPoint : CombatStatsField
        {
            public HealingPoint()
            {
                fieldValue = 0;
                children = new FieldName[] { FieldName.Healing };
            }
        }

        private class HealingEffect : CombatStatsField
        {
            public HealingEffect()
            {
                fieldValue = 0;
                children = new FieldName[] { FieldName.Healing };
            }
        }

        private class HealingBuff : CombatStatsField
        {
            public HealingBuff()
            {
                fieldValue = 0;
                children = new FieldName[] { FieldName.Healing };
            }
        }

        private class HealingDebuff : CombatStatsField
        {
            public HealingDebuff()
            {
                fieldValue = 0;
                children = new FieldName[] { FieldName.Healing };
            }
        }

        private class Healing : CombatStatsField
        {
            public Healing()
            {
                fieldValue = 0;
                children = new FieldName[] { };
            }

            public override void Compute(CombatStatsField[] fields, LocalCombatStats localCombatStats, ActorSynStats actorSynStats)
            {
            }
        }

        private class Health : CombatStatsField
        {
            public Health()
            {
                fieldValue = 0;
                children = new FieldName[] { }; //this is the player current health
            }
        }

        private class HealthMax : CombatStatsField
        {
            public HealthMax()
            {
                fieldValue = 0;
                children = new FieldName[] { FieldName.CombatScore };
            }

            public override void Compute(CombatStatsField[] fields, LocalCombatStats localCombatStats, ActorSynStats actorSynStats)
            {
                if (actorSynStats == null) return;

                float job = fields[(int)FieldName.HealthBase].GetValue();
                float hp = (job * (actorSynStats.Level + fields[(int)FieldName.Constitution].GetValue())) + fields[(int)FieldName.HealthBonus].GetValue();
                float hp_mod = fields[(int)FieldName.HealthPercBonus].GetValue() * 0.01f;
                fieldValue = (int)(hp * (1 + hp_mod));

                if (fields[(int)FieldName.Health].GetValue() > fieldValue)
                    fields[(int)FieldName.Health].SetValue(fieldValue);

                if (localCombatStats != null)
                {
                    hp = (job * (actorSynStats.Level + localCombatStats.Constitution)) + (fields[(int)FieldName.HealthBonus].GetValue() - fields[(int)FieldName.HealthBonus_NoScore].GetValue());
                    hp_mod -= fields[(int)FieldName.HealthPercBonus_NoScore].GetValue() * 0.01f;
                    localCombatStats.HealthMax = (int)(hp * (1 + hp_mod));

                    if (localCombatStats.Health > localCombatStats.HealthMax)
                        localCombatStats.Health = localCombatStats.HealthMax;
                }
            }
        }

        private class HealthBase : CombatStatsField
        {
            public HealthBase()
            {
                fieldValue = 0;
                children = new FieldName[] { FieldName.HealthMax, FieldName.CombatScore };
            }
        }

        private class HealthBonus : CombatStatsField
        {
            public HealthBonus()
            {
                fieldValue = 0;
                children = new FieldName[] { FieldName.HealthMax };
            }
        }

        private class HealthPercentBonus : CombatStatsField
        {
            public HealthPercentBonus()
            {
                fieldValue = 0;
                children = new FieldName[] { FieldName.HealthMax };
            }
        }

        private class HealthRegenBonus : CombatStatsField
        {
            public HealthRegenBonus()
            {
                fieldValue = 0;
                children = new FieldName[] { FieldName.HealthRegen };
            }
        }

        private class HealthRegenPercBonus : CombatStatsField
        {
            public HealthRegenPercBonus()
            {
                fieldValue = 0;
                children = new FieldName[] { FieldName.HealthRegen };
            }
        }

        private class HealthRegenBonus_NoScore : CombatStatsField
        {
            public HealthRegenBonus_NoScore()
            {
                fieldValue = 0;
                children = new FieldName[] { FieldName.HealthRegen };
            }
        }

        private class HealthRegenPercBonus_NoScore : CombatStatsField
        {
            public HealthRegenPercBonus_NoScore()
            {
                fieldValue = 0;
                children = new FieldName[] { FieldName.HealthRegen };
            }
        }

        private class HealthRegen : CombatStatsField
        {
            public HealthRegen()
            {
                fieldValue = 0;
                children = new FieldName[] { };
            }

            public override void Compute(CombatStatsField[] fields, LocalCombatStats localCombatStats, ActorSynStats actorSynStats)
            {
                if (actorSynStats == null) return;
                float x = 15 + (actorSynStats.Level * 0.1f) + (fields[(int)FieldName.Constitution].GetValue() * 3) + fields[(int)FieldName.HealthRegenBonus].GetValue();
                float x_mod = fields[(int)FieldName.HealthRegenPercBonus].GetValue() * 0.01f;
                fieldValue = (int)(x * (1 + x_mod));

                if (localCombatStats != null)
                {
                    x = 15 + (actorSynStats.Level * 0.1f) + (fields[(int)FieldName.Constitution].GetValue() * 3) + fields[(int)FieldName.HealthRegenBonus_NoScore].GetValue();
                    x_mod -= fields[(int)FieldName.HealthRegenPercBonus_NoScore].GetValue() * 0.01f;
                    localCombatStats.HealthRegen = (int)(x * (1 + x_mod));
                }
            }
        }

        private class ManaBase : CombatStatsField
        {
            public ManaBase()
            {
                fieldValue = 0;
                children = new FieldName[] { FieldName.ManaMax, FieldName.CombatScore };
            }
        }

        private class ManaBonus : CombatStatsField
        {
            public ManaBonus()
            {
                fieldValue = 0;
                children = new FieldName[] { FieldName.ManaMax };
            }
        }

        private class ManaPercBonus : CombatStatsField
        {
            public ManaPercBonus()
            {
                fieldValue = 0;
                children = new FieldName[] { FieldName.ManaMax };
            }
        }

        private class ManaBonus_NoScore : CombatStatsField
        {
            public ManaBonus_NoScore()
            {
                fieldValue = 0;
                children = new FieldName[] { FieldName.ManaMax };
            }
        }

        private class ManaPercBonus_NoScore : CombatStatsField
        {
            public ManaPercBonus_NoScore()
            {
                fieldValue = 0;
                children = new FieldName[] { FieldName.ManaMax };
            }
        }

        private class Mana : CombatStatsField
        {
            public Mana()
            {
                fieldValue = 0;
                children = new FieldName[] { }; // current mana
            }
        }

        private class ManaMax : CombatStatsField
        {
            public ManaMax()
            {
                fieldValue = 0;
                children = new FieldName[] { FieldName.CombatScore };
            }

            public override void Compute(CombatStatsField[] fields, LocalCombatStats localCombatStats, ActorSynStats actorSynStats)
            {
                if (actorSynStats == null) return;
                float mana = (fields[(int)FieldName.ManaBase].GetValue() * (actorSynStats.Level + fields[(int)FieldName.Intelligence].GetValue())) + fields[(int)FieldName.ManaBonus].GetValue();
                float mana_mod = fields[(int)FieldName.ManaPercBonus].GetValue() * 0.01f;
                fieldValue = (int)(mana * (1 + mana_mod));

                if (fields[(int)FieldName.Mana].GetValue() > fieldValue)
                    fields[(int)FieldName.Mana].SetValue(fieldValue);

                if (localCombatStats != null)
                {
                    mana = (fields[(int)FieldName.ManaBase].GetValue() * (actorSynStats.Level + fields[(int)FieldName.Intelligence].GetValue())) + (fields[(int)FieldName.ManaBonus].GetValue() - fields[(int)FieldName.ManaBonus_NoScore].GetValue());
                    mana_mod -= fields[(int)FieldName.ManaPercBonus_NoScore].GetValue() * 0.01f;
                    localCombatStats.ManaMax = (int)(mana * (1 + mana_mod));

                    if (localCombatStats.Mana > localCombatStats.ManaMax)
                        localCombatStats.Mana = localCombatStats.ManaMax;
                }
            }
        }


        private class ManaRegenBonus : CombatStatsField
        {
            public ManaRegenBonus()
            {
                fieldValue = 0;
                children = new FieldName[] { FieldName.ManaRegen };
            }
        }

        private class ManaRegenPercBonus : CombatStatsField
        {
            public ManaRegenPercBonus()
            {
                fieldValue = 0;
                children = new FieldName[] { FieldName.ManaRegen };
            }
        }

        private class ManaRegenBonus_NoScoe : CombatStatsField
        {
            public ManaRegenBonus_NoScoe()
            {
                fieldValue = 0;
                children = new FieldName[] { FieldName.ManaRegen };
            }
        }

        private class ManaRegenPercBonus_NoScore : CombatStatsField
        {
            public ManaRegenPercBonus_NoScore()
            {
                fieldValue = 0;
                children = new FieldName[] { FieldName.ManaRegen };
            }
        }

        private class ManaRegen : CombatStatsField
        {
            public ManaRegen()
            {
                fieldValue = 0;
                children = new FieldName[] { FieldName.ManaRegen };
            }

            public override void Compute(CombatStatsField[] fields, LocalCombatStats localCombatStats, ActorSynStats actorSynStats)
            {
                if (actorSynStats == null) return;
                float x = 4 + (actorSynStats.Level * 0.1f) + (fields[(int)FieldName.Intelligence].GetValue() / 5) + fields[(int)FieldName.ManaRegenBonus].GetValue();
                float x_mod = fields[(int)FieldName.ManaRegenPercBonus].GetValue() * 0.01f;
                fieldValue = (int)(x * (1 + x_mod));

                if (localCombatStats != null)
                {
                    x = 4 + (actorSynStats.Level * 0.1f) + (fields[(int)FieldName.Intelligence].GetValue() / 5) + fields[(int)FieldName.ManaRegenBonus_NoScore].GetValue();
                    x_mod = fields[(int)FieldName.ManaRegenPercBonus_NoScore].GetValue() * 0.01f;
                    localCombatStats.ManaRegen = (int)(x * (1 + x_mod));
                }
            }
        }

        private class ManaReduceBonus : CombatStatsField
        {
            public ManaReduceBonus()
            {
                fieldValue = 0;
                children = new FieldName[] {  };
            }

            public override void Compute(CombatStatsField[] fields, LocalCombatStats localCombatStats, ActorSynStats actorSynStats)
            {
                
                if (localCombatStats != null)
                    localCombatStats.ManaReduceBonus = (int)fieldValue;
            }
        }

        private class ManaReducePercBonus : CombatStatsField
        {
            public ManaReducePercBonus()
            {
                fieldValue = 0;
                children = new FieldName[] { };
            }

            public override void Compute(CombatStatsField[] fields, LocalCombatStats localCombatStats, ActorSynStats actorSynStats)
            {
                if (localCombatStats != null)
                    localCombatStats.ManaReducePercBonus = (int)fieldValue;
            }
        }

        private class EnergyShield : CombatStatsField
        {
            public EnergyShield()
            {
                fieldValue = 0;
                children = new FieldName[] { };
            }

            public override void Compute(CombatStatsField[] fields, LocalCombatStats localCombatStats, ActorSynStats actorSynStats)
            {
                if (actorSynStats != null)
                    actorSynStats.PassiveShieldBuff = (int)fieldValue;
            }
        }

        private class WeaponAttackBase : CombatStatsField
        {
            public WeaponAttackBase()
            {
                fieldValue = 0;
                children = new FieldName[] { FieldName.WeaponAttack, FieldName.CombatScore };
            }
        }

        private class WeaponAttackBonus : CombatStatsField
        {
            public WeaponAttackBonus()
            {
                fieldValue = 0;
                children = new FieldName[] { FieldName.WeaponAttack };
            }
        }

        private class WeaponAttackPercBonus : CombatStatsField
        {
            public WeaponAttackPercBonus()
            {
                fieldValue = 0;
                children = new FieldName[] { FieldName.WeaponAttack };
            }
        }

        private class WeaponAttack : CombatStatsField
        {
            public WeaponAttack()
            {
                fieldValue = 0;
                children = new FieldName[] { FieldName.CombatScore };
            }

            public override void Compute(CombatStatsField[] fields, LocalCombatStats localCombatStats, ActorSynStats actorSynStats)
            {
                float wdmg = fields[(int)FieldName.WeaponAttackBase].GetValue() + fields[(int)FieldName.WeaponAttackBonus].GetValue();
                float wdmg_mod = fields[(int)FieldName.WeaponAttackPercBonus].GetValue() * 0.01f;
                fieldValue = (int)((1 + wdmg) * (1 + wdmg_mod));

                if (localCombatStats != null)
                {
                    //wdmg -= fields[(int)FieldName.WeaponAttackBonus_NoScore].GetValue();
                    //wdmg_mod -= fields[(int)FieldName.WeaponAttackPercBonus_NoScore].GetValue() * 0.01f;
                    localCombatStats.WeaponAttack = (int)((1 + wdmg) * (1 + wdmg_mod));
                }
            }
        }

        //-----------------------------Attack----------------------------------------

        private class AttackBonus : CombatStatsField
        {
            public AttackBonus()
            {
                fieldValue = 0;
                children = new FieldName[] { FieldName.Attack };
            }
        }

        private class AttackPercentBonus : CombatStatsField
        {
            public AttackPercentBonus()
            {
                children = new FieldName[] { FieldName.Attack };
            }
        }

        private class Attack : CombatStatsField
        {
            public Attack()
            {
                fieldValue = 0;
                children = new FieldName[] { FieldName.CombatScore };
            }

            public override void Compute(CombatStatsField[] fields, LocalCombatStats localCombatStats, ActorSynStats actorSynStats)
            {
                float atk = fields[(int)FieldName.AttackBase].GetValue() + fields[(int)FieldName.AttackBonus].GetValue();
                float atk_mod = fields[(int)FieldName.AttackPercBonus].GetValue() * 0.01f;
                fieldValue = (atk * (1 + atk_mod));

                if (localCombatStats != null)
                {
                    atk -= fields[(int)FieldName.AttackBonus_NoScore].GetValue();
                    atk_mod -= fields[(int)FieldName.AttackPercBonus_NoScore].GetValue() * 0.01f;
                    localCombatStats.AttackPower = (int)(atk * (1 + atk_mod));
                }
            }
        }

        private class AttackBase : CombatStatsField
        {
            public AttackBase()
            {
                fieldValue = 10;
                children = new FieldName[] { FieldName.Attack, FieldName.CombatScore };
            }
        }

        //-----------------------------Armor----------------------------------------
        private class ArmorBase : CombatStatsField
        {
            public ArmorBase()
            {
                children = new FieldName[] { FieldName.Armor, FieldName.CombatScore };
            }

            public override void Compute(CombatStatsField[] fields, LocalCombatStats localCombatStats, ActorSynStats actorSynStats)
            {
                fieldValue = fields[(int)FieldName.ArmorItem].GetValue() + fields[(int)FieldName.ArmorBonus].GetValue();
            }
        }

        private class ArmorItem : CombatStatsField
        {
            public ArmorItem()
            {
                fieldValue = 0;
                children = new FieldName[] { FieldName.ArmorBase };
            }
        }

        private class ArmorBonus : CombatStatsField
        {
            public ArmorBonus()
            {
                fieldValue = 0;
                children = new FieldName[] { FieldName.ArmorBase };
            }
        }

        private class ArmorPercBonus : CombatStatsField
        {
            public ArmorPercBonus()
            {
                fieldValue = 0;
                children = new FieldName[] { FieldName.Armor };
            }
        }

        private class Armor : CombatStatsField
        {
            public Armor()
            {
                children = new FieldName[] { FieldName.IgnoreArmor, FieldName.CombatScore };
            }

            public override void Compute(CombatStatsField[] fields, LocalCombatStats localCombatStats, ActorSynStats actorSynStats)
            {
                float def = fields[(int)FieldName.ArmorBase].GetValue();
                float def_mod = (int)fields[(int)FieldName.ArmorPercBonus].GetValue() * 0.01f;
                fieldValue = (int)((def * (1 + def_mod)) * (1 - fields[(int)FieldName.IgnoreArmorBase].GetValue() * 0.01f));

                if (localCombatStats != null)
                {
                    def -= fields[(int)FieldName.ArmorBonus_NoScore].GetValue();
                    def_mod -= fields[(int)FieldName.ArmorPercBonus_NoScore].GetValue() * 0.01f;
                    localCombatStats.Armor = (int)((def * (1 + def_mod)));
                }
            }
        }

        private class IgnoreArmorBonus : CombatStatsField
        {
            public IgnoreArmorBonus()
            {
                fieldValue = 0;
                children = new FieldName[] { FieldName.IgnoreArmor };
            }
        }

        private class IgnoreArmorBase : CombatStatsField
        {
            public IgnoreArmorBase()
            {
                fieldValue = 0;
                children = new FieldName[] { FieldName.Armor };
            }

            public override void Compute(CombatStatsField[] fields, LocalCombatStats localCombatStats, ActorSynStats actorSynStats)
            {
                fieldValue = (int)((fields[(int)FieldName.Strength].GetValue() * 0.05f) + (fields[(int)FieldName.IgnoreArmorBonus].GetValue() * 0.01f));

                if (localCombatStats != null)
                {
                    localCombatStats.IgnoreArmor = (int)fieldValue;//(int)(((int)fields[(int)FieldName.Strength].GetValue() * 0.05f) + ((int)fields[(int)FieldName.IgnoreArmorBonus].GetValue() - (int)fields[(int)FieldName.IgnoreArmorBonus_No].GetValue()) * 100);
                }
            }
        }

        private class IgnoreArmor : CombatStatsField
        {
            public IgnoreArmor()
            {
                fieldValue = 0;
                children = new FieldName[] { };
            }

            public override void Compute(CombatStatsField[] fields, LocalCombatStats localCombatStats, ActorSynStats actorSynStats)
            {
                float regress = 4000 / (4000 + (10 * fields[(int)FieldName.Armor].GetValue()));
                fieldValue = Math.Min(Math.Max(regress, 0.05f), 1.0f);
            }
        }

        private class BlockRate : CombatStatsField
        {
            public BlockRate()
            {
                fieldValue = 0;
                children = new FieldName[] { };
            }
        }

        private class BlockValueBonus : CombatStatsField
        {
            public BlockValueBonus()
            {
                fieldValue = 0;
                children = new FieldName[] { FieldName.BlockValue };
            }
        }

        private class BlockValuePercBonus : CombatStatsField
        {
            public BlockValuePercBonus()
            {
                fieldValue = 0;
                children = new FieldName[] { FieldName.BlockValue };
            }
        }

        private class BlockValueBonus_NoScore : CombatStatsField
        {
            public BlockValueBonus_NoScore()
            {
                fieldValue = 0;
                children = new FieldName[] { FieldName.BlockValue };
            }
        }

        private class BlockValuePercBonus_NoScore : CombatStatsField
        {
            public BlockValuePercBonus_NoScore()
            {
                fieldValue = 0;
                children = new FieldName[] { FieldName.BlockValue };
            }
        }

        private class BlockValue : CombatStatsField
        {
            public BlockValue()
            {
                fieldValue = 0;
                children = new FieldName[] { FieldName.AbsorbDamage };
            }

            public override void Compute(CombatStatsField[] fields, LocalCombatStats localCombatStats, ActorSynStats actorSynStats)
            {
                float blockval = fields[(int)FieldName.BlockValueBonus].GetValue();
                float blockval_mod = fields[(int)FieldName.BlockValuePercBonus].GetValue() * 0.01f;
                fieldValue = (int)(blockval * (1 + blockval_mod));

                if (localCombatStats != null)
                {
                    blockval -= (int)fields[(int)FieldName.BlockValueBonus_NoScore].GetValue();
                    blockval_mod -= (int)fields[(int)FieldName.BlockValuePercBonus_NoScore].GetValue() * 0.01f;
                    localCombatStats.Block = (int)(blockval * (1 + blockval_mod));
                }
            }
        }

        private class AbsorbDamageField : CombatStatsField
        {
            public AbsorbDamageField()
            {
                fieldValue = 0;
                children = new FieldName[] { };
            }

            public override void Compute(CombatStatsField[] fields, LocalCombatStats localCombatStats, ActorSynStats actorSynStats)
            {
                //base.Compute(fields, localCombatStats, playerSynStats);
                float regress = 4000 / (4000 + (10 * fields[(int)FieldName.BlockValue].GetValue()));
                fieldValue = Math.Max(Math.Min(regress, 1), 0.05f);
                //if (localCombatStats != null)
                //    localCombatStats.AbsorbDamage = fieldValue;
            }
        }

        //--------------------
        // Unused
        private class AccuracyBase : CombatStatsField
        {
            public AccuracyBase()
            {
                children = new FieldName[] { FieldName.Accuracy, FieldName.CombatScore };
            }
        }

        private class AccuracyBonus : CombatStatsField
        {
            public AccuracyBonus()
            {
                children = new FieldName[] { FieldName.Accuracy };
            }
        }

        private class AccuracyPercBonus : CombatStatsField
        {
            public AccuracyPercBonus()
            {
                children = new FieldName[] { FieldName.Accuracy };
            }
        }

        private class AccuracyBonus_NoScore : CombatStatsField
        {
            public AccuracyBonus_NoScore()
            {
                fieldValue = 0;
                children = new FieldName[] { FieldName.Accuracy };
            }
        }

        private class AccuracyPercBonus_NoScore : CombatStatsField
        {
            public AccuracyPercBonus_NoScore()
            {
                fieldValue = 0;
                children = new FieldName[] { FieldName.Accuracy };
            }
        }

        private class Accuracy : CombatStatsField
        {
            public Accuracy()
            {
                children = new FieldName[] { FieldName.CombatScore };
            }

            public override void Compute(CombatStatsField[] fields, LocalCombatStats localCombatStats, ActorSynStats actorSynStats)
            {
                float accu = fields[(int)FieldName.AccuracyBonus].GetValue();
                float accu_mod = fields[(int)FieldName.AccuracyPercBonus].GetValue() * 0.01f;
                if (actorSynStats != null)
                    fieldValue = (int)((80 + accu + fields[(int)FieldName.Dexterity].GetValue() + actorSynStats.Level/*+ skill*/) * (1 + accu_mod));
                else
                    fieldValue = (int)fields[(int)FieldName.AccuracyBase].GetValue();  // For monster use
                if (localCombatStats != null)
                {
                    accu -= fields[(int)FieldName.AccuracyBonus_NoScore].GetValue();
                    accu_mod -= fields[(int)FieldName.AccuracyPercBonus_NoScore].GetValue() * 0.01f;
                    localCombatStats.Accuracy = (int)((80 + accu + fields[(int)FieldName.Dexterity].GetValue() + actorSynStats.Level/*+ skill*/) * (1 + accu_mod));
                }
            }
        }

        //--------------------
        private class EvasionBase : CombatStatsField
        {
            public EvasionBase()
            {
                children = new FieldName[] { FieldName.Evasion, FieldName.CombatScore };
            }
        }

        private class EvasionBonus : CombatStatsField
        {
            public EvasionBonus()
            {
                children = new FieldName[] { FieldName.Evasion };
            }
        }

        private class EvasionPercBonus : CombatStatsField
        {
            public EvasionPercBonus()
            {
                children = new FieldName[] { FieldName.Evasion };
            }
        }

        private class Evasion : CombatStatsField
        {
            public Evasion()
            {
                children = new FieldName[] { FieldName.CombatScore };
            }

            public override void Compute(CombatStatsField[] fields, LocalCombatStats localCombatStats, ActorSynStats actorSynStats)
            {
                float eva = fields[(int)FieldName.EvasionBonus].GetValue();
                float eva_mod = fields[(int)FieldName.EvasionPercBonus].GetValue() * 0.01f;
                if (actorSynStats != null)
                {
                    fieldValue = (int)((20 + eva + fields[(int)FieldName.Agility].GetValue() + actorSynStats.Level) * (1 + eva_mod));
                }
                else
                {
                    fieldValue = (int)fields[(int)FieldName.EvasionBase].GetValue();
                }
                if (localCombatStats != null)
                {
                    eva -= fields[(int)FieldName.EvasionBonus_NoScore].GetValue();
                    eva_mod = fields[(int)FieldName.EvasionPercBonus_NoScore].GetValue();
                    localCombatStats.Evasion = (int)((20 + eva + fields[(int)FieldName.Agility].GetValue() + actorSynStats.Level) * (1 + eva_mod));
                }
            }
        }

        //--------------------
        // Unused
        private class CriticalBase : CombatStatsField
        {
            public CriticalBase()
            {
                children = new FieldName[] { FieldName.Critical, FieldName.CombatScore };
            }
        }

        private class CriticalBonus : CombatStatsField
        {
            public CriticalBonus()
            {
                children = new FieldName[] { FieldName.Critical };
            }
        }

        private class CriticalPercBonus : CombatStatsField
        {
            public CriticalPercBonus()
            {
                children = new FieldName[] { FieldName.Critical };
            }
        }

        private class Critical : CombatStatsField
        {
            public Critical()
            {
                children = new FieldName[] { FieldName.CombatScore };
            }

            public override void Compute(CombatStatsField[] fields, LocalCombatStats localCombatStats, ActorSynStats actorSynStats)
            {
                float crit = fields[(int)FieldName.CriticalBonus].GetValue();
                float crit_mod = fields[(int)FieldName.CriticalPercBonus].GetValue() * 0.01f;
                fieldValue = (int)((1 + crit) * (1 + crit_mod));

                if (localCombatStats != null)
                {
                    crit -= fields[(int)FieldName.CriticalBonus_NoScore].GetValue();
                    crit_mod -= fields[(int)FieldName.CriticalPercBonus_NoScore].GetValue() * 0.01f;
                    localCombatStats.Critical = (int)((1 + crit) * (1 + crit_mod));
                }
            }
        }

        //--------------------
        // unused
        private class CocriticalBase : CombatStatsField
        {
            public CocriticalBase()
            {
                children = new FieldName[] { FieldName.Cocritical, FieldName.CombatScore };
            }
        }

        private class CocriticalBonus : CombatStatsField
        {
            public CocriticalBonus()
            {
                children = new FieldName[] { FieldName.Cocritical };
            }
        }

        private class CocriticalPercBonus : CombatStatsField
        {
            public CocriticalPercBonus()
            {
                children = new FieldName[] { FieldName.Cocritical };
            }
        }

        private class Cocritical : CombatStatsField
        {
            public Cocritical()
            {
                children = new FieldName[] { FieldName.CombatScore };
            }

            public override void Compute(CombatStatsField[] fields, LocalCombatStats localCombatStats, ActorSynStats actorSynStats)
            {
                float cocrit = fields[(int)FieldName.CocriticalBonus].GetValue();
                float cocrit_mod = fields[(int)FieldName.CocriticalPercBonus].GetValue() * 0.01f;
                fieldValue = (int)((1 + cocrit) * (1 + cocrit_mod));

                if (localCombatStats != null)
                {
                    cocrit -= fields[(int)FieldName.CocriticalBonus_NoScore].GetValue();
                    cocrit_mod -= fields[(int)FieldName.CocriticalPercBonus_NoScore].GetValue() * 0.01f;
                    localCombatStats.CoCritical = (int)((1 + cocrit) * (1 + cocrit_mod));
                }
            }
        }

        //--------------------
        // Unused
        private class CriticalDamageBase : CombatStatsField
        {
            public CriticalDamageBase()
            {
                children = new FieldName[] { FieldName.CriticalDamage, FieldName.CombatScore };
            }
        }

        private class CriticalDamageBonus : CombatStatsField
        {
            public CriticalDamageBonus()
            {
                children = new FieldName[] { FieldName.CriticalDamage, FieldName.CombatScore };
            }
        }

        // Unused
        private class CriticalDamagePercBonus : CombatStatsField
        {
            public CriticalDamagePercBonus()
            {
                children = new FieldName[] { FieldName.CriticalDamage, FieldName.CombatScore };
            }
        }

        private class CriticalDamage : CombatStatsField
        {
            public CriticalDamage()
            {
                children = new FieldName[] { FieldName.CombatScore };
            }

            public override void Compute(CombatStatsField[] fields, LocalCombatStats localCombatStats, ActorSynStats actorSynStats)
            {
                fieldValue = (int)fields[(int)FieldName.CriticalDamageBonus].GetValue();
                if (localCombatStats != null)
                {
                    localCombatStats.CriticalDamage = (int)(fieldValue - fields[(int)FieldName.CriticalDamagePercBonus_NoScore].GetValue());
                }
            }
        }

        private class SmashDamageBonus : CombatStatsField
        {
            public SmashDamageBonus()
            {
                fieldValue = 1;
                children = new FieldName[] { FieldName.SmashDamage };
            }
        }

        private class SmashDamagePercBonus : CombatStatsField
        {
            public SmashDamagePercBonus()
            {
                fieldValue = 0;
                children = new FieldName[] { FieldName.SmashDamage };
            }
        }

        private class SmashDamage : CombatStatsField
        {
            public SmashDamage()
            {
                fieldValue = 0;
                children = new FieldName[] { };
            }

            public override void Compute(CombatStatsField[] fields, LocalCombatStats localCombatStats, ActorSynStats actorSynStats)
            {
                float dmg = fields[(int)FieldName.SmashDamageBonus].GetValue();
                float dmg_mod = fields[(int)FieldName.SmashDamagePercBonus].GetValue() * 0.01f;
                fieldValue = dmg * ((1 + dmg_mod) / 4000);

                if (localCombatStats != null)
                {
                    dmg -= fields[(int)FieldName.SmashDamageBonus_NoScore].GetValue();
                    dmg_mod -= fields[(int)FieldName.SmashDamagePercBonus_NoScore].GetValue() * 0.01f;
                    localCombatStats.SmashDamage = (int)(dmg * ((1 + dmg) / 4000));
                }
            }
        }

        private class SliceDamageBonus : CombatStatsField
        {
            public SliceDamageBonus()
            {
                fieldValue = 1;
                children = new FieldName[] { FieldName.SliceDamage };
            }
        }

        private class SliceDamagePercBonus : CombatStatsField
        {
            public SliceDamagePercBonus()
            {
                fieldValue = 0;
                children = new FieldName[] { FieldName.SliceDamage };
            }
        }

        private class SliceDamage : CombatStatsField
        {
            public SliceDamage()
            {
                fieldValue = 0;
                children = new FieldName[] { };
            }

            public override void Compute(CombatStatsField[] fields, LocalCombatStats localCombatStats, ActorSynStats actorSynStats)
            {
                float dmg = fields[(int)FieldName.SliceDamageBonus].GetValue();
                float dmg_mod = fields[(int)FieldName.SliceDamagePercBonus].GetValue() * 0.01f;
                fieldValue = dmg * ((1 + dmg_mod) / 4000);

                if (localCombatStats != null)
                {
                    dmg -= fields[(int)FieldName.SliceDamageBonus_NoScore].GetValue();
                    dmg_mod = fields[(int)FieldName.SliceDamagePercBonus_NoScore].GetValue();
                    localCombatStats.SliceDamage = (int)(dmg * ((1 + dmg_mod) / 4000));
                }
            }
        }

        private class PierceDamageBonus : CombatStatsField
        {
            public PierceDamageBonus()
            {
                fieldValue = 1;
                children = new FieldName[] { FieldName.PierceDamage };
            }
        }

        private class PierceDamagePercBonus : CombatStatsField
        {
            public PierceDamagePercBonus()
            {
                fieldValue = 0;
                children = new FieldName[] { FieldName.PierceDamage };
            }
        }

        private class PierceDamage : CombatStatsField
        {
            public PierceDamage()
            {
                fieldValue = 0;
                children = new FieldName[] { };
            }

            public override void Compute(CombatStatsField[] fields, LocalCombatStats localCombatStats, ActorSynStats actorSynStats)
            {
                float dmg = fields[(int)FieldName.PierceDamageBonus].GetValue();
                float dmg_mod = fields[(int)FieldName.PierceDamagePercBonus].GetValue() * 0.01f;
                fieldValue = dmg * ((1 + dmg_mod) / 4000);

                if (localCombatStats != null)
                {
                    dmg -= fields[(int)FieldName.PierceDamageBonus_NoScore].GetValue();
                    dmg_mod -= fields[(int)FieldName.PierceDamagePercBonus_NoScore].GetValue() * 0.01f;
                    localCombatStats.PierceDamage = (int)(dmg * ((1 + dmg_mod) / 4000));
                }
            }
        }

        private class SmashDefenseBonus : CombatStatsField
        {
            public SmashDefenseBonus()
            {
                fieldValue = 1;
                children = new FieldName[] { FieldName.SmashDefense };
            }
        }

        private class SmashDefensePercBonus : CombatStatsField
        {
            public SmashDefensePercBonus()
            {
                fieldValue = 0;
                children = new FieldName[] { FieldName.SmashDefense };
            }
        }

        private class SmashDefense : CombatStatsField
        {
            public SmashDefense()
            {
                fieldValue = 0;
                children = new FieldName[] { };
            }

            public override void Compute(CombatStatsField[] fields, LocalCombatStats localCombatStats, ActorSynStats actorSynStats)
            {
                float def = fields[(int)FieldName.SmashDefenseBonus].GetValue();
                float def_mod = fields[(int)FieldName.SmashDefensePercBonus].GetValue() * 0.01f;
                float total = def * (1 + def_mod);
                float regress = 4000 / (4000 + 10 * total);
                fieldValue = Math.Max(Math.Min(regress, 1), 0.05f);

                if (localCombatStats != null)
                {
                    def -= fields[(int)FieldName.SmashDefenseBonus_NoScore].GetValue();
                    def_mod -= fields[(int)FieldName.SmashDefensePercBonus_NoScore].GetValue() * 0.01f;
                    total = def * (1 + def_mod);
                    regress = 4000 / (4000 + 10 * total);
                    localCombatStats.SmashDefence = (int)(Math.Max(Math.Min(regress, 1), 0.05f) * 100);
                }
            }
        }

        private class SliceDefenseBonus : CombatStatsField
        {
            public SliceDefenseBonus()
            {
                fieldValue = 1;
                children = new FieldName[] { FieldName.SliceDefense };
            }
        }

        private class SliceDefensePercBonus : CombatStatsField
        {
            public SliceDefensePercBonus()
            {
                fieldValue = 0;
                children = new FieldName[] { FieldName.SliceDefense };
            }
        }

        private class SliceDefense : CombatStatsField
        {
            public SliceDefense()
            {
                fieldValue = 0;
                children = new FieldName[] { };
            }

            public override void Compute(CombatStatsField[] fields, LocalCombatStats localCombatStats, ActorSynStats actorSynStats)
            {
                float def = fields[(int)FieldName.SliceDefenseBonus].GetValue();
                float def_mod = fields[(int)FieldName.SliceDefensePercBonus].GetValue() * 0.01f;
                float total = def * (1 + def_mod);
                float regress = 4000 / (4000 + 10 * total);
                fieldValue = Math.Max(Math.Min(regress, 1), 0.05f);

                if (localCombatStats != null)
                {
                    //def -= fields[(int)FieldName.SliceDefenseBonus_NoScore].GetValue();
                    //def_mod = fields[(int)FieldName.SliceDefensePercBonus_NoScore].GetValue() * 0.01f;
                    total = def * (1 + def_mod);
                    regress = 4000 / (4000 + 10 * total);
                    localCombatStats.SliceDefence = (int)(Math.Max(Math.Min(regress, 1), 0.05f) * 100);
                }
            }
        }

        private class PierceDefenseBonus : CombatStatsField
        {
            public PierceDefenseBonus()
            {
                fieldValue = 1;
                children = new FieldName[] { FieldName.PierceDefense };
            }
        }

        private class PierceDefensePercBonus : CombatStatsField
        {
            public PierceDefensePercBonus()
            {
                fieldValue = 0;
                children = new FieldName[] { FieldName.PierceDefense };
            }
        }

        private class PierceDefense : CombatStatsField
        {
            public PierceDefense()
            {
                fieldValue = 0;
                children = new FieldName[] { };
            }

            public override void Compute(CombatStatsField[] fields, LocalCombatStats localCombatStats, ActorSynStats actorSynStats)
            {
                float def = fields[(int)FieldName.PierceDefenseBonus].GetValue();
                float def_mod = fields[(int)FieldName.PierceDefensePercBonus].GetValue() * 0.01f;
                float total = def * (1 + def_mod);
                float regress = 4000 / (4000 + 10 * total);
                fieldValue = Math.Max(Math.Min(regress, 1.0f), 0.05f);

                if (localCombatStats != null)
                {
                    def -= (int)fields[(int)FieldName.PierceDefenseBonus_NoScore].GetValue();
                    def_mod -= (int)fields[(int)FieldName.PierceDefensePercBonus_NoScore].GetValue();
                    total = def * (1 + def_mod);
                    regress = 4000 / (4000 + 10 * total);
                    localCombatStats.PierceDefence = (int)(Math.Max(Math.Min(regress, 1), 0.05f) * 100);
                }
            }
        }

        private class NullDamageBonus : CombatStatsField
        {
            public NullDamageBonus()
            {
                fieldValue = 0;
                children = new FieldName[] { FieldName.NullDamage };
            }
        }

        private class NullDamagePercBonus : CombatStatsField
        {
            public NullDamagePercBonus()
            {
                fieldValue = 0;
                children = new FieldName[] { FieldName.NullDamage };
            }
        }

        private class NullDamage : CombatStatsField
        {
            public NullDamage()
            {
                fieldValue = 0;
                children = new FieldName[] { };
            }

            public override void Compute(CombatStatsField[] fields, LocalCombatStats localCombatStats, ActorSynStats actorSynStats)
            {
                float x = fields[(int)FieldName.NullDamageBonus].GetValue();
                float x_mod = fields[(int)FieldName.NullDamagePercBonus].GetValue() * 0.01f;
                fieldValue = x * ((1 + x_mod) / 4000);

                if (localCombatStats != null)
                {
                    //x = (int)fields[(int)FieldName.NullDamageBonus_NoScore].GetValue();
                    //x_mod = (int)fields[(int)FieldName.NullDamagePercBonus_NoScore].GetValue();
                    localCombatStats.IncElemNoneDamage = (int)(x * ((1 + x_mod) / 4000));
                }
            }
        }

        private class MetalDamageBonus : CombatStatsField
        {
            public MetalDamageBonus()
            {
                fieldValue = 1;
                children = new FieldName[] { FieldName.MetalDamage };
            }
        }

        private class MetalDamagePercBonus : CombatStatsField
        {
            public MetalDamagePercBonus()
            {
                fieldValue = 0;
                children = new FieldName[] { FieldName.MetalDamage };
            }
        }

        private class MetalDamage : CombatStatsField
        {
            public MetalDamage()
            {
                fieldValue = 0;
                children = new FieldName[] { };
            }

            public override void Compute(CombatStatsField[] fields, LocalCombatStats localCombatStats, ActorSynStats actorSynStats)
            {
                float x = fields[(int)FieldName.MetalDamageBonus].GetValue();
                float x_mod = fields[(int)FieldName.MetalDamagePercBonus].GetValue() * 0.01f;
                fieldValue = x * ((1 + x_mod) / 4000);

                if (localCombatStats != null)
                {
                    //x = (int)fields[(int)FieldName.MetalDamageBonus_NoScore].GetValue();
                    //x_mod = (int)fields[(int)FieldName.MetalDamagePercBonus_NoScore].GetValue();
                    localCombatStats.IncElemMetalDamage = (int)(x * ((1 + x_mod) / 4000));
                }
            }
        }

        private class WoodDamageBonus : CombatStatsField
        {
            public WoodDamageBonus()
            {
                fieldValue = 1;
                children = new FieldName[] { FieldName.WoodDamage };
            }
        }

        private class WoodDamagePercBonus : CombatStatsField
        {
            public WoodDamagePercBonus()
            {
                fieldValue = 0;
                children = new FieldName[] { FieldName.WoodDamage };
            }
        }

        private class WoodDamage : CombatStatsField
        {
            public WoodDamage()
            {
                fieldValue = 0;
                children = new FieldName[] { };
            }

            public override void Compute(CombatStatsField[] fields, LocalCombatStats localCombatStats, ActorSynStats actorSynStats)
            {
                float x = fields[(int)FieldName.WoodDamageBonus].GetValue();
                float x_mod = fields[(int)FieldName.WoodDamagePercBonus].GetValue() * 0.01f;
                fieldValue = x * ((1 + x_mod) / 4000);

                if (localCombatStats != null)
                {
                    //x = (int)fields[(int)FieldName.WoodDamageBonus_NoScore].GetValue();
                    //x_mod = (int)fields[(int)FieldName.WoodDamagePercBonus_NoScore].GetValue();
                    localCombatStats.IncElemWoodDamage = (int)(x * ((1 + x_mod) / 4000));
                }
            }
        }

        private class EarthDamageBonus : CombatStatsField
        {
            public EarthDamageBonus()
            {
                fieldValue = 1;
                children = new FieldName[] { FieldName.EarthDamage };
            }
        }

        private class EarthDamagePercBonus : CombatStatsField
        {
            public EarthDamagePercBonus()
            {
                fieldValue = 0;
                children = new FieldName[] { FieldName.EarthDamage };
            }
        }

        private class EarthDamage : CombatStatsField
        {
            public EarthDamage()
            {
                fieldValue = 0;
                children = new FieldName[] { };
            }

            public override void Compute(CombatStatsField[] fields, LocalCombatStats localCombatStats, ActorSynStats actorSynStats)
            {
                float x = fields[(int)FieldName.EarthDamageBonus].GetValue();
                float x_mod = fields[(int)FieldName.EarthDamagePercBonus].GetValue() * 0.01f;
                fieldValue = x * ((1 + x_mod) / 4000);

                if (localCombatStats != null)
                {
                    //x = (int)fields[(int)FieldName.EarthDamageBonus_NoScore].GetValue();
                    //x_mod = (int)fields[(int)FieldName.EarthDamagePercBonus_NoScore].GetValue();
                    localCombatStats.IncElemEarthDamage = (int)(x * ((1 + x_mod) / 4000));
                }
            }
        }

        private class WaterDamageBonus : CombatStatsField
        {
            public WaterDamageBonus()
            {
                fieldValue = 1;
                children = new FieldName[] { FieldName.WaterDamage };
            }
        }

        private class WaterDamagePercBonus : CombatStatsField
        {
            public WaterDamagePercBonus()
            {
                fieldValue = 0;
                children = new FieldName[] { FieldName.WaterDamage };
            }
        }

        private class WaterDamage : CombatStatsField
        {
            public WaterDamage()
            {
                fieldValue = 0;
                children = new FieldName[] { };
            }

            public override void Compute(CombatStatsField[] fields, LocalCombatStats localCombatStats, ActorSynStats actorSynStats)
            {
                float x = fields[(int)FieldName.WaterDamageBonus].GetValue();
                float x_mod = fields[(int)FieldName.WaterDamagePercBonus].GetValue() * 0.01f;
                fieldValue = x * ((1 + x_mod) / 4000);

                if (localCombatStats != null)
                {
                    //x = (int)fields[(int)FieldName.WaterDamageBonus_NoScore].GetValue();
                    //x_mod = (int)fields[(int)FieldName.WaterDamagePercBonus_NoScore].GetValue();
                    localCombatStats.IncElemWaterDamage = (int)(x * ((1 + x_mod) / 4000));
                }
            }
        }

        private class FireDamageBonus : CombatStatsField
        {
            public FireDamageBonus()
            {
                fieldValue = 1;
                children = new FieldName[] { FieldName.FireDamage };
            }
        }

        private class FireDamagePercBonus : CombatStatsField
        {
            public FireDamagePercBonus()
            {
                fieldValue = 0;
                children = new FieldName[] { FieldName.FireDamage };
            }
        }

        private class FireDamage : CombatStatsField
        {
            public FireDamage()
            {
                fieldValue = 0;
                children = new FieldName[] { };
            }

            public override void Compute(CombatStatsField[] fields, LocalCombatStats localCombatStats, ActorSynStats actorSynStats)
            {
                float x = fields[(int)FieldName.FireDamageBonus].GetValue();
                float x_mod = fields[(int)FieldName.FireDamagePercBonus].GetValue() * 0.01f;
                fieldValue = x * ((1.0f + x_mod) / 4000.0f);

                if (localCombatStats != null)
                {
                    //x = (int)fields[(int)FieldName.FireDamageBonus_NoScore].GetValue();
                    //x_mod = (int)fields[(int)FieldName.FireDamagePercBonus_NoScore].GetValue();
                    localCombatStats.IncElemFireDamage = (int)(x * ((1 + x_mod) / 4000));
                }
            }
        }

        private class NullDefenseBonus : CombatStatsField
        {
            public NullDefenseBonus()
            {
                fieldValue = 1;
                children = new FieldName[] { FieldName.NullDefense };
            }
        }

        private class NullDefensePercBonus : CombatStatsField
        {
            public NullDefensePercBonus()
            {
                fieldValue = 0;
                children = new FieldName[] { FieldName.NullDefense };
            }
        }

        private class NullDefensePercBonus_NoScore : CombatStatsField
        {
            public NullDefensePercBonus_NoScore()
            {
                fieldValue = 0;
                children = new FieldName[] { FieldName.NullDefense };
            }
        }

        private class NullDefenseBonus_NoScore : CombatStatsField
        {
            public NullDefenseBonus_NoScore()
            {
                fieldValue = 0;
                children = new FieldName[] { FieldName.NullDefense };
            }
        }

        private class NullDefense : CombatStatsField
        {
            public NullDefense()
            {
                fieldValue = 1;
                children = new FieldName[] { FieldName.CombatScore };
            }

            public override void Compute(CombatStatsField[] fields, LocalCombatStats localCombatStats, ActorSynStats actorSynStats)
            {
                float x = (int)fields[(int)FieldName.NullDefenseBonus].GetValue();
                float xmod = (int)fields[(int)FieldName.NullDefensePercBonus].GetValue();
                float totalx = x * (1 + xmod);
                float regress = 4000 / (4000 + (10 * totalx));
                fieldValue = Math.Max(Math.Min(regress, 1), 0.05f);

                if (localCombatStats != null)
                {
                    //x -= (int)fields[(int)FieldName.NullDefenseBonus_NoScore].GetValue();
                    //xmod -= (int)fields[(int)FieldName.NullDefensePercBonus_NoScore].GetValue();
                    //totalx = x * (1 + xmod);
                    //regress = 4000 / (4000 + (10 * totalx));
                    localCombatStats.IncElemNoneDefence = (int)(Math.Max(Math.Min(regress, 1), 0.05f) * 100);
                }
            }
        }

        private class MetalDefenseBonus : CombatStatsField
        {
            public MetalDefenseBonus()
            {
                fieldValue = 1;
                children = new FieldName[] { FieldName.MetalDefense };
            }
        }

        private class MetalDefensePercBonus : CombatStatsField
        {
            public MetalDefensePercBonus()
            {
                fieldValue = 0;
                children = new FieldName[] { FieldName.MetalDefense };
            }
        }

        private class MetalDefenseBonus_NoScore : CombatStatsField
        {
            public MetalDefenseBonus_NoScore()
            {
                fieldValue = 0;
                children = new FieldName[] { FieldName.MetalDefense };
            }
        }

        private class MetalDefensePercBonus_NoScore : CombatStatsField
        {
            public MetalDefensePercBonus_NoScore()
            {
                fieldValue = 0;
                children = new FieldName[] { FieldName.MetalDefense };
            }
        }

        private class MetalDefense : CombatStatsField
        {
            public MetalDefense()
            {
                fieldValue = 0;
                children = new FieldName[] { FieldName.CombatScore };
            }

            public override void Compute(CombatStatsField[] fields, LocalCombatStats localCombatStats, ActorSynStats actorSynStats)
            {
                float x = fields[(int)FieldName.MetalDefenseBonus].GetValue();
                float xmod = fields[(int)FieldName.MetalDefensePercBonus].GetValue() * 0.01f;
                float totalx = x * (1 + xmod);
                float regress = 4000 / (4000 + (10 * totalx));
                fieldValue = Math.Max(Math.Min(regress, 1), 0.05f);

                if (localCombatStats != null)
                {
                    //x -= (int)fields[(int)FieldName.MetalDefenseBonus_NoScore].GetValue();
                    //xmod -= (int)fields[(int)FieldName.MetalDefensePercBonus_NoScore].GetValue();
                    //totalx = x * (1 + xmod);
                    //regress = 4000 / (4000 + (10 * totalx));
                    localCombatStats.IncElemMetalDefence = (int)(Math.Max(Math.Min(regress, 1), 0.05f) * 100);
                }
            }
        }

        private class WoodDefenseBonus : CombatStatsField
        {
            public WoodDefenseBonus()
            {
                fieldValue = 1;
                children = new FieldName[] { FieldName.WoodDefense };
            }
        }

        private class WoodDefensePercBonus : CombatStatsField
        {
            public WoodDefensePercBonus()
            {
                fieldValue = 0;
                children = new FieldName[] { FieldName.WoodDefense };
            }
        }

        private class WoodDefenseBonus_NoScore : CombatStatsField
        {
            public WoodDefenseBonus_NoScore()
            {
                fieldValue = 0;
                children = new FieldName[] { FieldName.WoodDefense };
            }
        }

        private class WoodDefensePercBonus_NoScore : CombatStatsField
        {
            public WoodDefensePercBonus_NoScore()
            {
                fieldValue = 0;
                children = new FieldName[] { FieldName.WoodDefense };
            }
        }

        private class WoodDefense : CombatStatsField
        {
            public WoodDefense()
            {
                fieldValue = 0;
                children = new FieldName[] { FieldName.CombatScore };
            }

            public override void Compute(CombatStatsField[] fields, LocalCombatStats localCombatStats, ActorSynStats actorSynStats)
            {
                float x = (int)fields[(int)FieldName.WoodDefenseBonus].GetValue();
                float xmod = (int)fields[(int)FieldName.WoodDefensePercBonus].GetValue() * 0.01f;
                float totalx = x * (1 + xmod);
                float regress = 4000 / (4000 + (10 * totalx));
                fieldValue = Math.Max(Math.Min(regress, 100), 5);

                if (localCombatStats != null)
                {
                    //x -= (int)fields[(int)FieldName.WoodDefenseBonus_NoScore].GetValue();
                    //xmod -= (int)fields[(int)FieldName.WoodDefensePercBonus_NoScore].GetValue();
                    //totalx = x * (1 + xmod);
                    //regress = 4000 / (4000 + (10 * totalx));
                    localCombatStats.IncElemWoodDefence = (int)(Math.Max(Math.Min(regress, 1), 0.05f) * 100);
                }
            }
        }

        private class EarthDefenseBonus : CombatStatsField
        {
            public EarthDefenseBonus()
            {
                fieldValue = 1;
                children = new FieldName[] { FieldName.EarthDefense };
            }
        }

        private class EarthDefensePercBonus : CombatStatsField
        {
            public EarthDefensePercBonus()
            {
                fieldValue = 0;
                children = new FieldName[] { FieldName.EarthDefense };
            }
        }

        private class EarthDefenseBonus_NoScore : CombatStatsField
        {
            public EarthDefenseBonus_NoScore()
            {
                fieldValue = 0;
                children = new FieldName[] { FieldName.EarthDefense };
            }
        }

        private class EarthDefensePercBonus_NoScore : CombatStatsField
        {
            public EarthDefensePercBonus_NoScore()
            {
                fieldValue = 0;
                children = new FieldName[] { FieldName.EarthDefense };
            }
        }

        private class EarthDefense : CombatStatsField
        {
            public EarthDefense()
            {
                fieldValue = 0;
                children = new FieldName[] { FieldName.CombatScore };
            }

            public override void Compute(CombatStatsField[] fields, LocalCombatStats localCombatStats, ActorSynStats actorSynStats)
            {
                float x = fields[(int)FieldName.EarthDefenseBonus].GetValue();
                float xmod = fields[(int)FieldName.EarthDefensePercBonus].GetValue() * 0.01f;
                float totalx = x * (1 + xmod);
                float regress = 4000 / (4000 + (10 * totalx));
                fieldValue = Math.Max(Math.Min(regress, 1), 0.05f);

                if (localCombatStats != null)
                {
                    //x -= (int)fields[(int)FieldName.EarthDefenseBonus_NoScore].GetValue();
                    //xmod -= (int)fields[(int)FieldName.EarthDefensePercBonus_NoScore].GetValue();
                    //totalx = x * (1 + xmod);
                    //regress = 4000 / (4000 + (10 * totalx));
                    localCombatStats.IncElemEarthDefence = (int)(Math.Max(Math.Min(regress, 1), 0.05f) * 100);
                }
            }
        }

        private class WaterDefenseBonus : CombatStatsField
        {
            public WaterDefenseBonus()
            {
                fieldValue = 1;
                children = new FieldName[] { FieldName.WaterDefense };
            }
        }

        private class WaterDefensePercBonus : CombatStatsField
        {
            public WaterDefensePercBonus()
            {
                fieldValue = 0;
                children = new FieldName[] { FieldName.WaterDefense };
            }
        }

        private class WaterDefenseBonus_NoScore : CombatStatsField
        {
            public WaterDefenseBonus_NoScore()
            {
                fieldValue = 0;
                children = new FieldName[] { FieldName.WaterDefense };
            }
        }

        private class WaterDefensePercBonus_NoScore : CombatStatsField
        {
            public WaterDefensePercBonus_NoScore()
            {
                fieldValue = 0;
                children = new FieldName[] { FieldName.WaterDefense };
            }
        }

        private class WaterDefense : CombatStatsField
        {
            public WaterDefense()
            {
                fieldValue = 0;
                children = new FieldName[] { FieldName.CombatScore };
            }

            public override void Compute(CombatStatsField[] fields, LocalCombatStats localCombatStats, ActorSynStats actorSynStats)
            {
                float x = fields[(int)FieldName.WaterDefenseBonus].GetValue();
                float xmod = fields[(int)FieldName.WaterDefensePercBonus].GetValue() * 0.01f;
                float totalx = x * (1 + xmod);
                float regress = 4000 / (4000 + (10 * totalx));
                fieldValue = Math.Max(Math.Min(regress, 1), 0.05f);

                if (localCombatStats != null)
                {
                    //x -= (int)fields[(int)FieldName.WaterDefenseBonus_NoScore].GetValue();
                    //xmod -= (int)fields[(int)FieldName.WaterDefensePercBonus_NoScore].GetValue();
                    //totalx = x * (1 + xmod);
                    //regress = 4000 / (4000 + (10 * totalx));
                    localCombatStats.IncElemWaterDefence = (int)(Math.Max(Math.Min(regress, 1), 0.05f) * 100);
                }
            }
        }

        private class FireDefenseBonus : CombatStatsField
        {
            public FireDefenseBonus()
            {
                fieldValue = 1;
                children = new FieldName[] { FieldName.FireDefense };
            }
        }

        private class FireDefensePercBonus : CombatStatsField
        {
            public FireDefensePercBonus()
            {
                fieldValue = 0;
                children = new FieldName[] { FieldName.FireDefense };
            }
        }

        private class FireDefenseBonus_NoScore : CombatStatsField
        {
            public FireDefenseBonus_NoScore()
            {
                fieldValue = 0;
                children = new FieldName[] { FieldName.FireDefense };
            }
        }

        private class FireDefensePercBonus_NoScore : CombatStatsField
        {
            public FireDefensePercBonus_NoScore()
            {
                fieldValue = 0;
                children = new FieldName[] { FieldName.FireDefense };
            }
        }

        private class FireDefense : CombatStatsField
        {
            public FireDefense()
            {
                fieldValue = 0;
                children = new FieldName[] { FieldName.CombatScore };
            }

            public override void Compute(CombatStatsField[] fields, LocalCombatStats localCombatStats, ActorSynStats actorSynStats)
            {
                float x = fields[(int)FieldName.FireDefenseBonus].GetValue();
                float xmod = fields[(int)FieldName.FireDefensePercBonus].GetValue() * 0.01f;
                float totalx = x * (1 + xmod);
                float regress = 4000 / (4000 + (10 * totalx));
                fieldValue = Math.Max(Math.Min(regress, 1), 0.05f);

                if (localCombatStats != null)
                {
                    //x -= (int)fields[(int)FieldName.FireDefenseBonus_NoScore].GetValue();
                    //xmod -= (int)fields[(int)FieldName.FireDefensePercBonus_NoScore].GetValue();
                    //totalx = x * (1 + xmod);
                    //regress = 4000 / (4000 + (10 * totalx));
                    localCombatStats.IncElemFireDefence = (int)(Math.Max(Math.Min(regress, 1), 0.05f) * 100);
                }
            }
        }

        private class VSHumanDamageBonus : CombatStatsField
        {
            public VSHumanDamageBonus()
            {
                fieldValue = 1;
                children = new FieldName[] { FieldName.VSHumanDamage };
            }
        }

        private class VSHumanDamagePercBonus : CombatStatsField
        {
            public VSHumanDamagePercBonus()
            {
                fieldValue = 0;
                children = new FieldName[] { FieldName.VSHumanDamage };
            }
        }

        private class VSHumanDamageBonus_NoScore : CombatStatsField
        {
            public VSHumanDamageBonus_NoScore()
            {
                fieldValue = 0;
                children = new FieldName[] { FieldName.VSHumanDamage };
            }
        }

        private class VSHumanDamagePercBonus_NoScore : CombatStatsField
        {
            public VSHumanDamagePercBonus_NoScore()
            {
                fieldValue = 0;
                children = new FieldName[] { FieldName.VSHumanDamage };
            }
        }

        private class VSHumanDamage : CombatStatsField
        {
            public VSHumanDamage()
            {
                fieldValue = 0;
                children = new FieldName[] { FieldName.CombatScore };
            }

            public override void Compute(CombatStatsField[] fields, LocalCombatStats localCombatStats, ActorSynStats actorSynStats)
            {
                float x = fields[(int)FieldName.VSHumanDamageBonus].GetValue();
                float x_mod = fields[(int)FieldName.VSHumanDamagePercBonus].GetValue() * 0.01f;
                fieldValue = x * ((1 + x_mod) / 4000);

                if (localCombatStats != null)
                {
                    x -= fields[(int)FieldName.VSHumanDamageBonus_NoScore].GetValue();
                    x_mod -= fields[(int)FieldName.VSHumanDamagePercBonus_NoScore].GetValue();
                    localCombatStats.VSHumanDamage = (int)(x * ((1 + x_mod) / 4000));
                }
            }
        }

        private class VSZombieDamageBonus : CombatStatsField
        {
            public VSZombieDamageBonus()
            {
                fieldValue = 1;
                children = new FieldName[] { FieldName.VSZombieDamage };
            }
        }

        private class VSZombieDamagePercBonus : CombatStatsField
        {
            public VSZombieDamagePercBonus()
            {
                fieldValue = 0;
                children = new FieldName[] { FieldName.VSZombieDamage };
            }
        }

        private class VSZombieDamageBonus_NoScore : CombatStatsField
        {
            public VSZombieDamageBonus_NoScore()
            {
                fieldValue = 0;
                children = new FieldName[] { FieldName.VSZombieDamage };
            }
        }

        private class VSZombieDamagePercBonus_NoScore : CombatStatsField
        {
            public VSZombieDamagePercBonus_NoScore()
            {
                fieldValue = 0;
                children = new FieldName[] { FieldName.VSZombieDamage };
            }
        }

        private class VSZombieDamage : CombatStatsField
        {
            public VSZombieDamage()
            {
                fieldValue = 0;
                children = new FieldName[] { FieldName.CombatScore };
            }

            public override void Compute(CombatStatsField[] fields, LocalCombatStats localCombatStats, ActorSynStats actorSynStats)
            {
                float x = fields[(int)FieldName.VSZombieDamageBonus].GetValue();
                float x_mod = fields[(int)FieldName.VSZombieDamagePercBonus].GetValue() * 0.01f;
                fieldValue = x * ((1 + x_mod) / 4000);

                if (localCombatStats != null)
                {
                    x -= fields[(int)FieldName.VSZombieDamageBonus_NoScore].GetValue();
                    x_mod -= fields[(int)FieldName.VSZombieDamagePercBonus_NoScore].GetValue() * 0.01f;
                    localCombatStats.VSZombieDamage = (int)(x * ((1 + x_mod) / 4000));
                }
            }
        }

        private class VSVampireDamageBonus : CombatStatsField
        {
            public VSVampireDamageBonus()
            {
                fieldValue = 1;
                children = new FieldName[] { FieldName.VSVampireDamage };
            }
        }

        private class VSVampireDamagePercBonus : CombatStatsField
        {
            public VSVampireDamagePercBonus()
            {
                fieldValue = 0;
                children = new FieldName[] { FieldName.VSVampireDamage };
            }
        }

        private class VSVampireDamageBonus_NoScore : CombatStatsField
        {
            public VSVampireDamageBonus_NoScore()
            {
                fieldValue = 0;
                children = new FieldName[] { FieldName.VSVampireDamage };
            }
        }

        private class VSVampireDamagePercBonus_NoScore : CombatStatsField
        {
            public VSVampireDamagePercBonus_NoScore()
            {
                fieldValue = 0;
                children = new FieldName[] { FieldName.VSVampireDamage };
            }
        }

        private class VSVampireDamage : CombatStatsField
        {
            public VSVampireDamage()
            {
                fieldValue = 0;
                children = new FieldName[] { FieldName.CombatScore };
            }

            public override void Compute(CombatStatsField[] fields, LocalCombatStats localCombatStats, ActorSynStats actorSynStats)
            {
                float x = fields[(int)FieldName.VSVampireDamageBonus].GetValue();
                float x_mod = fields[(int)FieldName.VSVampireDamagePercBonus].GetValue() * 0.01f;
                fieldValue = x * ((1 + x_mod) / 4000);

                if (localCombatStats != null)
                {
                    x -= fields[(int)FieldName.VSVampireDamageBonus_NoScore].GetValue();
                    x_mod -= fields[(int)FieldName.VSVampireDamagePercBonus_NoScore].GetValue() * 0.01f;
                    localCombatStats.VSVampireDamage = (int)(x * ((1 + x_mod) / 4000));
                }
            }
        }

        private class VSBeastDamageBonus : CombatStatsField
        {
            public VSBeastDamageBonus()
            {
                fieldValue = 1;
                children = new FieldName[] { FieldName.VSBeastDamage };
            }
        }

        private class VSBeastDamagePercBonus : CombatStatsField
        {
            public VSBeastDamagePercBonus()
            {
                fieldValue = 0;
                children = new FieldName[] { FieldName.VSBeastDamage };
            }
        }

        private class VSBeastDamageBonus_NoScore : CombatStatsField
        {
            public VSBeastDamageBonus_NoScore()
            {
                fieldValue = 0;
                children = new FieldName[] { FieldName.VSBeastDamage };
            }
        }

        private class VSBeastDamagePercBonus_NoScore : CombatStatsField
        {
            public VSBeastDamagePercBonus_NoScore()
            {
                fieldValue = 0;
                children = new FieldName[] { FieldName.VSBeastDamage };
            }
        }

        private class VSBeastDamage : CombatStatsField
        {
            public VSBeastDamage()
            {
                fieldValue = 0;
                children = new FieldName[] { FieldName.CombatScore };
            }

            public override void Compute(CombatStatsField[] fields, LocalCombatStats localCombatStats, ActorSynStats actorSynStats)
            {
                float x = fields[(int)FieldName.VSBeastDamageBonus].GetValue() * 0.01f;
                float x_mod = fields[(int)FieldName.VSBeastDamagePercBonus].GetValue() * 0.01f;
                fieldValue = x * ((1 + x_mod) / 4000);

                if (localCombatStats != null)
                {
                    x -= fields[(int)FieldName.VSBeastDamageBonus_NoScore].GetValue() * 0.01f;
                    x_mod -= fields[(int)FieldName.VSBeastDamagePercBonus_NoScore].GetValue() * 0.01f;
                    localCombatStats.VSAnimalDamage = (int)(x * ((1 + x_mod) / 4000));
                }
            }
        }

        private class VSPlantDamageBonus : CombatStatsField
        {
            public VSPlantDamageBonus()
            {
                fieldValue = 1;
                children = new FieldName[] { FieldName.VSPlantDamage };
            }
        }

        private class VSPlantDamagePercBonus : CombatStatsField
        {
            public VSPlantDamagePercBonus()
            {
                fieldValue = 0;
                children = new FieldName[] { FieldName.VSPlantDamage };
            }
        }

        private class VSPlantDamageBonus_NoScore : CombatStatsField
        {
            public VSPlantDamageBonus_NoScore()
            {
                fieldValue = 0;
                children = new FieldName[] { FieldName.VSPlantDamage };
            }
        }

        private class VSPlantDamagePercBonus_NoScore : CombatStatsField
        {
            public VSPlantDamagePercBonus_NoScore()
            {
                fieldValue = 0;
                children = new FieldName[] { FieldName.VSPlantDamage };
            }
        }

        private class VSPlantDamage : CombatStatsField
        {
            public VSPlantDamage()
            {
                fieldValue = 0;
                children = new FieldName[] { FieldName.CombatScore };
            }

            public override void Compute(CombatStatsField[] fields, LocalCombatStats localCombatStats, ActorSynStats actorSynStats)
            {
                float x = fields[(int)FieldName.VSPlantDamageBonus].GetValue() * 0.01f;
                float x_mod = fields[(int)FieldName.VSPlantDamagePercBonus].GetValue() * 0.01f;
                fieldValue = x * ((1 + x_mod) / 4000);

                if (localCombatStats != null)
                {
                    x -= fields[(int)FieldName.VSPlantDamageBonus_NoScore].GetValue() * 0.01f;
                    x_mod -= fields[(int)FieldName.VSPlantDamagePercBonus_NoScore].GetValue() * 0.01f;
                    localCombatStats.VSPlantDamage = (int)(x * ((1 + x_mod) / 4000));
                }
            }
        }

        private class VSHumanDefenseBonus : CombatStatsField
        {
            public VSHumanDefenseBonus()
            {
                fieldValue = 1;
                children = new FieldName[] { FieldName.VSHumanDefense };
            }
        }

        private class VSHumanDefensePercBonus : CombatStatsField
        {
            public VSHumanDefensePercBonus()
            {
                fieldValue = 0;
                children = new FieldName[] { FieldName.VSHumanDefense };
            }
        }

        private class VSHumanDefenseBonus_NoScore : CombatStatsField
        {
            public VSHumanDefenseBonus_NoScore()
            {
                fieldValue = 0;
                children = new FieldName[] { FieldName.VSHumanDefense };
            }
        }

        private class VSHumanDefensePercBonus_NoScore : CombatStatsField
        {
            public VSHumanDefensePercBonus_NoScore()
            {
                fieldValue = 0;
                children = new FieldName[] { FieldName.VSHumanDefense };
            }
        }

        private class VSHumanDefense : CombatStatsField
        {
            public VSHumanDefense()
            {
                fieldValue = 0;
                children = new FieldName[] { FieldName.CombatScore };
            }

            public override void Compute(CombatStatsField[] fields, LocalCombatStats localCombatStats, ActorSynStats actorSynStats)
            {
                float x = fields[(int)FieldName.VSHumanDefenseBonus].GetValue() * 0.01f;
                float x_mod = fields[(int)FieldName.VSHumanDefensePercBonus].GetValue() * 0.01f;
                float total = x * (1 + x_mod);
                float regress = 4000 / (4000 + 10 * total);
                fieldValue = Math.Max(Math.Min(regress, 1), 0.05f);

                if (localCombatStats != null)
                {
                    x -= (int)fields[(int)FieldName.VSHumanDefenseBonus_NoScore].GetValue() * 0.01f;
                    x_mod -= (int)fields[(int)FieldName.VSHumanDefensePercBonus_NoScore].GetValue();
                    total = x * (1 + x_mod);
                    regress = 4000 / (4000 + 10 * total);
                    localCombatStats.VSHumanDefence = (int)Math.Max(Math.Min(regress * 100, 100), 5);
                }
            }
        }

        private class VSZombieDefenseBonus : CombatStatsField
        {
            public VSZombieDefenseBonus()
            {
                fieldValue = 1;
                children = new FieldName[] { FieldName.VSZombieDefense };
            }
        }

        private class VSZombieDefensePercBonus : CombatStatsField
        {
            public VSZombieDefensePercBonus()
            {
                fieldValue = 0;
                children = new FieldName[] { FieldName.VSZombieDefense };
            }
        }

        private class VSZombieDefenseBonus_NoScore : CombatStatsField
        {
            public VSZombieDefenseBonus_NoScore()
            {
                fieldValue = 0;
                children = new FieldName[] { FieldName.VSZombieDefense };
            }
        }

        private class VSZombieDefensePercBonus_NoScore : CombatStatsField
        {
            public VSZombieDefensePercBonus_NoScore()
            {
                fieldValue = 0;
                children = new FieldName[] { FieldName.VSZombieDefense };
            }
        }

        private class VSZombieDefense : CombatStatsField
        {
            public VSZombieDefense()
            {
                fieldValue = 0;
                children = new FieldName[] { FieldName.CombatScore };
            }

            public override void Compute(CombatStatsField[] fields, LocalCombatStats localCombatStats, ActorSynStats actorSynStats)
            {
                float x = fields[(int)FieldName.VSZombieDefenseBonus].GetValue() * 0.01f;
                float x_mod = fields[(int)FieldName.VSZombieDefensePercBonus].GetValue() * 0.01f;
                float total = x * (1 + x_mod);
                float regress = 4000 / (4000 + 10 * total);
                fieldValue = Math.Max(Math.Min(regress, 1), 0.05f);

                if (localCombatStats != null)
                {
                    x -= fields[(int)FieldName.VSZombieDefenseBonus_NoScore].GetValue() * 0.01f;
                    x_mod -= fields[(int)FieldName.VSZombieDefensePercBonus_NoScore].GetValue() * 0.01f;
                    total = x * (1 + x_mod);
                    regress = 4000 / (4000 + 10 * total);
                    localCombatStats.VSZombieDefence = (int)Math.Max(Math.Min(regress * 100, 100), 5);
                }
            }
        }

        private class VSVampireDefenseBonus : CombatStatsField
        {
            public VSVampireDefenseBonus()
            {
                fieldValue = 1;
                children = new FieldName[] { FieldName.VSVampireDefense };
            }
        }

        private class VSVampireDefensePercBonus : CombatStatsField
        {
            public VSVampireDefensePercBonus()
            {
                fieldValue = 0;
                children = new FieldName[] { FieldName.VSVampireDefense };
            }
        }

        private class VSVampireDefenseBonus_NoScore : CombatStatsField
        {
            public VSVampireDefenseBonus_NoScore()
            {
                fieldValue = 0;
                children = new FieldName[] { FieldName.VSVampireDefense };
            }
        }

        private class VSVampireDefensePercBonus_NoScore : CombatStatsField
        {
            public VSVampireDefensePercBonus_NoScore()
            {
                fieldValue = 0;
                children = new FieldName[] { FieldName.VSVampireDefense };
            }
        }

        private class VSVampireDefense : CombatStatsField
        {
            public VSVampireDefense()
            {
                fieldValue = 0;
                children = new FieldName[] { FieldName.CombatScore };
            }

            public override void Compute(CombatStatsField[] fields, LocalCombatStats localCombatStats, ActorSynStats actorSynStats)
            {
                float x = fields[(int)FieldName.VSVampireDefenseBonus].GetValue() * 0.01f;
                float x_mod = fields[(int)FieldName.VSVampireDefensePercBonus].GetValue() * 0.01f;
                float total = x * (1 + x_mod);
                float regress = 4000 / (4000 + 10 * total);
                fieldValue = Math.Max(Math.Min(regress, 1), 0.05f);

                if (localCombatStats != null)
                {
                    x -= fields[(int)FieldName.VSVampireDefenseBonus_NoScore].GetValue() * 0.01f;
                    x_mod -= fields[(int)FieldName.VSVampireDefensePercBonus_NoScore].GetValue() * 0.01f;
                    total = x * (1 + x_mod);
                    regress = 4000 / (4000 + 10 * total);
                    localCombatStats.VSVampireDefence = (int)Math.Max(Math.Min(regress * 100, 100), 5);
                }
            }
        }

        private class VSBeastDefenseBonus : CombatStatsField
        {
            public VSBeastDefenseBonus()
            {
                fieldValue = 1;
                children = new FieldName[] { FieldName.VSBeastDefense };
            }
        }

        private class VSBeastDefensePercBonus : CombatStatsField
        {
            public VSBeastDefensePercBonus()
            {
                fieldValue = 0;
                children = new FieldName[] { FieldName.VSBeastDefense };
            }
        }

        private class VSBeastDefenseBonus_NoScore : CombatStatsField
        {
            public VSBeastDefenseBonus_NoScore()
            {
                fieldValue = 0;
                children = new FieldName[] { FieldName.VSBeastDefense };
            }
        }

        private class VSBeastDefensePercBonus_NoScore : CombatStatsField
        {
            public VSBeastDefensePercBonus_NoScore()
            {
                fieldValue = 0;
                children = new FieldName[] { FieldName.VSBeastDefense };
            }
        }

        private class VSBeastDefense : CombatStatsField
        {
            public VSBeastDefense()
            {
                fieldValue = 0;
                children = new FieldName[] { FieldName.CombatScore };
            }

            public override void Compute(CombatStatsField[] fields, LocalCombatStats localCombatStats, ActorSynStats actorSynStats)
            {
                float x = fields[(int)FieldName.VSBeastDefenseBonus].GetValue() * 0.01f;
                float x_mod = fields[(int)FieldName.VSBeastDefensePercBonus].GetValue() * 0.01f;
                float total = x * (1 + x_mod);
                float regress = 4000 / (4000 + 10 * total);
                fieldValue = Math.Max(Math.Min(regress, 1), 0.05f);

                if (localCombatStats != null)
                {
                    x -= fields[(int)FieldName.VSBeastDefenseBonus_NoScore].GetValue() * 0.01f;
                    x_mod -= fields[(int)FieldName.VSBeastDefensePercBonus_NoScore].GetValue();
                    total = x * (1 + x_mod);
                    regress = 4000 / (4000 + 10 * total);
                    localCombatStats.VSAnimalDefence = (int)Math.Max(Math.Min(regress * 100, 100), 5);
                }
            }
        }

        private class VSPlantDefenseBonus : CombatStatsField
        {
            public VSPlantDefenseBonus()
            {
                fieldValue = 1;
                children = new FieldName[] { FieldName.VSPlantDefense };
            }
        }

        private class VSPlantDefensePercBonus : CombatStatsField
        {
            public VSPlantDefensePercBonus()
            {
                fieldValue = 0;
                children = new FieldName[] { FieldName.VSPlantDefense };
            }
        }

        private class VSPlantDefenseBonus_NoScore : CombatStatsField
        {
            public VSPlantDefenseBonus_NoScore()
            {
                fieldValue = 0;
                children = new FieldName[] { FieldName.VSPlantDefense };
            }
        }

        private class VSPlantDefensePercBonus_NoScore : CombatStatsField
        {
            public VSPlantDefensePercBonus_NoScore()
            {
                fieldValue = 0;
                children = new FieldName[] { FieldName.VSPlantDefense };
            }
        }

        private class VSPlantDefense : CombatStatsField
        {
            public VSPlantDefense()
            {
                fieldValue = 0;
                children = new FieldName[] { FieldName.CombatScore };
            }

            public override void Compute(CombatStatsField[] fields, LocalCombatStats localCombatStats, ActorSynStats actorSynStats)
            {
                float x = fields[(int)FieldName.VSPlantDefenseBonus].GetValue() * 0.01f;
                float x_mod = fields[(int)FieldName.VSPlantDefensePercBonus].GetValue() * 0.01f;
                float total = x * (1 + x_mod);
                float regress = 4000 / (4000 + 10 * total);
                fieldValue = Math.Max(Math.Min(regress, 1), 0.05f);

                if (localCombatStats != null)
                {
                    x -= fields[(int)FieldName.VSPlantDefenseBonus_NoScore].GetValue() * 0.01f;
                    x_mod -= fields[(int)FieldName.VSPlantDefensePercBonus_NoScore].GetValue() * 0.01f;
                    total = x * (1 + x_mod);
                    regress = 4000 / (4000 + 10 * total);
                    localCombatStats.VSPlantDefence = (int)Math.Max(Math.Min(regress * 100, 100), 5);
                }
            }
        }

        private class VSNullDamagePercBonus : CombatStatsField
        {
            public VSNullDamagePercBonus()
            {
                fieldValue = 1;
                children = new FieldName[] { FieldName.VSNullDamage };
            }
        }

        private class VSNullDamage : CombatStatsField
        {
            public VSNullDamage()
            {
                fieldValue = 0;
                children = new FieldName[] { };
            }

            public override void Compute(CombatStatsField[] fields, LocalCombatStats localCombatStats, ActorSynStats actorSynStats)
            {
                fieldValue = 1 + fields[(int)FieldName.VSNullPercDamage].GetValue() * 0.01f;

                if (localCombatStats != null)
                {
                    localCombatStats.VSElemNoneDamage = (int)fieldValue;
                }
            }
        }

        private class VSMetalDamagePercBonus : CombatStatsField
        {
            public VSMetalDamagePercBonus()
            {
                fieldValue = 0;
                children = new FieldName[] { FieldName.VSMetalDamage };
            }
        }

        private class VSMetalDamage : CombatStatsField
        {
            public VSMetalDamage()
            {
                fieldValue = 0;
                children = new FieldName[] { };
            }

            public override void Compute(CombatStatsField[] fields, LocalCombatStats localCombatStats, ActorSynStats actorSynStats)
            {
                fieldValue = 1 + fields[(int)FieldName.VSMetalPercDamage].GetValue() * 0.01f;

                if (localCombatStats != null)
                {
                    localCombatStats.VSElemMetalDamage = (int)fieldValue;
                }
            }
        }

        private class VSWoodDamagePercBonus : CombatStatsField
        {
            public VSWoodDamagePercBonus()
            {
                fieldValue = 0;
                children = new FieldName[] { FieldName.VSWoodDamage };
            }
        }

        private class VSWoodDamage : CombatStatsField
        {
            public VSWoodDamage()
            {
                fieldValue = 0;
                children = new FieldName[] { };
            }

            public override void Compute(CombatStatsField[] fields, LocalCombatStats localCombatStats, ActorSynStats actorSynStats)
            {
                fieldValue = 1 + fields[(int)FieldName.VSWoodPercDamage].GetValue() * 0.01f;

                if (localCombatStats != null)
                {
                    localCombatStats.VSElemWoodDamage = (int)fieldValue;
                }
            }
        }

        private class VSEarthDamagePercBonus : CombatStatsField
        {
            public VSEarthDamagePercBonus()
            {
                fieldValue = 0;
                children = new FieldName[] { FieldName.VSEarthDamage };
            }
        }

        private class VSEarthDamage : CombatStatsField
        {
            public VSEarthDamage()
            {
                fieldValue = 0;
                children = new FieldName[] { };
            }

            public override void Compute(CombatStatsField[] fields, LocalCombatStats localCombatStats, ActorSynStats actorSynStats)
            {
                fieldValue = 1 + fields[(int)FieldName.VSEarthPercDamage].GetValue() * 0.01f;

                if (localCombatStats != null)
                {
                    localCombatStats.VSElemEarthDamage = (int)fieldValue;
                }
            }
        }

        private class VSFireDamagePercBonus : CombatStatsField
        {
            public VSFireDamagePercBonus()
            {
                fieldValue = 0;
                children = new FieldName[] { FieldName.VSFireDamage };
            }
        }

        private class VSFireDamage : CombatStatsField
        {
            public VSFireDamage()
            {
                fieldValue = 1;
                children = new FieldName[] { };
            }

            public override void Compute(CombatStatsField[] fields, LocalCombatStats localCombatStats, ActorSynStats actorSynStats)
            {
                fieldValue = 1 + fields[(int)FieldName.VSFirePercDamage].GetValue() * 0.01f;

                if (localCombatStats != null)
                {
                    localCombatStats.VSElemFireDamage = (int)fieldValue;
                }
            }
        }

        private class VSWaterDamagePercBonus : CombatStatsField
        {
            public VSWaterDamagePercBonus()
            {
                fieldValue = 0;
                children = new FieldName[] { FieldName.VSWaterDamage };
            }
        }

        private class VSWaterDamage : CombatStatsField
        {
            public VSWaterDamage()
            {
                fieldValue = 0;
                children = new FieldName[] { };
            }

            public override void Compute(CombatStatsField[] fields, LocalCombatStats localCombatStats, ActorSynStats actorSynStats)
            {
                fieldValue = 1 + fields[(int)FieldName.VSWaterPercDamage].GetValue() * 0.01f;

                if (localCombatStats != null)
                {
                    localCombatStats.VSElemWaterDamage = (int)fieldValue;
                }
            }
        }

        private class VSBossDamageBonus : CombatStatsField
        {
            public VSBossDamageBonus()
            {
                fieldValue = 0;
                children = new FieldName[] { FieldName.VSBossDamage };
            }
        }

        private class VSBossDamage : CombatStatsField
        {
            public VSBossDamage()
            {
                fieldValue = 0;
                children = new FieldName[] { };
            }
        }

        private class IncreaseFinalDamageBonus : CombatStatsField
        {
            public IncreaseFinalDamageBonus()
            {
                fieldValue = 0;
                children = new FieldName[] { FieldName.IncreaseFinalDamage };
            }
        }

        private class IncreaseFinalDamage : CombatStatsField
        {
            public IncreaseFinalDamage()
            {
                fieldValue = 0;
                children = new FieldName[] { };
            }

            public override void Compute(CombatStatsField[] fields, LocalCombatStats localCombatStats, ActorSynStats actorSynStats)
            {
                fieldValue = 1 + fields[(int)FieldName.IncreaseFinalDamageBonus].GetValue() * 0.01f;

                if (localCombatStats != null)
                    localCombatStats.IncFinalDamage = (int)(Math.Round(fieldValue));
            }
        }

        private class DecreaseFinalDamageBonus : CombatStatsField
        {
            public DecreaseFinalDamageBonus()
            {
                fieldValue = 0;
                children = new FieldName[] { FieldName.DecreaseFinalDamage };
            }
        }

        private class DecreaseFinalDamage : CombatStatsField
        {
            public DecreaseFinalDamage()
            {
                fieldValue = 0;
                children = new FieldName[] { };
            }

            public override void Compute(CombatStatsField[] fields, LocalCombatStats localCombatStats, ActorSynStats actorSynStats)
            {
                fieldValue = 1 - fields[(int)FieldName.DecreaseFinalDamageBonus].GetValue() * 0.01f;

                if (localCombatStats != null)
                    localCombatStats.DncFinalDamage = (int)(Math.Round(fieldValue));
            }
        }

        // Aditional Stats

        //---------------------------CombatScore -----------------------------------------
        private class CombatScoreField : CombatStatsField
        {
            private static readonly int SCORE_HEALTH = InitConstValue(10, "CombatScore_Health");
            private static readonly int SCORE_Attack = InitConstValue(10, "CombatScore_Attack");
            private static readonly int SCORE_Armor = InitConstValue(10, "CombatScore_Armor");
            private static readonly int SCORE_PenetrateDamage = InitConstValue(10, "CombatScore_PenetrateDamage");
            private static readonly int SCORE_Critical = InitConstValue(10, "CombatScore_Critical");
            private static readonly int SCORE_Coctirical = InitConstValue(10, "CombatScore_Coctirical");
            private static readonly int SCORE_CriticalDamage = InitConstValue(10, "CombatScore_CriticalDamage");
            private static readonly int SCORE_CoctiricalDamage = InitConstValue(10, "CombatScore_CoctiricalDamage");
            private static readonly int SCORE_Evasion = InitConstValue(10, "CombatScore_Evasion");
            private static readonly int SCORE_Accuracy = InitConstValue(10, "CombatScore_Accuracy");

            private static int InitConstValue(int defVal, string constKey)
            {
                return GameConstantRepo.GetConstantInt(constKey, defVal);
            }

            public CombatScoreField()
            {
                fieldValue = 0;
                children = new FieldName[] { };
            }

            public override void Compute(CombatStatsField[] fields, LocalCombatStats localCombatStats, ActorSynStats actorSynStats)
            {
                if (localCombatStats != null)
                {
                    int health = localCombatStats.HealthMax;
                    //int attack = localCombatStats.AttackMin;
                    int armor = localCombatStats.Armor;
                    int critical = localCombatStats.Critical;
                    int cocritical = localCombatStats.CoCritical;
                    int criticalDamage = localCombatStats.CriticalDamage;
                    //int coCriticalDamage = localCombatStats.CoCriticalDamage;
                    int evasion = localCombatStats.Evasion;
                    int accuracy = localCombatStats.Accuracy;
                    //fieldValue = health * SCORE_HEALTH + attack * SCORE_Attack + armor * SCORE_Armor + critical * SCORE_Critical +
                    //    criticalDamage * SCORE_CriticalDamage + cocritical * SCORE_Coctirical + coCriticalDamage * SCORE_CoctiricalDamage + evasion * SCORE_Evasion + accuracy * SCORE_Accuracy;
                    //localCombatStats.CombatScore = fieldValue;
                }
            }
        }

        //------------------------------------------------------------------------------

        

        /// <summary>
        /// passing the IActor for computation of LocalSKillPassiveStats.
        /// </summary>
        /// <param name="localCombatStats"></param>
        /// <param name="playerSynStats"></param>
        /// <param name="actor"></param>
        public void SetPlayerLocalAndSyncStats(LocalCombatStats localCombatStats, ActorSynStats actorSynStats, IActor actor)
        {
            mLocalCombatStats = localCombatStats;
            mActorSynStats = actorSynStats;
            mActor = actor;
        }

        public PlayerCombatStats()
        {
            SuppressComputeAll = false;

            int totalFields = (int)FieldName.LastField;
            mFields = new CombatStatsField[totalFields];

            mFields[(int)FieldName.Strength] = new Strength();
            mFields[(int)FieldName.StrengthBase] = new StrengthBase();
            mFields[(int)FieldName.StrengthBonus] = new StrengthBonus();
            mFields[(int)FieldName.StrengthPercBonus] = new StrengthPercBonus();
            mFields[(int)FieldName.StrengthBonus_NoScore] = new StrengthBonus_NoScore();
            mFields[(int)FieldName.StrengthPercBonus_NoScore] = new StrengthPercBonus_NoScore();

            mFields[(int)FieldName.Agility] = new Agility();
            mFields[(int)FieldName.AgilityBase] = new AgilityBase();
            mFields[(int)FieldName.AgilityBonus] = new AgilityBonus();
            mFields[(int)FieldName.AgilityPercBonus] = new AgilityPercBonus();
            mFields[(int)FieldName.AgilityBonus_NoScore] = new AgilityBonus_NoScore();
            mFields[(int)FieldName.AgilityPercBonus_NoScore] = new AgilityPercBonus_NoScore();

            mFields[(int)FieldName.Dexterity] = new Dexterity();
            mFields[(int)FieldName.DexterityBase] = new DexterityBase();
            mFields[(int)FieldName.DexterityBonus] = new DexterityBonus();
            mFields[(int)FieldName.DexterityPercBonus] = new DexterityPercBonus();
            mFields[(int)FieldName.DexterityBonus_NoScore] = new DexterityBonus_NoScore();
            mFields[(int)FieldName.DexterityPercBonus_NoScore] = new DexterityPercBonus_NoScore();

            mFields[(int)FieldName.Constitution] = new Constitution();
            mFields[(int)FieldName.ConstitutionBase] = new ConstitutionBase();
            mFields[(int)FieldName.ConstitutionBonus] = new ConstitutionBonus();
            mFields[(int)FieldName.ConstitutionPercBonus] = new ConstitutionPercBonus();
            mFields[(int)FieldName.ConstitutionBonus_NoScore] = new ConstitutionBonus_NoScore();
            mFields[(int)FieldName.ConstitutionPercBonus_NoScore] = new ConstitutionPercBonus_NoScore();

            mFields[(int)FieldName.Intelligence] = new Intelligence();
            mFields[(int)FieldName.IntelligenceBase] = new IntelligenceBase();
            mFields[(int)FieldName.IntelligenceBonus] = new IntelligenceBonus();
            mFields[(int)FieldName.IntelligencePercBonus] = new IntelligencePercBonus();
            mFields[(int)FieldName.IntelligenceBonus_NoScore] = new IntelligenceBonus_NoScore();
            mFields[(int)FieldName.IntelligencePercBonus_NoScore] = new IntelligencePercBonus_NoScore();

            mFields[(int)FieldName.AttackSpeed] = new AttackSpeed();
            mFields[(int)FieldName.AttackSpeedBase] = new AttackSpeedBase();
            mFields[(int)FieldName.AttackSpeedBuff] = new AttackSpeedBuff();
            mFields[(int)FieldName.AttackSpeedDebuff] = new AttackSpeedDebuff();

            mFields[(int)FieldName.CastSpeed] = new CastSpeed();
            mFields[(int)FieldName.CastSpeedBuff] = new CastSpeedBuff();
            mFields[(int)FieldName.CastSpeedDebuff] = new CastSpeedDebuff();
            mFields[(int)FieldName.CastSpeedBase] = new CastSpeedBase();

            mFields[(int)FieldName.MoveSpeed] = new MoveSpeed();
            mFields[(int)FieldName.MoveSpeedBase] = new MoveSpeedBase();
            mFields[(int)FieldName.MoveSpeedBuff] = new MoveSpeedBuff();
            mFields[(int)FieldName.MoveSpeedDebuff] = new MoveSpeedDebuff();

            mFields[(int)FieldName.ExpBonus] = new ExpBonus();

            mFields[(int)FieldName.Element] = new Element();
            mFields[(int)FieldName.ElementWeapon] = new ElementWeapon();
            mFields[(int)FieldName.ElementSideEffect] = new ElementSideEffect();

            mFields[(int)FieldName.ManaReduceBonus] = new ManaReduceBonus();
            mFields[(int)FieldName.ManaReducePercBonus] = new ManaReducePercBonus();

            mFields[(int)FieldName.SkillAffect] = new SimpleField();

            mFields[(int)FieldName.Healing] = new Healing();
            mFields[(int)FieldName.HealingRejuvenation] = new HealingRejuvenation();
            mFields[(int)FieldName.HealingPoint] = new HealingPoint();
            mFields[(int)FieldName.HealingEffect] = new HealingEffect();
            mFields[(int)FieldName.HealingBuff] = new HealingBuff();
            mFields[(int)FieldName.HealingDebuff] = new HealingDebuff();

            mFields[(int)FieldName.HealthPotion] = new SimpleField();
            mFields[(int)FieldName.ManaPotion] = new SimpleField();

            mFields[(int)FieldName.Health] = new Health();
            mFields[(int)FieldName.HealthMax] = new HealthMax();
            mFields[(int)FieldName.HealthBonus] = new HealthBonus();
            mFields[(int)FieldName.HealthPercBonus] = new HealthPercentBonus();
            mFields[(int)FieldName.HealthBonus_NoScore] = new SimpleField();
            mFields[(int)FieldName.HealthPercBonus_NoScore] = new SimpleField();
            mFields[(int)FieldName.HealthBase] = new HealthBase();

            mFields[(int)FieldName.HealthRegen] = new HealthRegen();
            mFields[(int)FieldName.HealthRegenBonus] = new HealthRegenBonus();
            mFields[(int)FieldName.HealthRegenPercBonus] = new HealthRegenPercBonus();
            mFields[(int)FieldName.HealthRegenBonus_NoScore] = new HealthRegenBonus_NoScore();
            mFields[(int)FieldName.HealthRegenPercBonus_NoScore] = new HealthRegenPercBonus_NoScore();

            mFields[(int)FieldName.Mana] = new Mana();
            mFields[(int)FieldName.ManaBase] = new ManaBase();
            mFields[(int)FieldName.ManaMax] = new ManaMax();
            mFields[(int)FieldName.ManaBonus] = new ManaBonus();
            mFields[(int)FieldName.ManaPercBonus] = new ManaPercBonus();
            mFields[(int)FieldName.ManaBonus_NoScore] = new ManaBonus_NoScore();
            mFields[(int)FieldName.ManaPercBonus_NoScore] = new ManaPercBonus_NoScore();

            mFields[(int)FieldName.ManaRegen] = new ManaRegen();
            mFields[(int)FieldName.ManaRegenBonus] = new ManaRegenBonus();
            mFields[(int)FieldName.ManaRegenPercBonus] = new ManaRegenPercBonus();
            mFields[(int)FieldName.ManaRegenBonus_NoScore] = new ManaRegenBonus_NoScoe();
            mFields[(int)FieldName.ManaRegenPercBonus_NoScore] = new ManaRegenPercBonus_NoScore();

            mFields[(int)FieldName.EnergyShield] = new EnergyShield();

            mFields[(int)FieldName.WeaponAttack] = new WeaponAttack();
            mFields[(int)FieldName.WeaponAttackBase] = new WeaponAttackBase();
            mFields[(int)FieldName.WeaponAttackBonus] = new WeaponAttackBonus();
            mFields[(int)FieldName.WeaponAttackPercBonus] = new WeaponAttackPercBonus();

            mFields[(int)FieldName.Attack] = new Attack();
            mFields[(int)FieldName.AttackBonus] = new AttackBonus();
            mFields[(int)FieldName.AttackPercBonus] = new AttackPercentBonus();
            mFields[(int)FieldName.AttackBase] = new AttackBase();
            mFields[(int)FieldName.AttackBonus_NoScore] = new SimpleField();
            mFields[(int)FieldName.AttackPercBonus_NoScore] = new SimpleField();

            mFields[(int)FieldName.Armor] = new Armor();
            mFields[(int)FieldName.ArmorItem] = new ArmorItem();
            mFields[(int)FieldName.ArmorBonus] = new ArmorBonus();
            mFields[(int)FieldName.ArmorPercBonus] = new ArmorPercBonus();
            mFields[(int)FieldName.ArmorBase] = new ArmorBase();
            mFields[(int)FieldName.ArmorBonus_NoScore] = new SimpleField();
            mFields[(int)FieldName.ArmorPercBonus_NoScore] = new SimpleField();

            mFields[(int)FieldName.IgnoreArmor] = new IgnoreArmor();
            mFields[(int)FieldName.IgnoreArmorBonus] = new IgnoreArmorBonus();
            mFields[(int)FieldName.IgnoreArmorBase] = new IgnoreArmorBase();

            mFields[(int)FieldName.BlockRate] = new BlockRate();
            mFields[(int)FieldName.BlockValue] = new BlockValue();
            mFields[(int)FieldName.BlockValueBonus] = new BlockValueBonus();
            mFields[(int)FieldName.BlockValuePercBonus] = new BlockValuePercBonus();
            mFields[(int)FieldName.BlockValueBonus_NoScore] = new BlockValueBonus_NoScore();
            mFields[(int)FieldName.BlockValuePercBonus_NoScore] = new BlockValuePercBonus_NoScore();
            mFields[(int)FieldName.AbsorbDamage] = new AbsorbDamageField();

            mFields[(int)FieldName.Accuracy] = new Accuracy();
            mFields[(int)FieldName.AccuracyBase] = new AccuracyBase();
            mFields[(int)FieldName.AccuracyBonus] = new AccuracyBonus();
            mFields[(int)FieldName.AccuracyPercBonus] = new AccuracyPercBonus();
            mFields[(int)FieldName.AccuracyBonus_NoScore] = new AccuracyBonus_NoScore();
            mFields[(int)FieldName.AccuracyPercBonus_NoScore] = new AccuracyPercBonus_NoScore();

            mFields[(int)FieldName.Evasion] = new Evasion();
            mFields[(int)FieldName.EvasionBase] = new EvasionBase();
            mFields[(int)FieldName.EvasionBonus] = new EvasionBonus();
            mFields[(int)FieldName.EvasionPercBonus] = new EvasionPercBonus();
            mFields[(int)FieldName.EvasionBonus_NoScore] = new SimpleField();
            mFields[(int)FieldName.EvasionPercBonus_NoScore] = new SimpleField();

            mFields[(int)FieldName.Critical] = new Critical();
            mFields[(int)FieldName.CriticalBase] = new CriticalBase();
            mFields[(int)FieldName.CriticalBonus] = new CriticalBonus();
            mFields[(int)FieldName.CriticalPercBonus] = new CriticalPercBonus();
            mFields[(int)FieldName.CriticalBonus_NoScore] = new SimpleField();
            mFields[(int)FieldName.CriticalPercBonus_NoScore] = new SimpleField();

            mFields[(int)FieldName.Cocritical] = new Cocritical();
            mFields[(int)FieldName.CocriticalBase] = new CocriticalBase();
            mFields[(int)FieldName.CocriticalBonus] = new CocriticalBonus();
            mFields[(int)FieldName.CocriticalPercBonus] = new CocriticalPercBonus();
            mFields[(int)FieldName.CocriticalBonus_NoScore] = new SimpleField();
            mFields[(int)FieldName.CocriticalPercBonus_NoScore] = new SimpleField();

            mFields[(int)FieldName.CriticalDamage] = new CriticalDamage();
            //mFields[(int)FieldName.CriticalDamageBase] = new CriticalDamageBase();
            mFields[(int)FieldName.CriticalDamageBonus] = new CriticalDamageBonus();
            //mFields[(int)FieldName.CriticalDamagePercBonus] = new CriticalDamagePercBonus();
            mFields[(int)FieldName.CriticalDamageBonus_NoScore] = new SimpleField();
            mFields[(int)FieldName.CriticalDamagePercBonus_NoScore] = new SimpleField();

            mFields[(int)FieldName.SmashDamage] = new SmashDamage();
            mFields[(int)FieldName.SmashDamageBonus] = new SmashDamageBonus();
            mFields[(int)FieldName.SmashDamagePercBonus] = new SmashDamagePercBonus();
            //mFields[(int)FieldName.SmashDamageBonus_NoScore] = new SmashDamageBonus_NoScore();
            //mFields[(int)FieldName.SmashDamagePercBonus_NoScore] = new SmashDamagePercBonus_NoScore();

            mFields[(int)FieldName.SliceDamage] = new SliceDamage();
            mFields[(int)FieldName.SliceDamageBonus] = new SliceDamageBonus();
            mFields[(int)FieldName.SliceDamagePercBonus] = new SliceDamagePercBonus();
            //mFields[(int)FieldName.SliceDamageBonus_NoScore] = new SliceDamageBonus_NoScore();
            //mFields[(int)FieldName.SliceDamagePercBonus_NoScore] = new SliceDamagePercBonus_NoScore();

            mFields[(int)FieldName.PierceDamage] = new PierceDamage();
            mFields[(int)FieldName.PierceDamageBonus] = new PierceDamageBonus();
            mFields[(int)FieldName.PierceDamagePercBonus] = new PierceDamagePercBonus();
            //mFields[(int)FieldName.PierceDamageBonus_NoScore] = new PierceDamageBonus_NoScore();
            //mFields[(int)FieldName.PierceDamagePercBonus_NoScore] = new PierceDamagePercBonus_NoScore();

            mFields[(int)FieldName.SmashDefense] = new SmashDefense();
            mFields[(int)FieldName.SmashDefenseBonus] = new SmashDefenseBonus();
            mFields[(int)FieldName.SmashDefensePercBonus] = new SmashDefensePercBonus();
            //mFields[(int)FieldName.SmashDefenseBonus_NoScore] = new SmashDefenseBonus_NoScore();
            //mFields[(int)FieldName.SmashDefensePercBonus_NoScore] = new SmashDefensePercBonus_NoScore();

            mFields[(int)FieldName.SliceDefense] = new SliceDefense();
            mFields[(int)FieldName.SliceDefenseBonus] = new SliceDefenseBonus();
            mFields[(int)FieldName.SliceDefensePercBonus] = new SliceDefensePercBonus();
            //mFields[(int)FieldName.SliceDefenseBonus_NoScore] = new SliceDefenseBonus_NoScore();
            //mFields[(int)FieldName.SliceDefensePercBonus_NoScore] = new SliceDefensePercBonus_NoScore();

            mFields[(int)FieldName.PierceDefense] = new PierceDefense();
            mFields[(int)FieldName.PierceDefenseBonus] = new PierceDefenseBonus();
            mFields[(int)FieldName.PierceDefensePercBonus] = new PierceDefensePercBonus();
            //mFields[(int)FieldName.PierceDefenseBonus_NoScore] = new PierceDefenseBonus_NoScore();
            //mFields[(int)FieldName.PierceDefensePercBonus_NoScore] = new PierceDefensePercBonus_NoScore();

            mFields[(int)FieldName.NullDamage] = new NullDamage();
            mFields[(int)FieldName.NullDamageBonus] = new NullDamageBonus();
            mFields[(int)FieldName.NullDamagePercBonus] = new NullDamagePercBonus();

            mFields[(int)FieldName.NullDefense] = new NullDefense();
            mFields[(int)FieldName.NullDefenseBonus] = new NullDefenseBonus();
            mFields[(int)FieldName.NullDefensePercBonus] = new NullDefensePercBonus();

            mFields[(int)FieldName.MetalDamage] = new MetalDamage();
            mFields[(int)FieldName.MetalDamageBonus] = new MetalDamageBonus();
            mFields[(int)FieldName.MetalDamagePercBonus] = new MetalDamagePercBonus();

            mFields[(int)FieldName.WoodDamage] = new WoodDamage();
            mFields[(int)FieldName.WoodDamageBonus] = new WoodDamageBonus();
            mFields[(int)FieldName.WoodDamagePercBonus] = new WoodDamagePercBonus();

            mFields[(int)FieldName.EarthDamage] = new EarthDamage();
            mFields[(int)FieldName.EarthDamageBonus] = new EarthDamageBonus();
            mFields[(int)FieldName.EarthDamagePercBonus] = new EarthDamagePercBonus();

            mFields[(int)FieldName.WaterDamage] = new WaterDamage();
            mFields[(int)FieldName.WaterDamageBonus] = new WaterDamageBonus();
            mFields[(int)FieldName.WaterDamagePercBonus] = new WaterDamagePercBonus();

            mFields[(int)FieldName.FireDamage] = new FireDamage();
            mFields[(int)FieldName.FireDamageBonus] = new FireDamageBonus();
            mFields[(int)FieldName.FireDamagePercBonus] = new FireDamagePercBonus();

            mFields[(int)FieldName.NullDefense] = new NullDefense();
            mFields[(int)FieldName.NullDefenseBonus] = new NullDefenseBonus();
            mFields[(int)FieldName.NullDefensePercBonus] = new NullDefensePercBonus();
            mFields[(int)FieldName.NullDefenseBonus_NoScore] = new NullDefensePercBonus_NoScore();

            mFields[(int)FieldName.MetalDefense] = new MetalDefense();
            mFields[(int)FieldName.MetalDefenseBonus] = new MetalDefenseBonus();
            mFields[(int)FieldName.MetalDefensePercBonus] = new MetalDefensePercBonus();
            mFields[(int)FieldName.MetalDefenseBonus_NoScore] = new MetalDefenseBonus_NoScore();
            mFields[(int)FieldName.MetalDefensePercBonus_NoScore] = new MetalDefensePercBonus_NoScore();

            mFields[(int)FieldName.WoodDefense] = new WoodDefense();
            mFields[(int)FieldName.WoodDefenseBonus] = new WoodDefenseBonus();
            mFields[(int)FieldName.WoodDefensePercBonus] = new WoodDefensePercBonus();
            mFields[(int)FieldName.WoodDefenseBonus_NoScore] = new WoodDefenseBonus_NoScore();
            mFields[(int)FieldName.WoodDefensePercBonus_NoScore] = new WoodDefensePercBonus_NoScore();

            mFields[(int)FieldName.EarthDefense] = new EarthDefense();
            mFields[(int)FieldName.EarthDefenseBonus] = new EarthDefenseBonus();
            mFields[(int)FieldName.EarthDefensePercBonus] = new EarthDefensePercBonus();
            mFields[(int)FieldName.EarthDefenseBonus_NoScore] = new EarthDefenseBonus_NoScore();
            mFields[(int)FieldName.EarthDefensePercBonus_NoScore] = new EarthDefensePercBonus_NoScore();

            mFields[(int)FieldName.WaterDefense] = new WaterDefense();
            mFields[(int)FieldName.WaterDefenseBonus] = new WaterDefenseBonus();
            mFields[(int)FieldName.WaterDefensePercBonus] = new WaterDefensePercBonus();
            mFields[(int)FieldName.WaterDefenseBonus_NoScore] = new WaterDefenseBonus_NoScore();
            mFields[(int)FieldName.WaterDefensePercBonus_NoScore] = new WaterDefensePercBonus_NoScore();

            mFields[(int)FieldName.FireDefense] = new FireDefense();
            mFields[(int)FieldName.FireDefenseBonus] = new FireDefenseBonus();
            mFields[(int)FieldName.FireDefensePercBonus] = new FireDefensePercBonus();
            mFields[(int)FieldName.FireDefenseBonus_NoScore] = new FireDefenseBonus_NoScore();
            mFields[(int)FieldName.FireDefensePercBonus_NoScore] = new FireDefensePercBonus_NoScore();

            mFields[(int)FieldName.VSHumanDamage] = new VSHumanDamage();
            mFields[(int)FieldName.VSHumanDamageBonus] = new VSHumanDamageBonus();
            mFields[(int)FieldName.VSHumanDamagePercBonus] = new VSHumanDamagePercBonus();
            mFields[(int)FieldName.VSHumanDamageBonus_NoScore] = new VSHumanDamageBonus_NoScore();
            mFields[(int)FieldName.VSHumanDamagePercBonus_NoScore] = new VSHumanDamagePercBonus_NoScore();

            mFields[(int)FieldName.VSZombieDamage] = new VSZombieDamage();
            mFields[(int)FieldName.VSZombieDamageBonus] = new VSZombieDamageBonus();
            mFields[(int)FieldName.VSZombieDamagePercBonus] = new VSZombieDamagePercBonus();
            mFields[(int)FieldName.VSZombieDamageBonus_NoScore] = new VSZombieDamageBonus_NoScore();
            mFields[(int)FieldName.VSZombieDamagePercBonus_NoScore] = new VSZombieDamagePercBonus_NoScore();

            mFields[(int)FieldName.VSVampireDamage] = new VSVampireDamage();
            mFields[(int)FieldName.VSVampireDamageBonus] = new VSVampireDamageBonus();
            mFields[(int)FieldName.VSVampireDamagePercBonus] = new VSVampireDamagePercBonus();
            mFields[(int)FieldName.VSVampireDamageBonus_NoScore] = new VSVampireDamageBonus_NoScore();
            mFields[(int)FieldName.VSVampireDamagePercBonus_NoScore] = new VSVampireDamagePercBonus_NoScore();

            mFields[(int)FieldName.VSBeastDamage] = new VSBeastDamage();
            mFields[(int)FieldName.VSBeastDamageBonus] = new VSBeastDamageBonus();
            mFields[(int)FieldName.VSBeastDamagePercBonus] = new VSBeastDamagePercBonus();
            mFields[(int)FieldName.VSBeastDamageBonus_NoScore] = new VSBeastDamageBonus_NoScore();
            mFields[(int)FieldName.VSBeastDamagePercBonus_NoScore] = new VSBeastDamagePercBonus_NoScore();

            mFields[(int)FieldName.VSPlantDamage] = new VSPlantDamage();
            mFields[(int)FieldName.VSPlantDamageBonus] = new VSPlantDamageBonus();
            mFields[(int)FieldName.VSPlantDamagePercBonus] = new VSPlantDamagePercBonus();
            mFields[(int)FieldName.VSPlantDamageBonus_NoScore] = new VSPlantDamageBonus_NoScore();
            mFields[(int)FieldName.VSPlantDamagePercBonus_NoScore] = new VSPlantDamagePercBonus_NoScore();

            mFields[(int)FieldName.VSHumanDefense] = new VSHumanDefense();
            mFields[(int)FieldName.VSHumanDefenseBonus] = new VSHumanDefenseBonus();
            mFields[(int)FieldName.VSHumanDefensePercBonus] = new VSHumanDefensePercBonus();
            mFields[(int)FieldName.VSHumanDefenseBonus_NoScore] = new VSHumanDefenseBonus_NoScore();
            mFields[(int)FieldName.VSHumanDefensePercBonus_NoScore] = new VSHumanDefensePercBonus_NoScore();

            mFields[(int)FieldName.VSZombieDefense] = new VSZombieDefense();
            mFields[(int)FieldName.VSZombieDefenseBonus] = new VSZombieDefenseBonus();
            mFields[(int)FieldName.VSZombieDefensePercBonus] = new VSZombieDefensePercBonus();
            mFields[(int)FieldName.VSZombieDefenseBonus_NoScore] = new VSZombieDefenseBonus_NoScore();
            mFields[(int)FieldName.VSZombieDefensePercBonus_NoScore] = new VSZombieDefensePercBonus_NoScore();

            mFields[(int)FieldName.VSVampireDefense] = new VSVampireDefense();
            mFields[(int)FieldName.VSVampireDefenseBonus] = new VSVampireDefenseBonus();
            mFields[(int)FieldName.VSVampireDefensePercBonus] = new VSVampireDefensePercBonus();
            mFields[(int)FieldName.VSVampireDefenseBonus_NoScore] = new VSVampireDefenseBonus_NoScore();
            mFields[(int)FieldName.VSVampireDefensePercBonus_NoScore] = new VSVampireDefensePercBonus_NoScore();

            mFields[(int)FieldName.VSBeastDefense] = new VSBeastDefense();
            mFields[(int)FieldName.VSBeastDefenseBonus] = new VSBeastDefenseBonus();
            mFields[(int)FieldName.VSBeastDefensePercBonus] = new VSBeastDefensePercBonus();
            mFields[(int)FieldName.VSBeastDefenseBonus_NoScore] = new VSBeastDefenseBonus_NoScore();
            mFields[(int)FieldName.VSBeastDefensePercBonus_NoScore] = new VSBeastDefensePercBonus_NoScore();

            mFields[(int)FieldName.VSPlantDefense] = new VSPlantDefense();
            mFields[(int)FieldName.VSPlantDefenseBonus] = new VSPlantDefenseBonus();
            mFields[(int)FieldName.VSPlantDefensePercBonus] = new VSPlantDefensePercBonus();
            mFields[(int)FieldName.VSPlantDefenseBonus_NoScore] = new VSPlantDefenseBonus_NoScore();
            mFields[(int)FieldName.VSPlantDefensePercBonus_NoScore] = new VSPlantDefensePercBonus_NoScore();

            mFields[(int)FieldName.VSNullDamage] = new VSNullDamage();
            mFields[(int)FieldName.VSNullPercDamage] = new VSNullDamagePercBonus();

            mFields[(int)FieldName.VSMetalDamage] = new VSMetalDamage();
            mFields[(int)FieldName.VSMetalPercDamage] = new VSMetalDamagePercBonus();

            mFields[(int)FieldName.VSWoodDamage] = new VSWoodDamage();
            mFields[(int)FieldName.VSWoodPercDamage] = new VSWoodDamagePercBonus();

            mFields[(int)FieldName.VSEarthDamage] = new VSEarthDamage();
            mFields[(int)FieldName.VSEarthPercDamage] = new VSEarthDamagePercBonus();

            mFields[(int)FieldName.VSFireDamage] = new VSFireDamage();
            mFields[(int)FieldName.VSFirePercDamage] = new VSFireDamagePercBonus();

            mFields[(int)FieldName.VSWaterDamage] = new VSWaterDamage();
            mFields[(int)FieldName.VSWaterPercDamage] = new VSWaterDamagePercBonus();

            mFields[(int)FieldName.VSBossDamage] = new VSBossDamage();
            mFields[(int)FieldName.VSBossDamageBonus] = new VSBossDamageBonus();

            mFields[(int)FieldName.IncreaseFinalDamage] = new IncreaseFinalDamage();
            mFields[(int)FieldName.IncreaseFinalDamageBonus] = new IncreaseFinalDamageBonus();

            mFields[(int)FieldName.DecreaseFinalDamage] = new DecreaseFinalDamage();
            mFields[(int)FieldName.DecreaseFinalDamageBonus] = new DecreaseFinalDamageBonus();

            /*
            mFields[(int)FieldName.CoCriticalDamage] = new CoCriticalDamage();
            mFields[(int)FieldName.CoCriticalDamageBase] = new CoCriticalDamageBase();
            mFields[(int)FieldName.CoCriticalDamageBonus] = new CoCriticalDamageBonus();
            mFields[(int)FieldName.CoCriticalDamagePercBonus] = new CoCriticalDamagePercBonus();
            mFields[(int)FieldName.CoCriticalDamageBonus_NoScore] = new SimpleField();
            mFields[(int)FieldName.CoCriticalDamagePercBonus_NoScore] = new SimpleField();
            */

            //mFields[(int)FieldName.TalentPointScissors] = new TalentPointScissorsField();
            //mFields[(int)FieldName.TalentPointStone] = new TalentPointStoneField();
            //mFields[(int)FieldName.TalentPointCloth] = new TalentPointClothField();

            //mFields[(int)FieldName.AbsorbDamageBonus] = new AbsorbDamageBonusField();
            //mFields[(int)FieldName.RejSupress] = new SimpleField();
            mFields[(int)FieldName.CombatScore] = new CombatScoreField();

            mTierFieldNames = new List<FieldName>[4]; //Number of tiers. Increase with deep dependencies.

            //Rules of Tiers:
            //1) Fields in the same tier are independent of one another.
            //2) A field from n tier is dependent of at least one field from n-1 tier
            //3) A field should reside at the lowest tier possible without violating the above 2 rules.

            mTierFieldNames[0] = new List<FieldName>()
            {
                FieldName.StrengthBase,
                FieldName.StrengthBonus,
                FieldName.StrengthPercBonus,
                FieldName.AgilityBase,
                FieldName.AgilityBonus,
                FieldName.AgilityPercBonus,
                FieldName.DexterityBase,
                FieldName.DexterityBonus,
                FieldName.DexterityPercBonus,
                FieldName.ConstitutionBase,
                FieldName.ConstitutionBonus,
                FieldName.ConstitutionPercBonus,
                FieldName.IntelligenceBase,
                FieldName.IntelligenceBonus,
                FieldName.IntelligencePercBonus,
                FieldName.ExpBonus,
                FieldName.HealthBase,
                FieldName.HealthBonus,
                FieldName.HealthPercBonus,
                FieldName.HealthRegenBonus,
                FieldName.HealthRegenPercBonus,
                FieldName.ManaBase,
                FieldName.ManaBonus,
                FieldName.ManaPercBonus,
                FieldName.ManaRegenBonus,
                FieldName.ManaRegenPercBonus,
                FieldName.EnergyShield,
                FieldName.WeaponAttackBonus,
                FieldName.WeaponAttackPercBonus,
                FieldName.AttackBonus,
                FieldName.AttackPercBonus,
                FieldName.ArmorBonus,
                FieldName.ArmorPercBonus,
                FieldName.IgnoreArmorBonus,

                FieldName.CriticalBase,
                FieldName.CriticalBonus,
                FieldName.CriticalPercBonus,
                FieldName.CocriticalBase,
                FieldName.CocriticalBonus,
                FieldName.CocriticalPercBonus,
                FieldName.EvasionBase,
                FieldName.EvasionBonus,
                FieldName.EvasionPercBonus,
                FieldName.AccuracyBase,
                FieldName.AccuracyBonus,
                FieldName.AccuracyPercBonus,

                FieldName.VSBossDamageBonus,
                FieldName.VSHumanDamageBonus,
                FieldName.VSHumanDamagePercBonus,
                FieldName.VSZombieDamageBonus,
                FieldName.VSZombieDamagePercBonus,
                FieldName.VSVampireDamageBonus,
                FieldName.VSVampireDamagePercBonus,
                FieldName.VSBeastDamageBonus,
                FieldName.VSBeastDamagePercBonus,
                FieldName.VSPlantDamageBonus,
                FieldName.VSPlantDamagePercBonus,

                FieldName.VSHumanDefenseBonus,
                FieldName.VSHumanDefensePercBonus,
                FieldName.VSZombieDefenseBonus,
                FieldName.VSZombieDefensePercBonus,
                FieldName.VSVampireDefenseBonus,
                FieldName.VSVampireDefensePercBonus,
                FieldName.VSBeastDefenseBonus,
                FieldName.VSBeastDefensePercBonus,
                FieldName.VSPlantDefenseBonus,
                FieldName.VSPlantDefensePercBonus,

                FieldName.PierceDamageBonus,
                FieldName.PierceDamagePercBonus,
                FieldName.PierceDefenseBonus,
                FieldName.PierceDefensePercBonus,

                FieldName.SmashDamageBonus,
                FieldName.SmashDamagePercBonus,
                FieldName.SmashDefenseBonus,
                FieldName.SmashDefensePercBonus,

                FieldName.SliceDamageBonus,
                FieldName.SliceDamagePercBonus,
                FieldName.SliceDefenseBonus,
                FieldName.SliceDefensePercBonus,

                FieldName.NullDamageBonus,
                FieldName.NullDamagePercBonus,
                FieldName.NullDefenseBonus,
                FieldName.NullDefensePercBonus,

                FieldName.MetalDamageBonus,
                FieldName.MetalDamagePercBonus,
                FieldName.WoodDamageBonus,
                FieldName.WoodDamagePercBonus,
                FieldName.EarthDamageBonus,
                FieldName.EarthDamagePercBonus,
                FieldName.WaterDamageBonus,
                FieldName.WaterDamagePercBonus,
                FieldName.FireDamageBonus,
                FieldName.FireDamagePercBonus,

                FieldName.MetalDefenseBonus,
                FieldName.MetalDefensePercBonus,
                FieldName.WoodDefenseBonus,
                FieldName.WoodDefensePercBonus,
                FieldName.EarthDefenseBonus,
                FieldName.EarthDefensePercBonus,
                FieldName.WaterDefenseBonus,
                FieldName.WaterDefensePercBonus,
                FieldName.FireDefenseBonus,
                FieldName.FireDefensePercBonus,

                FieldName.IncreaseFinalDamageBonus,
                FieldName.DecreaseFinalDamageBonus,

                FieldName.SkillAffect,

                FieldName.BlockRate,
                FieldName.BlockValueBonus,
                FieldName.BlockValuePercBonus,

                FieldName.MoveSpeedBuff,
                FieldName.MoveSpeedDebuff,
                FieldName.ManaReduceBonus,
                FieldName.ManaReducePercBonus
            };

            mTierFieldNames[1] = new List<FieldName>()
            {
                 FieldName.Strength,
                 FieldName.Agility,
                 FieldName.Dexterity,
                 FieldName.Constitution,
                 FieldName.Intelligence,
                 FieldName.WeaponAttackBase,
                 FieldName.AttackBase,
                 FieldName.ArmorBase,
                 FieldName.IgnoreArmorBase,

                //FieldName.HealthMax,

                 FieldName.Critical,
                 FieldName.CriticalDamage,
                 FieldName.Cocritical,
                 //FieldName.CoCriticalDamage,
                 FieldName.Evasion,
                 FieldName.Accuracy,
                 //FieldName.AbsorbDamageBonus,

                 FieldName.VSBossDamage,

                 FieldName.PierceDamage,
                 FieldName.PierceDefense,

                 FieldName.SmashDamage,
                 FieldName.SmashDefense,

                 FieldName.SliceDamage,
                 FieldName.SliceDefense,

                 FieldName.NullDamage,
                 FieldName.NullDefense,

                 FieldName.MetalDamage,
                 FieldName.WoodDamage,
                 FieldName.EarthDamage,
                 FieldName.WaterDamage,
                 FieldName.FireDamage,
                 FieldName.MetalDefense,
                 FieldName.WoodDefense,
                 FieldName.EarthDefense,
                 FieldName.WaterDefense,
                 FieldName.FireDefense,

                 FieldName.VSHumanDamage,
                 FieldName.VSZombieDamage,
                 FieldName.VSVampireDamage,
                 FieldName.VSBeastDamage,
                 FieldName.VSPlantDamage,

                 FieldName.VSHumanDefense,
                 FieldName.VSZombieDefense,
                 FieldName.VSVampireDefense,
                 FieldName.VSBeastDefense,
                 FieldName.VSPlantDefense,

                 FieldName.IncreaseFinalDamage,
                 FieldName.DecreaseFinalDamage,

                 FieldName.BlockValue,

                 FieldName.MoveSpeed
            };

            mTierFieldNames[2] = new List<FieldName>()
            {
                FieldName.HealthMax,
                FieldName.HealthRegen,
                FieldName.ManaMax,
                FieldName.ManaRegen,
                FieldName.WeaponAttack,
                FieldName.Attack,
                FieldName.IgnoreArmor,

                FieldName.CombatScore,
                //FieldName.AbsorbDamage,
            };

            mTierFieldNames[3] = new List<FieldName>()
            {
                FieldName.Armor,
            };
        }

        public float GetField(EquipmentAttributeType eqAtribType)
        {
            FieldName fieldname = GetFieldNameByEquipmentAbilityType(eqAtribType);
            return mFields[(int)fieldname].GetValue();
        }

        //public void ComputeEquippedCombatStats(EquipItem equipItem, int gemGroup, int selectedGemGroup, bool bAdd) {
        //    float plusminus = (bAdd == true ? 1.0f : -1.0f); //true:equip new item; false:unequip item

        //    // Get equipment attribute value
        //    FieldName currBasicField = GetFieldNameByEquipmentAbilityType((EquipmentAttributeType)equipItem.BasicAttributeType);
        //    int currentAttribBaseVal = (int)mFields[(int)currBasicField].GetValue();
        //    AddToField(currBasicField, (int)(equipItem.BasicAttributeVal * plusminus));

        //    List<ushort> gemIDs = equipItem.DecodeGemIDs();
        //    List<ushort> gemAttributes = equipItem.DecodeGemAttributes();

        //    if (gemGroup == selectedGemGroup) {
        //        const int gemGroupSize = 2;
        //        for (int i = 0; i < gemGroupSize; ++i) {
        //            int pos = (gemGroup * gemGroupSize) + i;
        //            if (gemIDs[pos] > 0) {
        //                SideEffectJson gemAttrib = SideEffectRepo.GetSideEffect(gemAttributes[pos]);
        //                if (gemAttrib != null) {
        //                    FieldName currGemField = GetFieldNameByEffectType(gemAttrib.effecttype);
        //                    float gemAttribVal = gemAttrib.max;
        //                    AddToField(currGemField, (int)(gemAttribVal * plusminus));
        //                }
        //            }
        //        }
        //    }
        //}

        public void ComputeEquippedCombatStats(Equipment equipment, float multiplier, bool isEquipping, List<int> buffList = null)
        {
            int sign = isEquipping ? 1 : -1;

            // get the attribute to add
            // New stats values calculation: Base Equipment Attribute + Base Equipment Attribute * multiplier
            // Base Attribute Types: Attack -> Weapons, Accessory | Defence -> Armor
            // Add buffList side effect ids if buffList is not null and only if isEquipping is true

            if (buffList == null) return;

            float val = 0;

            // add to stats
            switch (equipment.EquipmentJson.equiptype)
            {
                case EquipmentType.Weapon:
                case EquipmentType.Accessory:
                    //int value = equipment.
                    //get side effects
                   
                    foreach(int ids in buffList)
                    {
                        SideEffectJson se = SideEffectRepo.GetSideEffect(ids);
                        val += se.max;
                    }
                    val = (val + val) * multiplier * sign;
                    AddToField(FieldName.WeaponAttackBonus, val);
                    break;
                case EquipmentType.Armor:
                    foreach (int ids in buffList)
                    {
                        SideEffectJson se = SideEffectRepo.GetSideEffect(ids);
                        val += se.max;
                    }
                    val = (val + val) * multiplier * sign;
                    AddToField(FieldName.ArmorBonus, val);                 
                    break;
            }
        }

        private EquipmentAttributeType GetEquipmentAbilityTypeByEffectType(EffectType efftype)
        {
            //return percent value
            EquipmentAttributeType eqType = EquipmentAttributeType.Attack;
            //switch (efftype) {
            //    case EffectType.Stats_HealthMax:
            //        eqType = EquipmentAttributeType.Health;
            //        break;

            //    case EffectType.StatsAttack_Attack:
            //        break;

            //    case EffectType.StatsDefence_Armor:
            //        eqType = EquipmentAttributeType.Armor;
            //        break;

            //    case EffectType.StatsAttack_CriticalDamage:
            //        eqType = EquipmentAttributeType.CriticalDamage;
            //        break;

            //    case EffectType.StatsDefence_CoCriticalDamage:
            //        eqType = EquipmentAttributeType.CocriticalDamage;
            //        break;

            //    case EffectType.StatsAttack_Accuracy:
            //        eqType = EquipmentAttributeType.Accuracy;
            //        break;

            //    case EffectType.StatsDefence_Evasion:
            //        eqType = EquipmentAttributeType.Evasion;
            //        break;
            //}
            return eqType;
        }

        private FieldName GetFieldNameByEffectType(EffectType efftype)
        {
            //return percent value
            FieldName fname = FieldName.AttackBonus;
            switch (efftype)
            {
                //case EffectType.Stats_HealthMax:
                //    fname = FieldName.HealthBonus;
                //    break;

                //case EffectType.StatsAttack_Attack:
                //    break;

                case EffectType.StatsDefence_Armor:
                    fname = FieldName.ArmorBonus;
                    break;

                case EffectType.StatsAttack_Critical:
                    fname = FieldName.CriticalBonus;
                    break;

                case EffectType.StatsAttack_CriticalDamage:
                    fname = FieldName.CriticalDamageBonus;
                    break;

                case EffectType.StatsDefence_CoCritical:
                    fname = FieldName.CocriticalBonus;
                    break;

                //case EffectType.StatsDefence_CoCriticalDamage:
                //    fname = FieldName.CoCriticalDamageBonus;
                //    break;

                case EffectType.StatsAttack_Accuracy:
                    fname = FieldName.AccuracyBonus;
                    break;

                case EffectType.StatsDefence_Evasion:
                    fname = FieldName.EvasionBonus;
                    break;
            }
            return fname;
        }

        private FieldName GetFieldNameByEquipmentAbilityType(EquipmentAttributeType abtype)
        {
            //return percent value
            FieldName fname = FieldName.AttackBonus;
            switch (abtype)
            {
                case EquipmentAttributeType.Health:
                    fname = FieldName.HealthBonus;
                    break;

                case EquipmentAttributeType.Attack:
                    fname = FieldName.AttackBonus;
                    break;

                case EquipmentAttributeType.Armor:
                    fname = FieldName.ArmorBonus;
                    break;

                case EquipmentAttributeType.CriticalDamage:
                    fname = FieldName.CriticalDamageBonus;
                    break;

                //case EquipmentAttributeType.CocriticalDamage:
                //    fname = FieldName.CoCriticalDamageBonus;
                //    break;

                case EquipmentAttributeType.Accuracy:
                    fname = FieldName.AccuracyBonus;
                    break;

                case EquipmentAttributeType.Evasion:
                    fname = FieldName.EvasionBonus;
                    break;
            }
            return fname;
        }
    }


    public class MonsterCombatStats : CombatStats
    {
        public override void ComputeAll()
        {
            if (SuppressComputeAll)
                return;

            for (int i = 0; i < mTierFieldNames.Length; i++)
            {
                List<FieldName> currentTierNames = mTierFieldNames[i];
                foreach (FieldName name in currentTierNames)
                {
                    CombatStatsField field = mFields[(int)name];
                    if (field.Dirty)
                    {
                        field.Compute(mFields);
                        field.Dirty = false;
                    }
                }
            }
            if (mActor != null)
            {
                mActor.UpdateLocalSkillPassiveStats();
                mActor.OnComputeCombatStats();
            }
        }

        private class SimpleField : CombatStatsField
        {
            public SimpleField()
            {
                fieldValue = 0;
                children = new FieldName[] { };
            }
        }

        private class SimpleFieldModifier : CombatStatsField
        {
            public SimpleFieldModifier(FieldName name)
            {
                fieldValue = 0;
                children = new FieldName[] { name };
            }
        }

        private class Attack : CombatStatsField
        {
            public Attack()
            {
                fieldValue = 0;
                children = new FieldName[] { };
            }

            public override void Compute(CombatStatsField[] fields, LocalCombatStats localCombatStats = null, ActorSynStats actorSynStats = null)
            {
                fieldValue = fields[(int)FieldName.AttackBase].GetValue() + fields[(int)FieldName.AttackBonus].GetValue();
                fieldValue *= 1.0f + fields[(int)FieldName.AttackPercBonus].GetValue() * 0.01f;
            }
        }

        private class Armor : CombatStatsField
        {
            public Armor()
            {
                fieldValue = 0;
                children = new FieldName[] { };
            }

            public override void Compute(CombatStatsField[] fields, LocalCombatStats localCombatStats = null, ActorSynStats actorSynStats = null)
            {
                fieldValue = fields[(int)FieldName.ArmorBase].GetValue() + fields[(int)FieldName.ArmorBonus].GetValue();
                fieldValue *= 1.0f + fields[(int)FieldName.AttackPercBonus].GetValue() * 0.01f;
            }
        }

        private class Strength : CombatStatsField
        {
            public Strength()
            {
                fieldValue = 0;
                children = new FieldName[] { };
            }

            public override void Compute(CombatStatsField[] fields, LocalCombatStats localCombatStats, ActorSynStats actorSynStats)
            {
                fieldValue = fields[(int)FieldName.StrengthBase].GetValue() + fields[(int)FieldName.StrengthBonus].GetValue();
                fieldValue *= 1.0f + fields[(int)FieldName.StrengthPercBonus].GetValue() * 0.01f;
            }
        }

        private class Agility : CombatStatsField
        {
            public Agility()
            {
                fieldValue = 0;
                children = new FieldName[] { };
            }

            public override void Compute(CombatStatsField[] fields, LocalCombatStats localCombatStats = null, ActorSynStats actorSynStats = null)
            {
                fieldValue = fields[(int)FieldName.AgilityBase].GetValue() + fields[(int)FieldName.AgilityBonus].GetValue();
                fieldValue *= 1.0f + fields[(int)FieldName.AgilityPercBonus].GetValue() * 0.01f;
            }
        }

        private class Dexterity : CombatStatsField
        {
            public Dexterity()
            {
                fieldValue = 0;
                children = new FieldName[] { };
            }

            public override void Compute(CombatStatsField[] fields, LocalCombatStats localCombatStats = null, ActorSynStats actorSynStats = null)
            {
                fieldValue = fields[(int)FieldName.DexterityBase].GetValue() + fields[(int)FieldName.DexterityBonus].GetValue();
                fieldValue *= 1.0f + fields[(int)FieldName.DexterityPercBonus].GetValue() * 0.01f;
            }
        }

        private class Constitution : CombatStatsField
        {
            public Constitution()
            {
                fieldValue = 0;
                children = new FieldName[] { };
            }

            public override void Compute(CombatStatsField[] fields, LocalCombatStats localCombatStats = null, ActorSynStats actorSynStats = null)
            {
                fieldValue = fields[(int)FieldName.ConstitutionBase].GetValue() + fields[(int)FieldName.ConstitutionBonus].GetValue();
                fieldValue *= 1.0f + fields[(int)FieldName.ConstitutionPercBonus].GetValue() * 0.01f;
            }
        }

        private class Intelligence : CombatStatsField
        {
            public Intelligence()
            {
                fieldValue = 0;
                children = new FieldName[] { };
            }

            public override void Compute(CombatStatsField[] fields, LocalCombatStats localCombatStats = null, ActorSynStats actorSynStats = null)
            {
                fieldValue = fields[(int)FieldName.IntelligenceBase].GetValue() + fields[(int)FieldName.IntelligenceBonus].GetValue();
                fieldValue *= 1.0f + fields[(int)FieldName.IntelligencePercBonus].GetValue() * 0.01f;
            }
        }

        private class Accuracy : CombatStatsField
        {
            public Accuracy()
            {
                fieldValue = 0;
                children = new FieldName[] { };
            }

            public override void Compute(CombatStatsField[] fields, LocalCombatStats localCombatStats = null, ActorSynStats actorSynStats = null)
            {
                fieldValue = fields[(int)FieldName.AccuracyBase].GetValue() + fields[(int)FieldName.AccuracyBonus].GetValue();
                fieldValue *= 1.0f + fields[(int)FieldName.AccuracyPercBonus].GetValue() * 0.01f;
            }
        }

        private class Evasion : CombatStatsField
        {
            public Evasion()
            {
                fieldValue = 0;
                children = new FieldName[] { };
            }

            public override void Compute(CombatStatsField[] fields, LocalCombatStats localCombatStats = null, ActorSynStats actorSynStats = null)
            {
                fieldValue = fields[(int)FieldName.EvasionBase].GetValue() + fields[(int)FieldName.EvasionBonus].GetValue();
                fieldValue *= 1.0f + fields[(int)FieldName.EvasionPercBonus].GetValue() * 0.01f;
            }
        }

        private class Critical : CombatStatsField
        {
            public Critical()
            {
                fieldValue = 0;
                children = new FieldName[] { };
            }

            public override void Compute(CombatStatsField[] fields, LocalCombatStats localCombatStats = null, ActorSynStats actorSynStats = null)
            {
                fieldValue = fields[(int)FieldName.CriticalBase].GetValue() + fields[(int)FieldName.CriticalBonus].GetValue();
                fieldValue *= 1.0f + fields[(int)FieldName.CriticalPercBonus].GetValue() * 0.01f;
            }
        }

        private class CriticalDamage : CombatStatsField
        {
            public CriticalDamage()
            {
                fieldValue = 0;
                children = new FieldName[] { };
            }

            public override void Compute(CombatStatsField[] fields, LocalCombatStats localCombatStats = null, ActorSynStats actorSynStats = null)
            {
                fieldValue = fields[(int)FieldName.CriticalDamageBase].GetValue() + fields[(int)FieldName.CriticalDamageBonus].GetValue();
                //fieldValue *= 1.0f + fields[(int)FieldName.CriticalDamagePercBonus].GetValue() * 0.01f;
            }
        }

        private class CoCritical : CombatStatsField
        {
            public CoCritical()
            {
                fieldValue = 0;
                children = new FieldName[] { };
            }

            public override void Compute(CombatStatsField[] fields, LocalCombatStats localCombatStats = null, ActorSynStats actorSynStats = null)
            {
                fieldValue = fields[(int)FieldName.CocriticalBase].GetValue() + fields[(int)FieldName.CocriticalBonus].GetValue();
                fieldValue *= 1.0f + fields[(int)FieldName.CocriticalPercBonus].GetValue() * 0.01f;
            }
        }

        private class Movespeed : CombatStatsField
        {
            public Movespeed()
            {
                fieldValue = 0;
                children = new FieldName[] { };
            }

            public override void Compute(CombatStatsField[] fields, LocalCombatStats localCombatStats = null, ActorSynStats actorSynStats = null)
            {
                fieldValue = fields[(int)FieldName.MoveSpeedBase].GetValue() + (1.0f + (fields[(int)FieldName.MoveSpeedBuff].GetValue() - fields[(int)FieldName.MoveSpeedDebuff].GetValue()) * 0.01f);

                fieldValue = Math.Max(18.0f, Math.Min(0.0f, fieldValue));
            }
        }

        public MonsterCombatStats()
        {
            SuppressComputeAll = false;

            int totalFields = (int)FieldName.LastField;
            mFields = new CombatStatsField[totalFields];

            mFields[(int)FieldName.Health] = new SimpleField();
            mFields[(int)FieldName.HealthMax] = new SimpleField();

            mFields[(int)FieldName.AttackBase] = new SimpleFieldModifier(FieldName.Attack);
            mFields[(int)FieldName.AttackBonus] = new SimpleFieldModifier(FieldName.Attack);
            mFields[(int)FieldName.AttackPercBonus] = new SimpleFieldModifier(FieldName.Attack);
            mFields[(int)FieldName.Attack] = new Attack();

            mFields[(int)FieldName.ArmorBase] = new SimpleFieldModifier(FieldName.Armor);
            mFields[(int)FieldName.ArmorBonus] = new SimpleFieldModifier(FieldName.Armor);
            mFields[(int)FieldName.ArmorPercBonus] = new SimpleFieldModifier(FieldName.Armor);
            mFields[(int)FieldName.Armor] = new Armor();

            mFields[(int)FieldName.StrengthBase] = new SimpleFieldModifier(FieldName.Strength);
            mFields[(int)FieldName.StrengthBonus] = new SimpleFieldModifier(FieldName.Strength);
            mFields[(int)FieldName.StrengthPercBonus] = new SimpleFieldModifier(FieldName.Strength);
            mFields[(int)FieldName.Strength] = new Strength();

            mFields[(int)FieldName.AgilityBase] = new SimpleFieldModifier(FieldName.Agility);
            mFields[(int)FieldName.AgilityBonus] = new SimpleFieldModifier(FieldName.Agility);
            mFields[(int)FieldName.AgilityPercBonus] = new SimpleFieldModifier(FieldName.Agility);
            mFields[(int)FieldName.Agility] = new Agility();

            mFields[(int)FieldName.DexterityBase] = new SimpleFieldModifier(FieldName.Dexterity);
            mFields[(int)FieldName.DexterityBonus] = new SimpleFieldModifier(FieldName.Dexterity);
            mFields[(int)FieldName.DexterityPercBonus] = new SimpleFieldModifier(FieldName.Dexterity);
            mFields[(int)FieldName.Dexterity] = new Dexterity();

            mFields[(int)FieldName.ConstitutionBase] = new SimpleFieldModifier(FieldName.Constitution);
            mFields[(int)FieldName.ConstitutionBonus] = new SimpleFieldModifier(FieldName.Constitution);
            mFields[(int)FieldName.ConstitutionPercBonus] = new SimpleFieldModifier(FieldName.Constitution);
            mFields[(int)FieldName.Constitution] = new Constitution();

            mFields[(int)FieldName.IntelligenceBase] = new SimpleFieldModifier(FieldName.Intelligence);
            mFields[(int)FieldName.IntelligenceBonus] = new SimpleFieldModifier(FieldName.Intelligence);
            mFields[(int)FieldName.IntelligencePercBonus] = new SimpleFieldModifier(FieldName.Intelligence);
            mFields[(int)FieldName.Intelligence] = new Intelligence();

            mFields[(int)FieldName.AccuracyBase] = new SimpleFieldModifier(FieldName.Accuracy);
            mFields[(int)FieldName.AccuracyBonus] = new SimpleFieldModifier(FieldName.Accuracy);
            mFields[(int)FieldName.AccuracyPercBonus] = new SimpleFieldModifier(FieldName.Accuracy);
            mFields[(int)FieldName.Accuracy] = new Accuracy();

            mFields[(int)FieldName.EvasionBase] = new SimpleFieldModifier(FieldName.Evasion);
            mFields[(int)FieldName.EvasionBonus] = new SimpleFieldModifier(FieldName.Evasion);
            mFields[(int)FieldName.EvasionPercBonus] = new SimpleFieldModifier(FieldName.Evasion);
            mFields[(int)FieldName.Evasion] = new Evasion();

            mFields[(int)FieldName.CriticalBase] = new SimpleFieldModifier(FieldName.Critical);
            mFields[(int)FieldName.CriticalBonus] = new SimpleFieldModifier(FieldName.Critical);
            mFields[(int)FieldName.CriticalPercBonus] = new SimpleFieldModifier(FieldName.Critical);
            mFields[(int)FieldName.Critical] = new Critical();

            mFields[(int)FieldName.CriticalDamageBase] = new SimpleFieldModifier(FieldName.CriticalDamage);
            mFields[(int)FieldName.CriticalDamageBonus] = new SimpleFieldModifier(FieldName.CriticalDamage);
            mFields[(int)FieldName.CriticalDamage] = new CriticalDamage();

            mFields[(int)FieldName.CocriticalBase] = new SimpleFieldModifier(FieldName.Cocritical);
            mFields[(int)FieldName.CocriticalBonus] = new SimpleFieldModifier(FieldName.Cocritical);
            mFields[(int)FieldName.CocriticalPercBonus] = new SimpleFieldModifier(FieldName.Cocritical);
            mFields[(int)FieldName.Cocritical] = new CoCritical();

            mFields[(int)FieldName.MoveSpeedBase] = new SimpleFieldModifier(FieldName.MoveSpeed);
            mFields[(int)FieldName.MoveSpeedBuff] = new SimpleFieldModifier(FieldName.MoveSpeed);
            mFields[(int)FieldName.MoveSpeedDebuff] = new SimpleFieldModifier(FieldName.MoveSpeed);
            mFields[(int)FieldName.MoveSpeed] = new Movespeed();

            mFields[(int)FieldName.Element] = new SimpleField();
            mFields[(int)FieldName.IgnoreArmor] = new SimpleField();

            mTierFieldNames = new List<FieldName>[2]; //Number of tiers. Increase with deep dependencies.

            mTierFieldNames[0] = new List<FieldName>()
            {
                FieldName.StrengthBase,
                FieldName.StrengthBonus,
                FieldName.StrengthPercBonus,
                FieldName.AgilityBase,
                FieldName.AgilityBonus,
                FieldName.AgilityPercBonus,
                FieldName.DexterityBase,
                FieldName.DexterityBonus,
                FieldName.DexterityPercBonus,
                FieldName.ConstitutionBase,
                FieldName.ConstitutionBonus,
                FieldName.ConstitutionPercBonus,
                FieldName.IntelligenceBase,
                FieldName.IntelligenceBonus,
                FieldName.IntelligencePercBonus,
                FieldName.HealthMax,
                FieldName.AttackBase,  
                FieldName.AttackBonus,
                FieldName.AttackPercBonus,
                FieldName.ArmorBase,
                FieldName.ArmorBonus,
                FieldName.ArmorPercBonus,

                FieldName.CriticalBase,
                FieldName.CriticalBonus,
                FieldName.CriticalPercBonus,
                FieldName.CocriticalBase,
                FieldName.CocriticalBonus,
                FieldName.CocriticalPercBonus,
                FieldName.EvasionBase,
                FieldName.EvasionBonus,
                FieldName.EvasionPercBonus,
                FieldName.AccuracyBase,
                FieldName.AccuracyBonus,
                FieldName.AccuracyPercBonus,

                FieldName.MoveSpeedBuff,
                FieldName.MoveSpeedDebuff,

                
            };

            mTierFieldNames[1] = new List<FieldName>()
            {
                 FieldName.Strength,
                 FieldName.Agility,
                 FieldName.Dexterity,
                 FieldName.Constitution,
                 FieldName.Intelligence,
                 FieldName.HealthMax,
                 FieldName.Attack,

                 FieldName.Critical,
                 FieldName.CriticalDamage,
                 FieldName.Cocritical,
                 //FieldName.CoCriticalDamage,
                 FieldName.Evasion,
                 FieldName.Accuracy,

                 FieldName.MoveSpeed,
                 FieldName.Armor,
                 FieldName.IgnoreArmor
            };
        }

        public override float GetField(FieldName fieldname)
        {
            if (mFields[(int)fieldname] == null) return 0;
            return mFields[(int)fieldname].GetValue();
        }

        public override void SetField(FieldName fieldname, float newval)
        {
            if (mFields[(int)fieldname] == null) return;
            base.SetField(fieldname, newval);
        }

        public override void AddToField(FieldName fieldname, float newval)
        {
            if (mFields[(int)fieldname] == null) return;
            base.AddToField(fieldname, newval);
        }
    }

    public enum SkillPassiveFieldName : int
    {
        None,
        OnCritical_Debuff_Attack,//the debuff on target when critical happen
        OnCritical_Debuff_Armor,
        OnCritical_Debuff_Critical,
        OnCritical_Debuff_CoCritical,
        OnCritical_Debuff_CriticalDamage,
        OnCritical_Debuff_CoCriticalDamage,
        OnCritical_Debuff_Evasion,
        OnCritical_Debuff_Accuracy,
        OnCritical_Buff_Attack,//the buff on self when critical
        OnCritical_Buff_Armor,
        OnCritical_Buff_Critical,
        OnCritical_Buff_CoCritical,
        OnCritical_Buff_CriticalDamage,
        OnCritical_Buff_CoCriticalDamage,
        OnCritical_Buff_Evasion,
        OnCritical_Buff_Accuracy,
        OnDeBuff_Buff_Attack,//the buff when have debuff
        OnDeBuff_Buff_Armor,
        OnDeBuff_Buff_Critical,
        OnDeBuff_Buff_CoCritical,
        OnDeBuff_Buff_CriticalDamage,
        OnDeBuff_Buff_CoCriticalDamage,
        OnDeBuff_Buff_Evasion,
        OnDeBuff_Buff_Accuracy,

        Dot_Buff_Attack,//the buff on caster when in dot
        Dot_Buff_Armor,
        Dot_Buff_Critical,
        Dot_Buff_CoCritical,
        Dot_Buff_CriticalDamage,
        Dot_Buff_CoCriticalDamage,
        Dot_Buff_Evasion,
        Dot_Buff_Accuracy,

        Evasion_Buff_Attack,//the buff on caster when evasion
        Evasion_Buff_Armor,
        Evasion_Buff_Critical,
        Evasion_Buff_CoCritical,
        Evasion_Buff_CriticalDamage,
        Evasion_Buff_CoCriticalDamage,
        Evasion_Buff_Evasion,
        Evasion_Buff_Accuracy,
        Evasion_Shield,//for shield.

        RedSkill_DamagePerAncient,
        GreenSkill_DamagePerAncient,
        BlueSkill_DamagePerAncient,
        RedSkill_DamagePerRare,
        GreenSkill_DamagePerRare,
        BlueSkill_DamagePerRare,
        JobSkill_CD,
        RSkill_CD,
        GSkill_CD,
        BSkill_CD,
        RGBSkill_CD,
        FlashSkill_CD,
        All_CD,
        All_Damage,
        Rej_Increase,
        RejSupress,
        Damage_Reduce,
        Potion_CD,
        FlashSkill_Dur,
        BasicAttack_DamageSupress,
        BasicAttack_DamageEnhance,
        Total,
    }

    //---------------used to save the sub skill effects .
    public class SkillPassiveCombatStats
    {
        private Timers timers;
        private Dictionary<SkillPassiveFieldName, object> mFields;
        private Dictionary<SkillPassiveFieldName, bool> mDirtyFields;
        private Dictionary<SkillPassiveFieldName, GameTimer> mFieldTimers;
        public List<SideEffectJson> SkillPassiveOnDebuff = new List<SideEffectJson>();
        public List<SideEffectJson> SkillPassiveOnEvasion = new List<SideEffectJson>();
        public List<SideEffectJson> SkillPassiveOnDot = new List<SideEffectJson>();
        private IActor actor;
        private int dotcount, debuffcount;

        public SkillPassiveCombatStats(Timers entityTimers, IActor iactor)
        {
            //TODO: NPC no need to do this to save server cost;
            mFields = new Dictionary<SkillPassiveFieldName, object>();//values used in combatformula
            mDirtyFields = new Dictionary<SkillPassiveFieldName, bool>();
            mFieldTimers = new Dictionary<SkillPassiveFieldName, GameTimer>();
            for (int i = 0; i < (int)SkillPassiveFieldName.Total; i++)
            {
                mFields.Add((SkillPassiveFieldName)i, (object)0);
                mDirtyFields.Add((SkillPassiveFieldName)i, false);
                mFieldTimers.Add((SkillPassiveFieldName)i, null);
            }
            timers = entityTimers;
            actor = iactor;
        }

        public void ResetAll()
        {
            for (int i = 0; i < (int)SkillPassiveFieldName.Total; i++)
            {
                //stop the timers will also decrease the amount of buff/debuff icons. so make sure it is triggered just one time
                GameTimer t = mFieldTimers[(SkillPassiveFieldName)i];
                if (t != null)
                {
                    timers.StopTimerAndTrigger(t);
                    t = null;
                    mFieldTimers[(SkillPassiveFieldName)i] = null;
                }
                mFields[(SkillPassiveFieldName)i] = 0;
                mDirtyFields[(SkillPassiveFieldName)i] = false;
            }
            SkillPassiveOnDebuff.Clear();
            SkillPassiveOnDot.Clear();
            SkillPassiveOnEvasion.Clear();
        }

        public void OnEvasion(int pid, int healthmax)
        {
            foreach (SideEffectJson sej in SkillPassiveOnEvasion)
            {
                //if (sej.effecttype == EffectType.OnEvasion_Buff) {
                //    SkillPassiveFieldName field = CombatUtils.GetSkillPassiveEvasionFieldByStatsType(sej.stat1, true);
                //    int val = (int)sej.max;
                //    long dur = (long)(sej.duration * 1000);//change the field time.
                //    if (mDirtyFields[field] == false) {
                //        mDirtyFields[field] = true;
                //        AddToField(field, val);
                //        actor.PlayerStats.havebuff++;
                //        mFieldTimers[field] = timers.SetTimer(dur, (object args) => {
                //            if (mDirtyFields[field]) {
                //                mDirtyFields[field] = false;
                //                AddToField(field, -val);
                //                actor.PlayerStats.havebuff--;
                //            }
                //        }, field);
                //    }
                //}
                //else if (sej.effecttype == EffectType.OnEvasion_Shield) {
                //    SkillPassiveFieldName field = SkillPassiveFieldName.Evasion_Shield;

                //    if (mDirtyFields[field] == false) {
                //        if (GameUtils.GetRandomGenerator().NextDouble() < sej.procchance * 0.01) {
                //            //System.Diagnostics.Debug.WriteLine("adding to filed Evasion_Shield ");
                //            actor.PlayerStats.PassiveShieldBuff = 1;
                //            actor.PlayerStats.havebuff++;
                //            int val = (int)(sej.max * 0.01f * healthmax);
                //            AddToField(field, val);
                //            long dur = (long)(sej.duration * 1000);
                //            mDirtyFields[field] = true;
                //            mFieldTimers[field] = timers.SetTimer(dur, (object args) => {
                //                StopEvasionShield();
                //            }, field);
                //        }
                //    }
                //}
                //else if (sej.effecttype == EffectType.OnEvasion_Rejuvenate) {
                //    if (GameUtils.GetRandomGenerator().NextDouble() < sej.procchance * 0.01) {
                //        int val = (int)(sej.max * 0.01 * healthmax);
                //        //AttackResult res = new AttackResult(pid, val);
                //        //res.IsHeal = true;
                //        actor.OnRecoverHealth(val);
                //    }
                //}
            }
        }

        private void StopEvasionShield()
        {
            SkillPassiveFieldName field = SkillPassiveFieldName.Evasion_Shield;
            if (mDirtyFields[field])
            {
                //System.Diagnostics.Debug.WriteLine("removing to filed Evasion_Shield ");
                mDirtyFields[field] = false;
                SetField(field, 0);//here reset to 0.
                actor.PlayerStats.PassiveShieldBuff = 0;
                actor.PlayerStats.Havebuff--;
            }
        }

        public int OnDamage(int dmg, IActor actor)
        {
            SkillPassiveFieldName field = SkillPassiveFieldName.Evasion_Shield;
            int val = (int)mFields[field];
            if (val > 0)
            {
                int orgval = val;
                val -= dmg;
                if (val <= 0)
                {
                    //stop the shield.
                    StopEvasionShield();
                    return dmg - orgval;
                }
                else
                {
                    mFields[field] = val;
                    return 0;
                }
            }
            return dmg;
        }

        private GameTimer DotTimer = null;

        public void OnDotStart()
        {
            dotcount++;
            if (dotcount != 1)
                return;
            if (SkillPassiveOnDot.Count == 0)
                return;
            OnSecondInterval(null);
            actor.PlayerStats.Havebuff += SkillPassiveOnDot.Count;
        }

        private void OnSecondInterval(object args)
        {
            System.Diagnostics.Debug.WriteLine("oninterval");
            //foreach (SideEffectJson sej in SkillPassiveOnDot)
            //{
            //    SkillPassiveFieldName field = CombatUtils.GetSkillPassiveDotFieldByStatsType(sej.stat1, true);
            //    int val = (int)sej.max;
            //    AddToField(field, val, true);
            //}
            DotTimer = timers.SetTimer(1000, OnSecondInterval, null);
        }

        public void OnDotEnd()
        {
            dotcount--;
            if (dotcount != 0)
                return;
            if (SkillPassiveOnDot.Count == 0)
                return;
            actor.PlayerStats.Havebuff -= SkillPassiveOnDot.Count;
            if (DotTimer != null)
            {
                timers.StopTimer(DotTimer);
                DotTimer = null;
                //foreach (SideEffectJson sej in SkillPassiveOnDot)
                //{
                //    SkillPassiveFieldName field = CombatUtils.GetSkillPassiveDotFieldByStatsType(sej.stat1, true);
                //    SetField(field, 0);
                //    AddToField(field, 0, true);//trigger a UpdateLocalSkillPassiveStats
                //}
            }
        }

        public void OnDebuffStart()
        {
            debuffcount++;
            if (debuffcount > 1)
                return;
            //foreach (SideEffectJson sej in SkillPassiveOnDebuff)
            //{
            //    SkillPassiveFieldName targetfield = CombatUtils.GetSkillOnDebuffPassiveFieldByStatsType(sej.stat1, true);
            //    if (targetfield != SkillPassiveFieldName.None) OnDebuffStart(targetfield, (int)sej.max);
            //    targetfield = CombatUtils.GetSkillOnDebuffPassiveFieldByStatsType(sej.stat2, true);
            //    if (targetfield != SkillPassiveFieldName.None) OnDebuffStart(targetfield, (int)sej.max);
            //    targetfield = CombatUtils.GetSkillOnDebuffPassiveFieldByStatsType(sej.stat3, true);
            //    if (targetfield != SkillPassiveFieldName.None) OnDebuffStart(targetfield, (int)sej.max);
            //}
        }

        public void OnDebuffEnd()
        {
            debuffcount--;
            if (debuffcount > 0)
                return;
            //foreach (SideEffectJson sej in SkillPassiveOnDebuff)
            //{
            //    SkillPassiveFieldName targetfield = CombatUtils.GetSkillOnDebuffPassiveFieldByStatsType(sej.stat1, true);
            //    if (targetfield != SkillPassiveFieldName.None) OnDebuffEnd(targetfield, (int)sej.max);
            //    targetfield = CombatUtils.GetSkillOnDebuffPassiveFieldByStatsType(sej.stat2, true);
            //    if (targetfield != SkillPassiveFieldName.None) OnDebuffEnd(targetfield, (int)sej.max);
            //    targetfield = CombatUtils.GetSkillOnDebuffPassiveFieldByStatsType(sej.stat3, true);
            //    if (targetfield != SkillPassiveFieldName.None) OnDebuffEnd(targetfield, (int)sej.max);
            //}
        }

        private void OnDebuffStart(SkillPassiveFieldName field, int val)
        {
            if (mDirtyFields[field] == false)
            {
                mDirtyFields[field] = true;
                AddToField(field, val, true);
            }
        }

        private void OnDebuffEnd(SkillPassiveFieldName field, int val)
        {
            mDirtyFields[field] = false;
            AddToField(field, -val, true);
        }

        public bool ChangeField(SkillPassiveFieldName field, int val, long dur, Action action)
        {
            if (mDirtyFields[field] == false)
            {
                mDirtyFields[field] = true;
                AddToField(field, val, true);
                mFieldTimers[field] = timers.SetTimer(dur, (object args) =>
                {
                    mDirtyFields[field] = false;
                    AddToField(field, -val, true);
                    action.Invoke();
                }, field);
                return true;
            }
            else
            {
                return false;
            }
        }

        public object GetField(SkillPassiveFieldName fieldname)
        {
            return mFields[fieldname];
        }

        public void SetField(SkillPassiveFieldName fieldname, object newval)
        {
            mFields[fieldname] = newval; //only set during initalization
        }

        //todo: opertimize this function. do not call UpdateLocalSkillPassiveStats to often;
        public void AddToField(SkillPassiveFieldName fieldname, object newval, bool updateCombatStats = false)
        {
            if (typeof(float) == newval.GetType())
            {
                throw new Exception("please passing in integar values");
            }
            //int orig = (int)mFields[fieldname];
            //orig = Math.Max(0, orig); //passing float value will cause an exception.
            mFields[fieldname] = (int)mFields[fieldname] + (int)newval;
            if (updateCombatStats)
            {
                actor.UpdateLocalSkillPassiveStats();
            }
        }
    }

}