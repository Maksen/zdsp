using Kopio.JsonContracts;
using System;
using Zealot.Common.Entities;

namespace Photon.LoadBalancing.GameServer.CombatFormula
{
    public class ConditionalNode {
        public bool condition;
        public DecisionNode trueNode, falseNode;

        public double Update() {
            if (condition) {
                trueNode.Update();
                return trueNode.result;
            }
            else {
                falseNode.Update();
                return falseNode.result;
            }
        }
    }

    public class DecisionNode {
        public ConditionalNode condition;
        public DecisionNode parent;
        public double result;
        
        public DecisionNode() { condition = new ConditionalNode(); }
        public virtual void SetConditionalStatement(Func<bool> evaluator) {
            condition.condition = evaluator();
        }

        public void SetTrueNode(DecisionNode node) { condition.trueNode = node; node.parent = this; }
        public void SetFalseNode(DecisionNode node) { condition.falseNode = node; node.parent = this; }

        public virtual void Update() {
            // execute stuff here
            result = condition.Update();
        }
    }

    public class LeafNode : DecisionNode {
        protected CombatFormula.FIELDNAMEPACKET info;

        public virtual void InitInfo(CombatFormula.FIELDNAMEPACKET info) {
            this.info = info;
        }

        public override void Update() {
            //execute stuff here
        }
    }
}
