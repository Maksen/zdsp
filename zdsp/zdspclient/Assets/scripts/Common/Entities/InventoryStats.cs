using Kopio.JsonContracts;
using System.Collections.Generic;
using Zealot.Common.Datablock;
using Zealot.Repository;

namespace Zealot.Common.Entities
{
    public class RealmPartyDamageList : LocalObject
    {
        private int _health;
        private int _listSize = 10;
        public RealmPartyDamageList()
            : base(LOTYPE.RealmPartyDamageList)
        {
            _health = 0;
            this.names = new CollectionHandler<object>(_listSize);
            this.damages = new CollectionHandler<object>(_listSize);
            names.SetParent(this, "names");
            damages.SetParent(this, "damages");
        }

        public int health
        {
            get { return _health; }
            set { this.OnSetAttribute("health", value); _health = value; }
        }
        public CollectionHandler<object> names { get; set; }
        public CollectionHandler<object> damages { get; set; }

        public int GetEmptySlot()
        {
            for (int index = 0; index < _listSize; index++)
            {
                if (names[index] == null)
                    return index;
            }
            return -1;
        }

        public void AddMember(int index, string value)
        {
            names[index] = value;
            damages[index] = 0;
        }

        public void UpdateDamage(int index, int value)
        {
            damages[index] = value;
        }

        public void AddDamage(int index, int value)
        {
            int damage = (int)damages[index];
            damages[index] = damage + value;
        }
    }
    
}