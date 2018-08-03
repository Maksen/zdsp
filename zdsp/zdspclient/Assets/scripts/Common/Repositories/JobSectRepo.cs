using System.Collections.Generic;
using Kopio.JsonContracts;
using Zealot.Common;

namespace Zealot.Repository
{
    public static class JobSectRepo
    {
        public static Dictionary<JobType, JobsectJson> mJobTypeMap;
        public static Dictionary<Gender, GenderInfoJson> mGenderMap;

        public static Dictionary<JobType, JobTree> m_JobTreeCache;

        public class JobTree
        {
            private JobType m_Type;
            private JobType m_Parent;
            private LinkedList<JobTree> m_JobChange;

            public JobTree(JobType type)
            {
                m_Type = type;
                m_JobChange = new LinkedList<JobTree>();
            }

            public JobTree AddJobAdvance(JobType type)
            {
                JobTree node = new JobTree(type);
                node.m_Parent = m_Type;
                m_JobChange.AddLast(node);
                return node;
            }

            public LinkedList<JobTree> GetNextJobs()
            {
                return m_JobChange;
            }

            public JobType GetPreviousJob()
            {
                return m_Parent;
            }
        }

        private static JobTree m_JobTree;

        static JobSectRepo()
		{
            mJobTypeMap = new Dictionary<JobType, JobsectJson>();
            mGenderMap = new Dictionary<Gender, GenderInfoJson>();
            m_JobTreeCache = new Dictionary<JobType, JobTree>();
            
        }

		public static void Init(GameDBRepo gameData)
		{
            mJobTypeMap.Clear();
            mGenderMap.Clear();
            foreach (var entry in gameData.Jobsect.Values)
                mJobTypeMap[entry.jobtype] = entry;
            foreach (var entry in gameData.GenderInfo.Values)
                mGenderMap[entry.gender] = entry;

            // create job tree
            foreach(var entry in gameData.JobTree.Values)
            {
                if (!m_JobTreeCache.ContainsKey(entry.job))
                {
                    JobTree node = new JobTree(entry.job);
                    JobTree next_node = node.AddJobAdvance(entry.nextjob);
                    m_JobTreeCache.Add(entry.job, node);
                    m_JobTreeCache.Add(entry.nextjob, next_node);
                }
                else
                {
                    JobTree next_node = m_JobTreeCache[entry.job].AddJobAdvance(entry.nextjob);
                    m_JobTreeCache.Add(entry.nextjob, next_node);
                }
            }
        }

        public static JobsectJson GetJobByType(JobType type)
		{
            JobsectJson json;
            if (mJobTypeMap.TryGetValue(type, out json))
                return json;
		    return null;
		}

        public static GenderInfoJson GetGenderInfo(Gender gender)
        {
            return mGenderMap[gender];
        }

        public static string GetJobLocalizedName(JobType type)
        {
            JobsectJson json = GetJobByType(type);
            if (json != null)
                return json.localizedname;
            return "";
        }

        /// <summary>
        /// Returns a list of jobs that derive to current job starting from the job pasted in to newbie
        /// </summary>
        /// <param name="job"></param>
        /// <returns></returns>
        public static List<JobType> GetJobHistoryToCurrent(JobType job)
        {
            List<JobType> result = new List<JobType>();
            result.Add(job);
            // loop to newbie
            JobType current = job;
            while(current != JobType.Newbie)
            {
                current = m_JobTreeCache[job].GetPreviousJob();
                result.Add(current);
            }

            return result;
        }
    }
}
