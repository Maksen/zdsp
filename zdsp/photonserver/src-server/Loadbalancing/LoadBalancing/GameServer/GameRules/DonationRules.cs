using ExitGames.Concurrency.Fibers;
using Photon.LoadBalancing.GameServer;
using System;

namespace Zealot.Server.Rules
{
    public static class DonationRules
    {
        private static int serverId;
        private static readonly PoolFiber executionFiber;

        static DonationRules()
        {
            executionFiber = GameApplication.Instance.executionFiber;
        }

        public static void Init()
        {
            serverId = GameApplication.Instance.GetMyServerline();
            StartNormalTimer();
            StartRareTimer();
        }

        private static void StartNormalTimer()
        {
            long duration = GetNextNormalDuration();
            executionFiber.Schedule(OnNormalTimeUp, duration);
        }

        private static void OnNormalTimeUp()
        {
            GameApplication.BroadcastDonateRefresh(true);
        }

        private static long GetNextNormalDuration()
        {
            DateTime now = DateTime.Now;
            DateTime nextDT;
            int hour = DateTime.Now.Hour;
            if (hour >= 0 && hour < 6)
            {
                nextDT = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 6, 0, 0);
            }
            else if (hour >= 12 && hour < 18)
            {
                nextDT = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 18, 0, 0);
            }
            else
            {
                nextDT = now;
            }

            return (long)(nextDT - now).TotalMilliseconds;
        }

        public static void StartRareTimer()
        {
            long duration = GetNextRareDuration();
            executionFiber.Schedule(OnRareTimeUp, duration);
        }

        private static long GetNextRareDuration()
        {
            DateTime now = DateTime.Now;
            DateTime nextDT;
            int hour = DateTime.Now.Hour;
            if (hour >= 0 && hour < 12)
            {
                nextDT = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 12, 0, 0);
            }
            else if (hour >= 12 && hour < 24)
            {
                nextDT = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day + 1, 0, 0, 0);
            }
            else
            {
                nextDT = now;
            }

            return (long)(nextDT - now).TotalMilliseconds;
        }

        private static void OnRareTimeUp()
        {
            GameApplication.BroadcastDonateRefresh(false);
        }
    }
}
