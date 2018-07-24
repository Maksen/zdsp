using Newtonsoft.Json;

[JsonObject(MemberSerialization = MemberSerialization.OptIn)]
public class CharacterInfoData
{
    [JsonProperty(PropertyName = "str")]
    public int Str { get; set; }
    [JsonProperty(PropertyName = "agi")]
    public int Agi { get; set; }
    [JsonProperty(PropertyName = "con")]
    public int Con { get; set; }
    [JsonProperty(PropertyName = "int")]
    public int Int { get; set; }
    [JsonProperty(PropertyName = "dex")]
    public int Dex { get; set; }

    [JsonProperty(PropertyName = "statpt")]
    public int StatPoint { get; set; }
}