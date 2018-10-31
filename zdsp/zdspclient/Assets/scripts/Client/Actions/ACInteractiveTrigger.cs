using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;
using Zealot.Common.Entities;
using Zealot.Common.Actions;
using Zealot.Client.Entities;

namespace Zealot.Client.Actions
{
    public class BaseInteractiveTrigger : Zealot.Common.Actions.Action
    {
        protected int entityId;
        protected int triggerTime;
        protected int count;
        protected bool isArea;

        public BaseInteractiveTrigger(Entity entity, InteractiveTriggerCommand cmd) : base(entity, cmd)
        {
            entityId = cmd.entityId;
            triggerTime = cmd.triggerTime;
            count = cmd.count;
            isArea = cmd.isArea;
        }

        protected override void OnActiveEnter(string prevstate)
        {
            base.OnActiveEnter(prevstate);

            NetEntityGhost ghost = mEntity as NetEntityGhost;
            if (ghost == null)
                return;

            UIManager.SetWidgetActive(HUDWidgetType.ProgressBar, true);
            Hud_ProgressBar progressBar = UIManager.GetWidget(HUDWidgetType.ProgressBar).GetComponent<Hud_ProgressBar>();
            progressBar.InitTimeBar(triggerTime, CompeletedProgress);
        }

        void CompeletedProgress()
        {
            RPCFactory.CombatRPC.OnInteractiveTrigger(entityId);
            CloseProgressBar();
        }
        
        void CloseProgressBar()
        {
            if (UIManager.IsWidgetActived(HUDWidgetType.ProgressBar))
            {
                Hud_ProgressBar progressBar = UIManager.GetWidget(HUDWidgetType.ProgressBar).GetComponent<Hud_ProgressBar>();
                progressBar.ForceEnd();
            }
            UIManager.SetWidgetActive(HUDWidgetType.ProgressBar, false);
        }

        protected override void OnActiveLeave()
        {
            base.OnActiveLeave();

            NetEntityGhost ghost = mEntity as NetEntityGhost;
            if (ghost == null)
                return;
            //GameInfo.gLocalPlayer.InteractiveController.OnActionLeave();

            CloseProgressBar();
        }
    }

    public class ClientAuthoACInteractiveTrigger : BaseInteractiveTrigger
    {
        public ClientAuthoACInteractiveTrigger(Entity entity, InteractiveTriggerCommand cmd) : base(entity, cmd) { }

        protected override void OnActiveEnter(string prevstate)
        {
            base.OnActiveEnter(prevstate);

            NetEntityGhost ghost = (NetEntityGhost)mEntity;
            ghost.SetAction(mdbCommand);
        }
    }

    public class NonClientAuthoACInteractiveTrigger : BaseInteractiveTrigger
    {
        public NonClientAuthoACInteractiveTrigger(Entity entity, InteractiveTriggerCommand cmd) : base(entity, cmd) { }
    }
}
