using Zealot.Common.Actions;

namespace Zealot.Common.Entities
{
    public interface IBaseNetEntity //Can be either movable/actable NetEntity or StaticNetEntity
    {
        int GetPersistentID();
        void SetPersistentID(int pid);
        void SetOwnerID(int id);
        int GetOwnerID();        
    }

    public interface INetEntity //Movable, able to act
    {
        bool PerformAction(Action action, bool force = false, bool queue = false);
        Action GetAction();
        void SetAction(ActionCommand cmd);
        ActionCommand GetActionCmd();
    }

    public interface IStaticNetEntity //Unable to move or act. Can only have state changes
    {                
        void GotoState(string stateName);
    }
}
