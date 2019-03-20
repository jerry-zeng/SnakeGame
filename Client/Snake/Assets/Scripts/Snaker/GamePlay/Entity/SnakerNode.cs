using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GamePlay
{
    public class SnakerNode : Entity 
    {
        protected Snaker _owner;

        protected SnakerNodeView _view;
        public SnakerNodeView View
        {
            get{ return _view; }
            set{ _view = value; }
        }

        protected Dictionary<string, CSVValue> _data;
        public Dictionary<string, CSVValue> Data
        {
            get{ return _data; }
        }

        protected SnakerNode _nextNode;
        public SnakerNode NextNode
        {
            get{ return _nextNode; }
        }

        protected int _index = 0;
        public int Index
        {
            get{ return _index; }
        }

        public int TeamID
        {
            get{ return _owner.OwnerPlayer.teamID; }
        }


        public void InitData(int id, int index, Snaker owner, Vector3 initPos)
        {
            _id = id;
            _index = index;
            _owner = owner;
            _data = CSVTableLoader.GetTableContainer("SnakerNode").GetRow(_id.ToString());
            _radius = _data["Radius"].IntValue;

            _prePosition = _position = initPos;
        }

        public void Release()
        {
            _id = 0;
            _nextNode = null;
            _owner = null;
            _view = null;
        }

        public void SetNextNode(SnakerNode node)
        {
            _nextNode = node;
        }

        public void MoveTo(Vector3 newPos)
        {
            _prePosition = _position;

            _position = newPos;

            if( _nextNode != null )
                _nextNode.MoveTo(_prePosition);
        }

        public virtual bool IsKeyNode()
        {
            return _owner.IsKeyNode(_index);
        }

        public virtual void Blast()
        {

        }
    }
}
