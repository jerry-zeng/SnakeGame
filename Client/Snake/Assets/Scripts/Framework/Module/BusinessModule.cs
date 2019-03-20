using System.Collections;
using System.Collections.Generic;

namespace Framework.Module
{
    public abstract class BusinessModule : Module 
    {
        protected string _name = null;
        public override string Name
        {
            get
            {
                if( string.IsNullOrEmpty(_name) )
                    _name = GetType().Name;
                return _name;
            }
        }

        public BusinessModule(){  }

        internal BusinessModule(string name)
        {
            this._name = name;
        }

        // pair with Release()
        public virtual void Create(object arg = null)
        {
            
        }


        public virtual void Open(object arg = null)
        {
            
        }

        public virtual void Close()
        {

        }
    }
}
