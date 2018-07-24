using System.Collections.Generic;
using Kopio.JsonContracts;
using Zealot.Common;

namespace Zealot.Repository
{
    public static class JobSectRepo
    {
        public static Dictionary<JobType, JobsectJson> mJobTypeMap;
        public static Dictionary<Gender, GenderInfoJson> mGenderMap;

        static JobSectRepo()
		{
            mJobTypeMap = new Dictionary<JobType, JobsectJson>();
            mGenderMap = new Dictionary<Gender, GenderInfoJson>();

        }

		public static void Init(GameDBRepo gameData)
		{
            mJobTypeMap.Clear();
            mGenderMap.Clear();
            foreach (var entry in gameData.Jobsect.Values)
                mJobTypeMap[entry.jobtype] = entry;
            foreach (var entry in gameData.GenderInfo.Values)
                mGenderMap[entry.gender] = entry;
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
    }
}
