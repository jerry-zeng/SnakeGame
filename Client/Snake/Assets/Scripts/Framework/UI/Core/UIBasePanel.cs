using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Framework.UI
{
    public enum UIType
    {
        Scene,  // 固定窗口(UIHeaderBar)
        Window, // 弹窗(UISkill, UIStore等)
        Popup,  // 消息(UIMessageBox, UIToast)
        Other,  // 其它(HUD等)
    }

    /// <summary>
    /// Do something on open.
    /// </summary>
    public enum UIOpenMode
    {
        DoNothing,     // not hide other panels, and not push them to navigation sequence
        HideAll,       //     hide all panels, and     push them to navigation sequence
        WindowStack,   //     hide all panels, and not push them to navigation sequence
    }

    /// <summary>
    /// UI full-screen collider type.
    /// </summary>
    public enum UIColliderType
    {
        None,        // none
        Transparent, // transparent
        DarkMask,    // dark mask
    }


    public class UIBasePanel : MonoBehaviour
    {
        // ui type
        public UIType uiType = UIType.Window;

        // how to show this ui
        public UIOpenMode openMode = UIOpenMode.DoNothing;

        // ui background collider type
        public UIColliderType colliderType = UIColliderType.None;


        protected bool isActivated = false;

        //show parameters
        protected object _args;
        public object Args
        {
            get{ return _args; }
        }

        protected GameObject _gameObject;
        public GameObject CachedGameObject
        {
            get{ 
                if(_gameObject == null) 
                    _gameObject = this.gameObject;
                return _gameObject;
            }
        }

        protected Transform _tran;
        public Transform CachedTransform
        {
            get{ 
                if(_tran == null) 
                    _tran = this.transform;
                return _tran;
            }
        }

        protected RectTransform _rectTran;
        public RectTransform CachedRectTransform
        {
            get{ 
                if(_rectTran == null) 
                    _rectTran = CachedGameObject.GetComponent<RectTransform>();
                return _rectTran;
            }
        }


        public bool IsActive()
        {
            return isActivated;
        }

        /// <summary>
        /// On ui game object instantiated, called only once
        /// </summary>
        public virtual void Setup()
        {

        }

        public virtual void Show(object args)
        {
            CachedGameObject.SetActive(true);
            isActivated = true;

            this._args = args;
        }

        public virtual void Hide()
        {
            CachedGameObject.SetActive(false);
            isActivated = false;
        }

        /// <summary>
        /// On ui game object released, called only once
        /// </summary>
        public virtual void Release()
        {
            isActivated = false;
            _gameObject = null;
            _tran = null;
            _rectTran = null;

            _args = null;
        }

    }
}
