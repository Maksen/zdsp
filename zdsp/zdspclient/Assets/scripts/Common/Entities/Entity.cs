using System;

namespace Zealot.Common.Entities
{
    using UnityEngine;

    [Flags]
    public enum EntityTypeAttribute : uint
    {
        ETA_STATIC = 0x00020000,
        ETA_NET = 0x00040000,
        ETA_GHOST = 0x00080000
    }

    public enum EntityType : uint
    {
        Player = 1 | EntityTypeAttribute.ETA_NET,
        PlayerGhost = 2 | EntityTypeAttribute.ETA_NET | EntityTypeAttribute.ETA_GHOST,
        Monster = 3 | EntityTypeAttribute.ETA_NET,
        MonsterGhost = 4 | EntityTypeAttribute.ETA_NET | EntityTypeAttribute.ETA_GHOST,
        Loot = 5 | EntityTypeAttribute.ETA_NET | EntityTypeAttribute.ETA_STATIC,
        LootGhost = 6 | EntityTypeAttribute.ETA_NET | EntityTypeAttribute.ETA_STATIC | EntityTypeAttribute.ETA_GHOST,
        ClientSpawner = 7,
        StaticNPC = 8,
        ShopNPC = 9,
        Gate = 10 | EntityTypeAttribute.ETA_NET | EntityTypeAttribute.ETA_STATIC,
        GateGhost = 11 | EntityTypeAttribute.ETA_NET | EntityTypeAttribute.ETA_STATIC | EntityTypeAttribute.ETA_GHOST,
        AIPlayer = 14 | EntityTypeAttribute.ETA_NET,
        AnimationObject = 16 | EntityTypeAttribute.ETA_NET | EntityTypeAttribute.ETA_STATIC,
        AnimationObjectGhost = 17 | EntityTypeAttribute.ETA_NET | EntityTypeAttribute.ETA_STATIC | EntityTypeAttribute.ETA_GHOST,
        Hero = 18 | EntityTypeAttribute.ETA_NET,
        HeroGhost = 19 | EntityTypeAttribute.ETA_NET | EntityTypeAttribute.ETA_GHOST,
        CompanionGhost = 20,
        InteractiveTrigger = 21 | EntityTypeAttribute.ETA_NET,
    }

    public abstract class Entity
    {
        public EntitySystem EntitySystem
        { get; set; }

        public EntityType EntityType
        { get; set; }

        protected int mnID;
        public int ID { get { return mnID; } }
        public void SetID(int id)
        {
            mnID = id;
        }

        protected Vector3 mPos;
        public virtual Vector3 Position
        {
            get { return mPos; }
            set
            {
                EntitySystem.UpdateGridId(this, value.x, value.z);
                mPos = value;
            }
        }

        protected Vector3 mForward;
        public virtual Vector3 Forward
        {
            get { return mForward; }
            set { mForward = value; }
        }

        public float Radius { get; set; }

        public bool Destroyed
        {
            get; set;
        }

        public Entity()
        {
            Radius = 0;
        }

        public virtual void Update(long dt)
        {
        }

        public bool IsActor()
        {
            switch (EntityType)
            {
                case EntityType.Player:
                case EntityType.PlayerGhost:
                case EntityType.Monster:
                case EntityType.MonsterGhost:
                case EntityType.AIPlayer:
                case EntityType.Hero:
                case EntityType.HeroGhost:
                    return true;
            }
            return false;
        }

        public bool IsPlayer()
        {
            return EntityType == EntityType.Player || EntityType == EntityType.PlayerGhost;
        }

        public bool IsAIPlayer()
        {
            return EntityType == EntityType.AIPlayer;
        }

        public bool IsMonster()
        {
            return EntityType == EntityType.Monster || EntityType == EntityType.MonsterGhost;
        }

        public bool IsLoot()
        {
            return EntityType == EntityType.Loot || EntityType == EntityType.LootGhost;
        }

        public bool IsNPC()
        {
            return EntityType == EntityType.StaticNPC;
        }

        public bool IsHero()
        {
            return EntityType == EntityType.Hero || EntityType == EntityType.HeroGhost;
        }

        public bool IsInteractiveTrigger()
        {
            return EntityType == EntityType.InteractiveTrigger;
        }

        public virtual void OnRemove()
        {
        }
    }

    public interface IRelevanceEntity
    {
        void OnRelevant();
        void OnIrrelevant();
    }
}
