namespace Photon.LoadBalancing.ClusterServer
{
    using System.Configuration;
    using System.Diagnostics;

    public sealed class ClusterServerSettings : ApplicationSettingsBase
    {
        #region Static Fields
        private static readonly ClusterServerSettings defaultInstance =
            ((ClusterServerSettings)(Synchronized(new ClusterServerSettings())));
        #endregion

        #region Public Properties
        public static ClusterServerSettings Default
        {
            get
            {
                return defaultInstance;
            }
        }

        [ApplicationScopedSetting]
        [DebuggerNonUserCode]
        [DefaultSettingValue("1000")]
        public int AppStatsPublishInterval
        {
            get
            {
                return ((int)(this["AppStatsPublishInterval"]));
            }
        }

        [ApplicationScopedSetting]
        [DebuggerNonUserCode]
        [DefaultSettingValue("5")]
        public int ConnectReytryInterval
        {
            get
            {
                return ((int)(this["ConnectReytryInterval"]));
            }
        }    

        [ApplicationScopedSetting]
        [DebuggerNonUserCode]
        [DefaultSettingValue("127.0.0.1")]
        public string MasterIPAddress
        {
            get
            {
                return ((string)(this["MasterIPAddress"]));
            }
        }

        [ApplicationScopedSetting]
        [DebuggerNonUserCode]
        [DefaultSettingValue("4520")]
        public int OutgoingMasterServerPeerPort
        {
            get
            {
                return ((int)(this["OutgoingMasterServerPeerPort"]));
            }
        }

        [ApplicationScopedSetting]
        [DebuggerNonUserCode]
        [DefaultSettingValue("127.0.0.1")]
        public string PublicIPAddress
        {
            get
            {
                return ((string)(this["PublicIPAddress"]));
            }
        }

        [ApplicationScopedSetting]
        [DebuggerNonUserCode]
        [DefaultSettingValue("4540")]
        public int PublicTcpPort
        {
            get
            {
                return ((int)(this["PublicTcpPort"]));
            }
        }

        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("")]
        public string ConnectionString
        {
            get
            {
                return ((string)(this["ConnectionString"]));
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
        #endregion
    }
}