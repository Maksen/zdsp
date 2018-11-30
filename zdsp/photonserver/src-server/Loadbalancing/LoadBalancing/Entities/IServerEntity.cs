using Zealot.Entities;

namespace Zealot.Server.Entities
{
    public interface IServerEntity
    {
        ServerEntityJson GetPropertyInfos();
        void InstanceStartUp();
    }
}
