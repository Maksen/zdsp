using System;
using System.Collections.Generic;
using Kopio.JsonContracts;
using Zealot.Common;

namespace Zealot.Repository
{
    class TutorialRepo
    {
        private static Dictionary<int, TutorialGuideJson> m_TutorialGuide;
        private static Dictionary<SystemName, List<TutorialDescriptionJson>> m_TutorialSystem;

        static TutorialRepo()
        {
            m_TutorialGuide = new Dictionary<int, TutorialGuideJson>();
            m_TutorialSystem = new Dictionary<SystemName, List<TutorialDescriptionJson>>();
        }

        public static void Init(GameDBRepo gameData)
        {
            foreach(TutorialGuideJson iter in gameData.TutorialGuide.Values)
            {
                m_TutorialGuide.Add(iter.lv, iter);
            }

            foreach(TutorialDescriptionJson iter in gameData.TutorialDescription.Values)
            {
                if (!m_TutorialSystem.ContainsKey(iter.system))
                    m_TutorialSystem.Add(iter.system, new List<TutorialDescriptionJson>());
                m_TutorialSystem[iter.system].Add(iter);
            }
        }

        public static TutorialDescriptionJson GetTutorialInfoWithLevel(int level, int step)
        {
            SystemName system;
            if (m_TutorialGuide.ContainsKey(level))
                system = m_TutorialGuide[level].system;

            else
                return null;

            if (m_TutorialSystem[system].Count >= step)
                return m_TutorialSystem[system][step];

            return null;
        }

        public static TutorialDescriptionJson GetTutorialWithSystem(SystemName system, int step)
        {
            if (!m_TutorialSystem.ContainsKey(system))
                return null;

            if (m_TutorialSystem[system].Count >= step)
                return m_TutorialSystem[system][step];

            return null;
        }
    }
}
