namespace Photon.LoadBalancing.MasterServer
{
    using System.ComponentModel;
    using System.Configuration;
    using System.Diagnostics;

    public sealed class MasterServerSettings : ApplicationSettingsBase
    {
        #region Static Fields

        private static readonly MasterServerSettings defaultInstance =
            ((MasterServerSettings)(Synchronized(new MasterServerSettings())));

        #endregion

        #region Public Properties

        public static MasterServerSettings Default
        {
            get
            {
                return defaultInstance;
            }
        }

        [ApplicationScopedSetting]
        [DebuggerNonUserCode]
        [DefaultSettingValue("5000")]
        public int AppStatsPublishInterval
        {
            get
            {
                return ((int)(this["AppStatsPublishInterval"]));
            }
        }

        [ApplicationScopedSetting]
        [DebuggerNonUserCode]
        [DefaultSettingValue("5055")]
        public int IncomingClientPeerPort
        {
            get
            {
                return ((int)(this["IncomingClientPeerPort"]));
            }
        }

        [ApplicationScopedSetting]
        [DebuggerNonUserCode]
        [DefaultSettingValue("0")]
        public int MasterRelayPortTcp
        {
            get
            {
                return ((int)(this["MasterRelayPortTcp"]));
            }
        }

        [ApplicationScopedSetting]
        [DebuggerNonUserCode]
        [DefaultSettingValue("0")]
        public int MasterRelayPortUdp
        {
            get
            {
                return ((int)(this["MasterRelayPortUdp"]));
            }
        }

        [ApplicationScopedSetting]
        [DebuggerNonUserCode]
        [DefaultSettingValue("0")]
        public int MasterRelayPortWebSocket
        {
            get
            {
                return ((int)(this["MasterRelayPortWebSocket"]));
            }
        }

        [ApplicationScopedSetting]
        [DebuggerNonUserCode]
        [DefaultSettingValue("")]
        public string PublicIPAddress
        {
            get
            {
                return ((string)(this["PublicIPAddress"]));
            }
        }

        [ApplicationScopedSetting]
        [DebuggerNonUserCode]
        [DefaultSettingValue("")]
        public string RedisDB
        {
            get
            {
                return ((string)(this["RedisDB"]));
            }
        }

        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("330354770792434")]
        public string FbAppID
        {
            get
            {
                return ((string)(this["FbAppID"]));
            }
        }

        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("a443d516b39e3ba9617a5a562d80835e")]
        public string FbAppSecret
        {
            get
            {
                return ((string)(this["FbAppSecret"]));
            }
        }

        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("785475024515-48cf9lt6vio79kdqj4va20c14isa4k29.apps.googleusercontent.com")]
        public string GoogleClientId
        {
            get
            {
                return ((string)(this["GoogleClientId"]));
            }
        }

        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("foW8GqFW_LuZioU6k4nnkDis")]
        public string GoogleClientSecret
        {
            get
            {
                return ((string)(this["GoogleClientSecret"]));
            }
        }

        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("http://localhost")]
        public string GoogleRedirectURI
        {
            get
            {
                return ((string)(this["GoogleRedirectURI"]));
            }
        }

        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("mail.zealotdigital.com.tw")]
        public string SmtpServer
        {
            get
            {
                return ((string)(this["SmtpServer"]));
            }
        }

        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("BuddhaOnline@zealotdigital.com.tw")]
        public string EmailAddr
        {
            get
            {
                return ((string)(this["EmailAddr"]));
            }
        }

        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("buddha1216")]
        public string EmailPassword
        {
            get
            {
                return ((string)(this["EmailPassword"]));
            }
        }

        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("")]
        public string GMConnectionString
        {
            get
            {
                return ((string)(this["GMConnectionString"]));
            }
        }

        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("")]
        public string MasterVersion
        {
            get
            {
                return ((string)(this["MasterVersion"]));
            }
        }
        #endregion

        protected override void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "MasterVersion")
                MasterApplication.Instance.InitVersion();
        }
    }
}