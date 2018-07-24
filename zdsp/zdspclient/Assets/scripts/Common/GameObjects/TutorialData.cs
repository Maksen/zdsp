using Newtonsoft.Json;
using System.Collections.Generic;

namespace Zealot.Common
{
    [JsonObject(MemberSerialization = MemberSerialization.OptIn)]
    public class TutorialData
    {
        [JsonProperty(PropertyName = "tutoriallist")]
        public List<int> list = new List<int>();

        [JsonProperty(PropertyName = "tutorialstep")]
        public int Step;

        [JsonProperty(PropertyName = "inprogress")]
        public int Current;

        

        public void InitDefault()
        {
            list = new List<int>();
            Step = -1;
            Current = -1; 
        }
    }
}
