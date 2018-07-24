using System;
using UnityEngine;

namespace Zealot.Common.RPC
{
    using Zealot.Common;

    public static class RPCDataConversion
    {
        public static short DirectionToYaw(Vector3 dir)
        {
            double yaw = GameUtils.DirectionToYaw(dir);
            ushort yawEncoded = (ushort) (yaw / (Math.PI * 2) * 0xFFFF);
            return unchecked( (short) yawEncoded);            
            //Convert to short because photonnetwork can only serialize short instead of ushort
        }

        public static Vector3 YawToDirection(short yawEncoded)
        {
            //ushort unsigned = unchecked((ushort)yawEncoded);
            double yradians = (double)yawEncoded / 0xFFFF * Math.PI * 2;
            return GameUtils.YawToDirection(yradians);
        }

        public static void PosToRelPos(Vector3 pos, out short x, out short y, out short z)
        {
            //We assume eligible scene position from (0,0) to (600,600) i.e. pos is relative to (0,0) origin of range 600,600
            //x and z range is [0..600]
            //while y range is [-300..300]
            ushort ux = (ushort)(pos.x * 100);            
            ushort uz = (ushort)(pos.z * 100);
            x = unchecked((short)ux);            
            z = unchecked((short)uz);
            y = (short)(pos.y * 100);
        }

        public static Vector3 RelPosToPos(short x, short y, short z)
        {
            ushort ux = unchecked((ushort)x);            
            ushort uz = unchecked((ushort)z);

            return new Vector3((float)ux/100.0f, (float)y/100.0f, (float)uz/100.0f);
        }
    }

    public class RPCPosition
    {
        private short x;
        public short X { get { return x; } }
        
        private short y;
        public short Y { get { return y; } }
        
        private short z;
        public short Z { get { return z; } }        

        public RPCPosition(Vector3 pos)
        {
            RPCDataConversion.PosToRelPos(pos, out x, out y, out z);
        }

        public RPCPosition(short x, short y, short z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }

        public Vector3 ToVector3()
        {
            return RPCDataConversion.RelPosToPos(x, y, z);
        }
    }

    public class RPCDirection
    {
        private short yawEncoded;
        public short YawEncodedPhoton { get { return yawEncoded; } } //Photon requires it in short
        public ushort YawEncoded { get{ return unchecked((ushort)yawEncoded); } }
        public double YawRadian { get { return (double)unchecked((ushort)yawEncoded) / 0xFFFF * Math.PI * 2; } }
        
        public RPCDirection(Vector3 dir)
        {
            yawEncoded = RPCDataConversion.DirectionToYaw(dir);
        }

        public RPCDirection(short yawEncoded)
        {
            this.yawEncoded = yawEncoded;
        }

        public Vector3 ToVector3()
        {
            return RPCDataConversion.YawToDirection(yawEncoded);
        }
    }

    //Extension methods for Vector3 to convert to RPC required objects
    public static class Vector3Extensions
    {
        public static RPCPosition ToRPCPosition(this Vector3 pos)
        {
            return new RPCPosition(pos);
        }

        public static RPCDirection ToRPCDirection(this Vector3 dir)
        {
            return new RPCDirection(dir);
        }
    }
}
