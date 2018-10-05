using System.Text;
using System.Collections.Generic;
using Kopio.JsonContracts;
using Zealot.Common;

namespace Zealot.Repository
{
    public static class WordFilterRepo
    {
        private static Dictionary<FilterType, List<string>> mWordFilterList;

        static WordFilterRepo()
		{
            mWordFilterList = new Dictionary<FilterType, List<string>>();
        }

        public static void Init(GameDBRepo gameData)
		{
            foreach (KeyValuePair<int, DisableWordJson> entry in gameData.DisableWord)
            {
                if (!mWordFilterList.ContainsKey(entry.Value.filtertype))
                {
                    mWordFilterList.Add(entry.Value.filtertype, new List<string>());
                }

                mWordFilterList[entry.Value.filtertype].Add(entry.Value.word);
            }
        }

        private static int CompareStr(string x, string y)
        {
            if(x == null)
            {
                if(y == null) return 0; // equal
                else return -1; // y greater
            }
            else
            {
                if(y == null) return 1; // x greater
                else
                {
                    if(x.Length == y.Length)
                        return string.Compare(x, y);
                    else if (x.Length > y.Length)
                        return 1;
                    else
                        return -1;
                }
            }
        }

        private static int IndexOf(char[] value, string compareTo, bool ignoreCase, int startIdx=0)
        {
            int valLen = value.Length;
            int compareToLen = compareTo.Length;
            if(valLen < compareToLen)
                return -1;

            int compToLenMaxIdx = compareToLen-1;
            for(int i=startIdx; i<valLen; ++i)
            {
                if (valLen-i < compareToLen) return -1;
                for(int j=0; j<compareToLen; ++j)
                {
                    int newIdx = i+j;
                    char charA = ignoreCase ? char.ToUpper(value[newIdx]) : value[newIdx];
                    char charB = ignoreCase ? char.ToUpper(compareTo[j]) : value[j];
                    if(charA != charB)
                    {
                        i = newIdx;
                        break;
                    }
                    else if(j == compToLenMaxIdx)
                        return i;
                }
            }
            return -1;
        }

        public static bool FilterString(string str, char symbol, FilterType type, out string filteredStr)
        {
            filteredStr = str;
            if (string.IsNullOrEmpty(str))
                return false;
            if(!mWordFilterList.ContainsKey(type))
                return false;
            List<string> wordFilterList = mWordFilterList[type];
            int wordFilterListCnt = wordFilterList.Count;
            if (wordFilterListCnt <= 0)
                return false;

            str = str.Normalize(NormalizationForm.FormKC); // Normalize text

            bool hasInvalid = false;
            int strLen = str.Length;
            char[] strArray = str.ToCharArray();
            for (int i=0; i<wordFilterListCnt; ++i)
            {
                string filterWord = wordFilterList[i];
                int filterWordLen = filterWord.Length;
                if(strLen >= filterWordLen)
                {
                    int idx = IndexOf(strArray, filterWord, true);
                    int startIdx = 0;
                    while(idx != -1)
                    {
                        hasInvalid = true;
                        for (int j=0; j<filterWordLen; ++j)
                            strArray[idx+j] = char.IsWhiteSpace(filterWord[j]) ? ' ' : symbol;
                        startIdx = idx+filterWordLen;
                        idx = IndexOf(strArray, filterWord, true, startIdx);
                    }
                }
            }
            filteredStr = hasInvalid ? new string(strArray) : str;
            return hasInvalid;
        }
    }
}
