using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

namespace Framework.UI
{
    public class UIRoot : MonoBehaviour 
    {
        public Canvas canvas;
        public Camera uiCamera;
        public RectTransform sceneRoot;  // used for scene ui (usually full-screen)
        public RectTransform windowRoot; // used for common ui panel
        public RectTransform popupRoot;  // used for Toast,Message ui
        public RectTransform otherRoot;  // used for other ui like HUD.
        public RectTransform maskRoot;   // used for full-screen collider
        public Image fullScreenMask;

        public GameObject CachedGameObject{ get; private set; }
        public Transform CachedTransform{ get; private set; }
        public RectTransform CachedRectTransform{ get; private set; }

        public UnityAction onMaskClick;


        void Cache()
        {
            if(CachedGameObject == null) CachedGameObject = this.gameObject;
            if(CachedTransform == null) CachedTransform = this.transform;
            if(CachedRectTransform == null) CachedRectTransform = CachedGameObject.GetComponent<RectTransform>();

            if(canvas == null)
                canvas = GetComponent<Canvas>();
            if(uiCamera == null)
                uiCamera = FindInChild<Camera>(CachedTransform, "UICamera");
            if(sceneRoot == null)
                sceneRoot = FindInChild<RectTransform>(CachedTransform, "SceneRoot");
            if(windowRoot == null)
                windowRoot = FindInChild<RectTransform>(CachedTransform, "WindowRoot");
            if(popupRoot == null)
                popupRoot = FindInChild<RectTransform>(CachedTransform, "PopupRoot");
            if(otherRoot == null)
                otherRoot = FindInChild<RectTransform>(CachedTransform, "OtherRoot");
            if(maskRoot == null)
                maskRoot = FindInChild<RectTransform>(CachedTransform, "MaskRoot");

            if( fullScreenMask == null )
                fullScreenMask = FindInChild<Image>(CachedTransform, "mask");

            fullScreenMask.gameObject.GetComponent<Button>().onClick.AddListener( OnClickMask );

            // find EventSystem
        }


        void Awake()
        {
            if( UIManager.Instance.uiRoot != null 
               && UIManager.Instance.uiRoot != this )
            {
                Destroy(gameObject);
                return;
            }

            Cache();
            UIManager.Instance.uiRoot = this;
        }


        void Start () 
        {
            DontDestroyOnLoad(CachedGameObject);
        }

        void OnClickMask()
        {
            if( onMaskClick != null )
                onMaskClick.Invoke();
        }

        public static T FindInChild<T>( Transform root, string path ) where T : Component
        {
            if(root == null)
                return null;

            Transform child = root.Find(path);
            if(child == null) 
                return null;

            return child.GetComponent<T>();
        }

    }
}
