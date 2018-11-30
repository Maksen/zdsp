#if DEBUG
using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Photon.LoadBalancing.GameServer
{
    public class CharacterDebugTool
    {
        static JToken update(JToken root, string path, string key, string value)
        {
            JToken node = JsonConvert.DeserializeObject<JToken>(value);
            JToken token = root.SelectToken(path);
            if (token == null)
                return root;
            if (string.IsNullOrEmpty(key))
            {
                if (string.IsNullOrEmpty(path))
                    root = node;
                else
                {
                    token.Replace(node);
                }
            }
            else if (token.Type == JTokenType.Object)
            {
                (token as JObject)[key] = node;
            }
            //array[index]=value
            else if (token.Type == JTokenType.Array)
            {
                JArray arr = token as JArray;
                int index;
                if (int.TryParse(key, out index))
                {
                    if (index < 0)
                    {
                        int insertIndex = (-index) - 1;
                        if (insertIndex >= arr.Count)
                            arr.Add(node);
                        else
                            arr.Insert(insertIndex, node);
                    }
                    else if (index < arr.Count)
                        arr[index] = node;
                    else
                        arr.Add(node);
                }
                else
                    arr.Add(node);
            }
            return root;
        }


        /// <summary>
        /// just for watch record
        /// </summary>
        public static void DebugSelectTool(GameClientPeer peer, string userid, string charname, string db_column, string path, string key)
        {
#if SOCIAL_COUNT_FIBER_QUEUE
            GameCounters.ExecutionFiberQueue.Increment();
#endif
            GameApplication.Instance.executionFiber.Enqueue(async () =>
            {
#if SOCIAL_COUNT_FIBER_QUEUE
                GameCounters.ExecutionFiberQueue.Decrement();
#endif
                string result = string.Empty;
                bool success = false;

                List<Dictionary<string, object>> chars = await GameApplication.dbRepository.Character.GetByUserID(userid, GameApplication.Instance.GetMyServerline());

                if (chars == null || chars.Count == 0)
                {
                    goto Final;
                }

                foreach (var ch in chars)
                {
                    if ((string)ch["charname"] == charname)
                    {

                        switch (db_column)
                        {
                            case "characterdata":
                            case "friends":
                                {
                                    string json = (string)ch[db_column];

                                    try
                                    {
                                        JToken root = JsonConvert.DeserializeObject<JToken>(json);
                                        JToken token = root.SelectToken(path);
                                        if (token != null)
                                        {
                                            if (!string.IsNullOrEmpty(key))
                                            {
                                                if (token.Type == JTokenType.Array)
                                                {
                                                    JArray arr = token as JArray;
                                                    int i;
                                                    if (int.TryParse(key, out i) && i >= 0 && i < arr.Count)
                                                        result = arr[i].ToString(Formatting.None);
                                                    else
                                                        result = "null";
                                                }
                                                else if (token.Type == JTokenType.Object)
                                                {
                                                    JObject obj = token as JObject;
                                                    JToken v;
                                                    if (obj.TryGetValue(key, out v))
                                                        result = v.ToString(Formatting.None);
                                                    else
                                                        result = "null";
                                                }
                                                else
                                                    result = "null";
                                            }
                                            else
                                                result = token.ToString(Formatting.None);
                                        }
                                        else
                                            result = "null";
                                        success = true;
                                    }
                                    catch (Exception ex)
                                    {
                                        result = ex.Message;
                                    }

                                }
                                break;
                        }


                    }
                }


                Final:
                peer.ZRPC.NonCombatRPC.Ret_DebugSelectTool(success, charname, result, peer);
            });
        }

        /// <summary>
        /// just for fix record, use it carefully
        /// </summary>
        public static void DebugFixTool(GameClientPeer peer, string userid, string charname, string db_column, string path, string key, string value, bool forceReset)
        {
#if SOCIAL_COUNT_FIBER_QUEUE
            GameCounters.ExecutionFiberQueue.Increment();
#endif
            GameApplication.Instance.executionFiber.Enqueue(async () =>
            {
#if SOCIAL_COUNT_FIBER_QUEUE
                GameCounters.ExecutionFiberQueue.Decrement();
#endif
                string msg = string.Empty;

                List<Dictionary<string, object>> chars = await GameApplication.dbRepository.Character.GetByUserID(userid, GameApplication.Instance.GetMyServerline());

                if (chars == null || chars.Count == 0)
                {
                    msg = "Chars count = 0";
                    goto Final;
                }

                foreach (var ch in chars)
                {
                    if ((string)ch["charname"] == charname)
                    {
                        switch (db_column)
                        {
                            case "friends":
                                {
                                    string json = (string)ch["friends"];

                                    try
                                    {
                                        if (forceReset)
                                        {
                                            json = new Zealot.Common.Entities.Social.SocialData(new JObject(),false).BuildRecordsString();
                                        }
                                        else
                                        {
                                            JToken root = JsonConvert.DeserializeObject<JToken>(json);
                                            root = update(root, path, key, value);
                                            json = root.ToString(Formatting.None);
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        msg = ex.Message;
                                    }

                                    await GameApplication.dbRepository.Character.UpdateSocialList(charname, json);

                                }
                                break;
                            case "characterdata":
                                {
                                    string json = (string)ch["characterdata"];
                                    try
                                    {
                                        JToken root = JsonConvert.DeserializeObject<JToken>(json);
                                        root = update(root, path, key, value);
                                        ch["characterdata"] = root.ToString(Formatting.None);
                                    }
                                    catch (Exception ex)
                                    {
                                        msg = ex.Message;
                                    }

                                    await GameApplication.dbRepository.Character.SaveCharacterAndUserAsync(
                                        ((Guid)ch["charid"]).ToString(),
                                        (long)ch["experience"],
                                        (int)ch["progresslevel"],
                                        (int)ch["combatscore"],
                                        (int)ch["portraitid"],
                                        (int)ch["money"],
                                        (int)ch["gold"],
                                        (int)ch["bindgold"],
                                        (int)ch["guildid"],
                                        (byte)ch["guildrank"],
                                        (byte)ch["viplevel"],
                                        (int)ch["fundtoday"],
                                        (long)ch["fundtotal"],
                                        (int)ch["factionkill"],
                                        (int)ch["factiondeath"],
                                        (short)ch["petcollected"],
                                        (int)ch["petscore"],
                                        (short)ch["herocollected"],
                                        (int)ch["heroscore"],
                                        new DateTime((ch["guildcdenddt"] != DBNull.Value) ? ((DateTime)ch["guildcdenddt"]).Ticks : 0),
                                        //(string)ch["friends"],
                                        //(string)ch["friendrequests"],
                                        0, 0,
                                        (string)ch["gamesetting"],
                                        (string)ch["characterdata"],
                                        DateTime.MinValue,
                                        (DateTime)ch["dtlogout"]
                                        );
                                }
                                break;
                        }

                        

                        msg = "success";
                    }
                }


                Final:
                peer.ZRPC.NonCombatRPC.Ret_DebugFixTool(msg, peer);
            });
        }

    }
}
#endif