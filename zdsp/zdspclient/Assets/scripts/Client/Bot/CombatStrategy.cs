using Zealot.Client.Entities;

namespace Zealot.Bot
{
    public interface IQueryEnemy
    {
        ActorGhost QueryResult();
    }

    public class BotQuery : IQueryEnemy
    {
        float radius;

        public BotQuery(float radius)
        {
            this.radius = radius;
        }

        public ActorGhost QueryResult()
        {
            return GameInfo.gLocalPlayer.Bot.GetNearestEnemyInRange(radius);
        }
    }

    public class CombatQuestQuery : IQueryEnemy
    {
        float radius;
        int targetID;

        public CombatQuestQuery(float radius, int targetID)
        {
            this.radius = radius;
            this.targetID = targetID;
        }

        public ActorGhost QueryResult()
        {
            return GameInfo.gLocalPlayer.Bot.GetNearestEnemyByID(radius, targetID);
        }
    }

    public abstract class CombatStrategy
    {
        protected IQueryEnemy queryNearestEnemy;

        public void SetQueryType(IQueryEnemy queryNearestEnemy)
        {
            this.queryNearestEnemy = queryNearestEnemy;
        }

        public ActorGhost QueryResult()
        {
            return queryNearestEnemy.QueryResult();
        }
    }

    public class BotCombat : CombatStrategy
    {
        public BotCombat(float radius)
        {
            queryNearestEnemy = new BotQuery(radius);
        }
    }

    public class QuestCombat : CombatStrategy
    {
        public QuestCombat(float radius, int targetID)
        {
            queryNearestEnemy = new CombatQuestQuery(radius, targetID);
        }
    }
}

