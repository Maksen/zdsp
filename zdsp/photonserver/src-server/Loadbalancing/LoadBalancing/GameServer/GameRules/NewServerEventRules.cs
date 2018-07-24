using System;

namespace Zealot.Server.Rules
{
    public static class NewServerEventRules
    {
        public static DateTime serverOpenDT;

        public static void Init()
        {
            serverOpenDT = DateTime.Today;
        }
    }
}