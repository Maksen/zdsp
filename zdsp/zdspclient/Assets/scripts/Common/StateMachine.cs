namespace Zealot.Common
{    
    using System.Collections.Generic;    
    //using System.Threading; //Should not use call callback in a separate thread. This will result in race condition.
    //using ExitGames.Logging;

    public class StateMachine
    {
        public class State
        {
            public string Name { get; protected set; }

            public delegate void OnEnterStateDelegate(string prevstate);
            public delegate void OnLeaveStateDelegate();
            public delegate void OnUpdateDelegate(long dt);

            public OnEnterStateDelegate OnEnterStateDel { get; set; }
            public OnLeaveStateDelegate OnLeaveStateDel { get; set; }
            public OnUpdateDelegate OnUpdateDel { get; set; }
            
            public State(string name, OnEnterStateDelegate enterDelegate, OnLeaveStateDelegate leaveDelegate, OnUpdateDelegate updateDelegate)
            {
                Name = name;
                OnEnterStateDel = enterDelegate;
                OnLeaveStateDel = leaveDelegate;
                OnUpdateDel = updateDelegate;
            }
        }

        //protected static readonly ILogger log = LogManager.GetCurrentClassLogger();
        public delegate void TimerCallbackDelgate(object arg);
        protected Dictionary<string, State> states;
        protected State currentState;        
       

        public StateMachine()
        {
            states = new Dictionary<string, State>();
            currentState = null;         
        }
        
        public void AddState(string stateName, State.OnEnterStateDelegate enterDelegate, State.OnLeaveStateDelegate leaveDelegate)
        {
            State state = new State(stateName, enterDelegate, leaveDelegate, null);
            states.Add(stateName, state);
        }

        public void AddState(string stateName, State.OnEnterStateDelegate enterDelegate, State.OnLeaveStateDelegate leaveDelegate, State.OnUpdateDelegate updateDelegate)
        {
            State state = new State(stateName, enterDelegate, leaveDelegate, updateDelegate);
            states.Add(stateName, state);
        } 
        
        public string GetCurrentStateName()
        {
            return currentState.Name;
        }

        public virtual void OnPreLeaveState()
        {            
        }

        public virtual void GotoState(string stateName)
        {
            string prevStateName = "undefined";
            if (currentState != null)
            {               
                OnPreLeaveState();

                prevStateName = currentState.Name;
                if (currentState.OnLeaveStateDel!= null)
                    currentState.OnLeaveStateDel();
            }

            currentState = states[stateName];
            currentState.OnEnterStateDel(prevStateName);
        }      
        
        public virtual void OnUpdate(long dt)
        {
            if (currentState!= null && currentState.OnUpdateDel !=null)
            {
                currentState.OnUpdateDel(dt);
            }
        }
    }
}
