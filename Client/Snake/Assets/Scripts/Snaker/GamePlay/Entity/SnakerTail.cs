using System.Collections;
using System.Collections.Generic;

namespace GamePlay
{
    public class SnakerTail : SnakerNode 
    {
        protected SnakerNode prevNode;
        public SnakerNode PrevNode
        {
            get{ return prevNode; }
        }

        public override bool IsKeyNode()
        {
            return true;
        }

        public void SetPrevNode(SnakerNode node)
        {
            prevNode = node;
        }
    }
}
