using System;
using System.Timers;
using System.Diagnostics;

namespace Photon.LoadBalancing.GameServer.Combat
{
    public class Chrono : Timer
    {
        private Stopwatch m_Watch;
        private double time;

        public Chrono(double interval) : base(interval)
        {
            m_Watch = new Stopwatch();
            time = interval;
        }

        public new void Start()
        {
            this.Start();
            m_Watch.Start();
        }

        public new void Stop()
        {
            this.Stop();
            m_Watch.Stop();
        }

        public TimeSpan TimeSpanElapsed()
        {
            return m_Watch.Elapsed;
        }

        public long TimeInMilisecondsElapsed()
        {
            return m_Watch.ElapsedMilliseconds;
        }

        public TimeSpan CurrentTimeLeft()
        {
            return TimeSpan.FromMilliseconds(time) - m_Watch.Elapsed;
        }

        public new void Close()
        {
            this.Close();
            m_Watch.Reset();
        }
    }
}
