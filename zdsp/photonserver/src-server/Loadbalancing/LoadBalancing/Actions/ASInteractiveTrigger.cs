namespace Zealot.Server.Actions
{
    using UnityEngine;
    using Zealot.Common;
    using Zealot.Common.Actions;
    using Zealot.Common.Entities;
    using Zealot.Server.Entities;

    public class ASInteractiveTrigger : Action
    {
        int entityId;
        int triggerTime;
        bool isArea;

        public ASInteractiveTrigger(Entity entity, ActionCommand cmd) : base(entity, cmd)
        {
            entityId = ((InteractiveTriggerCommand)cmd).entityId;
            triggerTime = ((InteractiveTriggerCommand)cmd).triggerTime;
            isArea = ((InteractiveTriggerCommand)cmd).isArea;
        }

        protected override void OnActiveEnter(string prevstate)
        {
            base.OnActiveEnter(prevstate);
            NetEntity entity = (NetEntity)mEntity;
            InteractiveTriggerCommand cmd = new InteractiveTriggerCommand();
            cmd.entityId = entityId;
            cmd.triggerTime = triggerTime;
            cmd.isArea = isArea;

            entity.SetAction(cmd);
        }
    }
}
