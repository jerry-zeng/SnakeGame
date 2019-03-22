using UnityEngine;

namespace Framework
{
    public class GameObjectPool<T> : ObjectPool<T> where T : Object, new()
    {
        protected T _prefab;

        public GameObjectPool( T prefab, PoolHandle onGet = null, PoolHandle onRelease = null )
            : base(onGet, onRelease)
        {
            this._prefab = prefab;
        }

        public override T Get()
        {
            if( _prefab == null ){
                Debug.LogWarning("No object in pool, you'd better to set a prefab to enable pool instantiate auto.");
                return base.Get();
            }

            T value;
            if( _stack.Count == 0 )
                value = Object.Instantiate(_prefab);
            else
                value = _stack.Pop();

            if( _onGet != null )
                _onGet.Invoke(value);

            return value;
        }

        public override void Clear()
        {
            base.Clear();

            if( _prefab != null )
                Object.Destroy(_prefab);
            _prefab = null;
        }
    }
}
