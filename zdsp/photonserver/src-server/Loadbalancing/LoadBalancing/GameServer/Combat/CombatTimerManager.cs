using Zealot.Server.Entities;
using System.Timers;
using System;

namespace Photon.LoadBalancing.GameServer.Combat
{
    // so once combat starts, start counting from MAXCOMBATTIME
    // once times up, decrease 1% by every 1 min
    // addition time is to go first and pause the main timer
    public class CombatTimerManager
    {
        // Statues Flags expandable
        [Flags]
        private enum eFlag : byte
        {
            NOTACTIVE       = 0, // Nothing happened, do not set to this as it zeros all bits
            GRACETIME       = 1 << 0, // 1st bit -> indicates grace time is over
            BASICACTIVE     = 1 << 1, // 1st bit -> is combat timer activated
            ADDITIONACTIVE  = 1 << 2, // 2nd bit -> is there addition time to consider, added by time of the day
            DECAYING        = 1 << 3, // 3rd bit -> this should always be set only by itself

            PAUSE           = 1 << 7 // 8th bit -> is the timer paused
        }

        private eFlag m_Statues;

        // player reference
        private Player m_Player;

        // time to count is 300 mins
        // might need to be dynamic loaded
        private const uint MAXCOMBATTIME = 300;
        private const uint TIMECEILING = 900;
        private const uint DECAYINTERVAL = 1;
        private uint m_CombatTime;
        private uint m_DecayTotalTime;

        private Chrono m_MainTimer;
        private Chrono m_AdditionTimer;
        private Chrono m_GraceTimer;
        private System.Collections.Generic.Queue<double> m_AdditionalTimers;

        private System.Collections.Generic.Dictionary<string, double> m_AdditionalTimer;

        public CombatTimerManager(Player player)
        {
            m_Player = player;
            // hack
            InitCombatTimer(player);
        }

        // potentially use this init to read gamedata
        public void InitCombatTimer(Player player)
        {
            m_Player = player;
            m_Statues = eFlag.NOTACTIVE;
            m_AdditionalTimers = new System.Collections.Generic.Queue<double>(2);
            m_AdditionalTimer = new System.Collections.Generic.Dictionary<string, double>();

            ////////
            // timer types
            m_AdditionalTimer.Add("Onsen", 0);

            ///////

            m_DecayTotalTime = 0;
            m_CombatTime = MAXCOMBATTIME;
            m_GraceTimer = new Chrono(TimeSpan.FromSeconds(10).TotalMilliseconds);
            m_GraceTimer.AutoReset = false;
            m_GraceTimer.Elapsed += GraceTimeEnded;
        }

        public void ResetCombatStatues()
        {
            //check for left over time
            if((m_Statues & eFlag.DECAYING) != eFlag.DECAYING)
            {
                // there's still combat time left
                TimeSpan span = m_MainTimer.TimeSpanElapsed();
                m_CombatTime = (uint)span.Minutes;
            }
            m_Statues = eFlag.NOTACTIVE;
            m_AdditionalTimers.Clear();
            m_DecayTotalTime = 0;
        }

        // this is the normal phase
        // We assume that the 10 second is done
        public void StartCombat()
        {
            //is already active so skip it
            if ((m_Statues & eFlag.NOTACTIVE) != eFlag.NOTACTIVE) return;

            // 10 secs grace period is over
            if ((m_Statues & eFlag.GRACETIME) == eFlag.GRACETIME)
            {
                // Check for addition timer to exc before main
                if ((m_Statues & eFlag.ADDITIONACTIVE) == eFlag.ADDITIONACTIVE)
                {
                    // Addition timer
                    m_AdditionTimer = new Chrono(TimeSpan.FromMinutes(m_AdditionalTimers.Dequeue()).Milliseconds);
                    m_AdditionTimer.AutoReset = false;
                    m_AdditionTimer.Elapsed += AdditionTimeEnded;
                }
                else
                {
                    // Main Timer
                    m_MainTimer = new Chrono(TimeSpan.FromMinutes(m_CombatTime).Milliseconds);
                    m_MainTimer.AutoReset = false;
                    m_MainTimer.Elapsed += MainTimeEnded;
                }
                m_Statues &= eFlag.BASICACTIVE;
            }
            else
            {
                // start grace time
                if(m_GraceTimer.Enabled == false)
                {
                    // start timer
                    m_GraceTimer.Start();
                }
            }
        }


        public void StartDecay()
        {
            if(m_MainTimer != null)
            {
                m_MainTimer.Close();
                m_MainTimer = null;
            }

            m_MainTimer = new Chrono(TimeSpan.FromMinutes(DECAYINTERVAL).TotalMilliseconds);
            m_MainTimer.AutoReset = true;
            m_MainTimer.Elapsed += Decay;
        }

        // need to change to support adding to category
        public void AddAdditionalTime(double time)
        {
            TimeSpan total = m_MainTimer.TimeSpanElapsed() + m_AdditionTimer.TimeSpanElapsed();
            foreach(var i in m_AdditionalTimers)
            {
                total = total.Add(TimeSpan.FromMinutes(i));
            }

            if(total.TotalMinutes >= TIMECEILING) return;

            TimeSpan t =TimeSpan.FromMinutes(900.0 - total.TotalMinutes);
            if (t.TotalMinutes < time)
            {
                // time overflows
                time = t.TotalMinutes;
            }

            m_AdditionalTimers.Enqueue(time);
            m_Statues |= eFlag.ADDITIONACTIVE;
        }

        // Additional timer addition
        public void RecoverAdditionalTime(string tag, double time)
        {
           
        }

        private void GraceTimeEnded(object sender, ElapsedEventArgs e)
        {
            m_GraceTimer.Enabled = false;
            m_GraceTimer.Stop();
            m_Statues &= eFlag.GRACETIME;
        }

        private void MainTimeEnded(object sender, ElapsedEventArgs e)
        {
            m_MainTimer.Enabled = false;
            m_MainTimer.Close();
            m_Statues &= (~eFlag.BASICACTIVE | eFlag.DECAYING);
            StartDecay();
        }

        private void AdditionTimeEnded(object sender, ElapsedEventArgs e)
        {
            m_AdditionTimer.Enabled = false;
            m_AdditionTimer.Close();
            if (m_AdditionalTimers.Count == 0)
                m_Statues &= ~eFlag.ADDITIONACTIVE;

            StartCombat();
        }

        private void Decay(object sender, ElapsedEventArgs e)
        {
            m_DecayTotalTime += DECAYINTERVAL;
            // show system message here
            // check time also!!!
            // message not done yet
            // might prob want to just get from db instead
            if(m_DecayTotalTime < 99)
                m_Player.Slot.ZRPC.CombatRPC.PushExpDecayNotification("", m_Player.Slot);
            else
                m_Player.Slot.ZRPC.CombatRPC.PushExpDecayNotification("", m_Player.Slot);

            // apply decay to exp
            m_Player.ApplyDecay();

            if(m_DecayTotalTime == 99) // reached 99% reduction
            {
                // no more liao
                m_MainTimer.Close();
                m_MainTimer = null;
            }
        }


        /*
         *  Might be possible it won't work! 
         */
        public void PauseTimers()
        {
            if((m_Statues & eFlag.ADDITIONACTIVE) == eFlag.ADDITIONACTIVE)
            {
                m_AdditionTimer.Stop();
            }
            else
            {
                m_MainTimer.Stop();
            }

            m_Statues |= eFlag.NOTACTIVE;
        }

        public void ResumeTimers()
        {
            if ((m_Statues & eFlag.ADDITIONACTIVE) == eFlag.ADDITIONACTIVE)
            {
                m_AdditionTimer.Start();
            }
            else
            {
                m_MainTimer.Start();
            }

            m_Statues &= ~eFlag.NOTACTIVE;
        }
    }
}
