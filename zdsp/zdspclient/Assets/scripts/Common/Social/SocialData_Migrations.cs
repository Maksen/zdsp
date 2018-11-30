using System.Collections.Generic;
using Zealot.Common.Datablock;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Zealot.Common.Entities.Social
{
    public partial class SocialData : AdvancedLocalObjectData
    {
        /// <summary>
        /// Migration List
        /// </summary>
        static List<System.Func<JToken, JToken>> Migrations = new List<System.Func<JToken, JToken>>()
        {
            Migrate_0,
            Migrate_1,
        };
        #region FinalVersion
        public static JToken FinalVersion()
        {
            return JsonConvert.DeserializeObject<JToken>(
                "{'version':" + VERSION + ",'goodFriends':[],'blackFriends':[],'requestFriends':[],'tempFriends':[],'maxCount':{" +
                string.Format("'good':{0},'black':{1},'request':{2},'temp':{3}",
                    DEFALT_MAX_GOOD_COUNT, DEFALT_MAX_BLACK_COUNT, DEFALT_MAX_REQUEST_COUNT, DEFALT_MAX_TEMP_COUNT)
                + "},'checkdate':''}"
                .Replace('\'', '\"'));
        }
        #endregion

        #region General
        public static JToken Migrate(bool debug, JToken root, int versionData)
        {
            int doAgainCount = 1;
            JToken temp = root;

            if(root.Type== JTokenType.Object)
            {
                JObject o = root as JObject;
                if (o.Count == 0)
                    return FinalVersion();
            }

            System.Action act = () =>
            {
                for (int i = versionData + 1; i < Migrations.Count; i++)
                {
                    temp = Migrations[i](temp);
                    if(temp==null)
                    {
                        temp=FinalVersion();
                        break;
                    }
                }
            };

            if (debug)
            {
            DoAgain:
                try
                {
                    act();
                }
                catch (System.Exception ex)
                {
                    DebugTools.DebugTool.Error(ex.Message);
                    temp = FinalVersion();
                    if (root != null)
                        temp["errorData"] = root.DeepClone();
                    if (doAgainCount > 0)
                    {
                        doAgainCount--;
                        goto DoAgain;
                    }
                    else
                        throw;
                }
            }
            else
                act();

            return temp;
        }
        #endregion

        #region Migrate_0
        static JToken Migrate_0(JToken root)
        {
            return JsonConvert.DeserializeObject<JToken>(
                "{'version':0,'goodFriends':[],'blackFriends':[],'requestFriends':[],'tempFriends':[],'maxCount':{" +
                string.Format("'good':{0},'black':{1},'request':{2},'temp':{3}",
                    DEFALT_MAX_GOOD_COUNT, DEFALT_MAX_BLACK_COUNT, DEFALT_MAX_REQUEST_COUNT, DEFALT_MAX_TEMP_COUNT)
                + "},'checkdate':''}"
                .Replace('\'', '\"'));
        }
        #endregion

        #region Migrate_1 (Current Version)
        static string[] keysFriends = { "goodFriends", "blackFriends", "requestFriends", "tempFriends" };
        static JToken Migrate_1(JToken root)
        {
            root = root.DeepClone();
            
            foreach (var key in keysFriends)
            {
                JArray list = root[key] as JArray;
                if (list == null)
                    root[key] = new JArray();
                else
                { 
                    for (int i = 0; i < list.Count; i++)
                    {
                        JObject item = list[i] as JObject;
                        if (item != null)
                        {
                            JArray newItem = new JArray();
                            string s;
                            // use TryGetValue to avoid throw exceptions
                            if (item.TryGetValue("name", out s))
                                newItem.Add(s);
                            else
                                goto Fail;
                            if (item.TryGetValue("id", out s))
                                newItem.Add(s);
                            else
                                goto Fail;
                            list[i].Replace(newItem);
                            continue;
                        }
                    Fail:
                        list.RemoveAt(i--);
                    }
                }
            }
            return root;
        }
        #endregion
    }
}