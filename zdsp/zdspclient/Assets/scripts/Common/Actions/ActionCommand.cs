using System;
using UnityEngine;
using System.Reflection;
using System.Collections.Generic;

namespace Zealot.Common.Actions
{
    using System.Linq;
    using Zealot.Common.RPC;

    public enum ACTIONTYPE : byte
    {
        IDLE,
        WALK,
        CASTSKILL,
        Flash,
        DEAD,
        SNAPSHOTUPDATE,
        APPROACH,
        APPROACH_PATHFIND,
        WALK_WAYPOINT,
        INTERACT,
        WALKANDCAST,
        DASHATTACK,
        KNOCKEDBACK,
        KNOCKEDUP,
        DRAGGED,
        FROZEN,
        GETHIT,
        SUMMON,
    }

    public static class ActionManager
    {
        private static Dictionary<ACTIONTYPE, Type> mActionCmds;
        private static Dictionary<ACTIONTYPE, Type> mActions; //non authoritative side actions

        static ActionManager()
        {
            mActionCmds = new Dictionary<ACTIONTYPE, Type>();
            mActions = new Dictionary<ACTIONTYPE, Type>();
        }

        public static ActionCommand CreateNewActionCmd(ACTIONTYPE code)
        {
            if (code == ACTIONTYPE.SNAPSHOTUPDATE)
                return new SnapShotUpdateCommand();
            if (code == ACTIONTYPE.IDLE)
                return new IdleActionCommand();
            if (code == ACTIONTYPE.WALK)
                return new WalkActionCommand();
            if (code == ACTIONTYPE.WALK_WAYPOINT)
                return new WalkToWaypointActionCommand();
            if (code == ACTIONTYPE.CASTSKILL)
                return new CastSkillCommand();
            if (code == ACTIONTYPE.Flash)
                return new FlashActionCommand();
            if (code == ACTIONTYPE.DEAD)
                return new DeadActionCommand();
            if (code == ACTIONTYPE.APPROACH)
                return new ApproachCommand();
            if (code == ACTIONTYPE.APPROACH_PATHFIND)
                return new ApproachWithPathFindCommand();
            if (code == ACTIONTYPE.WALKANDCAST)
                return new WalkAndCastCommand();
            if (code == ACTIONTYPE.INTERACT)
                return new InteractCommand();
            if (code == ACTIONTYPE.DASHATTACK)
                return new DashAttackCommand();
            if (code == ACTIONTYPE.DRAGGED)
                return new DraggedActionCommand();
            if (code == ACTIONTYPE.GETHIT)
                return new GetHitCommand();
            if (code == ACTIONTYPE.SUMMON)
                return new SummonCommand();
            if (code == ACTIONTYPE.FROZEN)
                return new FrozenActionCommand();
            return null;
        }

        public static void RegisterAction(ACTIONTYPE actionType, Type cmd, Type nonauthoAction)
        {
            if (mActions.ContainsKey(actionType))
                return;
            if (nonauthoAction != null)
                mActions.Add(actionType, nonauthoAction);
            mActionCmds.Add(actionType, cmd);
            ActionCommand.InitFieldInfos(cmd);
        }

        public static Type GetActionCommandType(ACTIONTYPE code)
        {
            return mActionCmds[code];
        }

        public static Type GetAction(ACTIONTYPE code)
        {
            Type type;
            if (mActions.TryGetValue(code, out type))
                return type;
            else
                return null; //some authoriatative actions do not have a corresponding non authoritative action
        }
    }


    public interface IActionCommand
    {
         
    }

    public abstract class ActionCommand: IActionCommand
    {
        static Dictionary<Type, FieldInfo[]> commandFields = new Dictionary<Type, FieldInfo[]>();
        static public void InitFieldInfos(Type t)
        {
            if (!commandFields.ContainsKey(t))
            {
                IEnumerable<FieldInfo> orderedFields = t.GetFields()
                                                 .OrderBy(field => field.MetadataToken);

                commandFields.Add(t, orderedFields.ToArray());
            }
        }

        protected ACTIONTYPE mActionType;
        private const byte PCODE = 0;

        public ActionCommand(ACTIONTYPE actiontype)
        {
            mActionType = actiontype;
        }

        public ACTIONTYPE GetActionType()
        {
            return mActionType;
        }

        public void SerializeRelVector3(Vector3 val, ref byte pcode, ref Dictionary<byte, object> dic)
        {
            short x, y, z;
            RPCDataConversion.PosToRelPos(val, out x, out y, out z);
            dic.Add(pcode++, x);
            dic.Add(pcode++, y);
            dic.Add(pcode++, z);
        }

        public Vector3 DeserializeRelVector3(Dictionary<byte, object> dic, ref byte pcode)
        {
            short x = (short)dic[pcode++];
            short y = (short)dic[pcode++];
            short z = (short)dic[pcode++];
            return RPCDataConversion.RelPosToPos(x, y, z);
        }

        // multip commands serialize
        public virtual bool SerializeStream(int persid, ref byte pcode, ref Dictionary<byte, object> dic)
        {
            Type t = this.GetType();
            var finfos = commandFields[GetType()];

            int vcnt = 0;
            foreach (FieldInfo f in finfos)
            {
                if (f.GetValue(this) is Vector3)
                {
                    RPCFieldUsageAttribute att = (RPCFieldUsageAttribute)Attribute.GetCustomAttribute(f, typeof(RPCFieldUsageAttribute));
                    if (att != null && att.UsageType == RPCUsageType.Direction) //Direction Vector3 will be converted to 1 entry in dictionary
                        continue;

                    vcnt++;
                }
            }

            // to use known length when created 
            if ((finfos.Length - vcnt + (vcnt * 3) + pcode + 2) > 255)
                return false;

            dic.Add(pcode++, persid);
            dic.Add(pcode++, mActionType);

            foreach (FieldInfo f in finfos)
            {
                if (f.GetValue(this) is Vector3)
                {
                    RPCFieldUsageAttribute att = (RPCFieldUsageAttribute)Attribute.GetCustomAttribute(f, typeof(RPCFieldUsageAttribute));
                    Vector3 v = (Vector3)f.GetValue(this);
                    if (att == null || att.UsageType == RPCUsageType.Default)
                    {
                        dic.Add(pcode++, v.x);
                        dic.Add(pcode++, v.y);
                        dic.Add(pcode++, v.z);
                    }
                    else
                    {
                        if (att.UsageType == RPCUsageType.Direction)
                        {
                            short encoded = RPCDataConversion.DirectionToYaw(v);
                            dic.Add(pcode++, encoded);
                        }
                        else if (att.UsageType == RPCUsageType.RelativePosition)
                        {
                            short x, y, z;
                            RPCDataConversion.PosToRelPos(v, out x, out y, out z);
                            dic.Add(pcode++, x);
                            dic.Add(pcode++, y);
                            dic.Add(pcode++, z);
                        }
                    }
                }
                else
                    dic.Add(pcode++, f.GetValue(this));
            }
            return true;
        }

        // single action command serialize
        public Dictionary<byte, object> Serialize(int persid)
        {
            byte pcode = PCODE;
            Dictionary<byte, object> dic = new Dictionary<byte, object>();
            dic.Add(pcode++, 0);
            bool ret = SerializeStream(persid, ref pcode, ref dic);

            return dic;
        }

        public virtual void Deserialize(Dictionary<byte, object> dic, ref byte pcode)
        {
            //byte pcode = PCODE + 2;
            Type t = this.GetType();
            var finfos = commandFields[GetType()];

            foreach (FieldInfo f in finfos)
            {
                if (f.GetValue(this) is Vector3)
                {
                    RPCFieldUsageAttribute att = (RPCFieldUsageAttribute)Attribute.GetCustomAttribute(f, typeof(RPCFieldUsageAttribute));
                    Vector3 v;
                    if (att == null || att.UsageType == RPCUsageType.Default)
                    {
                        v = new Vector3();
                        v.x = (float)dic[pcode++];
                        v.y = (float)dic[pcode++];
                        v.z = (float)dic[pcode++];
                        f.SetValue(this, v);
                    }
                    else
                    {
                        if (att.UsageType == RPCUsageType.Direction)
                        {
                            v = RPCDataConversion.YawToDirection((short)dic[pcode++]);
                            f.SetValue(this, v);
                        }
                        else if (att.UsageType == RPCUsageType.RelativePosition)
                        {
                            short x = (short)dic[pcode++];
                            short y = (short)dic[pcode++];
                            short z = (short)dic[pcode++];
                            v = RPCDataConversion.RelPosToPos(x, y, z);
                            f.SetValue(this, v);
                        }
                    }
                }
                else
                    f.SetValue(this, dic[pcode++]);
            }
        }
    }


    //------------------------------------------------------------------------------
    public class IdleActionCommand : ActionCommand
    {
        public IdleActionCommand() : base(ACTIONTYPE.IDLE) { }

        public override bool SerializeStream(int persid, ref byte pcode, ref Dictionary<byte, object> dic)
        {
            dic.Add(pcode++, persid);
            dic.Add(pcode++, mActionType);
            return true;
        }

        public override void Deserialize(Dictionary<byte, object> dic, ref byte pcode)
        {
        }
    }

    public class WalkActionCommand : ActionCommand
    {
        [RPCFieldUsage(RPCUsageType.RelativePosition)]
        public Vector3 targetPos;

        public float speed = 0f; //0 means use playersynstats speed.

        public WalkActionCommand() : base(ACTIONTYPE.WALK) { }

        public override bool SerializeStream(int persid, ref byte pcode, ref Dictionary<byte, object> dic)
        {
            dic.Add(pcode++, persid);
            dic.Add(pcode++, mActionType);
            SerializeRelVector3(targetPos, ref pcode, ref dic);
            dic.Add(pcode++, speed);
            return true;
        }

        public override void Deserialize(Dictionary<byte, object> dic, ref byte pcode)
        {
            targetPos = DeserializeRelVector3(dic, ref pcode);
            speed = (float)dic[pcode++];
        }
    }

    public class WalkToWaypointActionCommand : ActionCommand
    {
        [RPCFieldUsage(RPCUsageType.RelativePosition)]
        public Vector3 waypoint;

        [RPCFieldUsage(RPCUsageType.RelativePosition)]
        public Vector3 waypoint2;

        public WalkToWaypointActionCommand() : base(ACTIONTYPE.WALK_WAYPOINT) { }

        public override bool SerializeStream(int persid, ref byte pcode, ref Dictionary<byte, object> dic)
        {
            dic.Add(pcode++, persid);
            dic.Add(pcode++, mActionType);
            SerializeRelVector3(waypoint, ref pcode, ref dic);
            SerializeRelVector3(waypoint2, ref pcode, ref dic);
            return true;
        }

        public override void Deserialize(Dictionary<byte, object> dic, ref byte pcode)
        {
            waypoint = DeserializeRelVector3(dic, ref pcode);
            waypoint2 = DeserializeRelVector3(dic, ref pcode);
        }
    }

    public interface ISkillCastCommandCommon:IActionCommand
    {
        int GetSkillID();
        int GetTargetID();

        bool IsDashed();

        int GetFeedbackIndex();
         

        int GetSkillLevel();

    }

    public class CastSkillCommand : ActionCommand, ISkillCastCommandCommon
    {
        public int skillid; //this is the skillgorup id./ 
        public int targetpid = 0; 
        public int feedbackindex = 2;//defeault
        public bool dashed = false; 
        public int skilllevel = 1;
        //public bool ultimate = false;

        [RPCFieldUsage(RPCUsageType.RelativePosition)]
        public Vector3 targetPos = Vector3.zero;

        public CastSkillCommand() : base(ACTIONTYPE.CASTSKILL) { }
        public CastSkillCommand(ACTIONTYPE type) : base(type) { }

        public override bool SerializeStream(int persid, ref byte pcode, ref Dictionary<byte, object> dic)
        {
            dic.Add(pcode++, persid);
            dic.Add(pcode++, mActionType);
            dic.Add(pcode++, skillid); 
            dic.Add(pcode++, targetpid);
            dic.Add(pcode++, feedbackindex);
            dic.Add(pcode++, dashed); 
            dic.Add(pcode++, skilllevel);
            SerializeRelVector3(targetPos, ref pcode, ref dic);
            return true;
        }

        public override void Deserialize(Dictionary<byte, object> dic, ref byte pcode)
        {
            skillid = (int)dic[pcode++]; 
            targetpid = (int)dic[pcode++];
            feedbackindex = (int)dic[pcode++];
            dashed = (bool)dic[pcode++]; 
            skilllevel = (int)dic[pcode++];
            targetPos = DeserializeRelVector3(dic, ref pcode);
        }

        public int GetSkillID()
        {
            return skillid;
        }

        public int GetTargetID()
        {
            return targetpid;
        }

        public bool IsDashed()
        {
            return dashed;
        }

        public int GetFeedbackIndex()
        {
            return feedbackindex;
        }

        public int GetSkillLevel()
        {
            return skilllevel;
        }
    }

    public class DraggedActionCommand:ActionCommand
    {
        [RPCFieldUsage(RPCUsageType.RelativePosition)]
        public Vector3 pos;

        public float dur = 0.4f;
        public float speed = 10f;
        public DraggedActionCommand():base(ACTIONTYPE.DRAGGED) { }

        public override bool SerializeStream(int persid, ref byte pcode, ref Dictionary<byte, object> dic)
        { 
            dic.Add(pcode++, persid);
            dic.Add(pcode++, mActionType); 
            SerializeRelVector3(pos, ref pcode, ref dic);
            dic.Add(pcode++, dur);
            dic.Add(pcode++, speed);
            return true;
        }

        public override void Deserialize(Dictionary<byte, object> dic, ref byte pcode)
        {
            pos = DeserializeRelVector3(dic, ref pcode);
            dur = (float)dic[pcode++];
            speed = (float)dic[pcode++];
        }
    }

    public class FrozenActionCommand : ActionCommand
    {
        public float dur;

        public FrozenActionCommand() : base(ACTIONTYPE.FROZEN) { }
        public override bool SerializeStream(int persid, ref byte pcode, ref Dictionary<byte, object> dic)
        {
            dic.Add(pcode++, persid);
            dic.Add(pcode++, mActionType);
            dic.Add(pcode++, dur);
            return true;
        }
        public override void Deserialize(Dictionary<byte, object> dic, ref byte pcode)
        {
            dur = (float)dic[pcode++];
        }
    }

    public class FlashActionCommand : ActionCommand
    {
        public float dur;
        public FlashActionCommand() : base(ACTIONTYPE.Flash) { }

        public override bool SerializeStream(int persid, ref byte pcode, ref Dictionary<byte, object> dic)
        {
            dic.Add(pcode++, persid);
            dic.Add(pcode++, mActionType);
            dic.Add(pcode++, dur);
            return true;
        }

        public override void Deserialize(Dictionary<byte, object> dic, ref byte pcode)
        {
            dur = (float)dic[pcode++];
        }
    }

    public class DeadActionCommand : ActionCommand
    {
        public DeadActionCommand() : base(ACTIONTYPE.DEAD) { }

        public override bool SerializeStream(int persid, ref byte pcode, ref Dictionary<byte, object> dic)
        {
            dic.Add(pcode++, persid);
            dic.Add(pcode++, mActionType);
            return true;
        }

        public override void Deserialize(Dictionary<byte, object> dic, ref byte pcode)
        {
        }
    }

    public class SnapShotUpdateCommand : ActionCommand
    {
        [RPCFieldUsage(RPCUsageType.RelativePosition)]
        public Vector3 pos;

        [RPCFieldUsage(RPCUsageType.Direction)]
        public Vector3 forward;

        public SnapShotUpdateCommand() : base(ACTIONTYPE.SNAPSHOTUPDATE) { }

        public override bool SerializeStream(int persid, ref byte pcode, ref Dictionary<byte, object> dic)
        {
            dic.Add(pcode++, persid);
            dic.Add(pcode++, mActionType);
            SerializeRelVector3(pos, ref pcode, ref dic);
            short encoded = RPCDataConversion.DirectionToYaw(forward);
            dic.Add(pcode++, encoded);
            return true;
        }

        public override void Deserialize(Dictionary<byte, object> dic, ref byte pcode)
        {
            pos = DeserializeRelVector3(dic, ref pcode);
            forward = RPCDataConversion.YawToDirection((short)dic[pcode++]);
        }
    }

    public class ApproachCommand : ActionCommand
    {
        [RPCFieldUsage(RPCUsageType.RelativePosition)]
        public Vector3 targetpos;

        public int targetpid = -1;

        public float range = 0.4f;

        public ApproachCommand() : base(ACTIONTYPE.APPROACH) { }

        public override bool SerializeStream(int persid, ref byte pcode, ref Dictionary<byte, object> dic)
        {
            dic.Add(pcode++, persid);
            dic.Add(pcode++, mActionType);
            SerializeRelVector3(targetpos, ref pcode, ref dic);
            dic.Add(pcode++, targetpid);
            dic.Add(pcode++, range);
            return true;
        }

        public override void Deserialize(Dictionary<byte, object> dic, ref byte pcode)
        {
            targetpos = DeserializeRelVector3(dic, ref pcode);
            targetpid = (int)dic[pcode++];
            range = (float)dic[pcode++];
        }
    }

    // aiklong: the following action command is not sent to the client. so, don't need special serialize, deserialize, targetpos could be nullable
    public class ApproachWithPathFindCommand : ActionCommand
    {
        public Vector3? targetpos;
        public int targetpid = -1;
        public float range = 0.4f;
        public bool targetposSafe = true; //targetpos is assumed to be 100% reachable
        public bool movedirectonpathfound = false; //whether to move straight to target pos after path is found
        public float speed = 0;
        public ApproachWithPathFindCommand() : base(ACTIONTYPE.APPROACH_PATHFIND) { }
    }

    public class DashAttackCommand : ActionCommand, ISkillCastCommandCommon
    {
        [RPCFieldUsage(RPCUsageType.RelativePosition)]
        public Vector3 targetpos;

         
        public float range = 0.4f;

        public float dashduration = 0.4f;//notused,as dash duration is skillduration
        public int skillid = 0;
        public int targetpid = 0;
        public int skilllevel = 1;
        public DashAttackCommand() : base(ACTIONTYPE.DASHATTACK) { }

        public override bool SerializeStream(int persid, ref byte pcode, ref Dictionary<byte, object> dic)
        {
            dic.Add(pcode++, persid);
            dic.Add(pcode++, mActionType);
            SerializeRelVector3(targetpos, ref pcode, ref dic); 
            dic.Add(pcode++, range);
            dic.Add(pcode++, dashduration);
            dic.Add(pcode++, skillid);
            dic.Add(pcode++, targetpid);
            dic.Add(pcode++, skilllevel);
            return true;
        }

        public override void Deserialize(Dictionary<byte, object> dic, ref byte pcode)
        {
            targetpos = DeserializeRelVector3(dic, ref pcode); 
            range = (float)dic[pcode++];
            dashduration = (float)dic[pcode++];
            skillid = (int)dic[pcode++];
            targetpid = (int)dic[pcode++];
            skilllevel = (int)dic[pcode++];
        }

        public int GetSkillID()
        {
            return skillid;
        }

        public int GetTargetID()
        {
            return targetpid;
        }

        public bool IsDashed()
        {
            return false;
        }

        public int GetFeedbackIndex()
        {
            return 2;
        }

        public bool IsCompoundSkill()
        {
            return false;
        }

        public int GetCompoundSkillIndex()
        {
            return -1;
        }

        public int GetSkillLevel()
        {
            return skilllevel;
        }
    }


    public class InteractCommand : ActionCommand
    {
        public int chargetime = 1000; //miliseconds
        public int targetpid = -1;
        public InteractCommand() : base(ACTIONTYPE.INTERACT) { }

        public override bool SerializeStream(int persid, ref byte pcode, ref Dictionary<byte, object> dic)
        {
            dic.Add(pcode++, persid);
            dic.Add(pcode++, mActionType);
            dic.Add(pcode++, chargetime);
            dic.Add(pcode++, targetpid);
            return true;
        }

        public override void Deserialize(Dictionary<byte, object> dic, ref byte pcode)
        {
            chargetime = (int)dic[pcode++];
            targetpid = (int)dic[pcode++];
        }
    }

    public class WalkAndCastCommand : CastSkillCommand
    {
        public WalkAndCastCommand() : base(ACTIONTYPE.WALKANDCAST) { }
    }

    public class KnockedBackCommand : ActionCommand
    {
        [RPCFieldUsage(RPCUsageType.RelativePosition)]
        public Vector3 targetpos;
        public KnockedBackCommand() : base(ACTIONTYPE.KNOCKEDBACK) { }
        public override bool SerializeStream(int persid, ref byte pcode, ref Dictionary<byte, object> dic)
        {
            dic.Add(pcode++, persid);
            dic.Add(pcode++, mActionType);
            SerializeRelVector3(targetpos, ref pcode, ref dic);
            return true;
        }

        public override void Deserialize(Dictionary<byte, object> dic, ref byte pcode)
        {
            targetpos = DeserializeRelVector3(dic, ref pcode);
        }
    }

    public class KnockedUpCommand : ActionCommand
    {
        public float dur = 0;
        public KnockedUpCommand() : base(ACTIONTYPE.KNOCKEDUP) { }

        public override bool SerializeStream(int persid, ref byte pcode, ref Dictionary<byte, object> dic)
        {
            dic.Add(pcode++, persid);
            dic.Add(pcode++, mActionType);
            dic.Add(pcode++, dur);
            return true;
        }

        public override void Deserialize(Dictionary<byte, object> dic, ref byte pcode)
        {
            dur = (float)dic[pcode++];//this must match with serializestream to work correctly;althought this value is not used at clientside
        }
    }

    public class GetHitCommand : ActionCommand
    {
        public int skillid; //this is the skillgorup id./
        public GetHitCommand():base(ACTIONTYPE.GETHIT) { }

        public override bool SerializeStream(int persid, ref byte pcode, ref Dictionary<byte, object> dic) {
            dic.Add(pcode++, persid);
            dic.Add(pcode++, mActionType);
            dic.Add(pcode++, skillid);
            return true;
        }

        public override void Deserialize(Dictionary<byte, object> dic, ref byte pcode) {
            skillid = (int)dic[pcode++];
        }
    }

    //junming: the following action command is only used at client so don't need serialize, deserialize,
    public class SummonCommand : ActionCommand
    {
        public SummonCommand() : base(ACTIONTYPE.SUMMON) { }
    }
}
