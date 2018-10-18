using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel;
using Zealot.Common.Datablock;
using Zealot.Repository;
using Newtonsoft.Json;

namespace Zealot.Common.Entities
{
    //TODO: add all other sideeffect which should play its effect simmutanlously
    [Flags]
    public enum EffectVisualTypes : byte
    {
        Stun = 1,
        Slow = 2,
        Silence = 4,
        Root = 8,
        Disarmed = 16,
        Frozen = 32,
        NUM
    }

    public class ActorSynStats : LocalObject // Send to all relevant player
    {
        public ActorSynStats(LOTYPE actortype) : base(actortype)
        {
            // Default value here
            _moveSpeed = 6;
            _level = -1;
            _positiveVisualSE = 0;
            _negativeVisualSE = 0;
            _Team = 0;
            _TargetPID = 0;
            _alive = true;
            _displayHp = 1f;
            _invincible_dmg = false;
            _invincible_dot = false;
            _invincible_Control = false;
            _invincible_StatsAttack = false;
            _invincible_StatsDefence = false;
            _baSpeed = 1.0f;
            _rtReduction = 1f;
            _immune_Status = 0x0;
            _control_Status = 0x0;
            _heavystand = false;
             _visualEffectTypes = 0;
            _passiveShieldBuff = 0;
        }

        private float _baSpeed;
        public float baSpeed
        {
            get { return _baSpeed; }
            set { _baSpeed = Math.Min(4f, Math.Max(0.125f, value)); OnSetAttribute("baSpeed", _baSpeed); }
        }

        private float _rtReduction;
        public float rtReduction
        {
            get { return _rtReduction; }
            set { _rtReduction = Math.Min(4f, Math.Max(0.125f, value)); OnSetAttribute("rtReduction", _rtReduction); }
        }

        private float _moveSpeed;
        public float MoveSpeed
        {
            get { return _moveSpeed; }
            set { OnSetAttribute("MoveSpeed", value); _moveSpeed = value; }
        }

        private int _level;
        public int Level
        {
            get { return _level; }
            set { OnSetAttribute("Level", value); _level = value; }
        }

        /// <summary>
        /// set this to true will stop apply SideEffect of any type.
        /// It is not supposed to be used by SideEffect.
        /// </summary>
        private bool _invincible;
        public bool invincible
        {
            get { return _invincible; }
            set { OnSetAttribute("invincible", value); _invincible = value; }
        }

        private bool _invincible_dot;
        public bool InvincibleDot
        {
            get { return _invincible_dot; }
            set { OnSetAttribute("invincibleDot", value); _invincible_dot = value; }
        }

        private bool _invincible_dmg;
        public bool InvincibleDmg
        {
            get { return _invincible_dmg; }
            set { OnSetAttribute("invincibleDmg", value); _invincible_dmg = value; }
        }

        private bool _silence;
        public bool Silence
        {
            get { return _silence; }
            set { OnSetAttribute("silence", value); _silence = value; }
        }

        private bool _invincible_Control;
        public bool InvincibleCtl
        {
            get { return _invincible_Control; }
            set { OnSetAttribute("invincibleCtl", value); _invincible_Control = value; }
        }

        private bool _invincible_StatsAttack;
        public bool InvincibleStatsAtk
        {
            get { return _invincible_StatsAttack; }
            set { OnSetAttribute("invincibleStatsAtk", value); _invincible_StatsAttack = value; }
        }

        private bool _invincible_StatsDefence;
        public bool InvincibleStatsDef
        {
            get { return _invincible_StatsDefence; }
            set { OnSetAttribute("invincibleStatsDef", value); _invincible_StatsDefence = value; }
        }

        private byte _immune_Status;
        public byte ImmuneStatus
        {
            get { return _immune_Status; }
            set { OnSetAttribute("immuneStatus", value); _immune_Status = value; }
        }

        private byte _control_Status;
        public byte ControlStatus
        {
            get { return _control_Status; }
            set { OnSetAttribute("controlStatus", value); _control_Status = value; }
        }

        private bool _heavystand;
        public bool HeavyStand
        {
            get { return _heavystand; }
            set { OnSetAttribute("heavystand", value); _heavystand = value; }
        }

        private int _visualEffectTypes;
        public int VisualEffectTypes
        {
            get { return _visualEffectTypes; }
            set { OnSetAttribute("VisualEffectTypes", value); _visualEffectTypes = value; }
        }

        private int _passiveShieldBuff;
        public int PassiveShieldBuff
        {
            get { return _passiveShieldBuff; }
            set { OnSetAttribute("PassiveShieldBuff", value); _passiveShieldBuff = value; }
        }

        private int _positiveVisualSE;
        public int PositiveVisualSE
        {
            get { return _positiveVisualSE; }
            set { OnSetAttribute("positiveVisualSE", value); _positiveVisualSE = value; }
        } //The last buff that has particle effect

        private int _negativeVisualSE;
        public int NegativeVisualSE
        {
            get { return _negativeVisualSE; }
            set { OnSetAttribute("negativeVisualSE", value); _negativeVisualSE = value; }
        } //The last debuff that has particle effect

        private int _elementalVisualSE;
        public int ElementalVisualSE
        {
            get { return _elementalVisualSE; }
            set { OnSetAttribute("ElementalVisualSE", value); _elementalVisualSE = value; }
        }

        private int _Team;
        public int Team
        {
            get { return _Team; }
            set { OnSetAttribute("Team", value); _Team = value; }
        }

        private int _TargetPID;
        public int TargetPID
        {
            get { return _TargetPID; }
            set { OnSetAttribute("TargetPID", value); _TargetPID = value; }
        }

        private bool _alive;
        public bool Alive
        {
            get { return _alive; }
            set { OnSetAttribute("Alive", value); _alive = value; }
        }

        private int _havebuff;
        public int Havebuff
        {
            get { return _havebuff; }
            set { OnSetAttribute("havebuff", value); _havebuff = value; }
        }

        private int _havedebuff;
        public int Havedebuff
        {
            get { return _havedebuff; }
            set { OnSetAttribute("havedebuff", value); _havedebuff = value; }
        }

        private int _haveDot;
        public int Havedot
        {
            get { return _haveDot; }
            set { OnSetAttribute("havedot", value); _haveDot = value; }
        }

        private int _haveHot;
        public int Havehot
        {
            get { return _haveHot; }
            set { OnSetAttribute("havehot", value); _haveHot = value; }
        }

        private int _haveControl;
        public int Havecontrol
        {
            get { return _haveControl; }
            set { OnSetAttribute("havecontrol", value); _haveControl = value; }
        }

        private float _displayHp;
        public float DisplayHp
        {
            get { return _displayHp; }
            set { OnSetAttribute("DisplayHp", value); _displayHp = value; }
        }
    }

    public class PlayerSynStats : ActorSynStats //send to all relavant player
    {
        public PlayerSynStats() : base(LOTYPE.PlayerSynStats)
        {
            _name = "";
            _jobsect = 1;
            _faction = 0;
            _guildName = "";          
            Team = -1;
            _portraitID = 1;
            _Gender = 0;
            _Party = 0;
            _QuestCompanionId = -1;
            _achievementLevel = 1;
    }

        private int _Party;
        public int Party
        {
            get { return _Party; }
            set { OnSetAttribute("Party", value); _Party = value; }
        }

        private string _name;
        public string name
        {
            get { return _name; }
            set { OnSetAttribute("name", value); _name = value; }
        }

        private int _progressJobLevel;
        public int progressJobLevel
        {
            get { return _progressJobLevel; }
            set { OnSetAttribute("progressJobLevel", value); _progressJobLevel = value; }
        }

        private byte _jobsect;
        public byte jobsect
        {
            get { return _jobsect; }
            set { OnSetAttribute("jobsect", value); _jobsect = value; }
        }

        private byte _faction;
        public byte faction
        {
            get { return _faction; }
            set { OnSetAttribute("faction", value); _faction = value; }
        }

        private string _guildName;
        public string guildName
        {
            get { return _guildName; }
            set { OnSetAttribute("guildName", value); _guildName = value; }
        }

        private int _portraitID;
        public int PortraitID
        {
            get { return _portraitID; }
            set { OnSetAttribute("PortraitID", value); _portraitID = value; }
        }

        private byte _Gender;
        public byte Gender
        {
            get { return _Gender; }
            set { OnSetAttribute("Gender", value); _Gender = value; }
        }

        private int _MountID;
        public int MountID
        {
            get { return _MountID; }
            set { OnSetAttribute("MountID", value); _MountID = value; }
        }

        private int _QuestCompanionId;
        public int QuestCompanionId
        {
            get { return _QuestCompanionId; }
            set { OnSetAttribute("QuestCompanionId", value); _QuestCompanionId = value; }
        }

        private int _achievementLevel;
        public int AchievementLevel
        {
            get { return _achievementLevel; }
            set { OnSetAttribute("AchievementLevel", value); _achievementLevel = value; }
        }
    }

    public class NPCSynStats : ActorSynStats
    {
        public NPCSynStats() : base(LOTYPE.NPCSynStats)
        {
            Team = -100;
        }
    }

    public class HeroSynStats : ActorSynStats
    {
        private int _heroId;
        private int _modelTier;
        private bool _summoning;

        public HeroSynStats() : base(LOTYPE.HeroSynStats)
        {
            _heroId = 0;
            _modelTier = 0;
            _summoning = false;
        }

        public int HeroId
        {
            get { return _heroId; }
            set { OnSetAttribute("HeroId", value); _heroId = value; }
        }

        public int ModelTier
        {
            get { return _modelTier; }
            set { OnSetAttribute("ModelTier", value); _modelTier = value; }
        }

        public bool Summoning
        {
            get { return _summoning; }
            set { OnSetAttribute("Summoning", value); _summoning = value; }
        }
    }

    public class LocalCombatStats : LocalObject //only to 1 player client
    {
        public LocalCombatStats() : base(LOTYPE.LocalCombatStats)
        {
            _Health = 0;
            _HealthMax = 0;
            _HealthRegen = 0;
            _Mana = 0;
            _ManaMax = 0;
            _ManaRegen = 0;
            _ManaReduceBonus = 0;
            _ManaReducePercBonus = 0;
            _ExpBonus = 0;
            _WeaponAttack = 0;
            _AttackPower = 0;
            _Armor = 0;
            _IgnoreArmor = 0;
            _Block = 0;
            _BlockValue = 0;
            _Accuracy = 0;
            _Evasion = 0;
            _Critical = 0;
            _CoCritical = 0;
            _CriticalDamage = 0;

            _Strength = 0;
            _Agility = 0;
            _Dexterity = 0;
            _Constitution = 0;
            _Intelligence = 0;
            _StatsPoint = 0;

            _SmashDamage = 0;
            _SliceDamage = 0;
            _PierceDamage = 0;
            _IncElemNoneDamage = 0;
            _IncElemMetalDamage = 0;
            _IncElemWoodDamage = 0;
            _IncElemEarthDamage = 0;
            _IncElemWaterDamage = 0;
            _IncElemFireDamage = 0;
            _VSHumanDamage = 0;
            _VSZombieDamage = 0;
            _VSVampireDamage = 0;
            _VSAnimalDamage = 0;
            _VSPlantDamage = 0;
            _VSElemNoneDamage = 0;
            _VSElemMetalDamage = 0;
            _VSElemWoodDamage = 0;
            _VSElemEarthDamage = 0;
            _VSElemWaterDamage = 0;
            _VSElemFireDamage = 0;
            _VSBossDamage = 0;
            _IncFinalDamage = 0;

            _SmashDefence = 0;
            _SliceDefence = 0;
            _PierceDefence = 0;
            _IncElemNoneDefence = 0;
            _IncElemMetalDefence = 0;
            _IncElemWoodDefence = 0;
            _IncElemEarthDefence = 0;
            _IncElemWaterDefence = 0;
            _IncElemFireDefence = 0;
            _VSHumanDefence = 0;
            _VSZombieDefence = 0;
            _VSVampireDefence = 0;
            _VSAnimalDefence = 0;
            _VSPlantDefence = 0;
            _DncFinalDamage = 0;

            _isInSafeZone = false;
            _isIncombat = false;
            _ComboHit = 0;
            _SkillPoints = 0;
        }

        private int _Health;
        private int _HealthMax;
        private int _HealthRegen;
        private int _Mana;
        private int _ManaMax;
        private int _ManaRegen;
        private int _ManaReduceBonus;
        private int _ManaReducePercBonus;
        private int _ExpBonus;
        private int _WeaponAttack;
        private int _AttackPower;
        private int _Armor;
        private int _IgnoreArmor;
        private int _Block;
        private int _BlockValue;
        private int _Accuracy;
        private int _Evasion;
        private int _Critical;
        private int _CoCritical;
        private int _CriticalDamage;

        private int _Strength;
        private int _Agility;
        private int _Dexterity;
        private int _Constitution;
        private int _Intelligence;
        private int _StatsPoint;

        private int _SmashDamage;
        private int _SliceDamage;
        private int _PierceDamage;
        private int _IncElemNoneDamage;
        private int _IncElemMetalDamage;
        private int _IncElemWoodDamage;
        private int _IncElemEarthDamage;
        private int _IncElemWaterDamage;
        private int _IncElemFireDamage;
        private int _VSHumanDamage;
        private int _VSZombieDamage;
        private int _VSVampireDamage;
        private int _VSAnimalDamage;
        private int _VSPlantDamage;
        private int _VSElemNoneDamage;
        private int _VSElemMetalDamage;
        private int _VSElemWoodDamage;
        private int _VSElemEarthDamage;
        private int _VSElemWaterDamage;
        private int _VSElemFireDamage;
        private int _VSBossDamage;
        private int _IncFinalDamage;

        private int _SmashDefence;
        private int _SliceDefence;
        private int _PierceDefence;
        private int _IncElemNoneDefence;
        private int _IncElemMetalDefence;
        private int _IncElemWoodDefence;
        private int _IncElemEarthDefence;
        private int _IncElemWaterDefence;
        private int _IncElemFireDefence;
        private int _VSHumanDefence;
        private int _VSZombieDefence;
        private int _VSVampireDefence;
        private int _VSAnimalDefence;
        private int _VSPlantDefence;
        private int _DncFinalDamage;

        private int _SkillPoints;   //not shown in UI_CharacterInfo
        private int _ComboHit;
        private bool _isInSafeZone;
        private bool _isIncombat;

        public int Health
        {
            get { return _Health; }
            set { OnSetAttribute("Health", value); _Health = value; }
        }

        public int HealthMax
        {
            get { return _HealthMax; }
            set { OnSetAttribute("HealthMax", value); _HealthMax = value; }
        }

        public int HealthRegen
        {
            get { return _HealthRegen; }
            set { OnSetAttribute("HealthRegen", value); _HealthRegen = value; }
        }

        public int Mana
        {
            get { return _Mana; }
            set { OnSetAttribute("Mana", value); _Mana = value; }
        }

        public int ManaMax
        {
            get { return _ManaMax; }
            set { OnSetAttribute("ManaMax", value); _ManaMax = value; }
        }

        public int ManaRegen
        {
            get { return _ManaRegen; }
            set { OnSetAttribute("ManaRegen", value); _ManaRegen = value; }
        }

        public int ManaReduceBonus
        {
            get { return _ManaReduceBonus; }
            set { OnSetAttribute("ManaReduceBonus", value); _ManaReduceBonus = value; }
        }

        public int ManaReducePercBonus
        {
            get { return _ManaReducePercBonus; }
            set { OnSetAttribute("ManaReducePercBonus", value); _ManaReducePercBonus = value; }
        }

        public int ExpBonus
        {
            get { return _ExpBonus; }
            set { OnSetAttribute("ExpBonus", value); _ExpBonus = value; }
        }

        public int WeaponAttack
        {
            get { return _WeaponAttack; }
            set { OnSetAttribute("WeaponAttack", value); _WeaponAttack = value; }
        }

        public int AttackPower
        {
            get { return _AttackPower; }
            set { OnSetAttribute("AttackPower", value); _AttackPower = value; }
        }

        public int Armor
        {
            get { return _Armor; }
            set { OnSetAttribute("Armor", value); _Armor = value; }
        }

        public int IgnoreArmor
        {
            get { return _IgnoreArmor; }
            set { OnSetAttribute("IgnoreArmor", value); _IgnoreArmor = value; }
        }

        public int Block
        {
            get { return _Block; }
            set { OnSetAttribute("Block", value); _Block = value; }
        }

        public int BlockValue
        {
            get { return _BlockValue; }
            set { OnSetAttribute("BlockValue", value); _BlockValue = value; }
        }

        public int Accuracy
        {
            get { return _Accuracy; }
            set { OnSetAttribute("Accuracy", value); _Accuracy = value; }
        }

        public int Evasion
        {
            get { return _Evasion; }
            set { OnSetAttribute("Evasion", value); _Evasion = value; }
        }

        public int Critical
        {
            get { return _Critical; }
            set { OnSetAttribute("Critical", value); _Critical = value; }
        }

        public int CoCritical
        {
            get { return _CoCritical; }
            set { OnSetAttribute("CoCritical", value); _CoCritical = value; }
        }

        public int CriticalDamage
        {
            get { return _CriticalDamage; }
            set { OnSetAttribute("CriticalDamage", value); _CriticalDamage = value; }
        }

        public int Strength
        {
            get { return _Strength; }
            set { OnSetAttribute("Strength", value); _Strength = value; }
        }

        public int Agility
        {
            get { return _Agility; }
            set { OnSetAttribute("Agility", value); _Agility = value; }
        }

        public int Dexterity
        {
            get { return _Dexterity; }
            set { OnSetAttribute("Dexterity", value); _Dexterity = value; }
        }

        public int Constitution
        {
            get { return _Constitution; }
            set { OnSetAttribute("Constitution", value); _Constitution = value; }
        }

        public int Intelligence
        {
            get { return _Intelligence; }
            set { OnSetAttribute("Intelligence", value); _Intelligence = value; }
        }

        public int StatsPoint
        {
            get { return _StatsPoint; }
            set { _StatsPoint = Math.Max(0, value); OnSetAttribute("StatsPoint", value); }
        }

        public int SmashDamage
        {
            get { return _SmashDamage; }
            set { OnSetAttribute("SmashDamage", value); _SmashDamage = value;  }
        }

        public int SliceDamage
        {
            get { return _SliceDamage; }
            set { OnSetAttribute("SliceDamage", value); _SliceDamage = value; }
        }

        public int PierceDamage
        {
            get { return _PierceDamage; }
            set { OnSetAttribute("PierceDamage", value); _PierceDamage = value; }
        }

        public int IncElemNoneDamage
        {
            get { return _IncElemNoneDamage; }
            set { OnSetAttribute("IncElemNoneDamage", value); _IncElemNoneDamage = value; }
        }

        public int IncElemMetalDamage
        {
            get { return _IncElemMetalDamage; }
            set { OnSetAttribute("IncElemMetalDamage", value); _IncElemMetalDamage = value; }
        }

        public int IncElemWoodDamage
        {
            get { return _IncElemWoodDamage; }
            set { OnSetAttribute("IncElemWoodDamage", value); _IncElemWoodDamage = value; }
        }

        public int IncElemEarthDamage
        {
            get { return _IncElemEarthDamage; }
            set { OnSetAttribute("IncElemEarthDamage", value); _IncElemEarthDamage = value; }
        }

        public int IncElemWaterDamage
        {
            get { return _IncElemWaterDamage; }
            set { OnSetAttribute("IncElemWaterDamage", value); _IncElemWaterDamage = value; }
        }

        public int IncElemFireDamage
        {
            get { return _IncElemFireDamage; }
            set { OnSetAttribute("IncElemFireDamage", value); _IncElemFireDamage = value; }
        }

        public int VSHumanDamage
        {
            get { return _VSHumanDamage; }
            set { OnSetAttribute("VSHumanDamage", value); _VSHumanDamage = value; }
        }

        public int VSZombieDamage
        {
            get { return _VSZombieDamage; }
            set { OnSetAttribute("VSZombieDamage", value); _VSZombieDamage = value; }
        }

        public int VSVampireDamage
        {
            get { return _VSVampireDamage; }
            set { OnSetAttribute("VSVampireDamage", value); _VSVampireDamage = value; }
        }

        public int VSAnimalDamage
        {
            get { return _VSAnimalDamage; }
            set { OnSetAttribute("VSAnimalDamage", value); _VSAnimalDamage = value; }
        }

        public int VSPlantDamage
        {
            get { return _VSPlantDamage; }
            set { OnSetAttribute("VSPlantDamage", value); _VSPlantDamage = value; }
        }

        public int VSElemNoneDamage
        {
            get { return _VSElemNoneDamage; }
            set { OnSetAttribute("VSElemNoneDamage", value); _VSElemNoneDamage = value; }
        }

        public int VSElemMetalDamage
        {
            get { return _VSElemMetalDamage; }
            set { OnSetAttribute("VSElemMetalDamage", value); _VSElemMetalDamage = value; }
        }

        public int VSElemWoodDamage
        {
            get { return _VSElemWoodDamage; }
            set { OnSetAttribute("VSElemWoodDamage", value); _VSElemWoodDamage = value; }
        }

        public int VSElemEarthDamage
        {
            get { return _VSElemEarthDamage; }
            set { OnSetAttribute("VSElemEarthDamage", value); _VSElemEarthDamage = value; }
        }

        public int VSElemWaterDamage
        {
            get { return _VSElemWaterDamage; }
            set { OnSetAttribute("VSElemWaterDamage", value); _VSElemWaterDamage = value; }
        }

        public int VSElemFireDamage
        {
            get { return _VSElemFireDamage; }
            set { OnSetAttribute("VSElemFireDamage", value); _VSElemFireDamage = value; }
        }

        public int VSBossDamage
        {
            get { return _VSBossDamage; }
            set { OnSetAttribute("VSBossDamage", value); _VSBossDamage = value; }
        }

        public int IncFinalDamage
        {
            get { return _IncFinalDamage; }
            set { OnSetAttribute("IncFinalDamage", value); _IncFinalDamage = value; }
        }

        public int SmashDefence
        {
            get { return _SmashDefence; }
            set { OnSetAttribute("SmashDefence", value); _SmashDefence = value; }
        }

        public int SliceDefence
        {
            get { return _SliceDefence; }
            set { OnSetAttribute("SliceDefence", value); _SliceDefence = value; }
        }

        public int PierceDefence
        {
            get { return _PierceDefence; }
            set { OnSetAttribute("PierceDefence", value); _PierceDefence = value; }
        }

        public int IncElemNoneDefence
        {
            get { return _IncElemNoneDefence; }
            set { OnSetAttribute("IncElemNoneDefence", value); _IncElemNoneDefence = value; }
        }

        public int IncElemMetalDefence
        {
            get { return _IncElemMetalDefence; }
            set { OnSetAttribute("IncElemMetalDefence", value); _IncElemMetalDefence = value; }
        }

        public int IncElemWoodDefence
        {
            get { return _IncElemWoodDefence; }
            set { OnSetAttribute("IncElemWoodDefence", value); _IncElemWoodDefence = value; }
        }

        public int IncElemEarthDefence
        {
            get { return _IncElemEarthDefence; }
            set { OnSetAttribute("IncElemEarthDefence", value); _IncElemEarthDefence = value; }
        }

        public int IncElemWaterDefence
        {
            get { return _IncElemWaterDefence; }
            set { OnSetAttribute("IncElemWaterDefence", value); _IncElemWaterDefence = value; }
        }

        public int IncElemFireDefence
        {
            get { return _IncElemFireDefence; }
            set { OnSetAttribute("IncElemFireDefence", value); _IncElemFireDefence = value; }
        }

        public int VSHumanDefence
        {
            get { return _VSHumanDefence; }
            set { OnSetAttribute("VSHumanDefence", value); _VSHumanDefence = value; }
        }

        public int VSZombieDefence
        {
            get { return _VSZombieDefence; }
            set { OnSetAttribute("VSZombieDefence", value); _VSZombieDefence = value; }
        }

        public int VSVampireDefence
        {
            get { return _VSVampireDefence; }
            set { OnSetAttribute("VSVampireDefence", value); _VSVampireDefence = value; }
        }

        public int VSAnimalDefence
        {
            get { return _VSAnimalDefence; }
            set { OnSetAttribute("VSAnimalDefence", value); _VSAnimalDefence = value; }
        }

        public int VSPlantDefence
        {
            get { return _VSPlantDefence; }
            set { OnSetAttribute("VSPlantDefence", value); _VSPlantDefence = value; }
        }

        public int DncFinalDamage
        {
            get { return _DncFinalDamage; }
            set { OnSetAttribute("DncFinalDamage", value); _DncFinalDamage = value; }
        }

        public int ComboHit
        {
            get { return _ComboHit; }
            set { OnSetAttribute("ComboHit", value); _ComboHit = value; }
        }

        public bool IsInCombat
        {
            get { return _isIncombat; }
            set { OnSetAttribute("IsInCombat", value); _isIncombat = value; }
        }

        public bool IsInSafeZone
        {
            get { return _isInSafeZone; }
            set { OnSetAttribute("IsInSafeZone", value); _isInSafeZone = value; }
        }

        public int SkillPoints
        {
            get { return _SkillPoints; }
            set { OnSetAttribute("SkillPoints", value); _SkillPoints = value; }
        }
    }

    /// <summary>
    /// sync to local player only
    /// </summary>
    public class LocalSkillPassiveStats : LocalObject //only to 1 player client
    {
        public LocalSkillPassiveStats() : base(LOTYPE.LocalSkillPassiveStats)
        {
            _status = 0;
        }

        private int _status;
        public int Status
        {
            get { return _status; }
            set { OnSetAttribute("Status", value); _status = value; }
        }

        private int _HealthMax;
        public int HealthMax
        {
            get { return _HealthMax; }
            set { OnSetAttribute("HealthMax", value); _HealthMax = value; }
        }

        private int _Accuracy;
        public int Accuracy
        {
            get { return _Accuracy; }
            set { OnSetAttribute("Accuracy", value); _Accuracy = value; }
        }


        private int _Armor;
        public int Armor
        {
            get { return _Armor; }
            set { OnSetAttribute("Armor", value); _Armor = value; }
        }


        private int _Evasion;
        public int Evasion
        {
            get { return _Evasion; }
            set { OnSetAttribute("Evasion", value); _Evasion = value; }
        }


        private int _Attack;
        public int Attack
        {
            get { return _Attack; }
            set { OnSetAttribute("Attack", value); _Attack = value; }
        }

        private int _Critical;
        public int Critical
        {
            get { return _Critical; }
            set { OnSetAttribute("Critical", value); _Critical = value; }
        }

        private int _CoCritical;
        public int CoCritical
        {
            get { return _CoCritical; }
            set { OnSetAttribute("CoCritical", value); _CoCritical = value; }
        }

        private int _CriticalDamage;
        public int CriticalDamage
        {
            get { return _CriticalDamage; }
            set { OnSetAttribute("CriticalDamage", value); _CriticalDamage = value; }
        }

        private int _CoCriticalDamage;
        public int CoCriticalDamage
        {
            get { return _CoCriticalDamage; }
            set { OnSetAttribute("CoCriticalDamage", value); _CoCriticalDamage = value; }
        }
    }

    public class SecondaryStats : LocalObject //send to local player
    {
        private int _experience;
        private int _jobexperience;
        private int _realmscore;

        private int _achievementExp;

        private int _money;
        private int _gold;
        private int _bindgold;
        private int _lotterypoints;
        private int _honor;
        private int _contribute;
        private int _battlecoin;

        private int _UnlockedSlotCount;
        private int _guildId;
        private byte _guildRank;
        private string _guildShopBuyCount;
        private int _guildSMBossEntry;
        private int _guildSMBossExtraEntry;
        private long _guildLeaveGuildCDEnd;
        private byte _guildDreamHouseUsed;
        private string _guildDreamHouseCollected;
        private bool _guildDonateDot;
        private long _lastFreeLotteryRoll;
        private int _FreeReviveOnSpot;
        private long _RandomBoxTimeTick;
        private int _costbuffid;
        private int _costbuffgold;
        private byte _CurrencyExchangeTime;
        private int _UnlockWorldBossLevel;
        private int _tutorialreddot;
        private int _BattleTime;

        public SecondaryStats() : base(LOTYPE.SecondaryStats)
        {
            _experience = 0;
            _jobexperience = 0;
            _realmscore = 0;
            _achievementExp = 0;

        // Currency start
            _money = 0;
            _gold = 0;
            _bindgold = 0;

            _lotterypoints = 0;
            _honor = 0;
            _contribute = 0;
            _battlecoin = 0;
            _BattleTime = 0;
            // currency end

            _UnlockedSlotCount = 30;

            // Guild
            _guildId = 0;
            _guildRank = 0;
            _guildShopBuyCount = "";
            _guildSMBossEntry = 0;
            _guildSMBossExtraEntry = 0;
            _guildLeaveGuildCDEnd = 0;
            _guildDreamHouseUsed = 0;
            _guildDreamHouseCollected = "";
            _guildDonateDot = false;

            // Lottery
            _lastFreeLotteryRoll = long.MaxValue;

            _FreeReviveOnSpot = 0;

            _RandomBoxTimeTick = 0;
            _costbuffid = 0;
            _costbuffgold = 0;

            _CurrencyExchangeTime = 0;

            _UnlockWorldBossLevel = 0;
            _tutorialreddot = 0;
        }

        public int experience
        {
            get { return _experience; }
            set { OnSetAttribute("experience", value); _experience = value; }
        }

        public int jobexperience
        {
            get { return _jobexperience; }
            set { OnSetAttribute("jobexperience", value); _jobexperience = value; }
        }

        public int realmscore
        {
            get { return _realmscore; }
            set { OnSetAttribute("realmscore", value); _realmscore = value; }
        }

        public int AchievementExp
        {
            get { return _achievementExp; }
            set { OnSetAttribute("AchievementExp", value); _achievementExp = value; }
        }

        public int Money
        {
            get { return _money; }
            set { OnSetAttribute("Money", value); _money = value; }
        }

        // this is the currency top up from the appstore
        public int Gold
        {
            get { return _gold; }
            set { OnSetAttribute("Gold", value); _gold = value; }
        }

        public int bindgold
        {
            get { return _bindgold; }
            set { OnSetAttribute("bindgold", value); _bindgold = value; }
        }

        public int lotterypoints // Special points awarded when spending unbound diamond
        {
            get { return _lotterypoints; }
            set { OnSetAttribute("lotterypoints", value); _lotterypoints = value; }
        }

        public int honor
        {
            get { return _honor; }
            set { OnSetAttribute("honor", value); _honor = value; }
        }

        public int contribute
        {
            get { return _contribute; }
            set { OnSetAttribute("contribute", value); _contribute = value; }
        }

        public int battlecoin
        {
            get { return _battlecoin; }
            set { OnSetAttribute("battlecoin", value); _battlecoin = value; }
        }

        public int BattleTime 
        {
            get { return _BattleTime; }
            set { OnSetAttribute("BattleTime", value); _BattleTime = value; }
        }

        public int UnlockedSlotCount
        {
            get { return _UnlockedSlotCount; }
            set { this.OnSetAttribute("UnlockedSlotCount", value); _UnlockedSlotCount = value; }
        }

        // Guild
        public int guildId
        {
            get { return _guildId; }
            set { OnSetAttribute("guildId", value); _guildId = value; }
        }

        public byte guildRank
        {
            get { return _guildRank; }
            set { OnSetAttribute("guildRank", value); _guildRank = value; }
        }

        public string guildShopBuyCount
        {
            get { return _guildShopBuyCount; }
            set { OnSetAttribute("guildShopBuyCount", value); _guildShopBuyCount = value; }
        }

        public int GuildSMBossEntry
        {
            get { return _guildSMBossEntry; }
            set { OnSetAttribute("GuildSMBossEntry", value); _guildSMBossEntry = value; }
        }

        public int GuildSMBossExtraEntry
        {
            get { return _guildSMBossExtraEntry; }
            set { OnSetAttribute("GuildSMBossExtraEntry", value); _guildSMBossExtraEntry = value; }
        }

        public long guildLeaveGuildCDEnd
        {
            get { return _guildLeaveGuildCDEnd; }
            set { OnSetAttribute("guildLeaveGuildCDEnd", value); _guildLeaveGuildCDEnd = value; }
        }

        public byte GuildDreamHouseUsed
        {
            get { return _guildDreamHouseUsed; }
            set { OnSetAttribute("GuildDreamHouseUsed", value); _guildDreamHouseUsed = value; }
        }

        public string GuildDreamHouseCollected
        {
            get { return _guildDreamHouseCollected; }
            set { OnSetAttribute("GuildDreamHouseCollected", value); _guildDreamHouseCollected = value; }
        }

        public bool guildDonateDot
        {
            get { return _guildDonateDot; }
            set { OnSetAttribute("guildDonateDot", value); _guildDonateDot = value; }
        }

        // Lottery
        public long lastFreeLotteryRoll
        {
            get { return _lastFreeLotteryRoll; }
            set { OnSetAttribute("lastFreeLotteryRoll", value); _lastFreeLotteryRoll = value; }
        }

        public int FreeReviveOnSpot
        {
            get { return _FreeReviveOnSpot; }
            set { OnSetAttribute("FreeReviveOnSpot", value); _FreeReviveOnSpot = value; }
        }

        public long RandomBoxTimeTick
        {
            get { return _RandomBoxTimeTick; }
            set { OnSetAttribute("RandomBoxTS", value); _RandomBoxTimeTick = value; }
        }

        public int costbuffid
        {
            get { return _costbuffid; }
            set { OnSetAttribute("costbuffid", value); _costbuffid = value; }
        }

        public int costbuffgold
        {
            get { return _costbuffgold; }
            set { OnSetAttribute("costbuffgold", value); _costbuffgold = value; }
        }

        public byte CurrencyExchangeTime
        {
            get { return _CurrencyExchangeTime; }
            set { OnSetAttribute("CurrencyExchangeTime", value); _CurrencyExchangeTime = value; }
        }

        public int UnlockWorldBossLevel
        {
            get { return _UnlockWorldBossLevel; }
            set { OnSetAttribute("UnlockWBLv", value); _UnlockWorldBossLevel = value; }
        }

        public int tutorialreddot
        {
            get { return _tutorialreddot; }
            set { OnSetAttribute("tutorialreddot", value); _tutorialreddot = value; }
        }

        public void ResetOnNewDay(CharacterData characterData)
        {
            GuildSMBossEntry = characterData.GuildSMBossEntry;
            CurrencyExchangeTime = characterData.CurrencyExchangeTime;
            GuildDreamHouseUsed = characterData.GuildDreamHouseUsed;
            GuildDreamHouseCollected = characterData.GuildDreamHouseCollected;
            FreeReviveOnSpot = characterData.FreeReviveOnSpot;
            BattleTime = characterData.BattleTime;
        }

        public bool IsGoldEnough(int value, bool useBind = true)
        {
            if (useBind)
                return bindgold >= value - Gold;
            else
                return Gold >= value;
        }

        public long GetGoldWithBind()
        {
            return ((long)bindgold + Gold);
        }
    }

    public class InventoryStats : LocalObject
    {
        public InventoryStats(LOTYPE type) : base(type)
        {
            ItemInventory = new CollectionHandler<object>((int)InventorySlot.COLLECTION_SIZE);
            ItemInventory.SetParent(this, "ItemInventory");
        }

        public CollectionHandler<object> ItemInventory { get; set; }
    }

    public class EquipmentStats : LocalObject
    {
        public EquipmentStats() : base(LOTYPE.EquipmentStats)
        {
            _HideHelm = false;
            FashionInventory = new CollectionHandler<object>((int)FashionSlot.MAXSLOTS);
            FashionInventory.SetParent(this, "FashionInventory");

            EquipInventory = new CollectionHandler<object>((int)EquipmentSlot.MAXSLOTS);
            EquipInventory.SetParent(this, "EquipInventory");

            AppearanceInventory = new CollectionHandler<object>((int)AppearanceSlot.MAXSLOTS);
            AppearanceInventory.SetParent(this, "AppearanceInventory");
        }

        private bool _HideHelm;
        public bool HideHelm
        {
            get { return _HideHelm; }
            set { OnSetAttribute("HideHelm", value); _HideHelm = value; }
        }

        public CollectionHandler<object> FashionInventory { get; set; }
        public CollectionHandler<object> EquipInventory { get; set; }
        public CollectionHandler<object> AppearanceInventory { get; set; }
    }

    public class ItemHotbarStats : LocalObject
    {
        public ItemHotbarStats() : base(LOTYPE.ItemHotbarStats)
        {
            ItemHotbar = new CollectionHandler<object>(4);
            ItemHotbar.SetParent(this, "ItemHotbar");
        }

        public CollectionHandler<object> ItemHotbar { get; set; }

        public void InitItemHotbarFromString(string str)
        {
            if (!string.IsNullOrEmpty(str))
            {
                string[] splitStr = str.Split(';');
                if (splitStr.Length == 4)
                {
                    int itemId = 0;
                    for (int i = 0; i < 4; ++i)
                    {
                        int.TryParse(splitStr[i], out itemId);
                        ItemHotbar[i] = itemId;
                    }
                    return;
                }
            }
            for (int i = 0; i < 4; ++i)
                ItemHotbar[i] = 0;
        }

        public override string ToString()
        {
            return string.Format("{0};{1};{2};{3}",
                ItemHotbar[0], ItemHotbar[1], ItemHotbar[2], ItemHotbar[3]);
        }
    }

    public class ExchangeShopSynStats : LocalObject
    {
        //exchangeLeftMapJsonString[0 ~ Max(exid)] = exchanges left
        private string _exchangeLeftMapJsonString;

        public ExchangeShopSynStats() : base(LOTYPE.ExchangeShopStats)
        {
            _exchangeLeftMapJsonString = "";
        }

        public string exchangeLeftMapJsonString
        {
            get { return _exchangeLeftMapJsonString; }
            set { OnSetAttribute("exchangeLeftMapJsonString", value); _exchangeLeftMapJsonString = value; }
        }
    }

    public class PortraitDataStats : LocalObject
    {
        private string _portraitDataInfoString;

        public PortraitDataStats() : base(LOTYPE.PortraitDataStats)
        {
            _portraitDataInfoString = "";
        }

        public string portraitDataInfoString
        {
            get { return _portraitDataInfoString; }
            set { OnSetAttribute("portraitDataInfoString", value); _portraitDataInfoString = value; }
        }

        public void LoadCharacterPortraitData(PortraitData data)
        {
            if (data == null || data.mInfoDic == null || data.mInfoDic.Count == 0)
                return;

            portraitDataInfoString = JsonConvert.SerializeObject(data.mInfoDic);
        }

        /// <summary>
        /// Used to unset red dot on the portrait selection
        /// </summary>
        /// <param name="portraitIDList"></param>
        public void SetPortraitData_OldPortrait(List<int> portraitIDList)
        {
            //Do nothing if portrait list is null or empty
            if (portraitIDList == null || portraitIDList.Count == 0)
                return;

            var dic = GetDeserializedData();
            for (int i = 0; i < portraitIDList.Count; ++i)
            {
                //Skip if dictionary does not have this portrait id
                if (!dic.ContainsKey(portraitIDList[i]))
                    continue;

                //Get the info and update isNew
                dic[portraitIDList[i]] = false;
            }

            //Update the json string
            portraitDataInfoString = JsonConvert.SerializeObject(dic);
        }

        /// <summary>
        /// Unlock new portrait
        /// </summary>
        /// <param name="portraitID"></param>
        public void SetPortraitData_Unlock(int portraitID)
        {
            Dictionary<int, string> generalMap = PortraitPathRepo.mGeneralMap;

            //Do nothing if invalid ID
            if (!generalMap.ContainsKey(portraitID))
                return;

            //Do nothing if already unlocked
            var dic = GetDeserializedData();
            if (dic.ContainsKey(portraitID))
                return;

            //Add as new portrait
            dic.Add(portraitID, true);

            //Update the json string
            portraitDataInfoString = JsonConvert.SerializeObject(dic);
        }

        public Dictionary<int, bool> GetDeserializedData()
        {
            return JsonConvert.DeserializeObject<Dictionary<int, bool>>(_portraitDataInfoString);
        }
    }

    public class Avatar
    {
        public int archetypeId = 0;
        public string modelPrefab = "";
    }

    #region Quest
    public class QuestSynStats : LocalObject
    {
        private string _mainQuest;
        private string _completedMain;

        public CollectionHandler<object> AdventureQuest { get; set; }
        private Dictionary<int, object> _adventurIdMap;
        private Dictionary<int, string> _adventureQuest;
        private string _completedAdventure;

        public CollectionHandler<object> SublineQuest { get; set; }
        private Dictionary<int, object> _sublineIdMap;
        private Dictionary<int, string> _sublineQuest;
        private string _completedSubline;

        public CollectionHandler<object> GuildQuest { get; set; }
        private Dictionary<int, object> _guildIdMap;
        private Dictionary<int, string> _guildQuest;
        private string _completedGuild;

        public CollectionHandler<object> SignboardQuest { get; set; }
        private Dictionary<int, object> _signboarIdMap;
        private Dictionary<int, string> _signboardQuest;
        private string _completedSignboard;
        private int _signboardRewardBoost;
        private int _signboardLimit;

        public CollectionHandler<object> EventQuest { get; set; }
        private Dictionary<int, object> _eventIdMap;
        private Dictionary<int, string> _eventQuest;
        private string _completedEvent;

        private string _trackingList;
        private string _wonderfulList;
        private string _unlockQuestList;
        private string _unlockSignboardList;

        public QuestSynStats() : base(LOTYPE.QuestSynStats)
        {
            _mainQuest = "";
            _completedMain = "";

            AdventureQuest = new CollectionHandler<object>(QuestRepo.GetMaxQuestCountByType(QuestType.Destiny));
            AdventureQuest.SetParent(this, "AdventureQuest");
            _adventurIdMap = new Dictionary<int, object>();
            _adventureQuest = new Dictionary<int, string>();
            _completedAdventure = "";

            SublineQuest = new CollectionHandler<object>(QuestRepo.GetMaxQuestCountByType(QuestType.Sub));
            SublineQuest.SetParent(this, "SublineQuest");
            _sublineIdMap = new Dictionary<int, object>();
            _sublineQuest = new Dictionary<int, string>();
            _completedSubline = "";

            GuildQuest = new CollectionHandler<object>(QuestRepo.GetMaxQuestCountByType(QuestType.Guild));
            GuildQuest.SetParent(this, "GuildQuest");
            _guildIdMap = new Dictionary<int, object>();
            _guildQuest = new Dictionary<int, string>();
            _completedGuild = "";

            SignboardQuest = new CollectionHandler<object>(QuestRepo.GetMaxQuestCountByType(QuestType.Signboard));
            SignboardQuest.SetParent(this, "SignboardQuest");
            _signboarIdMap = new Dictionary<int, object>();
            _signboardQuest = new Dictionary<int, string>();
            _completedSignboard = "";
            _signboardRewardBoost = 100;
            _signboardLimit = 0;

            EventQuest = new CollectionHandler<object>(QuestRepo.GetMaxQuestCountByType(QuestType.Event));
            EventQuest.SetParent(this, "EventQuest");
            _eventIdMap = new Dictionary<int, object>();
            _eventQuest = new Dictionary<int, string>();

            _completedEvent = "";
            _trackingList = "";
            _wonderfulList = "";
            _unlockQuestList = "";
            _unlockSignboardList = "";
    }

        public string mainQuest
        {
            get { return _mainQuest; }
            set { OnSetAttribute("mainQuest", value); _mainQuest = value; }
        }

        public string completedMain
        {
            get { return _completedMain; }
            set { OnSetAttribute("completedMain", value); _completedMain = value; }
        }

        public Dictionary<int, string> adventureQuest
        {
            get { return _adventureQuest; }
            set
            {
                _adventureQuest = value;
                InitCollectionData(QuestType.Destiny, _adventureQuest);
            }
        }

        public string completedAdventure
        {
            get { return _completedAdventure; }
            set { OnSetAttribute("completedAdventure", value); _completedAdventure = value; }
        }

        public Dictionary<int, string> sublineQuest
        {
            get { return _sublineQuest; }
            set
            {
                _sublineQuest = value;
                InitCollectionData(QuestType.Sub, _sublineQuest);
            }
        }

        public string completedSubline
        {
            get { return _completedSubline; }
            set { OnSetAttribute("completedSubline", value); _completedSubline = value; }
        }

        public Dictionary<int, string> guildQuest
        {
            get { return _guildQuest; }
            set
            {
                _guildQuest = value;
                InitCollectionData(QuestType.Guild, _guildQuest);
            }
        }

        public string completedGuild
        {
            get { return _completedGuild; }
            set { OnSetAttribute("completedGuild", value); _completedGuild = value; }
        }

        public Dictionary<int, string> signboardQuest
        {
            get { return _signboardQuest; }
            set
            {
                _signboardQuest = value;
                InitCollectionData(QuestType.Signboard, _signboardQuest);
            }
        }

        public string completedSignboard
        {
            get { return _completedSignboard; }
            set { OnSetAttribute("completedSignboard", value); _completedSignboard = value; }
        }

        public int signboardRewardBoost
        {
            get { return _signboardRewardBoost; }
            set { OnSetAttribute("signboardRewardBoost", value); _signboardRewardBoost = value; }
        }

        public int signboardLimit
        {
            get { return _signboardLimit; }
            set { OnSetAttribute("signboardLimit", value); _signboardLimit = value; }
        }

        public Dictionary<int, string> eventQuest
        {
            get { return _eventQuest; }
            set
            {
                _eventQuest = value;
                InitCollectionData(QuestType.Event, _eventQuest);
            }
        }

        public string completedEvent
        {
            get { return _completedEvent; }
            set { OnSetAttribute("completedEvent", value); _completedEvent = value; }
        }

        public string trackingList
        {
            get { return _trackingList; }
            set { OnSetAttribute("trackingList", value); _trackingList = value; }
        }

        public string wonderfulList
        {
            get { return _wonderfulList; }
            set { OnSetAttribute("wonderfulList", value); _wonderfulList = value; }
        }

        public string unlockQuestList
        {
            get { return _unlockQuestList; }
            set { OnSetAttribute("unlockQuestList", value); _unlockQuestList = value; }
        }

        public string unlockSignboardList
        {
            get { return _unlockSignboardList; }
            set { OnSetAttribute("unlockSignboardList", value); _unlockSignboardList = value; }
        }

        private void InitCollectionData(QuestType type, Dictionary<int, string> datas)
        {
            int maxCount = 0;
            Dictionary<int, object> idmap = null;
            switch (type)
            {
                case QuestType.Destiny:
                    maxCount = QuestRepo.GetMaxQuestCountByType(QuestType.Destiny);
                    idmap = _adventurIdMap;
                    break;
                case QuestType.Sub:
                    maxCount = QuestRepo.GetMaxQuestCountByType(QuestType.Sub);
                    idmap = _sublineIdMap;
                    break;
                case QuestType.Guild:
                    maxCount = QuestRepo.GetMaxQuestCountByType(QuestType.Guild);
                    idmap = _guildIdMap;
                    break;
                case QuestType.Signboard:
                    maxCount = QuestRepo.GetMaxQuestCountByType(QuestType.Signboard);
                    idmap = _signboarIdMap;
                    break;
                case QuestType.Event:
                    maxCount = QuestRepo.GetMaxQuestCountByType(QuestType.Event);
                    idmap = _eventIdMap;
                    break;
            }

            if (idmap != null)
            {
                int count = 0;
                foreach(KeyValuePair<int, string> entry in datas)
                {
                    idmap.Add(entry.Key, entry.Value);
                    SetColletionData(type, count, entry.Value);
                    count += 1;
                }

                for (int i = count; i < maxCount; i++)
                {
                    SetColletionData(type, i, null);
                }
            }
        }

        private void SetColletionData(QuestType type, int num, object data)
        {
            switch(type)
            {
                case QuestType.Destiny:
                    AdventureQuest[num] = data;
                    break;
                case QuestType.Sub:
                    SublineQuest[num] = data;
                    break;
                case QuestType.Guild:
                    GuildQuest[num] = data;
                    break;
                case QuestType.Signboard:
                    SignboardQuest[num] = data;
                    break;
                case QuestType.Event:
                    EventQuest[num] = data;
                    break;
            }
        }

        private int GetCollectionId(QuestType type, int questid)
        {
            Dictionary<int, object> idmap = null;
            CollectionHandler<object> collection = null;
            switch (type)
            {
                case QuestType.Destiny:
                    idmap = _adventurIdMap;
                    collection = AdventureQuest;
                    break;
                case QuestType.Sub:
                    idmap = _sublineIdMap;
                    collection = SublineQuest;
                    break;
                case QuestType.Guild:
                    idmap = _guildIdMap;
                    collection = GuildQuest;
                    break;
                case QuestType.Signboard:
                    idmap = _signboarIdMap;
                    collection = SignboardQuest;
                    break;
                case QuestType.Event:
                    idmap = _eventIdMap;
                    collection = EventQuest;
                    break;
            }

            if (idmap != null && collection != null)
            {
                if (idmap.ContainsKey(questid))
                {
                    object olddata = idmap[questid];
                    for (int i = 0; i < collection.Count; i++)
                    {
                        if (collection[i] == olddata)
                            return i;
                    }
                }

                for (int i = 0; i < collection.Count; i++)
                {
                    if (collection[i] == null)
                        return i;
                }
            }
            return -1;
        }

        private void UpdateCollectionData(QuestType type, int questid, object data)
        {
            Dictionary<int, object> idmap = null;
            switch (type)
            {
                case QuestType.Destiny:
                    idmap = _adventurIdMap;
                    break;
                case QuestType.Sub:
                    idmap = _sublineIdMap;
                    break;
                case QuestType.Guild:
                    idmap = _guildIdMap;
                    break;
                case QuestType.Signboard:
                    idmap = _signboarIdMap;
                    break;
                case QuestType.Event:
                    idmap = _eventIdMap;
                    break;
            }

            int num = GetCollectionId(type, questid);
            if (num != -1)
            {
                if (idmap.ContainsKey(questid))
                {
                    idmap[questid] = data;
                }
                else
                {
                    idmap.Add(questid, data);
                }
                SetColletionData(type, num, data);
            }
        }

        public void UpdateStats(QuestType type, int questid, string stats)
        {
            Dictionary<int, string> statslist = null;
            switch(type)
            {
                case QuestType.Destiny:
                    statslist = adventureQuest;
                    break;
                case QuestType.Sub:
                    statslist = sublineQuest;
                    break;
                case QuestType.Guild:
                    statslist = guildQuest;
                    break;
                case QuestType.Signboard:
                    statslist = signboardQuest;
                    break;
                case QuestType.Event:
                    statslist = eventQuest;
                    break;
            }

            if (statslist == null)
                return;

            if (!string.IsNullOrEmpty(stats))
            {
                if (statslist.ContainsKey(questid))
                {
                    statslist[questid] = stats;
                }
                else
                {
                    statslist.Add(questid, stats);
                }
                UpdateCollectionData(type, questid, stats);
            }
            else
            {
                if (statslist.ContainsKey(questid))
                {
                    statslist.Remove(questid);
                    UpdateCollectionData(type, questid, null);
                }
            }
        }

        public void UpdateCompletedList(QuestType type, string stats)
        {
            switch (type)
            {
                case QuestType.Main:
                    completedMain = stats;
                    break;
                case QuestType.Destiny:
                    completedAdventure = stats;
                    break;
                case QuestType.Sub:
                    completedSubline = stats;
                    break;
                case QuestType.Guild:
                    completedGuild = stats;
                    break;
                case QuestType.Signboard:
                    completedSignboard = stats;
                    break;
                case QuestType.Event:
                    completedEvent = stats;
                    break;
            }
        }
    }
    #endregion

    #region Donate
    public class DonateSynStats : LocalObject
    {
        private string _donateData;

        public DonateSynStats() : base(LOTYPE.DonateSynStats)
        {
            _donateData = "";
        }

        public string donateData
        {
            get { return _donateData; }
            set { OnSetAttribute("donateData", value); _donateData = value; }
        }
    }
    #endregion

    #region Destiny Clue
    public class DestinyClueSynStats : LocalObject
    {
        private string _destinyClues;
        private string _unlockMemory;
        private string _unlockClues;
        private string _unlockTimeClues;

        public DestinyClueSynStats() : base(LOTYPE.DestinyClueSynStats)
        {
            _destinyClues = "";
            _unlockMemory = "";
            _unlockClues = "";
            _unlockTimeClues = "";
        }

        public string destinyClues
        {
            get { return _destinyClues; }
            set { OnSetAttribute("destinyClues", value); _destinyClues = value; }
        }

        public string unlockMemory
        {
            get { return _unlockMemory; }
            set { OnSetAttribute("unlockMemory", value); _unlockMemory = value; }
        }

        public string unlockClues
        {
            get { return _unlockClues; }
            set { OnSetAttribute("unlockClues", value); _unlockClues = value; }
        }

        public string unlockTimeClues
        {
            get { return _unlockTimeClues; }
            set { OnSetAttribute("unlockTimeClues", value); _unlockTimeClues = value; }
        }
    }
    #endregion

    public class SkillSynStats : LocalObject
    {
        //public static readonly int MAX_SIDEEFFECT = 10;
        public static readonly int MAX_SKILLCOUNT = 40;
        public static readonly int MAX_EQUIPPED = 36;
        private int _basicAttack1SId;
        private int _basicAttack2SId;
        private int _basicAttack3SId;

        private int _jobSkillAttackSId;
        private int _SkillInvCount;
        private int _equipGroup;
        private int _autoGroup;
        private int _equipSize;
        
        /// <summary>
        /// How to use equip skills and Auto skills collection
        /// index -> [slot + ((Slot Group - 1) * number of slots)]
        /// Warning : Slot Group is required to start with 0, thus the - 1
        /// </summary>
        public CollectionHandler<object> EquippedSkill { get; set; }
        public CollectionHandler<object> AutoSkill { get; set; }
        public CollectionHandler<object> SkillInv { get; set; }

        public Dictionary<int, int> SkillGroupIndex { get; set; }

        public SkillSynStats() : base(LOTYPE.SkillStats)
        {
            _basicAttack1SId = 0;
            _basicAttack2SId = 0;
            _basicAttack3SId = 0;
            //_basicAttack4SId = 0;
            //_basicAttack5SId = 0;

            _jobSkillAttackSId = 0;
            _SkillInvCount = 0;
            _equipGroup = 0;
            _autoGroup = 0;
            _equipSize = 0;
            //_RedHeroCardSkillAttackSId = 0;
            //_GreenHeroSkillAttackSId = 0;
            //_BlueHeroSkillAttackSId = 0;

            SkillInv = new CollectionHandler<object>(MAX_SKILLCOUNT);
            SkillInv.SetParent(this, "SkillInv");
            SkillInv.SetNotifyParent(false);
            for (int i = 0; i < SkillInv.Count; ++i)
            {
                SkillInv[i] = 0;
            }
            SkillInv.SetNotifyParent(true);
            EquippedSkill = new CollectionHandler<object>(MAX_EQUIPPED);
            EquippedSkill.SetParent(this, "EquippedSkill");
            EquippedSkill.SetNotifyParent(false);
            for (int i = 0; i < 36; ++i)
            {
                EquippedSkill[i] = 0;
            }
            EquippedSkill.SetNotifyParent(true);

            AutoSkill = new CollectionHandler<object>(MAX_EQUIPPED);
            AutoSkill.SetParent(this, "AutoSkill");
            AutoSkill.SetNotifyParent(false);
            for (int i = 0; i < 36; ++i)
            {
                AutoSkill[i] = 0;
            }
            AutoSkill.SetNotifyParent(true);

            SkillGroupIndex = new Dictionary<int, int>();
        }

        public void CopyFromInvData(SkillInventoryData sid)
        {

            // Init basicattack skill id from inventory;
            basicAttack1SId = sid.basicAttack1SId;
            //basicAttack2SId = sid.basicAttack2SId;
            //basicAttack3SId = sid.basicAttack3SId;


            _SkillInvCount = sid.SkillInvCount;
            _equipGroup = sid.equipGroup;
            _autoGroup = sid.autoGroup;
            _equipSize = sid.EquipSize;

            for (int i = 0; i < sid.EquippedSkill.Count; ++i)
            {
                EquippedSkill[i] = sid.EquippedSkill[i];
                AutoSkill[i] = sid.AutoSkill[i];
            }

            for(int i = 0; i < sid.SkillInv.Count; ++i)
            {
                SkillInv[i] = sid.SkillInv[i];
                if (sid.SkillInv[i] != 0 && i % 2 == 0)
                    SkillGroupIndex[sid.SkillInv[i]] = i;
            }
        }

        public override void SetDirty()
        {
            base.SetDirty();
        }

        public int basicAttack1SId
        {
            get { return _basicAttack1SId; }
            set { OnSetAttribute("basicAttack1SId", value); _basicAttack1SId = value; }
        }

        public int basicAttack2SId
        {
            get { return _basicAttack2SId; }
            set { OnSetAttribute("basicAttack2SId", value); _basicAttack2SId = value; }
        }

        public int basicAttack3SId
        {
            get { return _basicAttack3SId; }
            set { OnSetAttribute("basicAttack3SId", value); _basicAttack3SId = value; }
        }

        //public int basicAttack4SId
        //{
        //    get { return _basicAttack4SId; }
        //    set { this.OnSetAttribute("basicAttack4SId", value); _basicAttack4SId = value; }
        //}
        //public int basicAttack5SId
        //{
        //    get { return _basicAttack5SId; }
        //    set { this.OnSetAttribute("basicAttack5SId", value); _basicAttack5SId = value; }
        //}

        public int JobskillAttackSId
        {
            get { return _jobSkillAttackSId; }
            set { OnSetAttribute("JobskillAttackSId", value); _jobSkillAttackSId = value; }
        }

        public int SkillInvCount
        {
            get { return _SkillInvCount; }
            set { OnSetAttribute("SkillInvCount", value); _SkillInvCount = value; }
        }

        public int EquipGroup
        {
            get { return _equipGroup; }
            set { OnSetAttribute("EquipGroup", value); _equipGroup = value; }
        }
        public int AutoGroup
        {
            get { return _autoGroup; }
            set { OnSetAttribute("AutoGroup", value); _autoGroup = value; }
        }
        public int EquipSize
        {
            get { return _equipSize; }
            set { OnSetAttribute("EquipSize", value); _equipSize = value; }
        }
    }


    //All static object local object should inherit from this class:
    public class StaticObjectStat : LocalObject
    {
        private byte _State;

        public StaticObjectStat(LOTYPE lotype) : base(lotype)
        {
            _State = 0;
        }

        public byte State
        {
            get { return _State; }
            set { OnSetAttribute("State", value); _State = value; }
        }
    }

    public class BuffTimeStats : LocalObject //send only to local client
    {
        public static readonly int MAX_EFFECTS = 30;

        public BuffTimeStats() : base(LOTYPE.BuffTimeStats)
        {
            Positives = new CollectionHandler<object>(MAX_EFFECTS);
            Positives.SetParent(this, "Positives");

            Negatives = new CollectionHandler<object>(MAX_EFFECTS);
            Negatives.SetParent(this, "Negatives");

            Persistents = new CollectionHandler<object>(MAX_EFFECTS);
            Persistents.SetParent(this, "Persistents");

            PersistentsDur = new CollectionHandler<object>(MAX_EFFECTS);
            PersistentsDur.SetParent(this, "PersistentsDur");
            Init();
        }

        private void Init()
        {
            Positives.SetNotifyParent(false);
            Negatives.SetNotifyParent(false);
            Persistents.SetNotifyParent(false);
            PersistentsDur.SetNotifyParent(false);
            for (int i = 0; i < MAX_EFFECTS; i++)
            {
                Positives[i] = 0;
                Negatives[i] = 0;
                Persistents[i] = 0;
                PersistentsDur[i] = 0;
            }
            Positives.SetNotifyParent(true);
            Negatives.SetNotifyParent(true);
            Persistents.SetNotifyParent(true);
            PersistentsDur.SetNotifyParent(true);
        }

        public CollectionHandler<object> Positives { get; set; }
        public CollectionHandler<object> Negatives { get; set; }
        //public CollectionHandler<object> StartTime { get; set; } // For positives only
        //public CollectionHandler<object> Duration { get; set; }
        public CollectionHandler<object> Persistents { get; set; }
        public CollectionHandler<object> PersistentsDur { get; set; }

    }

    #region Social
    public class SocialInfoBase
    {
        public string charName = "";
        public int portraitId = 0;
        public byte jobSect = 0;
        public byte vipLvl = 0;
        public int charLvl = 0;
        public int combatScore = 0;
        public int localObjIdx = 0;

        public SocialInfoBase(string charname, int portrait, byte job, byte viplvl, int progresslvl, int combatscore, int mLocalObjIdx)
        {
            charName = charname;
            portraitId = portrait;
            jobSect = job;
            vipLvl = viplvl;
            charLvl = progresslvl;
            combatScore = combatscore;
            localObjIdx = mLocalObjIdx;
        }

        public SocialInfoBase(string str = "")
        {
            InitFromString(str);
        }

        public void InitFromString(string str)
        {
            string[] infos = str.Split('`');
            if (infos.Length == 6)
            {
                int idx = 0;
                charName = infos[idx++];
                portraitId = int.Parse(infos[idx++]);
                jobSect = byte.Parse(infos[idx++]);
                vipLvl = byte.Parse(infos[idx++]);
                charLvl = int.Parse(infos[idx++]);
                combatScore = int.Parse(infos[idx++]);
            }
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(charName);
            sb.Append("`");
            sb.Append(portraitId);
            sb.Append("`");
            sb.Append(jobSect);
            sb.Append("`");
            sb.Append(vipLvl);
            sb.Append("`");
            sb.Append(charLvl);
            sb.Append("`");
            sb.Append(combatScore);
            return sb.ToString();
        }
    }

    public class SocialInfo : SocialInfoBase
    {
        public byte faction = 0;
        public string guildName = "";
        public bool isOnline = false;

        public SocialInfo(string charname, int portrait, byte job, byte viplvl, int progresslvl, int combatscore,
                          byte factiontype, string guildname, bool online, int mLocalObjIdx)
                        : base(charname, portrait, job, viplvl, progresslvl, combatscore, mLocalObjIdx)
        {
            faction = factiontype;
            guildName = guildname;
            isOnline = online;
        }

        public SocialInfo(string str = "")
        {
            InitFromString(str);
        }

        public new void InitFromString(string str)
        {
            string[] infos = str.Split('`');
            if (infos.Length == 9)
            {
                int idx = 0;
                charName = infos[idx++];
                portraitId = int.Parse(infos[idx++]);
                jobSect = byte.Parse(infos[idx++]);
                vipLvl = byte.Parse(infos[idx++]);
                charLvl = int.Parse(infos[idx++]);
                combatScore = int.Parse(infos[idx++]);
                faction = byte.Parse(infos[idx++]);
                guildName = infos[idx++];
                isOnline = bool.Parse(infos[idx++]);
            }
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(charName);
            sb.Append("`");
            sb.Append(portraitId);
            sb.Append("`");
            sb.Append(jobSect);
            sb.Append("`");
            sb.Append(vipLvl);
            sb.Append("`");
            sb.Append(charLvl);
            sb.Append("`");
            sb.Append(combatScore);
            sb.Append("`");
            sb.Append(faction);
            sb.Append("`");
            sb.Append(guildName);
            sb.Append("`");
            sb.Append(isOnline);
            return sb.ToString();
        }
    }

    public class SocialStats : LocalObject // Send only to local client
    {
        public SocialStats() : base(LOTYPE.SocialStats)
        {
            friendList = new CollectionHandler<object>(SocialInventoryData.MAX_FRIENDS);
            friendList.SetParent(this, "friendList");
            friendListDict = new Dictionary<string, SocialInfo>();
            friendRequestList = new CollectionHandler<object>(SocialInventoryData.MAX_FRIENDS);
            friendRequestList.SetParent(this, "friendRequestList");
            friendRequestListDict = new Dictionary<string, SocialInfoBase>();
        }

        public CollectionHandler<object> friendList { get; set; }
        public CollectionHandler<object> friendRequestList { get; set; }

        public Dictionary<string, SocialInfo> GetFriendListDict() { return friendListDict; }
        private Dictionary<string, SocialInfo> friendListDict;
        public Dictionary<string, SocialInfoBase> GetFriendRequestListDict() { return friendRequestListDict; }
        private Dictionary<string, SocialInfoBase> friendRequestListDict;

        public int GetAvailableSlotFriends()
        {
            int cnt = friendList.Count;
            for (int i = 0; i < cnt; ++i)
            {
                if (friendList[i] == null)
                    return i;
            }
            return -1;
        }

        public int GetAvailableSlotRequests()
        {
            int cnt = friendRequestList.Count;
            for (int i = 0; i < cnt; ++i)
            {
                if (friendRequestList[i] == null)
                    return i;
            }
            return -1;
        }
    }
    #endregion

    #region Lottery
    [JsonObject(MemberSerialization = MemberSerialization.OptIn)]
    public class LotteryInfo
    {
        [DefaultValue(0)]
        [JsonProperty(PropertyName = "0")]
        public int id { get; set; }

        [DefaultValue(0)]
        [JsonProperty(PropertyName = "1")]
        public int freeTicket { get; set; }

        [DefaultValue(0)]
        [JsonProperty(PropertyName = "2")]
        public int point { get; set; }

        [DefaultValue(0)]
        [JsonProperty(PropertyName = "3")]
        public long lastUpdateTime { get; set; }

        [DefaultValue(0)]
        [JsonProperty(PropertyName = "4")]
        public int totalLotteryCount { get; set; }

        [JsonProperty(PropertyName = "5")]
        public List<int> rewardPoints { get; set; }
    }

    public class LotteryInfoStats : LocalObject
    {
        public LotteryInfoStats() : base(LOTYPE.LotteryShopItemsTabStats)
        {
            lotteryInfos = new CollectionHandler<object>(8);
            lotteryInfos.SetParent(this, "lotteryInfos");

            Init();
        }

        public void Init()
        {
            for (int i = 0; i < lotteryInfos.Count; i++)
                lotteryInfos[i] = null;
        }
        public CollectionHandler<object> lotteryInfos { get; set; }
    }
    #endregion

    #region Welfare
    public class WelfareStats : LocalObject
    {
        private int _firstLoginYear;
        private int _firstLoginMonth;
        private int _firstLoginDay;
        private int _serverStartYear;
        private int _serverStartMonth;
        private int _serverStartDay;
        private short _continuousLoginDayNum;
        private int _continuousLoginFullCollection;
        private int _continuousLoginPartCollection;

        private int _onlineRewardsClaims;
        private long _onlineDuration;

        private int _serviceFundsLvlCollection;
        private string _serviceFundLvlClaims;
        private int _serviceFundsPplCollection;
        private string _serviceFundPplClaims;
        private int _serviceFundsClaimNum;
        private int _serviceFundsJoinMemberNum;
        private bool _serviceFundsBought;

        private int _firstBuyFlag;
        private int _firstBuyCollected;
        private bool _firstGoldBuyCollected;

        private int _totalGoldCredited;
        private string _totalCreditClaims;

        private int _totalGoldSpent;
        private string _totalSpendClaims;

        private bool _dailyGoldFirstLogin;
        private bool _monthCardBought;
        private int _monthCardBoughtDayNum;
        private bool _monthCardGoldCollected;
        private bool _permanentCardBought;
        private bool _permanentCardGoldCollected;

        private string _goldJackpotResult;
        private int _goldJackpotAllGold;
        private int _goldJackpotCurrTier;

        private string _contLoginClaims;

        public WelfareStats() : base(LOTYPE.WelfareStats)
        {
            _firstLoginYear = 0;
            _firstLoginMonth = 0;
            _firstLoginDay = 0;
            _serverStartYear = 0;
            _serverStartMonth = 0;
            _serverStartDay = 0;
            _continuousLoginDayNum = 0;
            _continuousLoginFullCollection = 0;
            _continuousLoginPartCollection = 0;

            _onlineRewardsClaims = 0;
            _onlineDuration = 0;

            _serviceFundsLvlCollection = 0;
            _serviceFundLvlClaims = "";
            _serviceFundsPplCollection = 0;
            _serviceFundPplClaims = "";
            _serviceFundsClaimNum = 0;
            _serviceFundsJoinMemberNum = 0;
            _serviceFundsBought = false;

            _totalGoldCredited = 0;
            _totalCreditClaims = "";

            _totalGoldSpent = 0;
            _totalSpendClaims = "";

            _dailyGoldFirstLogin = true;
            _monthCardBought = false;
            _monthCardBoughtDayNum = 0;
            _monthCardGoldCollected = false;
            _permanentCardBought = false;
            _permanentCardGoldCollected = false;

            _firstBuyFlag = 0;
            _firstBuyCollected = 0;
            _firstGoldBuyCollected = false;

            _goldJackpotResult = "";
            _goldJackpotAllGold = 0;
            _goldJackpotCurrTier = 0;
        }
        public int firstLoginYear
        {
            get { return _firstLoginYear; }
            set { OnSetAttribute("firstLoginYear", value); _firstLoginYear = value; }
        }

        public int firstLoginMonth
        {
            get { return _firstLoginMonth; }
            set { OnSetAttribute("firstLoginMonth", value); _firstLoginMonth = value; }
        }

        public int firstLoginDay
        {
            get { return _firstLoginDay; }
            set { OnSetAttribute("firstLoginDay", value); _firstLoginDay = value; }
        }

        public int serverStartYear
        {
            get { return _serverStartYear; }
            set { OnSetAttribute("serverStartYear", value); _serverStartYear = value; }
        }

        public int serverStartMonth
        {
            get { return _serverStartMonth; }
            set { OnSetAttribute("serverStartMonth", value); _serverStartMonth = value; }
        }

        public int serverStartDay
        {
            get { return _serverStartDay; }
            set { OnSetAttribute("serverStartDay", value); _serverStartDay = value; }
        }

        public short continuousLoginDayNum
        {
            get { return _continuousLoginDayNum; }
            set { OnSetAttribute("continuousLoginDayNum", value); _continuousLoginDayNum = value; }
        }

        public int continuousLoginFullCollection
        {
            get { return _continuousLoginFullCollection; }
            set { OnSetAttribute("continuousLoginFullCollection", value); _continuousLoginFullCollection = value; }
        }

        public int continuousLoginPartCollection
        {
            get { return _continuousLoginPartCollection; }
            set { OnSetAttribute("continuousLoginPartCollection", value); _continuousLoginPartCollection = value; }
        }

        public int onlineRewardsClaims
        {
            get { return _onlineRewardsClaims; }
            set { OnSetAttribute("onlineRewardsClaims", value); _onlineRewardsClaims = value; }
        }

        public long onlineDuration
        {
            get { return _onlineDuration; }
            set { OnSetAttribute("onlineDuration", value); _onlineDuration = value; }
        }

        public int serviceFundsLvlCollection
        {
            get { return _serviceFundsLvlCollection; }
            set { OnSetAttribute("serviceFundsLvlCollection", value); _serviceFundsLvlCollection = value; }
        }

        public string serviceFundLvlClaims
        {
            get { return _serviceFundLvlClaims; }
            set { OnSetAttribute("serviceFundLvlClaims", value); _serviceFundLvlClaims = value; }
        }

        public int serviceFundsPplCollection
        {
            get { return _serviceFundsPplCollection; }
            set { OnSetAttribute("serviceFundsPplCollection", value); _serviceFundsPplCollection = value; }
        }

        public string serviceFundPplClaims
        {
            get { return _serviceFundPplClaims; }
            set { OnSetAttribute("serviceFundPplClaims", value); _serviceFundPplClaims = value; }
        }

        public int serviceFundsClaimNum
        {
            get { return _serviceFundsClaimNum; }
            set { OnSetAttribute("serviceFundsClaimNum", value); _serviceFundsClaimNum = value; }
        }

        public int serviceFundsJoinMemberNum
        {
            get { return _serviceFundsJoinMemberNum; }
            set { OnSetAttribute("serviceFundsJoinMemberNum", value); _serviceFundsJoinMemberNum = value; }
        }

        public bool serviceFundsBought
        {
            get { return _serviceFundsBought; }
            set { OnSetAttribute("serviceFundsBought", value); _serviceFundsBought = value; }
        }

        public int firstBuyFlag
        {
            get { return _firstBuyFlag; }
            set { OnSetAttribute("firstBuyFlag", value); _firstBuyFlag = value; }
        }

        public int firstBuyCollected
        {
            get { return _firstBuyCollected; }
            set { OnSetAttribute("firstBuyCollected", value); _firstBuyCollected = value; }
        }

        public bool firstGoldBuyCollected
        {
            get { return _firstGoldBuyCollected; }
            set { OnSetAttribute("firstGoldBuyCollected", value); _firstGoldBuyCollected = value; }
        }

        public int totalGoldCredited
        {
            get { return _totalGoldCredited; }
            set { OnSetAttribute("totalGoldCreditedd", value); _totalGoldCredited = value; }
        }

        public string totalCreditClaims
        {
            get { return _totalCreditClaims; }
            set { OnSetAttribute("totalCreditClaims", value); _totalCreditClaims = value; }
        }

        public int totalGoldSpent
        {
            get { return _totalGoldSpent; }
            set { OnSetAttribute("totalGoldSpent", value); _totalGoldSpent = value; }
        }

        public string totalSpendClaims
        {
            get { return _totalSpendClaims; }
            set { OnSetAttribute("totalSpendClaims", value); _totalSpendClaims = value; }
        }

        public bool dailyGoldFirstLogin
        {
            get { return _dailyGoldFirstLogin; }
            set { OnSetAttribute("dailyGoldFirstLogin", value); _dailyGoldFirstLogin = value; }
        }

        public bool monthCardBought
        {
            get { return _monthCardBought; }
            set { OnSetAttribute("monthCardBought", value); _monthCardBought = value; }
        }

        public int monthCardBoughtDayNum
        {
            get { return _monthCardBoughtDayNum; }
            set { OnSetAttribute("monthCardBoughtDayNum", value); _monthCardBoughtDayNum = value; }
        }

        public bool monthCardGoldCollected
        {
            get { return _monthCardGoldCollected; }
            set { OnSetAttribute("monthCardGoldCollected", value); _monthCardGoldCollected = value; }
        }

        public bool permanentCardBought
        {
            get { return _permanentCardBought; }
            set { OnSetAttribute("permanentCardBought", value); _permanentCardBought = value; }
        }

        public bool permanentCardGoldCollected
        {
            get { return _permanentCardGoldCollected; }
            set { OnSetAttribute("permanentCardGoldCollected", value); _permanentCardGoldCollected = value; }
        }

        public string goldJackpotResult
        {
            get { return _goldJackpotResult; }
            set { OnSetAttribute("goldJackpotResult", value); _goldJackpotResult = value; }
        }

        public int goldJackpotAllGold
        {
            get { return _goldJackpotAllGold; }
            set { OnSetAttribute("goldJackpotAllGold", value); _goldJackpotAllGold = value; }
        }

        public int goldJackpotCurrTier
        {
            get { return _goldJackpotCurrTier; }
            set { OnSetAttribute("goldJackpotCurrTier", value); _goldJackpotCurrTier = value; }
        }

        public string contLoginClaims
        {
            get { return _contLoginClaims; }
            set { OnSetAttribute("contLoginClaims", value); _contLoginClaims = value; }
        }
    }
    #endregion

    #region SevenDays

    public class SevenDaysStats : LocalObject
    {
        private long _sevenDaysEventStart;

        private string _discountItemBoughtNums;

        private string _taskProgress;
        private string _taskCollected;

        public CollectionHandler<object> dayOneTasksCollected { get; set; }
        private bool _dayOneTasksAllCollected;
        public CollectionHandler<object> dayTwoTasksCollected { get; set; }
        private bool _dayTwoTasksAllCollected;
        public CollectionHandler<object> dayThreeTasksCollected { get; set; }
        private bool _dayThreeTasksAllCollected;
        public CollectionHandler<object> dayFourTasksCollected { get; set; }
        private bool _dayFourTasksAllCollected;
        public CollectionHandler<object> dayFiveTasksCollected { get; set; }
        private bool _dayFiveTasksAllCollected;
        public CollectionHandler<object> daySixTasksCollected { get; set; }
        private bool _daySixTasksAllCollected;
        public CollectionHandler<object> daySevenTasksCollected { get; set; }
        private bool _daySevenTasksAllCollected;

        public CollectionHandler<object> dayOneTasksCompleted { get; set; }
        public CollectionHandler<object> dayTwoTasksCompleted { get; set; }
        public CollectionHandler<object> dayThreeTasksCompleted { get; set; }
        public CollectionHandler<object> dayFourTasksCompleted { get; set; }
        public CollectionHandler<object> dayFiveTasksCompleted { get; set; }
        public CollectionHandler<object> daySixTasksCompleted { get; set; }
        public CollectionHandler<object> daySevenTasksCompleted { get; set; }

        public SevenDaysStats()
            : base(LOTYPE.SevenDaysStats)
        {
            _sevenDaysEventStart = 0;

            _discountItemBoughtNums = "";

            // Task reward collection
            dayOneTasksCollected = new CollectionHandler<object>(SevenDaysInvData.MAX_SUBCAT);
            dayOneTasksCollected.SetParent(this, "dayOneTasksCollected");

            dayTwoTasksCollected = new CollectionHandler<object>(SevenDaysInvData.MAX_SUBCAT);
            dayTwoTasksCollected.SetParent(this, "dayTwoTasksCollected");

            dayThreeTasksCollected = new CollectionHandler<object>(SevenDaysInvData.MAX_SUBCAT);
            dayThreeTasksCollected.SetParent(this, "dayThreeTasksCollected");

            dayFourTasksCollected = new CollectionHandler<object>(SevenDaysInvData.MAX_SUBCAT);
            dayFourTasksCollected.SetParent(this, "dayFourTasksCollected");

            dayFiveTasksCollected = new CollectionHandler<object>(SevenDaysInvData.MAX_SUBCAT);
            dayFiveTasksCollected.SetParent(this, "dayFiveTasksCollected");

            daySixTasksCollected = new CollectionHandler<object>(SevenDaysInvData.MAX_SUBCAT);
            daySixTasksCollected.SetParent(this, "daySixTasksCollected");

            daySevenTasksCollected = new CollectionHandler<object>(SevenDaysInvData.MAX_SUBCAT);
            daySevenTasksCollected.SetParent(this, "daySevenTasksCollected");

            // Task completion
            dayOneTasksCompleted = new CollectionHandler<object>(SevenDaysInvData.MAX_SUBCAT);
            dayOneTasksCompleted.SetParent(this, "dayOneTasksCompleted");

            dayTwoTasksCompleted = new CollectionHandler<object>(SevenDaysInvData.MAX_SUBCAT);
            dayTwoTasksCompleted.SetParent(this, "dayTwoTasksCompleted");

            dayThreeTasksCompleted = new CollectionHandler<object>(SevenDaysInvData.MAX_SUBCAT);
            dayThreeTasksCompleted.SetParent(this, "dayThreeTasksCompleted");

            dayFourTasksCompleted = new CollectionHandler<object>(SevenDaysInvData.MAX_SUBCAT);
            dayFourTasksCompleted.SetParent(this, "dayFourTasksCompleted");

            dayFiveTasksCompleted = new CollectionHandler<object>(SevenDaysInvData.MAX_SUBCAT);
            dayFiveTasksCompleted.SetParent(this, "dayFiveTasksCompleted");

            daySixTasksCompleted = new CollectionHandler<object>(SevenDaysInvData.MAX_SUBCAT);
            daySixTasksCompleted.SetParent(this, "daySixTasksCompleted");

            daySevenTasksCompleted = new CollectionHandler<object>(SevenDaysInvData.MAX_SUBCAT);
            daySevenTasksCompleted.SetParent(this, "daySevenTasksCompleted");
        }

        public long sevenDaysEventStart
        {
            get { return _sevenDaysEventStart; }
            set { this.OnSetAttribute("sevenDaysEventStart", value); _sevenDaysEventStart = value; }
        }

        public string discountItemBoughtNums
        {
            get { return _discountItemBoughtNums; }
            set { this.OnSetAttribute("discountItemBoughtNums", value); _discountItemBoughtNums = value; }
        }

        public string taskProgress
        {
            get { return _taskProgress; }
            set { this.OnSetAttribute("taskProgress", value); _taskProgress = value; }
        }

        public string taskCollected
        {
            get { return _taskCollected; }
            set { this.OnSetAttribute("taskCollected", value); _taskCollected = value; }
        }

        public bool dayOneTasksAllCollected
        {
            get { return _dayOneTasksAllCollected; }
            set { this.OnSetAttribute("dayOneTasksAllCollected", value); _dayOneTasksAllCollected = value; }
        }

        public bool dayTwoTasksAllCollected
        {
            get { return _dayTwoTasksAllCollected; }
            set { this.OnSetAttribute("dayTwoTasksAllCollected", value); _dayTwoTasksAllCollected = value; }
        }

        public bool dayThreeTasksAllCollected
        {
            get { return _dayThreeTasksAllCollected; }
            set { this.OnSetAttribute("dayThreeTasksAllCollected", value); _dayThreeTasksAllCollected = value; }
        }

        public bool dayFourTasksAllCollected
        {
            get { return _dayFourTasksAllCollected; }
            set { this.OnSetAttribute("dayFourTasksAllCollected", value); _dayFourTasksAllCollected = value; }
        }

        public bool dayFiveTasksAllCollected
        {
            get { return _dayFiveTasksAllCollected; }
            set { this.OnSetAttribute("dayFiveTasksAllCollected", value); _dayFiveTasksAllCollected = value; }
        }

        public bool daySixTasksAllCollected
        {
            get { return _daySixTasksAllCollected; }
            set { this.OnSetAttribute("daySixTasksAllCollected", value); _daySixTasksAllCollected = value; }
        }

        public bool daySevenTasksAllCollected
        {
            get { return _daySevenTasksAllCollected; }
            set { this.OnSetAttribute("daySevenTasksAllCollected", value); _daySevenTasksAllCollected = value; }
        }
    }

    #endregion

    #region QuestExtraRewards

    public class QuestExtraRewardsStats : LocalObject
    {
        private string _taskProgress;
        private int _taskCompletion;
        private string _taskCompleted;
        private int _taskCollection;
        private string _taskCollected;
        private int _boxRewardCollection;
        private string _boxRewardCollected;
        private int _activePts;

        public QuestExtraRewardsStats()
            : base(LOTYPE.QuestExtraRewardsStats)
        {
            _taskProgress = "0";
            _taskCompletion = 0;
            _taskCompleted = "0";
            _taskCollection = 0;
            _taskCollected = "0";
            _boxRewardCollection = 0;
            _boxRewardCollected = "0";
            _activePts = 0;
        }

        public string taskProgress
        {
            get { return _taskProgress; }
            set { this.OnSetAttribute("taskProgress", value); _taskProgress = value; }
        }

        public int taskCompletion
        {
            get { return _taskCompletion; }
            set { this.OnSetAttribute("taskCompletion", value); _taskCompletion = value; }
        }

        public string taskCompleted
        {
            get { return _taskCompleted; }
            set { this.OnSetAttribute("taskCompleted", value); _taskCompleted = value; }
        }

        public int taskCollection
        {
            get { return _taskCollection; }
            set { this.OnSetAttribute("taskCollection", value); _taskCollection = value; }
        }

        public string taskCollected
        {
            get { return _taskCollected; }
            set { this.OnSetAttribute("taskCollected", value); _taskCollected = value; }
        }

        public int boxRewardCollection
        {
            get { return _boxRewardCollection; }
            set { this.OnSetAttribute("boxRewardCollection", value); _boxRewardCollection = value; }
        }

        public string boxRewardCollected
        {
            get { return _boxRewardCollected; }
            set { this.OnSetAttribute("boxRewardCollected", value); _boxRewardCollected = value; }
        }

        public int activePts
        {
            get { return _activePts; }
            set { this.OnSetAttribute("activePts", value); _activePts = value; }
        }
    }

    #endregion

    #region PowerUp

    public class PowerUpStats : LocalObject
    {
        // Method #1
        private string _powerUpLvl;

        // Method #2
        public CollectionHandler<object> powerUpSlots { get; set; }

        public PowerUpStats() : base(LOTYPE.PowerUpStats)
        {
            _powerUpLvl = "0";

            powerUpSlots = new CollectionHandler<object>(PowerUpInventoryData.MAX_POWERUPSLOTS);
            powerUpSlots.SetParent(this, "powerUpSlots");
        }

        public string powerUpLvl
        {
            get { return _powerUpLvl; }
            set { OnSetAttribute("powerUpLvl", value); _powerUpLvl = value; }
        }
    }

    public class MeridianStats : LocalObject
    {
        public CollectionHandler<object> meridianLevelSlots { get; set; }
        public CollectionHandler<object> meridianExpSlots { get; set; }

        public MeridianStats() : base(LOTYPE.MeridianStats)
        {
            meridianLevelSlots = new CollectionHandler<object>(PowerUpInventoryData.MAX_MERIDIANLEVELSLOTS);
            meridianLevelSlots.SetParent(this, "meridianLevelSlots");
            meridianExpSlots = new CollectionHandler<object>(PowerUpInventoryData.MAX_MERIDIANLEVELSLOTS);
            meridianExpSlots.SetParent(this, "meridianExpSlots");
        }
    }

    #endregion

    #region EquipmentCraft
    public class EquipmentCraftStats : LocalObject
    {
        private bool _finishedCraft;

        public Dictionary<int, int> achievementRequireList { get; set; }

        public EquipmentCraftStats() : base(LOTYPE.EquipmentCraftStats)
        {
            _finishedCraft = false;
            achievementRequireList = new Dictionary<int, int>(EquipmentCraftInventoryData.MAX_EQUIPMENTACHIEVEMENT);
        }

        public bool finishedCraft
        {
            get { return _finishedCraft; }
            set { OnSetAttribute("finishedCraft", value); _finishedCraft = value; }
        }
    }
    #endregion

    #region EquipFusion
    public class EquipFusionStats : LocalObject
    {
        private bool _FinishedFusion;
        private int _EquipFusionCoin;
        private int _FusionItemSort;
        private string _FusionData;

        public EquipFusionStats() : base(LOTYPE.EquipFusionStats)
        {
            _FinishedFusion = false;
            _EquipFusionCoin = 0;
            _FusionItemSort = 0;
            _FusionData = string.Empty;
        }

        public bool FinishedFusion
        {
            get { return _FinishedFusion; }
            set { OnSetAttribute("FinishedFusion", value); _FinishedFusion = value; }
        }

        public int EquipFusionCoin
        {
            get { return _EquipFusionCoin; }
            set { OnSetAttribute("EquipFusionCoin", value); _EquipFusionCoin = value; }
        }

        public int FusionItemSort
        {
            get { return _FusionItemSort; }
            set { OnSetAttribute("FusionItemSort", value); _FusionItemSort = value; }
        }

        public string FusionData
        {
            get { return _FusionData; }
            set { OnSetAttribute("FusionData", value); _FusionData = value; }
        }
    }
    #endregion

    #region InteractiveTrigger
    public class InteractiveTriggerStats : LocalObject
    {
        private bool _canTrigger;
        private bool _waitResponse;

        public InteractiveTriggerStats() : base(LOTYPE.InteractiveTriggerStats)
        {
            _canTrigger = false;
            _waitResponse = false;
        }

        public bool canTrigger
        {
            get { return _canTrigger; }
            set { OnSetAttribute("canTrigger", value); _canTrigger = value; }
        }

        public bool waitResponse
        {
            get { return _waitResponse; }
            set { OnSetAttribute("waitResponse", value); _waitResponse = value; }
        }
    }
    #endregion

    public class TongbaoCostBuffInfo : LocalObject
    {
        public int _id;
        public string _name;
        public string _starttime;
        public string _endtime;
        public int _costamount;
        public int _skillid;
        public bool _isuse;

        public TongbaoCostBuffInfo() : base(LOTYPE.TongbaoCostBuff)
        {
            _id = 0;
            _name = "";
            _starttime = "";
            _endtime = "";
            _costamount = 0;
            _skillid = 0;
            _isuse = false;
        }

        public int id
        {
            get { return _id; }
            set { this.OnSetAttribute("id", value); _id = value; }
        }

        public string name
        {
            get { return _name; }
            set { this.OnSetAttribute("name", value); _name = value; }
        }

        public string starttime
        {
            get { return _starttime; }
            set { this.OnSetAttribute("starttime", value); _starttime = value; }
        }

        public string endtime
        {
            get { return _endtime; }
            set { this.OnSetAttribute("endtime", value); _endtime = value; }
        }

        public int costamount
        {
            get { return _costamount; }
            set { this.OnSetAttribute("costamount", value); _costamount = value; }
        }

        public int skillid
        {
            get { return _skillid; }
            set { this.OnSetAttribute("skillid", value); _skillid = value; }
        }

        public bool isuse
        {
            get { return _isuse; }
            set { this.OnSetAttribute("isuse", value); _isuse = value; }
        }

        public bool Set(int __id, string __name, string __starttime, string __endtime, int __costamount, int __skillid, bool __isuse)
        {
            bool change = false;
            if (id != __id || name != __name || starttime != __starttime || endtime != __endtime
                || costamount != __costamount || skillid != __skillid || isuse != __isuse)
            {
                change = true;
            }

            id = __id;
            name = __name;
            starttime = __starttime;
            endtime = __endtime;
            costamount = __costamount;
            skillid = __skillid;
            isuse = __isuse;

            return change;
        }

        public bool CheckInTime()
        {
            if (id > 0)
            {
                DateTime st = DateTime.ParseExact(starttime, "yyyy/MM/dd HH:mm", null);
                DateTime et = DateTime.ParseExact(endtime, "yyyy/MM/dd HH:mm", null);
                DateTime nowtime = DateTime.Now;
                int result1 = DateTime.Compare(nowtime, st);
                int result2 = DateTime.Compare(nowtime, et);
                if (result1 >= 0 && result2 < 0)
                {
                    return true;
                }
            }
            return false;
        }
    }
}
