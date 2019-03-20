using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Framework;

namespace GamePlay
{
    public class SnakerActor : BaseView 
    {
        protected Snaker _model;
        public Snaker Model
        {
            get{ return _model; }
        }

        protected SnakerNodeView _head;
        protected SnakerNodeView _tail;
        protected List<SnakerNodeView> _nodeList = new List<SnakerNodeView>();

        public void Bind(Snaker model)
        {
            _model = model;

            _nodeList.Clear();

            _head = CreateNode( _model.Head );
            _head.CachedTransform.SetParent(CachedTransform);

            _tail = CreateNode( _model.Tail );
            _tail.CachedTransform.SetParent(CachedTransform);

            SyncNodeList();

            DoUpdate(0f);
        }

        public override void Unbind()
        {
            base.Unbind();

            _model = null;

            DestroyNode(_head);
            DestroyNode(_tail);

            for(int i = 0; i < _nodeList.Count; i++)
            {
                DestroyNode(_nodeList[i]);
            }
            _nodeList.Clear();
        }

        void SyncNodeList()
        {
            if( _nodeList.Count != _model.GetKeyNodeCount() )
            {
                SnakerNode node = _model.Head.NextNode;

                while( node != null)
                {
                    if(node != _model.Tail && node.View == null && node.IsKeyNode())
                    {
                        SnakerNodeView _node = CreateNode(node);
                        _node.CachedTransform.SetParent(CachedTransform);
                        node.View = _node;

                        _nodeList.Add(_node);
                    }

                    node = node.NextNode;
                }
            }

        }

        SnakerNodeView CreateNode(SnakerNode node)
        {
            GameObject prefab = Resources.Load<GameObject>( node.Data["Prefab"].StringValue );
            GameObject go = Instantiate<GameObject>(prefab);
            go.name = prefab.name;
            go.transform.SetParent(CachedTransform);

            SnakerNodeView result = go.EnsureComponent<SnakerNodeView>();
            result.Bind(node);

            return result;
        }

        void DestroyNode(SnakerNodeView node)
        {
            if( node != null ){
                node.Unbind();
                Destroy(node.gameObject);
            }
        }

        public override void DoUpdate(float dt)
        {
            base.DoUpdate(dt);

            if( _model == null )
                return;

            if(_head != null)
                _head.DoUpdate(dt);

            SyncNodeList();

            for(int i = 0; i < _nodeList.Count; i++)
            {
                _nodeList[i].DoUpdate(dt);
            }

            if(_tail != null)
                _tail.DoUpdate(dt);
        }
    }
}
