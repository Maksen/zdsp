namespace Zealot.Server.Actions
{
    using Zealot.Common;
    using Zealot.Common.Actions;
    using Zealot.Common.Entities;
    using Zealot.Server.Entities;

    using ExitGames.Logging;
    
    public class ServerAuthoASIdle : AIdle //Used for entities owned by server e.g. monster entity
    {
        protected static readonly ILogger log = LogManager.GetCurrentClassLogger();

        public ServerAuthoASIdle(Entity entity, ActionCommand cmd) : base(entity, cmd)
        {
        }

        protected override void OnActiveEnter(string prevstate)
        {
            base.OnActiveEnter(prevstate);
            //Just do nothing at server
            NetEntity ne = (NetEntity) mEntity;
            log.DebugFormat("NetEntity {0} activeenter ServerAuthoASIdle", ne.GetPersistentID());

            IdleActionCommand cmd = new IdleActionCommand();
            ne.SetAction(cmd);
        }

        /*                
        public ASIdle(Entity entity):base(entity)
        {
            AddState("RandomIdle", OnRandomIdleEnter, OnRandomIdleLeave);
        }

        ~ASIdle()
        {
            log.Debug("ASIdle destroyed");
        }

        protected override void OnActiveEnter(string prevstate)
        {
            base.OnActiveEnter(prevstate);
            log.Debug("Server idle action OnActiveEnter" + System.DateTime.Now);
            //GotoState("RandomIdle");

            //SetTimer(100, new TimerCallbackDelgate((arg) =>
            //{
            //    if (arg != null)
            //    {
            //        log.Debug(System.DateTime.Now + (string)arg);                    
            //    }
            //    GotoState("RandomIdle");
            //}), "active state argument");
            SetTimer(100, new TimerDelegate((arg) =>
            {
                if (arg != null)
                {
                    log.Debug(System.DateTime.Now + (string)arg);
                }
                GotoState("RandomIdle");
            }), "active state argument");
        }

        protected override void OnActiveLeave()
        {
            base.OnActiveLeave();
            log.Debug("Server idle action OnActiveLeave");
        }

        protected override void OnCompleteEnter(string prevstate)
        {
            base.OnCompleteEnter(prevstate);
            log.Debug("Server idle action OnCompleteEnter");
        }

        #region RandomIdle state        
        protected virtual void OnRandomIdleEnter(string prevstate)
        {
            log.Debug("Server idle action OnRandomIdleEnter" + System.DateTime.Now);                        
            //SetTimer(100, new TimerCallbackDelgate((arg) => {
            //    if (arg != null)
            //    {
            //        log.Debug(System.DateTime.Now + (string)arg);
            //    }                
            //    GotoState("Active");
            //}) , "randomidle state argument");

            SetTimer(100, new TimerDelegate((arg) =>
            {
                if (arg != null)
                {
                    log.Debug(System.DateTime.Now + (string)arg);
                }
                GotoState("Active");
            }), "RandomIdle argument");
        }

        protected virtual void OnRandomIdleLeave()
        {
            log.Debug("Server idle action OnRandomIdleLeave");
        }
        #endregion 
         */
    }

    public class NonServerAuthoASIdle : AIdle //Used by entities at server not owned by server e.g. player entity
    {
        protected static readonly ILogger log = LogManager.GetCurrentClassLogger();

        public NonServerAuthoASIdle(Entity entity, ActionCommand cmd) : base(entity, cmd)
        {
        }        
    }


    public class NonServerAuthoASWalk : Action //place holder action for server to stop basic attack chain
    {
        protected static readonly ILogger log = LogManager.GetCurrentClassLogger();

        public NonServerAuthoASWalk(Entity entity, ActionCommand cmd) : base(entity, cmd)
        {
        }
    }

    public class NonServerAuthoFlash:Action
    {
        protected long dur;
        public NonServerAuthoFlash(Entity entity, ActionCommand cmd) : base(entity, cmd)
        {
            FlashActionCommand flashcmd = cmd as FlashActionCommand;
            dur = (long)(flashcmd.dur * 1000);
            
        }

        protected override void OnActiveEnter(string prevstate)
        {
            base.OnActiveEnter(prevstate);
            if (mEntity.IsPlayer())
            {
                Player player = mEntity as Player;
                player.InvincibleMode = true; 
            }
            SetTimer(dur, (args) =>
            {
                GotoState("Completed");
            }, null);//this timer is just for ensure , normally a client Idle will be issued after Flash.
        }

        protected override void OnActiveLeave()
        {
            base.OnActiveLeave();
            if (mEntity.IsPlayer())
            {
                Player player = mEntity as Player;
                player.InvincibleMode = false;
            }
        } 
        
    }
}
