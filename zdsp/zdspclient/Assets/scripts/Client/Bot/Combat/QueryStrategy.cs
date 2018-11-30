using System;
using Zealot.Client.Entities;

namespace Zealot.Bot
{
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

        private QueryData mQueryData = null;

        private QueryContext()
        {
            mQueryData = new QueryData();
            SetQueryType(BotQueryType.NearestEnemyQuery);
        }

        public QueryData GetQueryData()
        {
            return mQueryData;
        }

        public void SetQueryType(BotQueryType queryType)
        {
            switch (queryType)
            {
                case BotQueryType.NearestEnemyQuery:
                    mQueryData.SetQueryStrategy(new NearestEnemyQuery());
                    break;
                case BotQueryType.SpecificEnemyQuery:
                    mQueryData.SetQueryStrategy(new SpecificEnemyQuery());
                    break;
                default:
                    break;
            }

            mQueryData.InitData();
        }

        public ActorGhost QueryResult()
        {
            try
            {
                return mQueryData.GetQeuryStrategy().QueryResult();
            }
            catch (Exception ex)
            {
                throw new Exception("Bot query error..." + ex.Message);
            }
        }
    }

    #region Query Strategy
    public abstract class IQueryStrategy
    {
        public abstract void Init(QueryData queryData);
        public abstract ActorGhost QueryResult();
    }

    public class NearestEnemyQuery : IQueryStrategy
    {
        float radius;

        public override void Init(QueryData queryData)
        {
            if (queryData == null)
                return;

            radius = queryData.GetRadius();
        }

        public override ActorGhost QueryResult()
        {
            return BotQuerySystem.Instance.GetNearestEnemyInRange(radius);
        }
    }

    public class SpecificEnemyQuery : IQueryStrategy
    {
        float radius;
        int targetID;

        public override void Init(QueryData queryData)
        {
            if (queryData == null)
                return;

            radius = queryData.GetRadius();
            targetID = queryData.GetTargetID();
        }

        public override ActorGhost QueryResult()
        {
            return BotQuerySystem.Instance.GetNearestEnemyByID(radius, targetID);
        }
    }
    #endregion

    #region Query Data
    public class QueryData
    {
        protected IQueryStrategy mQueryStrategy = null;

        protected float mRadius;
        protected int mTargetID;

        public void InitData()
        {
            SetRadius(BotController.MaxQueryRadius);
            mQueryStrategy.Init(this);
        }

        public void SetQueryStrategy(IQueryStrategy strategy)
        {
            mQueryStrategy = strategy;
        }

        public IQueryStrategy GetQeuryStrategy()
        {
            return mQueryStrategy;
        }

        public void SetRadius(float radius)
        {
            mRadius = radius;
        }

        public float GetRadius()
        {
            return mRadius;
        }

        public void SetTargetID(int id)
        {
            mTargetID = id;
        }

        public int GetTargetID()
        {
            return mTargetID;
        }
    }
    #endregion
}