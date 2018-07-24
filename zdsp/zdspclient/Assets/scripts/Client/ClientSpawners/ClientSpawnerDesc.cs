using UnityEngine;
using System.Collections;
using Zealot.Common.Entities;
using Zealot.Spawners;

namespace Zealot.ClientSpawners
{    
    public abstract class ClientSpawnerDesc : ServerEntity
    {
        public abstract void Spawn(ClientEntitySystem es);
    }
}