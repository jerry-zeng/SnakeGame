using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GamePlay
{
    [RequireComponent(typeof(SpriteRenderer))]
    public abstract class BaseView : MonoBehaviour 
    {
        protected Transform _tran;
        public Transform CachedTransform
        {
            get{ 
                if( _tran == null )
                    _tran = transform;
                return _tran;
            }
        }

        protected SpriteRenderer _render;


        protected virtual void Awake()
        {
            if( _render == null )
                _render = GetComponent<SpriteRenderer>();
        }

        public virtual void Unbind()
        {
            
        }

        public virtual void DoUpdate(float dt)
        {
            
        }

        protected Vector3 EntityPosToViewPos(Vector3 pos)
        {
            return BattleView.Instance.EntityToViewPos(pos);
        }
    }
}
