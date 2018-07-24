namespace Zealot.Common
{
    using System.Diagnostics;
    using System.Collections.Generic;
    using System;    

    public delegate void TimerDelegate(object arg);

    public class GameTimer
    {
        public double Duration { get; set; }
        public double ElapsedTime { get; set; }
        public TimerDelegate TimerDelegate { get; set; }
        public object Argument { get; set; }

        public GameTimer(long duration, TimerDelegate del, object arg)
        {
            Duration = (double)duration / 1000;
            ElapsedTime = 0;
            TimerDelegate = del;
            Argument = arg;
        }
    }

    public delegate void TimerLogHandler(string errmessage);

    public class Timers
    {        
        private Stopwatch stopwatch;
        private Dictionary<GameTimer, GameTimer> gameTimers;
        private long deltatime;
        private uint tick;
        private double currtime;
        private long synchronizedTimeStamp;
        public double mDiffTotalMiliSecondsWithServerDT;
        private List<GameTimer> mDeleteList; //for GC friendliness
        private TimerLogHandler mLogHandler;

        //private static readonly ILogger log = LogManager.GetCurrentClassLogger();

        public Timers()
        {
            stopwatch = new Stopwatch();
            stopwatch.Start();
            gameTimers = new Dictionary<GameTimer, GameTimer>();
            deltatime = 0;
            synchronizedTimeStamp = 0;
            mDiffTotalMiliSecondsWithServerDT = 0;
            mDeleteList = new List<GameTimer>();
            mLogHandler = null;
        }

        public void SetLogHandler(TimerLogHandler del)
        {
            this.mLogHandler = del;
        }

        public long GetSynchronizedTime()
        {
            return synchronizedTimeStamp + (long)(currtime * 1000);
        }

        public void SetSynchronizedTimeStamp(long time) //used only by clients to synch time with server
        {
            synchronizedTimeStamp = time;
            currtime = 0;
        }

        public DateTime GetSynchronizedServerDT()
        {
            if (mDiffTotalMiliSecondsWithServerDT != 0)
                return DateTime.Now.AddMilliseconds(-mDiffTotalMiliSecondsWithServerDT);
            return DateTime.Now;
        }

        public long GetDeltaTime()
        {
            return deltatime;
        }

        public void Update()
        {
            tick++;
            double dt = (double)stopwatch.ElapsedTicks / Stopwatch.Frequency;
            stopwatch.Reset();
            stopwatch.Start();
            deltatime = (long)(dt * 1000);
            currtime += dt;
           
            if (dt > 0)
            {                
                foreach(KeyValuePair<GameTimer, GameTimer> entry in gameTimers)
                {
                    GameTimer timer = entry.Value;
                    timer.ElapsedTime += dt;
                    if (timer.ElapsedTime >= timer.Duration)
                    {                        
                        mDeleteList.Add(timer);
                    }
                }
                
                for (int i=0; i<mDeleteList.Count; ++i)
                {
                    GameTimer timer = mDeleteList[i];
                    gameTimers.Remove(timer);
                    try
                    {
                        timer.TimerDelegate(timer.Argument);
                    }
                    catch (Exception ex)
                    {
                        if (mLogHandler != null)
                        {
                            mLogHandler(string.Format("TimerDelegate InnerException: {0}", ex.StackTrace));
                        }                        
                    }
                }
                mDeleteList.Clear();
            }
        }

        public GameTimer SetTimer(long duration, TimerDelegate del, object arg)
        {
            GameTimer t = new GameTimer(duration, del, arg);
            gameTimers.Add(t, t);
            return t;
        }

        public void StopTimer(GameTimer timer)
        {
            if (gameTimers.ContainsKey(timer))
            {
                gameTimers.Remove(timer);
            }
        }

        public void StopTimerAndTrigger(GameTimer timer)
        {
            if (gameTimers.ContainsKey(timer))
            {
                timer.TimerDelegate(timer.Argument);
                gameTimers.Remove(timer);
            }
        }

        public uint GetTick() //How many times the timer has ticked since it started (incremented every update)
        {
            return tick;
        }

        public void PrepareTickForNetworkEvents() //So that actions performed outside mainloop can be broadcasted in the next tick
        {
            tick++;
        }

        public void PrepareTickForMainLoop() //Restore tick usable by mainloop
        {
            tick--;
        }
    }
}
