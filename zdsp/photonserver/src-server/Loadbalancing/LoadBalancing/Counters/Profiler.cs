using System.Diagnostics;

namespace Zealot.Server.Counters
{
    public class Profiler
    {
        private Stopwatch mStopWatch;
        public Profiler()
        {
            mStopWatch = new Stopwatch();
        }

        public void Start()
        {
            mStopWatch.Restart();            
        }

        public double StopAndGetElapsed()
        {
            mStopWatch.Stop();
            return (double)mStopWatch.ElapsedTicks / (double)Stopwatch.Frequency;
        }
    }
}