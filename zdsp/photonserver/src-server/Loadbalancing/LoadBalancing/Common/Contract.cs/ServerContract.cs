using Newtonsoft.Json;
using Photon.LoadBalancing.ClusterServer.GameServer;
using System;
using System.ComponentModel;

namespace Photon.LoadBalancing
{
    [JsonObject(MemberSerialization = MemberSerialization.OptIn)]
    public class CharacterSyncData
    {
        [DefaultValue(1)]
        [JsonProperty(PropertyName = "lvl")]
        public int lvl;

        [DefaultValue("")]
        [JsonProperty(PropertyName = "server")]
        public string server;
    }

    [JsonObject(MemberSerialization = MemberSerialization.OptIn)]
    public class RegCharInfo
    {
        [DefaultValue("")]
        [JsonProperty(PropertyName = "0")]
        public string userid;

        [DefaultValue(1)]
        [JsonProperty(PropertyName = "1")]
        public int lvl;
    }
}