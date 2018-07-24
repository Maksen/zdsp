using Zealot.Common.Entities;
using Zealot.Client.Entities;
using Zealot.Common.Actions;
using Zealot.Repository;

namespace Zealot.Client.Actions {
    public class BaseClientGetHit : Action {
        protected SkillData mSkillData;

        public BaseClientGetHit(Entity entity, GetHitCommand cmd) : base(entity, cmd) {
            mSkillData = SkillRepo.GetSkill(cmd.skillid);
        }

        protected override void OnActiveEnter(string prevstate) {
            ActorGhost ghost = (ActorGhost)mEntity;
            ghost.StartHitted(500);
            ghost.PlayEffect("", mSkillData.skillJson.name + "_gethit");
            
            if(ghost.IsMonster())
                ((MonsterGhost)ghost).Flash(); //flash on monster
            
            SetTimer(500, OnActiveTimeUp, null);
        }

        protected virtual void OnActiveTimeUp(object arg) {
            GotoState("Completed");
        }

        public override bool Update(Action newAction) {
            if (newAction.mdbCommand.GetActionType() == mdbCommand.GetActionType()) {
                mdbCommand = newAction.mdbCommand;
                GotoState("Active");
                return true;
            }
            return false;
        }
    }

    public class ClientAuthoACGetHit : BaseClientGetHit {

        public ClientAuthoACGetHit(Entity entity, GetHitCommand cmd) : base(entity, cmd) { }

        protected override void OnActiveEnter(string prevstate) {
            base.OnActiveEnter(prevstate);

            ActorGhost ghost = (ActorGhost)mEntity;
            ghost.SetActionDontSend(mdbCommand);
        }
    }

    public class NonClientAuthoACGetHit : BaseClientGetHit {
        public NonClientAuthoACGetHit(Entity entity, GetHitCommand cmd) : base(entity, cmd) { }
    }
}
