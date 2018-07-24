using Zealot.Common;
namespace Zealot.Common.Entities
{
	public interface IActor
	{
		ActorSynStats PlayerStats{ get; set; }
        string Name { get; set; }
        bool IsAlive();
        bool IsInvalidTarget();
        bool IsInSafeZone();
        int Team { get; set; }
        int GetParty();

        void OnDamage(IActor attacker, AttackResult res, bool pbasicAttack);
        void OnRecoverHealth(int origamount);//Only for server
        void QueueDmgResult(AttackResult res);
        //void QueueSideEffectHit(int attackerpid, int sideeffectID); //hit effect to show at client on this actor
        void OnAttacked(IActor attacker, int aggro);
        void OnKilled(IActor attacker);
        void UpdateLocalSkillPassiveStats();
        void OnComputeCombatStats();
        ICombatStats CombatStats { get; set; }

        SkillPassiveCombatStats SkillPassiveStats { get;}

        int GetMinDmg();

        bool MaxEvasionChance { get; set; }
        bool MaxCriticalChance { get; set; }

        int GetAccuracy();

        int GetAttack();

        int GetCritical();

        int GetCriticalDamage();

        int GetArmor();

        int GetEvasion();

        int GetCocritical();

        int GetCocriticalDamage();

        float GetExDamage();
    }
}
