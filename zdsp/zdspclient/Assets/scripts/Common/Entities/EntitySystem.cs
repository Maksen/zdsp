using UnityEngine;

namespace Zealot.Common.Entities
{
    using System.Collections.Generic;
    
    public class EntitySystem
    {
        public static float MAX_ENTITY_RADIUS = 2.0f;
        public struct GridId
        {
            public int x, z;
            public GridId(int _x, int _z)
            {
                x = _x;
                z = _z;
            }
        }
        // world size = grid size * grid n
        // world size and origin shall be passed to initgrid
        protected int gridSizeX = 4, gridSizeZ = 4;
        protected float gridSizeHX = 2.0f, gridSizeHZ = 2.0f, gridOriX = 0.0f, gridOriZ = 0.0f;
        protected int gridNX = 64, gridNZ = 64;
        protected LinkedList<Entity>[,] mDEntityGridList, mDNetEntityGridList;        

        protected Dictionary<int, Entity> mEntities;
        protected Dictionary<int, Entity> mNetEntities;
        private List<int> mPendingPIDRemoveList;
        private List<int> mPendingIDRemoveList;
        protected IDPool mIDPool;
        protected IDPool mPIDPool;
        protected Timers mTimers;
        public Timers Timers
        { get{return mTimers;} }        
        
        public delegate bool QueryEntityFilter(Entity entity);

        //list for every update frame
#if UNITY_ANDROID || UNITY_IPHONE
        List<Entity> entities = new List<Entity>(64);
#else
        //server
        List<Entity> entities = new List<Entity>(512);
#endif

        public EntitySystem(Timers timers)
        {                        
            mIDPool = new IDPool();
            mIDPool.AllocID(0); //allocates the id 0 that happens to be invalid
            mPIDPool = new IDPool();
            mPIDPool.AllocID(0);            
            mEntities = new Dictionary<int, Entity>();
            mNetEntities = new Dictionary<int, Entity>();
            mTimers = timers;
            mPendingPIDRemoveList = new List<int>();
            mPendingIDRemoveList = new List<int>();
        }      

        public void InitGrid(float lsizex, float lsizez, float lorix = 0.0f, float loriz = 0.0f, int csizex = 4, int csizez = 4)
        {
            if (csizex < 1) csizex = 1; if (csizez < 1) csizez = 1;
            gridNX = (int)System.Math.Ceiling((double)lsizex/csizex); if (gridNX < 1) gridNX = 1;
            gridNZ = (int)System.Math.Ceiling((double)lsizez/csizez); if (gridNZ < 1) gridNZ = 1;
            gridSizeX = csizex; gridSizeHX = 0.5f * gridSizeX;
            gridSizeZ = csizez; gridSizeHZ = 0.5f * gridSizeZ;
            gridOriX = lorix; gridOriZ = loriz;

            mDEntityGridList = new LinkedList<Entity>[gridNZ, gridNX];
            mDNetEntityGridList = new LinkedList<Entity>[gridNZ, gridNX];
            for (int i = 0; i < gridNZ; i++)
            {
                for (int j = 0; j < gridNX; j++)
                {
                    mDEntityGridList[i, j] = new LinkedList<Entity>();
                    mDNetEntityGridList[i, j] = new LinkedList<Entity>();
                }
            }
        }

        public void UpdateGridId(Entity ent, float newx, float newz)
        {
            float oldx = ent.Position.x, oldz = ent.Position.z;
            int oldgx, oldgz;
            ComputeGridId(oldx, oldz, out oldgx, out oldgz);
            int newgx, newgz;
            ComputeGridId(newx, newz, out newgx, out newgz);

            if (oldgx != newgx || oldgz != newgz)
            {
                mDEntityGridList[oldgz, oldgx].Remove(ent);
                mDEntityGridList[newgz, newgx].AddLast(ent);                
                uint type = (uint)ent.EntityType;
                if ((type & (uint)EntityTypeAttribute.ETA_NET) > 0)                
                {
                    mDNetEntityGridList[oldgz, oldgx].Remove(ent);
                    mDNetEntityGridList[newgz, newgx].AddLast(ent);
                }
            }
        }

        protected void ComputeGridId(float posx, float posz, out int gidx, out int gidz)
        {
            gidx = (int)((posx - gridOriX) / gridSizeX);
            gidz = (int)((posz - gridOriZ) / gridSizeZ);

            //can be removed if later all level origin and size are set properly
            if (gidx < 0) gidx = 0;
            if (gidx >= gridNX) gidx = gridNX - 1;
            if (gidz < 0) gidz = 0;
            if (gidz >= gridNZ) gidz = gridNZ - 1;
        }        

        protected void OnAddEntity(int id, int pid, Entity entity)
        {
            int gx, gz;
            ComputeGridId(entity.Position.x, entity.Position.z, out gx, out gz);

            mEntities.Add(id, entity);
            mDEntityGridList[gz, gx].AddLast(entity);

            if (pid > 0)
            {
                mNetEntities.Add(pid, entity);
                mDNetEntityGridList[gz, gx].AddLast(entity);
            }
        }               

        public bool RemoveEntityByID(int id)
        {
            if (mEntities.ContainsKey(id))
            {
                uint currentTick = mTimers.GetTick();
                Entity entity = mEntities[id];
                int gx, gz;
                ComputeGridId(entity.Position.x, entity.Position.z, out gx, out gz);
                uint type = (uint)entity.EntityType;
                if ((type & (uint)EntityTypeAttribute.ETA_NET) > 0)                
                {
                    IBaseNetEntity netEnt = (IBaseNetEntity)entity;
                    int pid = netEnt.GetPersistentID();
                    mNetEntities.Remove(pid);
                    mPIDPool.FreeID(pid, currentTick);
                    mDNetEntityGridList[gz, gx].Remove(entity);                    
                }
                mEntities.Remove(id);
                mIDPool.FreeID(id, currentTick);
                mDEntityGridList[gz, gx].Remove(entity);
				entity.Destroyed = true;
                entity.OnRemove();
                return true;
            }
            return false;
        }

        public virtual bool RemoveEntityByPID(int pid, bool logflag = false)
        {
            if (mNetEntities.ContainsKey(pid))
            {
                uint currentTick = mTimers.GetTick();
                Entity entity = mNetEntities[pid];
                int gx, gz;
                ComputeGridId(entity.Position.x, entity.Position.z, out gx, out gz);
                int id = entity.ID;
                mNetEntities.Remove(pid);
                mPIDPool.FreeID(pid, currentTick);
                mDNetEntityGridList[gz, gx].Remove(entity);
                mEntities.Remove(id);
                mIDPool.FreeID(id, currentTick);
                mDEntityGridList[gz, gx].Remove(entity);
				entity.Destroyed = true;
                entity.OnRemove();
              
                return true;
            }
            return false;
        }
        
        public Entity GetEntityByID(int id)
        {
            if (mEntities.ContainsKey(id))
            {
                return mEntities[id];                
            }
            return null;
        }

        public Entity GetEntityByPID(int pid)
        {
            if (mNetEntities.ContainsKey(pid))
            {
                return mNetEntities[pid];
            }
            return null;
        }

        public List<IBaseNetEntity> GetNetEntitiesByOwner(int ownerID) //0 = server
        {
            List<IBaseNetEntity> list = new List<IBaseNetEntity>();
            foreach (KeyValuePair<int, Entity> entry in mNetEntities)
            {
                IBaseNetEntity ne = (IBaseNetEntity)(entry.Value);
                if (ne.GetOwnerID() == ownerID)
                {
                    list.Add(ne);
                }
            }
            return list;
        }

        //Entity that resides only in either client or server
        public T SpawnEntity<T>() where T : Entity, new()
        {
            int id = mIDPool.AllocID(mTimers.GetTick());
            T entity = new T();
            entity.EntitySystem = this;
            entity.SetID(id);

            OnAddEntity(id, 0, entity);
            return entity;
        }       
        
        public virtual void Update(long dt)
        {            
            entities.AddRange(mEntities.Values);

            //foreach (KeyValuePair<int, Entity> entry in mEntities) //we may modify the dictionary while processing each entity
            foreach (Entity entity in entities)
            {            
                if (!entity.Destroyed)
                    entity.Update(dt);
            }
            //Any new entity added during the update will only be updated in the next frame
            //Any entity removed during the update will be dereferenced on function exit

            entities.Clear();
        }

        public Dictionary<int, Entity> GetAllEntities()
        {
            return mEntities;
        }

        public Dictionary<int, Entity> GetAllNetEntities()
        {
            return mNetEntities;
        }        

#region Entities Queries        

        public void QueryEntitiesInCircle(Vector3 center, float radius, QueryEntityFilter filterFor, List<Entity> retList)
        {
            int gidxmin, gidxmax, gidzmin, gidzmax;
            ComputeGridId(center.x - radius - MAX_ENTITY_RADIUS, center.z - radius - MAX_ENTITY_RADIUS, out gidxmin, out gidzmin);
            ComputeGridId(center.x + radius + MAX_ENTITY_RADIUS, center.z + radius + MAX_ENTITY_RADIUS, out gidxmax, out gidzmax);

            //float radiusSq = radius * radius;
            for (int gz = gidzmin; gz <= gidzmax; gz++)
            {
                for (int gx = gidxmin; gx <= gidxmax; gx++)
                {
                    LinkedList<Entity> dEntities = mDEntityGridList[gz, gx];
                    foreach (Entity ent in dEntities)
                    {
                        if (filterFor == null || filterFor(ent))
                        {
                            float sqrDist = Vector3.SqrMagnitude(center - ent.Position);
                            float combinedRadius = radius + ent.Radius;
                            float combinedRadiusSq = combinedRadius * combinedRadius;
                            if (sqrDist <= combinedRadiusSq)
                                retList.Add(ent);
                        }
                    }
                }
            }
        }

        //this function will be frequently called in server by NetServerSlot, make it polish
        public void QueryNetEntitiesInCircle(Vector3 center, float radius, QueryEntityFilter filterFor, List<Entity> retList)
        {
            int gidxmin, gidxmax, gidzmin, gidzmax;
            ComputeGridId(center.x - radius - MAX_ENTITY_RADIUS, center.z - radius - MAX_ENTITY_RADIUS, out gidxmin, out gidzmin);
            ComputeGridId(center.x + radius + MAX_ENTITY_RADIUS, center.z + radius + MAX_ENTITY_RADIUS, out gidxmax, out gidzmax);

            float defaultRadius = CombatUtils.DEFAULT_ACTOR_RADIUS;
            float defaultCombinedRadiusSq = (radius + defaultRadius) * (radius + defaultRadius);
            float combinedRadius = radius;
            float combinedRadiusSq = defaultCombinedRadiusSq;
            float sqrDist;
            if (filterFor == null) //check here better than check within loop
            {
                for (int gz = gidzmin; gz <= gidzmax; gz++)
                {
                    for (int gx = gidxmin; gx <= gidxmax; gx++)
                    {
                        LinkedList<Entity> dEntities = mDNetEntityGridList[gz, gx];
                        foreach (Entity ent in dEntities)
                        {
                            sqrDist = Vector3.SqrMagnitude(center - ent.Position);
                            if (ent.Radius == defaultRadius)
                            {
                                if (sqrDist <= defaultCombinedRadiusSq)
                                    retList.Add(ent);
                            }
                            else
                            {
                                combinedRadius = radius + ent.Radius;
                                combinedRadiusSq = combinedRadius * combinedRadius;
                                if (sqrDist <= combinedRadiusSq)
                                    retList.Add(ent);
                            }
                        }
                    }
                }
            }
            else
            {
                for (int gz = gidzmin; gz <= gidzmax; gz++)
                {
                    for (int gx = gidxmin; gx <= gidxmax; gx++)
                    {
                        LinkedList<Entity> dEntities = mDNetEntityGridList[gz, gx];
                        foreach (Entity ent in dEntities)
                        {
                            if (filterFor(ent))
                            {
                                sqrDist = Vector3.SqrMagnitude(center - ent.Position);
                                if (ent.Radius == defaultRadius)
                                {
                                    if (sqrDist <= defaultCombinedRadiusSq)
                                        retList.Add(ent);
                                }
                                else
                                {
                                    combinedRadius = radius + ent.Radius;
                                    combinedRadiusSq = combinedRadius * combinedRadius;
                                    if (sqrDist <= combinedRadiusSq)
                                        retList.Add(ent);
                                }
                            }
                        }
                    }
                }
            }
        }

        public Entity QueryForClosestEntityInCircle(Vector3 center, float radius, QueryEntityFilter filterFor)
        {
            Entity closestEntity = null;
            int gidxmin, gidxmax, gidzmin, gidzmax;
            ComputeGridId(center.x - radius, center.z - radius, out gidxmin, out gidzmin);
            ComputeGridId(center.x + radius, center.z + radius, out gidxmax, out gidzmax);

            float sqrDistRes = 1000000.0f;
            float radiusSq = radius * radius;

            for(int gz=gidzmin;gz<=gidzmax;gz++)
                for(int gx=gidxmin;gx<=gidxmax;gx++)
                {
                    LinkedList<Entity> dEntities = mDEntityGridList[gz, gx];
                    foreach (Entity ent in dEntities)
                    {                        
                        if (!ent.Destroyed && filterFor(ent))
                        {
                            float sqrDist = Vector3.SqrMagnitude(center - ent.Position);
                            if (sqrDist <= radiusSq)
                            {
                                if (sqrDist < sqrDistRes)
                                {
                                    sqrDistRes = sqrDist;
                                    closestEntity = ent;
                                }
                            }
                        }
                    }
                }
            return closestEntity;
        }      

        //coumpte the four corners of a front half rectangle given center, extents and direction
        //note that may get either cw or ccw rectangle, but it does not matter when computing grid bound
        public static void ComputeRectFABCD(Vector2 O, Vector2 dir1, Vector2 dir2,
                                                out Vector2 A, out Vector2 B, out Vector2 C, out Vector2 D)
        {                         
            A = O + dir2;
            B = A + dir1;
            D = O - dir2;
            C = D + dir1;
        }

        public List<Entity> QueryEntitiesInRectangle(Vector3 center, Vector3 size, QueryEntityFilter filterFor)
        {
            List<Entity> retList = new List<Entity>();
            int gidxmin, gidxmax, gidzmin, gidzmax;
            Vector2 pA = new Vector2(center.x - size.x / 2, center.z - size.z / 2);
            Vector2 pB = new Vector2(center.x + size.x / 2, center.z - size.z / 2);
            Vector2 pC = new Vector2(center.x + size.x / 2, center.z + size.z / 2);
            Vector2 pD = new Vector2(center.x - size.x / 2, center.z + size.z / 2);
            ComputeGridMinMaxInRect(pA, pB, pC, pD, out gidxmin, out gidzmin, out gidxmax, out gidzmax);

            for (int gz = gidzmin; gz <= gidzmax; gz++)
                for (int gx = gidxmin; gx <= gidxmax; gx++)
                {
                    LinkedList<Entity> dEntities = mDEntityGridList[gz, gx];
                    foreach (Entity ent in dEntities)
                    {
                        retList.Add(ent);
                    }
                }

            return retList;
        }

        private void ComputeGridMinMaxInRect(Vector2 pA, Vector2 pB, Vector2 pC, Vector2 pD,
                                                out int gidXMin, out int gidZMin, out int gidXMax, out int gidZMax)
        {
            int tx, tz;
            ComputeGridId(pA.x, pA.y, out tx, out tz);
            gidXMin = gidXMax = tx;
            gidZMin = gidZMax = tz;
            ComputeGridId(pB.x, pB.y, out tx, out tz);
            if (gidXMax < tx) gidXMax = tx; if (gidZMax < tz) gidZMax = tz;
            if (gidXMin > tx) gidXMin = tx; if (gidZMin > tz) gidZMin = tz;
            ComputeGridId(pC.x, pC.y, out tx, out tz);
            if (gidXMax < tx) gidXMax = tx; if (gidZMax < tz) gidZMax = tz;
            if (gidXMin > tx) gidXMin = tx; if (gidZMin > tz) gidZMin = tz;
            ComputeGridId(pD.x, pD.y, out tx, out tz);
            if (gidXMax < tx) gidXMax = tx; if (gidZMax < tz) gidZMax = tz;
            if (gidXMin > tx) gidXMin = tx; if (gidZMin > tz) gidZMin = tz;
        }

        //query entities within front half rectangular region in normalizedDir
        public List<Entity> QueryEntitiesInRectangleF(Vector3 center, Vector3 normalizedDir, float range, float width, QueryEntityFilter filterFor)
        {
            List<Entity> retList = new List<Entity>();
            float hw = 0.5f * width;
            Vector2 pO = new Vector2(center.x, center.z);
            Vector2 v1 = new Vector2(range * normalizedDir.x, range * normalizedDir.z);
            Vector2 v1offset = new Vector2(v1.x + MAX_ENTITY_RADIUS * normalizedDir.x, v1.y + MAX_ENTITY_RADIUS * normalizedDir.z);
            Vector2 v2 = new Vector2(-hw * normalizedDir.z, hw * normalizedDir.x);
            Vector2 v2offset = new Vector2(v2.x - MAX_ENTITY_RADIUS * normalizedDir.z, v2.y + MAX_ENTITY_RADIUS * normalizedDir.x);
            Vector2 pA, pB, pC, pD;
            ComputeRectFABCD(pO, v1offset, v2offset, out pA, out pB, out pC, out pD);            
 
            int gidxmin, gidxmax, gidzmin, gidzmax;
            ComputeGridMinMaxInRect(pA, pB, pC, pD, out gidxmin, out gidzmin, out gidxmax, out gidzmax);

            float l1 = v1.magnitude, l2 = v2.magnitude;
            for (int gz = gidzmin; gz <= gidzmax; gz++)
                for (int gx = gidxmin; gx <= gidxmax; gx++)
                {
                    LinkedList<Entity> dEntities = mDEntityGridList[gz, gx];
                    foreach (Entity ent in dEntities)
                    {                        
                        if (!ent.Destroyed && filterFor(ent))
                        {   //Peter, TODO: factor in radius of entity i.e. check circle overlap rectangle instead of point in rectangle only
                            //will see a difference with bigger entities e.g. boss
                            Vector2 v = new Vector2(ent.Position.x - center.x, ent.Position.z - center.z);
                            float t1 = Vector2.Dot(v, new Vector2(v1.x + ent.Radius * normalizedDir.x, v1.y + ent.Radius * normalizedDir.z));
                            float t2 = Vector2.Dot(v, new Vector2(v2.x - ent.Radius * normalizedDir.z, v2.y + ent.Radius * normalizedDir.x));
                            if (t2 < 0) t2 = -t2;
                            if (0 <= t1 && t1 <= (l1 + ent.Radius) * (l1 + ent.Radius) && t2 <= (l2 + ent.Radius) * (l2 + ent.Radius))
                                retList.Add(ent);
                        }
                    }
                }
            return retList;
        }

        public Entity QueryForClosestEntityInRectangleF(Vector3 center,  Vector3 normalizedDir, float range, float width, QueryEntityFilter filterFor)
        {
            Entity closestEntity = null;
            float resDist2 = 1000000.0f;
            float hw = 0.5f * width;
            Vector2 pO = new Vector2(center.x, center.z);
            Vector2 v1 = new Vector2(range * normalizedDir.x, range * normalizedDir.z);
            Vector2 v2 = new Vector2(-hw * normalizedDir.z, hw * normalizedDir.x);
            Vector2 pA, pB, pC, pD;
            ComputeRectFABCD(pO, v1, v2, out pA, out pB, out pC, out pD);

            int gidxmin, gidxmax, gidzmin, gidzmax;
            ComputeGridMinMaxInRect(pA, pB, pC, pD, out gidxmin, out gidzmin, out gidxmax, out gidzmax);

            float l1 = v1.sqrMagnitude, l2 = v2.sqrMagnitude;
            for (int gz = gidzmin; gz <= gidzmax; gz++)
                for (int gx = gidxmin; gx <= gidxmax; gx++)
                {
                    LinkedList<Entity> dEntities = mDEntityGridList[gz, gx];
                    foreach (Entity ent in dEntities)
                    {                        
                        if (!ent.Destroyed && filterFor(ent))
                        {
                            Vector3 vDist = ent.Position - center;
                            Vector2 v = new Vector2(vDist.x, vDist.z);                                                        
                            float t1 = Vector2.Dot(v, v1);
                            float t2 = Vector2.Dot(v, v2);
                            if (t2 < 0) t2 = -t2;
                            if (0 <= t1 && t1 <= l1 && t2 <= l2) {
                                float d = vDist.sqrMagnitude;
                                if (d < resDist2)
                                {
                                    closestEntity = ent;
                                    resDist2 = d;
                                }
                            }                                
                        }
                    }
                }

            return closestEntity;
        }

        //Return IActor so that server and client can use them. Cast accordingly after that.
        public List<Entity> QueryEntitiesInSphere(Vector3 center, float radius, QueryEntityFilter filterFor)
        {
            List<Entity> retList = new List<Entity>();
            QueryEntitiesInCircle(center, radius, filterFor, retList);
            //sort maybe costy
            //retList.Sort(delegate (Entity e1, Entity e2)
            //{
            //    bool nearer = (e1.Position - center).sqrMagnitude < (e2.Position - center).sqrMagnitude;
            //    return nearer ? -1:1;//retrun -1 means in the front of list
            //});
            return retList;
        }

        public void QueryEntitiesInSphere(Vector3 center, float radius, QueryEntityFilter filterFor, List<Entity> retList)
        {
            QueryEntitiesInCircle(center, radius, filterFor, retList);
        }

        public Entity QueryForClosestEntityInSphere(Vector3 center, float radius, QueryEntityFilter filterFor)
        {
            return QueryForClosestEntityInCircle(center, radius, filterFor);
        }
#endregion

    }
}
