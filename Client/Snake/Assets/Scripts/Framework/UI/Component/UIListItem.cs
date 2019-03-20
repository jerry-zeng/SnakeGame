using UnityEngine;
using UnityEngine.Events;

namespace Framework.UI
{
    public class UIListItem : MonoBehaviour
    {
        public UnityAction<UIListItem,int,object> onRefresh;

        protected GameObject _go;
        public GameObject CachedGameObject
        {
            get{ 
                if( _go == null )
                    _go = this.gameObject;
                return _go;
            }
        }

        protected RectTransform _tran;
        public RectTransform CachedRectTransform
        {
            get{ 
                if( _tran == null )
                    _tran = this.transform as RectTransform;
                return _tran;
            }
        }


        public virtual void SetVisible(bool value)
        {
            CachedGameObject.SetActive(value);
        }

        public bool IsVisible { get { return CachedGameObject.activeSelf;} }


        public virtual void UpdateItem(int index, object data)
        { 
            if( onRefresh != null )
                onRefresh(this, index, data);
        }

        public virtual void Clear()
        {
            
        }
    }
}
