using Zealot.Common.Entities;
using Zealot.Spawners;

namespace Zealot.ClientSpawners
{    
    public abstract class ClientSpawnerBase : ServerEntity
    {
        public abstract void Spawn(ClientEntitySystem ces);
    }
}