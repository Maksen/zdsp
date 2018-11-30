using Newtonsoft.Json;
using System.IO;
using UnityEngine;

public static class GameSettings
{
    public static ClientSettingsData ClientSettingsData;
    public static ServerSettingsData ServerSettingsData;

    public static bool MusicEnabled
    {
        get { return ClientSettingsData.MusicEnabled; }
        set { ClientSettingsData.MusicEnabled = value; }
    }

    public static bool SoundFXEnabled
    {
        get { return ClientSettingsData.SoundFXEnabled; }
        set { ClientSettingsData.SoundFXEnabled = value; }
    }

    public static float MusicVolume
    {
        get { return ClientSettingsData.MusicVolume; }
        set { ClientSettingsData.MusicVolume = value; }            
    }

    public static float SoundFXVolume
    {
        get { return ClientSettingsData.SoundFXVolume; }
        set { ClientSettingsData.SoundFXVolume = value; }
    }

    public static bool NotificationEnabled
    {
        get { return ClientSettingsData.NotificationEnabled; }
        set { ClientSettingsData.NotificationEnabled = value; }
    }

    public static bool AutoBotEnabled
    {
        get { return ClientSettingsData.AutoBotEnabled; }
        set { ClientSettingsData.AutoBotEnabled = value; }
    }

    public static bool HideOtherPlayers
    {
        get { return ClientSettingsData.HideOtherPlayers; }
        set { ClientSettingsData.HideOtherPlayers = value; }
    }

    public static bool SpendConfirmationEnabled
    {
        get { return ClientSettingsData.SpendConfirmationEnabled; }
        set { ClientSettingsData.SpendConfirmationEnabled = value; }
    }

    public static bool AutoWorldVoice
    {
        get { return ClientSettingsData.AutoWorldVoice; }
        set { ClientSettingsData.AutoWorldVoice = value; }
    }

    public static bool AutoGuildVoice
    {
        get { return ClientSettingsData.AutoGuildVoice; }
        set { ClientSettingsData.AutoGuildVoice = value; }
    }

    public static bool AutoPartyVoice
    {
        get { return ClientSettingsData.AutoPartyVoice; }
        set { ClientSettingsData.AutoPartyVoice = value; }
    }

    public static bool AutoFactionVoice
    {
        get { return ClientSettingsData.AutoFactionVoice; }
        set { ClientSettingsData.AutoFactionVoice = value; }
    }

    public static bool AutoWhisperVoice
    {
        get { return ClientSettingsData.AutoWhisperVoice; }
        set { ClientSettingsData.AutoWhisperVoice = value; }
    }

    public static bool WifiAutoVoice
    {
        get { return ClientSettingsData.WifiAutoVoice; }
        set { ClientSettingsData.WifiAutoVoice = value; }
    }

    public static bool VoiceEnabled
    {
        get { return ClientSettingsData.VoiceEnabled; }
        set { ClientSettingsData.VoiceEnabled = value; }
    }

    public static bool AutoAcceptFriendRequest
    {
        get { return ServerSettingsData.AutoAcceptFriendRequest; }
        set { ServerSettingsData.AutoAcceptFriendRequest = value; }
    }

    public static bool AutoAcceptPartyRequest
    {
        get { return ServerSettingsData.AutoAcceptPartyRequest; }
        set { ServerSettingsData.AutoAcceptPartyRequest = value; }
    }

    public static bool RejectWhisper
    {
        get { return ServerSettingsData.RejectWhisper; }
        set { ServerSettingsData.RejectWhisper = value; }
    }

    public static void SerializeClient()
    {
        string dataFile = ClientSettingsData.SettingsDataFile;
        ClientSettingsData.SerializeToFile(string.Format("{0}/{1}", Application.persistentDataPath, dataFile));
    }

    public static void DeserializeClient(ClientSettingsData settingsData)
    {
        string dataFileJson = ClientSettingsData.SettingsDataFile;
        string dataFileJsonPath = string.Format("{0}/{1}", Application.persistentDataPath, dataFileJson);

        if (File.Exists(dataFileJsonPath))
        {
            settingsData = settingsData.DeserializeFromFile(dataFileJsonPath);
        }

        ClientSettingsData = settingsData;
    }

    public static string SerializeServer()
    {
        return ServerSettingsData.Serialize();
    }

    public static void DeserializeServer(string data)
    {
        ServerSettingsData =  ServerSettingsData.Deserialize(data);
    }
}

[JsonObject(MemberSerialization = MemberSerialization.OptIn)]
public class ClientSettingsData
{
    public static string SettingsDataFile = "GameSettings.json";

    [JsonProperty(PropertyName = "music")]
    public bool MusicEnabled { get; set; }

    [JsonProperty(PropertyName = "sfx")]
    public bool SoundFXEnabled { get; set; }

    [JsonProperty(PropertyName = "musicvol")]
    public float MusicVolume { get; set; }

    [JsonProperty(PropertyName = "sfxvol")]
    public float SoundFXVolume { get; set; }

    [JsonProperty(PropertyName = "notify")]
    public bool NotificationEnabled { get; set; }

    [JsonProperty(PropertyName = "bot")]
    public bool AutoBotEnabled { get; set; }

    [JsonProperty(PropertyName = "hideplayers")]
    public bool HideOtherPlayers { get; set; }

    [JsonProperty(PropertyName = "spendconfim")]
    public bool SpendConfirmationEnabled { get; set; }

    [JsonProperty(PropertyName = "voiceworld")]
    public bool AutoWorldVoice { get; set; }

    [JsonProperty(PropertyName = "voiceguild")]
    public bool AutoGuildVoice { get; set; }

    [JsonProperty(PropertyName = "voiceparty")]
    public bool AutoPartyVoice { get; set; }

    [JsonProperty(PropertyName = "voicefaction")]
    public bool AutoFactionVoice { get; set; }

    [JsonProperty(PropertyName = "voicewhisper")]
    public bool AutoWhisperVoice { get; set; }

    [JsonProperty(PropertyName = "voicewifi")]
    public bool WifiAutoVoice { get; set; }

    [JsonProperty(PropertyName = "voice")]
    public bool VoiceEnabled { get; set; }

    public ClientSettingsData()
    {
        MusicEnabled = true;
        SoundFXEnabled = true;
        MusicVolume = 100;
        SoundFXVolume = 100;
        NotificationEnabled = true;
        AutoBotEnabled = false;
        HideOtherPlayers = false;
        SpendConfirmationEnabled = true;
        AutoWorldVoice = false;
        AutoGuildVoice = false;
        AutoPartyVoice = false;
        AutoFactionVoice = false;
        AutoWhisperVoice = false;
        WifiAutoVoice = false;
        VoiceEnabled = true;
    }

    public void SerializeToFile(string path)
    {
        SerializeToFile<ClientSettingsData>(path, this);
    }

    public static void SerializeToFile<T>(string path, object obj)
    {
        using (StreamWriter sw = new StreamWriter(@path))
        {
            JsonSerializer serializer = new JsonSerializer();
            serializer.Serialize(sw, obj);
            sw.Close();
        }
    }

    public ClientSettingsData DeserializeFromFile(string path)
    {
        return DeserializeFromFile<ClientSettingsData>(path);
    }

    public static T DeserializeFromFile<T>(string path)
    {
        using (StreamReader sr = new StreamReader(@path))
        {
            JsonSerializer serializer = new JsonSerializer();
            return (T)serializer.Deserialize(sr, typeof(T));
        }
    }
}

[JsonObject(MemberSerialization = MemberSerialization.OptIn)]
public class ServerSettingsData
{
    [JsonProperty(PropertyName = "friend")]
    public bool AutoAcceptFriendRequest { get; set; }

    [JsonProperty(PropertyName = "party")]
    public bool AutoAcceptPartyRequest { get; set; }

    [JsonProperty(PropertyName = "whisper")]
    public bool RejectWhisper { get; set; }

    public ServerSettingsData()
    {
        AutoAcceptFriendRequest = false;
        AutoAcceptPartyRequest = false;
        RejectWhisper = false;
    }

    public string Serialize()
    {
        JsonSerializerSettings jsonSetting = new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.None, DefaultValueHandling = DefaultValueHandling.Ignore, NullValueHandling = NullValueHandling.Ignore };
        return JsonConvert.SerializeObject(this, Formatting.None, jsonSetting);
    }

    public static ServerSettingsData Deserialize(string data)
    {
        JsonSerializerSettings jsonSetting = new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.None, DefaultValueHandling = DefaultValueHandling.Ignore, NullValueHandling = NullValueHandling.Ignore };
        return JsonConvert.DeserializeObject<ServerSettingsData>(data, jsonSetting);
    }
}
