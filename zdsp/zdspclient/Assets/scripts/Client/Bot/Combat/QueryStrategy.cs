using System;
using System.Collections.Generic;
using System.Reflection;
using Zealot.Client.Entities;

namespace Zealot.Bot
{
    public interface IQueryEntity
    {
        ActorGhost QueryResult();
    }

    public class NearestEnemyQuery : IQueryEntity
    {
        float radius;

        public NearestEnemyQuery(float radius)
        {
            this.radius = radius;
        }

        public ActorGhost QueryResult()
        {
            return BotQuerySystem.Instance.GetNearestEnemyInRange(radius);
        }
    }

    public class SpecificEnemyQuery : IQueryEntity
    {
        float radius;
        int targetID;

        public SpecificEnemyQuery(float radius, int targetID)
        {
            this.radius = radius;
            this.targetID = targetID;
        }

        public ActorGhost QueryResult()
        {
            return BotQuerySystem.Instance.GetNearestEnemyByID(radius, targetID);
        }
    }

    public enum BotQueryType : byte
    {
        NearestEnemyQuery = 0,
        SpecificEnemyQuery = 1
    }

    public class QueryContext
    {
        #region Singleton
        private static QueryContext instance = null;
        public static QueryContext Instance
        {
            get
            {
                if (instance == null)
                    instance = new QueryContext();
                return instance;
            }
        }
        #endregion

        private BotQueryType mBotQueryType = BotQueryType.NearestEnemyQuery;
        private IQueryEntity queryEntity = null;

        private QueryContext()
        {
            SetQueryType(BotQueryType.NearestEnemyQuery);
        }

        public void SetQueryType(BotQueryType queryType, int targetID = 0)
        {
            mBotQueryType = queryType;
            queryEntity = QueryStrategyFactory.Create(mBotQueryType, targetID);
        }

        // 依照目前的Query方式去回傳目標
        public ActorGhost QueryResult()
        {
            try
            {
                return queryEntity.QueryResult();
            }
            catch (Exception ex)
            {
                throw new Exception("Bot query error..." + ex.Message);
            }
        }
    }

    public static class QueryStrategyFactory
    {
        public static IQueryEntity Create(BotQueryType queryType, int targetID)
        {
            switch (queryType)
            {
                case BotQueryType.NearestEnemyQuery:
                    return new NearestEnemyQuery(BotController.MaxQueryRadius);
                case BotQueryType.SpecificEnemyQuery:
                    return new SpecificEnemyQuery(BotController.MaxQueryRadius, targetID);
                default:
                    return null;
            }
        }
    }
}