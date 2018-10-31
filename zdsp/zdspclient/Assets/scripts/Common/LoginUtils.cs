using Newtonsoft.Json;
using System;
using System.IO;
using System.ComponentModel;
using UnityEngine;

namespace Zealot.Common
{
    #region Authentication Type
    public enum LoginType : short
    {   
        EstablishConnection = -1,
        Device = 0,
        Username = 1,
        Facebook = 2,
        Google = 3, 
    }
    #endregion

    public class ServerInfo
    {        
        public int id;
        public string ipAddr;
        public string serverName;
        public int serverLine;
        public ServerLoad serverLoad;

        public ServerInfo(int id, string ipAddr, string serverName, int serverLine, ServerLoad serverLoad)
        {
            this.id = id;
            this.ipAddr = ipAddr;
            this.serverName = serverName;
            this.serverLine = serverLine;
            this.serverLoad = serverLoad;
        }
    }

    [JsonObject(MemberSerialization = MemberSerialization.OptIn)]
    public class LoginData
    {
        public static string clientDataFile = "ClientData.json";
        public static string editorDataFile = "EditorData.json";

        #region Serializable properties

        [DefaultValue(1)]
        [JsonProperty(PropertyName = "logintype")]
        public short LoginType { get; set; }

        [DefaultValue("")]
        [JsonProperty(PropertyName = "loginid")]
        public string LoginId { get; set; }

        [DefaultValue("")]
        [JsonProperty(PropertyName = "deviceid")]
        public string DeviceId { get; set; }

        [DefaultValue("")]
        [JsonProperty(PropertyName = "encryptpass")]
        public string EncryptedPass { get; set; }

        [DefaultValue("")]
        [JsonProperty(PropertyName = "iv")]
        public string IV { get; set; }

        [DefaultValue(0)]
        [JsonProperty(PropertyName = "svriddisplay")]
        public int ServerId { get; set; }

        [DefaultValue(0)]
        [JsonProperty(PropertyName = "firstlogintick")]
        public long FirstLoginTick { get; set; }

        [DefaultValue(false)]
        [JsonProperty(PropertyName = "showannounce")]
        public bool ShowAnnounce { get; set; }

        [DefaultValue(false)]
        [JsonProperty(PropertyName = "hasreadlicense")]
        public bool HasReadLicense { get; set; }

        #endregion

        #region Non-Serializable properties

        public Guid? cookieId { get; set; }
        public Guid? userId { get; set; }

        #endregion

        public LoginData()
        {
            ResetData();
        }

        #region Singleton
        private static LoginData instance;
        public static LoginData Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new LoginData();
                }
                return instance;
            }
        }
        #endregion

        public void ResetData()
        {
            cookieId = Guid.Empty;
            userId = Guid.Empty;
            this.LoginType = (short)Zealot.Common.LoginType.Username;
        }

        public bool IsDataValid
        {
            get { return !string.IsNullOrEmpty(Instance.DeviceId) && !string.IsNullOrEmpty(Instance.LoginId); }
        }

        public void SerializeLoginData()
        {
            string dataFile = (Application.platform == RuntimePlatform.WindowsEditor) ? editorDataFile
                                                                                      : clientDataFile;
            SerializeToFile(string.Format("{0}/{1}", Application.persistentDataPath, dataFile));
        }

        public bool DeserializeLoginData()
        {
            string dataFileJson = (Application.platform == RuntimePlatform.WindowsEditor) ? editorDataFile
                                                                                          : clientDataFile;
            string dataFileJsonPath = string.Format("{0}/{1}", Application.persistentDataPath, dataFileJson);
            if (!File.Exists(dataFileJsonPath))
                return false;

            LoginData loginData = DeserializeFromFile(dataFileJsonPath);
            Instance.LoginType = loginData.LoginType;
            Instance.LoginId = loginData.LoginId;
            Instance.DeviceId = loginData.DeviceId;
            Instance.EncryptedPass = loginData.EncryptedPass;
            Instance.IV = loginData.IV;
            Instance.ServerId = loginData.ServerId;
            Instance.FirstLoginTick = loginData.FirstLoginTick;
            Instance.ShowAnnounce = loginData.ShowAnnounce;
            Instance.HasReadLicense = loginData.HasReadLicense;
            return true;
        }

        #region Json Serialization Methods

        public void SerializeToFile(string path)
        {
            SerializeToFile<LoginData>(path, this);
        }

        public static void SerializeToFile<T>(string path, object obj)
        {
            using(StreamWriter sw = new StreamWriter(@path))
            {
                JsonSerializer serializer = new JsonSerializer();
                serializer.Serialize(sw, obj);
                sw.Close();
            }
        }

        public LoginData DeserializeFromFile(string path)
        {
            return DeserializeFromFile<LoginData>(path);
        }

        public static T DeserializeFromFile<T>(string path)
        {
            using(StreamReader sr = new StreamReader(@path))
            {
                JsonSerializer serializer = new JsonSerializer();
                return (T)serializer.Deserialize(sr, typeof(T));
            }
        }

        #endregion
    }

    [Obsolete]
    public class LoginUtils
    {
        public static string clientDataFile = "ClientData.txt";
        public static string editorDataFile = "EditorData.txt";

        #region Text Serialization Methods

        public static void ReadClientData(string path, ref string loginType, ref string loginId, ref string deviceId, 
                                          ref string ipaddr, ref string encryptedPass, ref string IV)
        {
            //if (File.Exists (path)) 
            //{
            using(StreamReader reader = new StreamReader(path))
            {
                loginType = reader.ReadLine();
                loginId = reader.ReadLine();
                deviceId = reader.ReadLine();
                ipaddr = reader.ReadLine();
                encryptedPass = reader.ReadLine();
                IV = reader.ReadLine();

                reader.Close();
            }
            //} 
        }

        public static void WriteClientData(string path, string loginType, string loginId, string deviceId,
                                           string ipaddr="", string encryptedPass="", string IV="")
        {
            using(StreamWriter writer = new StreamWriter(path))
            {
                writer.WriteLine(loginType);
                writer.WriteLine(loginId);
                writer.WriteLine(deviceId);
                if(!string.IsNullOrEmpty(ipaddr))
                    writer.WriteLine(ipaddr);            
                if(!string.IsNullOrEmpty(encryptedPass))
                    writer.WriteLine(encryptedPass);
                if(!string.IsNullOrEmpty(IV))
                    writer.WriteLine(IV);

                writer.Close();
            }
        }

        #endregion
    }
}
