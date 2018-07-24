using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Zealot.Server.Entities
{
    /// <summary>
    /// this is the class which update actor control stats , 
    /// any skill or sideeffect applied will modify this,  and it keep tracks of all 
    /// the latest status.  Local Object value is set corrspondingly after this changes.
    /// </summary>
    public class ControlStats
    {
        public int Rootnum { get; set; }
        public int Stunnum { get; set; }
        public int Slownum { get; set; }
        public int Silencenum { get; set; }
        public int Disarmnum { get; set; }

        public bool Stuned {
            get { return Stunnum > 0; } }

        public bool Silenced
        {
            get { return Silencenum > 0; }
        }

        public bool Disarmed
        {
            get { return Silencenum > 0; }
        }
        public bool Slowed
        {
            get { return Slownum > 0; }
        }

        public bool Rooted
        {
            get { return Rootnum > 0; }
        }

        //immune only use flag, so make sure at one time only one immune is supported  . 
        public bool StunImmuned { get; set; }
        public bool RootImmuned { get; set; }
        public bool DisarmImmuned { get; set; } //todo: this sideeffect type have not added
        public bool SlowImmuned { get; set; }
        public bool SilenceImmuned { get; set; } 
    }
}
