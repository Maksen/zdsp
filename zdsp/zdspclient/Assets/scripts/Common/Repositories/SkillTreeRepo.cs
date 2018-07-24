using System;
using System.Text;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using Kopio.JsonContracts;
using Zealot.Common;

namespace Zealot.Repository {
    class SkillTreeRepo {
        private static Dictionary<JobType ,List<Dictionary<int, SkillTreeJson>>> m_SkillTree = new Dictionary<JobType, List<Dictionary<int, SkillTreeJson>>>();

        public static void Init(GameDBRepo gameData) {

            foreach(KeyValuePair<int, SkillTreeJson> kvp in gameData.SkillTree) {
                if (!m_SkillTree.ContainsKey(kvp.Value.jobclass)) {
                    m_SkillTree.Add(kvp.Value.jobclass, new List<Dictionary<int, SkillTreeJson>>());
                    for (int i = 0; i < 3; ++i) {
                        m_SkillTree[kvp.Value.jobclass].Add(new Dictionary<int, SkillTreeJson>());
                    }
                }
                int key = 65;
                key = Encoding.ASCII.GetBytes(kvp.Value.gridrow.ToUpper())[0];

                m_SkillTree[kvp.Value.jobclass][key - 65][kvp.Value.gridcol] = kvp.Value;
            }
        }

        public static List<Dictionary<int, SkillTreeJson>> GetSkillTreeInfo(JobType job) {
            if (!m_SkillTree.ContainsKey(job)) return null;
            return m_SkillTree[job];
        }
    }
}
