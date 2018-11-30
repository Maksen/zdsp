// --------------------------------------------------------------------------------------------------------------------
// <copyright file="RoomCacheBase.cs" company="Exit Games GmbH">
//   Copyright (c) Exit Games GmbH.  All rights reserved.
// </copyright>
// <summary>
//   Base class for room caches.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Photon.Hive.Caching
{
    #region using directives

    using System;
    using System.Collections.Generic;
    using System.Text;
    using ExitGames.Logging;
    using Diagnostics.OperationLogging;
    using Photon.SocketServer;

    #endregion

    /// <summary>
    /// Base class for room caches.
    /// </summary>
    public abstract class RoomCacheBase
    {
        /// <summary>
        /// An <see cref="ILogger"/> instance used to log messages to the logging framework.
        /// </summary>
        private static readonly ILogger log = LogManager.GetCurrentClassLogger();

        /// <summary>A Dictionary used to store room instances. RoomInstances[Room guid]</summary>
        public readonly Dictionary<string, RoomInstance> RoomInstances = new Dictionary<string, RoomInstance>();
        /// <summary>A Dictionary used to store default room guid</summary>
        public readonly Dictionary<string, string> mDefaultRooms = new Dictionary<string, string>();
        /// <summary>A Dictionary used to store realm Id to a list of realm room guid</summary>
        public readonly Dictionary<int, List<string>> mRealmRooms = new Dictionary<int, List<string>>();

        /// <summary>used to syncronize acces to the cache.</summary>
        protected readonly object SyncRoot = new object();

        /// <summary>
        /// Tries to get room reference for a room with the specified id, without holding a reference to that room. 
        /// </summary>
        /// <param name="roomId">The room id.(GUID)</param>
        /// <param name="room">The room, in case it exists.</param>
        /// <returns>
        /// True if the cache contains a room with the specified room id; otherwise, false.
        /// </returns>
        public bool TryGetRoomWithoutReference(string roomId, out Room room)
        {
            lock (this.SyncRoot)
            {
                RoomInstance roomInstance;
                if (!string.IsNullOrEmpty(roomId) && this.RoomInstances.TryGetValue(roomId, out roomInstance))
                {
                    room = roomInstance.Room;
                    return true;
                }

                room = null;
                return false;
            }
        }

        /// <summary>
        /// Gets a room reference for a room with a specified id.
        /// If the room with the specified id does not exists, a new room will be created.
        /// </summary>
        /// <param name="roomName">
        /// The room id.
        /// </param>
        /// <param name="ownerPeer">
        /// The peer that holds this reference.
        /// </param>
        /// <param name="args">
        /// Optionally arguments used for room creation.
        /// </param>
        /// <returns>
        /// a <see cref="RoomReference"/>
        /// </returns>
        public RoomReference GetRoomReference(string roomName, PeerBase ownerPeer, params object[] args)
        {
            lock (this.SyncRoot)
            {
                RoomInstance roomInstance;
                if (!this.RoomInstances.TryGetValue(roomName, out roomInstance))
                {
                    if (log.IsDebugEnabled)
                    {
                        log.DebugFormat("Creating room instance: roomName={0}", roomName);
                    }

                    Room room = this.CreateRoom(roomName, false, args);
                    roomInstance = new RoomInstance(this, room);
                    this.RoomInstances.Add(roomName, roomInstance);
                }

                return roomInstance.AddReference(ownerPeer);
            }
        }

        /// <summary>
        /// Returns the names of all rooms that are currently cached in this <see cref="RoomCacheBase"/>.
        /// </summary>
        /// <returns>The list of room names.</returns>
        public List<string> GetRoomNames()
        {
            lock (this.SyncRoot)
            {
                return new List<string>(this.RoomInstances.Keys);
            }
        }

        /// <summary>
        /// Gathers debug information about the specified room (actors, peers, references etc.). 
        /// </summary>
        /// <param name="roomName">The room name.</param>
        /// <returns>A string with debug information.</returns>
        public virtual string GetDebugString(string roomName)
        {
            lock (this.SyncRoot)
            {
                RoomInstance roomInstance;
                if (!this.RoomInstances.TryGetValue(roomName, out roomInstance))
                {
                    return string.Format("RoomCache: No entry for room name {0}", roomName);
                }

                return string.Format("RoomCache: RoomInstance entry found for room {0}: {1}", roomName, roomInstance);
            }
        }

        /// <summary>
        /// Tries to create a new room.
        /// </summary>
        /// <param name="roomName">The real room name (not guid).</param>
        public bool TryCreateRoom(string roomName, PeerBase ownerPeer, out Room room, out RoomReference roomReference, params object[] args)
        {
            Guid guid = Guid.Empty;
            while (guid == Guid.Empty || this.RoomInstances.ContainsKey(guid.ToString()))
                guid = Guid.NewGuid();

            string guid_str = guid.ToString();
            room = this.CreateRoom(guid_str, args);
            room.Name = roomName;

            var roomInstance = new RoomInstance(this, room);
            this.RoomInstances.Add(guid_str, roomInstance);

            if (ownerPeer != null)
                roomReference = roomInstance.AddReference(ownerPeer);
            else
                roomReference = null;

            return true;
        }

        /// <summary>
        /// Tries to create a new default room.
        /// </summary>
        public bool TryCreateDefaultRoom(string roomName, PeerBase ownerPeer, out Room room, out RoomReference roomReference, params object[] args)
        {
            lock (this.SyncRoot)
            {
                if (TryCreateRoom(roomName, ownerPeer, out room, out roomReference, args))
                {
                    mDefaultRooms.Add(roomName, room.Guid);
                    return true;
                }
                return false;
            }
        }

        /// <summary>
        /// Tries to create a new realm room.
        /// </summary>
        public bool TryCreateRealmRoom(int realmId, string roomName, bool isWorld, PeerBase ownerPeer, out Room room,
                                       out RoomReference roomReference, params object[] args)
        {
            lock (this.SyncRoot)
            {
                if (TryCreateRoom(roomName, ownerPeer, out room, out roomReference, args))
                {
                    room.RealmID = realmId;
                    room.IsWorld = isWorld; // To differentiate World which is also a Realm
                    if (!mRealmRooms.ContainsKey(realmId))
                        mRealmRooms.Add(realmId, new List<string>());
                    mRealmRooms[realmId].Add(room.Guid);
                    return true;
                }
                return false;
            }
        }

        public bool TryGetRoomReference(string roomGuid, PeerBase ownerPeer, out RoomReference roomReference)
        {
            lock (this.SyncRoot)
            {
                RoomInstance roomInstance;
                if (!this.RoomInstances.TryGetValue(roomGuid, out roomInstance))
                {
                    roomReference = null;
                    return false;
                }

                roomReference = roomInstance.AddReference(ownerPeer);
                return true;
            }
        }

        public int TryGetRoomReferenceCount(string roomGuid)
        {
            lock (this.SyncRoot)
            {
                RoomInstance roomInstance;
                if (this.RoomInstances.TryGetValue(roomGuid, out roomInstance))
                    return roomInstance.ReferenceCount;
            }
            return -1;
        }

        [Obsolete("Use TryGetRealmRoomGuid instead")]
        public void TryGetRoomByID(int realmid, out Room room, int capacity)
        {
            lock (this.SyncRoot)
            {
                if (mRealmRooms.ContainsKey(realmid))
                {
                    foreach (string roomGuid in mRealmRooms[realmid])
                    {
                        RoomInstance roomInstance = RoomInstances[roomGuid];
                        if (capacity == 0)
                        {
                            room = roomInstance.Room;
                            return;
                        }
                        else if (roomInstance.SlotAssigned < capacity)
                        {
                            room = roomInstance.Room;
                            roomInstance.SlotAssigned++;
                            return;
                        }
                    }
                }
                room = null;
            }
        }

        public string TryGetRealmRoomGuid(int realmId, int capacity)
        {
            lock (this.SyncRoot)
            {
                List<string> roomGuids = null;
                if (mRealmRooms.TryGetValue(realmId, out roomGuids))
                {
                    int roomGuidCount = roomGuids.Count;
                    if (roomGuidCount > 0)
                    {
                        string roomGuid = roomGuids[0];
                        if (capacity == 0) // If no capacity limit, choose first in the list
                            return roomGuid;

                        // Search for lowest capacity room                        
                        int refCount = RoomInstances[roomGuid].ReferenceCount;
                        if (refCount >= capacity)
                            roomGuid = "";
                        for (int i = 1; i < roomGuidCount; ++i)
                        {
                            int roomCapacity = RoomInstances[roomGuids[i]].ReferenceCount;
                            if (roomCapacity < capacity && roomCapacity < refCount)
                            {
                                roomGuid = roomGuids[i];
                                refCount = roomCapacity;
                            }
                        }
                        return roomGuid;
                    }
                }
            }
            return "";
        }

        public string TryGetDefaultRoomGuid(string roomName)
        {
            string roomGuid = "";
            mDefaultRooms.TryGetValue(roomName, out roomGuid);
            return roomGuid;
        }

        public void ReleaseRoomReference(RoomReference roomReference)
        {
            RoomInstance roomInstance;

            lock (this.SyncRoot)
            {
                if (!this.RoomInstances.TryGetValue(roomReference.Room.Guid, out roomInstance))
                {
                    return;
                }

                roomInstance.ReleaseReference(roomReference);

                // if there are still references to the room left 
                // the room stays into the cache
                if (roomInstance.ReferenceCount > 0)
                {
                    return;
                }

                // ask the room implementation if the room should be removed automaticly from the cache
                // in certain case if last removed reference is due to disconnection, need to keep the room. 
                bool shouldRemoveRoom = roomInstance.Room.BeforeRemoveFromCache(roomReference.removeDueDc);
                if (!shouldRemoveRoom) // Remove room if true
                {
                    return;
                }
            }

            RemoveRoom(roomInstance.Room);
        }

        public void RemoveRoom(Room room)
        {
            if (log.IsDebugEnabled)
            {
                log.DebugFormat("Removed room instance: Level Name = {0}, Guid = {1}", room.Name, room.Guid);
            }

            lock (this.SyncRoot)
            {
                this.RoomInstances.Remove(room.Guid);
                List<string> roomGuidsByRealmId;
                if (mRealmRooms.TryGetValue(room.RealmID, out roomGuidsByRealmId))
                    roomGuidsByRealmId.Remove(room.Guid);
            }

            room.Dispose();
            this.OnRoomRemoved(room);
        }

        /// <summary>
        /// Tries to remove a romm instance from the room cache. 
        /// The room will only be removed if there are no references to the romm instance left.
        /// </summary>
        /// <param name="room">
        /// The room to remove.
        /// </param>
        /// <returns>
        /// Returns true if the room was removed from the cache; otherwise false.
        /// </returns>
        public bool TryRemoveRoomInstance(Room room)
        {
            RoomInstance roomInstance;

            lock (this.SyncRoot)
            {
                if (this.RoomInstances.TryGetValue(room.Guid, out roomInstance) == false)
                    return false;
                if (roomInstance.ReferenceCount > 0)
                    return false;
            }

            RemoveRoom(roomInstance.Room);
            return true;
        }

        /// <summary>
        /// Must be implementated by inheritors to create new room instances.
        /// This method is called when a room reference is requesteted for a
        /// room that does not exists in the cache.
        /// </summary>
        /// <param name="roomId">
        /// The room id.
        /// </param>
        /// <param name="args">
        /// Optionally arguments used for room creation.
        /// </param>
        /// <returns>
        /// a new room
        /// </returns>
        protected abstract Room CreateRoom(string roomId, params object[] args);

        /// <summary>
        /// Invoked if the last reference for a room is released and the room was removed from the cache. 
        /// Can be overloaded by inheritors to provide a custom cleanup logic after a room has been disposed. 
        /// </summary>
        /// <param name="room">The <see cref="Room"/> that was removed from the cache.</param>
        protected virtual void OnRoomRemoved(Room room)
        {
        }

        /// <summary>
        /// Used to track references for a room instance.
        /// </summary>
        public class RoomInstance
        {
            /// <summary>
            /// The references.
            /// </summary>
            private readonly Dictionary<Guid, RoomReference> references;

            /// <summary>
            /// The room factory.
            /// </summary>
            private readonly RoomCacheBase roomFactory;

            private readonly LogQueue logQueue;

            /// <summary>
            /// Initializes a new instance of the <see cref="RoomInstance"/> class.
            /// </summary>
            /// <param name="roomFactory">
            /// The room factory.
            /// </param>
            /// <param name="room">
            /// The room.
            /// </param>
            public RoomInstance(RoomCacheBase roomFactory, Room room)
            {
                this.roomFactory = roomFactory;
                this.Room = room;
                this.references = new Dictionary<Guid, RoomReference>();
                this.logQueue = new LogQueue("RoomInstance = " + room.Name + ", Guid " + room.Guid, LogQueue.DefaultCapacity);
            }

            /// <summary>
            /// Gets the number of references for the room instance.
            /// </summary>
            public int ReferenceCount
            {
                get
                {
                    return this.references.Count;
                }
            }

            /// <summary>
            /// Gets the room.
            /// </summary>
            public Room Room { get; private set; }

            public int SlotAssigned { get; set; }
            /// <summary>
            /// Adds a reference to the room instance.
            /// </summary>
            /// <param name="ownerPeer">
            /// The peer that holds this reference.
            /// </param>
            /// <returns>
            /// a new <see cref="RoomReference"/>
            /// </returns>
            public RoomReference AddReference(PeerBase ownerPeer)
            {
                var reference = new RoomReference(this.roomFactory, this.Room, ownerPeer);
                this.references.Add(reference.Id, reference);

                if (log.IsDebugEnabled)
                {
                    log.DebugFormat(
                        "Created room instance reference: roomName={0}, referenceCount={1}",
                        this.Room.Guid,
                        this.ReferenceCount);
                }

                if (this.logQueue.Log.IsDebugEnabled)
                {
                    this.logQueue.Add(
                        new LogEntry(
                            "AddReference",
                            string.Format(
                                "RoomName={0}, ReferenceCount={1}, OwnerPeer={2}",
                                this.Room.Guid,
                                this.ReferenceCount,
                                ownerPeer)));
                }

                return reference;
            }

            /// <summary>
            /// Releases a reference from this instance.
            /// </summary>
            /// <param name="reference">
            /// The room reference.
            /// </param>
            public void ReleaseReference(RoomReference reference)
            {
                this.references.Remove(reference.Id);

                if (log.IsDebugEnabled)
                {
                    log.DebugFormat(
                        "Removed room instance reference: roomName={0}, referenceCount={1}",
                        this.Room.Guid,
                        this.ReferenceCount);
                }

                if (this.logQueue.Log.IsDebugEnabled)
                {
                    this.logQueue.Add(
                        new LogEntry(
                            "ReleaseReference",
                            string.Format(
                                "RoomName={0}, ReferenceCount={1}, OwnerPeer={2}",
                                this.Room.Guid,
                                this.ReferenceCount,
                                reference.OwnerPeer)));
                }
            }

            public override string ToString()
            {
                var sb = new StringBuilder();
                sb.AppendFormat("RoomInstance for Room {0}: {1} References", this.Room.Guid, this.ReferenceCount).AppendLine();
                foreach (var reference in this.references)
                {
                    sb.AppendFormat("- Reference ID {0}, hold by Peer {1}", reference.Value.Id, reference.Value.OwnerPeer);
                    sb.AppendLine();
                }
                return sb.ToString();
            }
        }
    }
}