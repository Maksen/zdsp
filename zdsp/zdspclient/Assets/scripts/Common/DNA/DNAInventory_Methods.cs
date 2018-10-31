using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using Zealot.Common.Entities;
using Zealot.Common;
using Kopio.JsonContracts;
using Zealot.Repository;

namespace Zealot.Common
{
    public partial class DNAInvData
    {
        public void InitFromInventory(DNAStats dnaStats)
        {
            for(int i = 0; i < DNASlots.Count; ++i)
            {
                dnaStats.dnaSlots[i] = DNASlots[i];
            }
        }

        public void InitFromStats(DNAStats dnaStats)
        {
            for(int i = 0; i < DNASlots.Count; ++i)
            {
                DNASlots[i] = dnaStats.dnaSlots[i] as DNAItemElement;
            }
        }

        public void UpdateInventory(DNAStats dnaStats)
        {
            InitFromStats(dnaStats);
        }

        public void SaveToInventory(DNAStats dnaStats)
        {
            InitFromStats(dnaStats);
        }

        public void SlotDNA(int dnaSlot, DNA dna)
        {
            if(dnaSlot < 0 || dnaSlot >= MAX_DNASLOTS)
            {
                // Invalid slot!
                return;
            }

            if(DNASlots[dnaSlot] != null)
            {
                // Swap DNA

                return;
            }

            DNAItemElement newDNA = new DNAItemElement();
            newDNA.DNAId = dna.ItemID;
            newDNA.DNAStage = dna.DNAStage;
            newDNA.DNALvl = dna.DNALevel;
            newDNA.DNAExp = dna.DNAExp;
        }

        public void SwapDNA(int dnaSlot, DNA dna)
        {

        }

        public void UnslotDNA(int dnaSlot)
        {

        }
    }
}
