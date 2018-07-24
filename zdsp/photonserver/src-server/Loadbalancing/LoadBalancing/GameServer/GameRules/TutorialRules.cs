using System;
using System.Collections.Generic;
using Zealot.Server.Entities;

namespace Zealot.Server.Rules
{
    public static class TutorialRules
    {
        private static int currentStep = -1;
        private static Dictionary<int, EventHandler> TutorialEvents = new Dictionary<int, EventHandler>();

        public static void SubscribeTutorialEvent(int id, EventHandler handler)
        {
            if (TutorialEvents.ContainsKey(id))
                TutorialEvents[id] = handler;
            else
                TutorialEvents.Add(id, handler);
        }

        public static void TriggleTutorialEvent(int id, Player player)
        {
            if (TutorialEvents.ContainsKey(id))
                TutorialEvents[id](player, null);
        }

        public static bool UpdateProcessingStep(int newStep)
        {
            if(currentStep == newStep)
            {
                return false;
            }

            currentStep = newStep;
            return true;
        }

        public static void ResetProcessingStep()
        {
            currentStep = -1;
        }
    }
}
