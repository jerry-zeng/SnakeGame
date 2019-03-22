using System.Collections.Generic;

namespace Framework
{
    public class ObjectPool<T> where T : new()
    {
        public delegate void PoolHandle(T t);

        protected Stack<T> _stack = new Stack<T>();
        protected PoolHandle _onGet;
        protected PoolHandle _onRelease;

        public int Count
        { 
            get{ return _stack.Count; }
        }

        public ObjectPool( PoolHandle onGet = null, PoolHandle onRelease = null )
        {
            this._onGet = onGet;
            this._onRelease = onRelease;
        }

        public virtual T Get()
        {
            T value;
            if( _stack.Count == 0 )
                value = new T();
            else
                value = _stack.Pop();

            if( _onGet != null )
                _onGet.Invoke(value);

            return value;
        }

        public virtual void Release(T value)
        {
            if( System.Object.ReferenceEquals(_stack.Peek(), value) )
                return;

            if( _onRelease != null )
                _onRelease.Invoke( value );

            _stack.Push(value);
        }

        public virtual void Clear()
        {
            if( _onRelease != null )
            {
                foreach( T v in _stack )
                    _onRelease.Invoke(v);
            }

            _stack.Clear();
        }
    }
}
