using Kopio.JsonContracts;
using System.Collections.Generic;
using Zealot.Common;

namespace Zealot.Repository
{ 
    class ElementalChartRepo
    {
        public static Dictionary<Element, Dictionary<Element, int>> elementChart = new Dictionary<Element, Dictionary<Element, int>>();
        public static Dictionary<AttackStyle, Dictionary<AttackStyle, int>> weaknessChart = new Dictionary<AttackStyle, Dictionary<AttackStyle, int>>();

        public static void Init(GameDBRepo gameData)
        {    
            foreach(KeyValuePair<int, ElementChartJson> kvp in gameData.ElementChart) {
                elementChart.Add(kvp.Value.elementid, new Dictionary<Element, int>());
                elementChart[kvp.Value.elementid].Add(Element.None, kvp.Value.none);
                elementChart[kvp.Value.elementid].Add(Element.Metal, kvp.Value.metal);
                elementChart[kvp.Value.elementid].Add(Element.Wood, kvp.Value.wood);
                elementChart[kvp.Value.elementid].Add(Element.Earth, kvp.Value.earth);
                elementChart[kvp.Value.elementid].Add(Element.Water, kvp.Value.water);
                elementChart[kvp.Value.elementid].Add(Element.Fire, kvp.Value.fire);
            }

            foreach(KeyValuePair<int, WeaknessChartJson> kvp in gameData.WeaknessChart) {
                weaknessChart.Add(kvp.Value.attacktype, new Dictionary<AttackStyle, int>());
                weaknessChart[kvp.Value.attacktype].Add(AttackStyle.Slice, kvp.Value.slice);
                weaknessChart[kvp.Value.attacktype].Add(AttackStyle.Pierce, kvp.Value.pierce);
                weaknessChart[kvp.Value.attacktype].Add(AttackStyle.Smash, kvp.Value.smash);
                weaknessChart[kvp.Value.attacktype].Add(AttackStyle.God, kvp.Value.god);
                weaknessChart[kvp.Value.attacktype].Add(AttackStyle.Normal, kvp.Value.normal);
            }
        }

        public static float ElementChartQuery(Element attacker, Element defender)
        {
            return elementChart[attacker][defender] * 0.01f;
        }

        public static float WeaknessChartQuery(AttackStyle attacker, AttackStyle defender)
        {
            return weaknessChart[attacker][defender] * 0.01f;
        }
    }
}
