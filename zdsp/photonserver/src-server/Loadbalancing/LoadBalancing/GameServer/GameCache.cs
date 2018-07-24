// --------------------------------------------------------------------------------------------------------------------
// <copyright file="GameCache.cs" company="Exit Games GmbH">
//   Copyright (c) Exit Games GmbH.  All rights reserved.
// </copyright>
// <summary>
//   Defines the GameCache type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Photon.LoadBalancing.GameServer
{
    #region using directives

    using Photon.Hive;
    using Photon.Hive.Caching;
    using Photon.Hive.Plugin;

    #endregion

    public class GameCache : RoomCacheBase
    {
        public static readonly GameCache Instance = new GameCache();

        private static PluginManager pluginManager;

        private static GameApplication application_;
        public static GameApplication Application
        {
            get { return application_; }

            set {
                application_ = value;
                pluginManager = new PluginManager(Application.ApplicationRootPath);
            }
        }

        public PluginManager PluginManager { get { return pluginManager; } }

        public GameCache()
        {

        }

        protected override Room CreateRoom(string roomId, params object[] args)
        {
            if(args == null || args.Length < 1)
                return new Game(Application, roomId, this, pluginManager, "");
            else
                return new Game(Application, roomId, this, pluginManager, (string)args[0]);
        }
    }
}