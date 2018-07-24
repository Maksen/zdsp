using System;
using System.Collections.Generic;
using Zealot.Entities;
using Zealot.Common;

namespace Zealot.Server.Entities
{
    public interface IServerEntity
    {
        ServerEntityJson GetPropertyInfos();
        void InstanceStartUp();
    }
}
