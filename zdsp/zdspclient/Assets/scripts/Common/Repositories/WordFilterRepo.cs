using System.Text;
using System.Collections.Generic;
using Kopio.JsonContracts;
using Zealot.Common;

namespace Zealot.Repository
{
    public static class WordFilterRepo
    {
        public static Dictionary<int, WordFilterJson> mWordFilterIdMap; // Word Id <- WordFilterJson
        public static Dictionary<int, List<string>> wordFilterListMap;

        static WordFilterRepo()
		{
            mWordFilterIdMap = new Dictionary<int, WordFilterJson>();
            wordFilterListMap = new Dictionary<int, List<string>>();
        }

        public static void Init(GameDBRepo gameData)
		{
            mWordFilterIdMap = gameData.WordFilter;

            foreach(KeyValuePair<int, WordFilterJson> kvp in mWordFilterIdMap)
            {
                int wordType = (int)kvp.Value.dirtywordtype;
                if(!wordFilterListMap.ContainsKey(wordType))
                    wordFilterListMap.Add(wordType, new List<string>());

                wordFilterListMap[wordType].Add(kvp.Value.word.Normalize(NormalizationForm.FormKC));
            }
            // Add BothChatName filter list into other filter list
            int dWordIdx = (int)DirtyWordType.BothChatName;
            if(wordFilterListMap.ContainsKey(dWordIdx))
            {
                List<string> bFilterList = wordFilterListMap[dWordIdx];
                if(bFilterList != null && bFilterList.Count > 0)
                {
                    for(int i=0; i<dWordIdx; ++i)
                    {
                        if(!wordFilterListMap.ContainsKey(i))
                            wordFilterListMap.Add(i, new List<string>());
                        wordFilterListMap[i].AddRange(bFilterList);
                    }
                }
            }
            // Sort words
            foreach(KeyValuePair<int, List<string>> kvp in wordFilterListMap)
            {
                kvp.Value.Sort(CompareStr);
                kvp.Value.Reverse();
            }
            // For testing
            /*int tmpWordType = (int)DirtyWordType.Chat;
            if(!wordFilterListMap.ContainsKey(tmpWordType))
                wordFilterListMap.Add(tmpWordType, new List<string>());
            wordFilterListMap[tmpWordType].Add("appa");
            wordFilterListMap[tmpWordType].Add("apaa");
            wordFilterListMap[tmpWordType].Add("papa");*/
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

        public static int IndexOf(char[] value, string compareTo, bool ignoreCase, int startIdx=0)
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

        public static bool FilterString(string str, char symbol, DirtyWordType type, out string filteredStr)
        {
            filteredStr = str;
            if (string.IsNullOrEmpty(str))
                return false;
            int dWordType = (int)type;
            if(!wordFilterListMap.ContainsKey(dWordType))
                return false;
            List<string> wordFilterList = wordFilterListMap[dWordType];
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
