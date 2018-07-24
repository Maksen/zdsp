namespace Zealot.Common
{
    using System.Collections.Generic;

    public class IDPool
    {
        //The number of ticks/frames to elapse before it is considered safe to reuse the id
        private const uint ID_REUSE_TICKS = 12000; //12000 ticks which roughly equivalent to 10mins, each tick is 50msec        

        struct FreedID
        {
            public int id;
            public uint frameFreed;
            public FreedID(int id, uint frameFreed)
            {
                this.id = id;
                this.frameFreed = frameFreed;
            }
        }


        private Queue<FreedID> pool;        
        private int mnLastID;       

        public IDPool()
        {
            mnLastID = 0;
            pool = new Queue<FreedID>(0);            
        }

        public int AllocID(uint currentTick,bool useTick = true)
        {
            if (pool.Count <= 0)
            {
                return mnLastID++;
            }


            
            FreedID fid = pool.Peek();
            if (useTick == true)
            {
                if (currentTick > fid.frameFreed + ID_REUSE_TICKS)
                {
                    pool.Dequeue();
                    return fid.id;
                }
                else
                {
                    return mnLastID++;
                }
            }
            else
            {
                pool.Dequeue();
                return fid.id;
            }
        }

        public void FreeID(int id, uint frameFreed)
        {            
            FreedID fid = new FreedID(id, frameFreed);
            pool.Enqueue(fid);
        }        
    }
}
