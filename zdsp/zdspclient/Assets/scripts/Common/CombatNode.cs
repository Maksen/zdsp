using Kopio.JsonContracts;
using System;
using Zealot.Common.Entities;

namespace Zealot.Common {
    public class ConditionalNode {
        public bool condition;
        public DecisionNode trueNode, falseNode;

        public float Update() {
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
        public float result;
        
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
        protected Zealot.Common.CombatFormula.FIELDNAMEPACKET info;

        public virtual void InitInfo(Zealot.Common.CombatFormula.FIELDNAMEPACKET info) {
            this.info = info;
        }

        public override void Update() {
            //execute stuff here
        }
    }
}
