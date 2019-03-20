using System.Collections;
using System.Collections.Generic;

namespace GamePlay
{
    public abstract class SnakerComponent 
    {
        protected Snaker _owner;

        public SnakerComponent(Snaker owner)
        {
            _owner = owner;
        }


        public virtual void Release()
        {
            _owner = null;
        }

        public virtual void EnterFrame(int frame)
        {
            if( _owner == null )
                return;
        }
    }
}
