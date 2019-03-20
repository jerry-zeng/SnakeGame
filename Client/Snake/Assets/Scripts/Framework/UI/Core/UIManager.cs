using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using Framework.Module;

namespace Framework.UI
{
    public class UIManager : ServiceModule<UIManager>
    {
        /**
         * The ui stack is like:
         * --Root
         * ----UIType.Scene
         * ----UIType.Window
         * ----...
         * ----UIType.Window
         * ----UIType.Scene
         * ----UIType.Window
         * ----...
         * ----UIType.Window
         */
        public class UITracker
        {
            public string uiScene;
            public List<string> windowStack = new List<string>();
        }

        // release hiden ui panels when there are too many ui
        private const int MaxAliveUICount = 10;
        private bool UIReleaseCheckOnHide = true;


        public UIRoot uiRoot { get; set;}

        // all UIPanel include active and inactive panels
        private Dictionary<string, UIBasePanel> _allPanels = new Dictionary<string, UIBasePanel>();

        private Stack<UITracker> _uiStack = new Stack<UITracker>();


        protected override void Init()
        {
            base.Init();

            ClearUITrackers();
        }

        private void ClearUITrackers()
        {
            _uiStack.Clear();
        }

        public bool IsTrackedUIType(UIType uiType)
        {
            return uiType == UIType.Scene || uiType == UIType.Window;
        }

        public bool IsInTrackerStack(string uiName)
        {
            foreach(UITracker stack in _uiStack)
            {
                if( stack.uiScene == uiName )
                    return true;

                if( stack.windowStack.Contains(uiName) )
                    return true;
            }
            return false;
        }

        private void SetFullScreenMask(bool state)
        {
            if( uiRoot != null ){
                uiRoot.onMaskClick = null;

                SetMaskState(uiRoot.fullScreenMask, state);
            }
        }

        public void SetFullScreenMaskClickListener(UnityAction onClick)
        {
            if( uiRoot != null ){
                uiRoot.onMaskClick = onClick;
            }
        }

        #region Show UI
        public void ShowUI(string uiName, object args = null, Action<UIBasePanel> onUIReady = null)
        {
            if( string.IsNullOrEmpty(uiName) ){
                this.LogWarning("Null ui name!!!");
                return;
            }

            if(!_allPanels.ContainsKey(uiName))
            {
                UIAssetManager.RequestUIPopup(uiName, delegate(string popupName, GameObject uiPopup)
                {
                    if(uiPopup == null){
                        Debug.LogError("Can't load the ui prefab: " + popupName);
                        return;
                    }

                    UIBasePanel panel = uiPopup.GetComponent<UIBasePanel>();
                    AnchorUIGameObject(panel);
                    panel.Setup();

                    _allPanels.Add(uiName, panel); // add to pool.

                    Show_Internel(panel, args, onUIReady);

                    if(UIReleaseCheckOnHide == false)
                        CheckToReleaseUI();
                });
            }
            else
            {
                UIBasePanel panel = _allPanels[uiName];
                Show_Internel(panel, args, onUIReady);
            }
        }

        void AnchorUIGameObject(UIBasePanel ui)
        {
            if( ui == null )
                return;
            
            if( uiRoot == null )
            {
                Debug.LogWarning("UIRoot was not initialized.");
                GameObject prefab = Resources.Load<GameObject>("UI/UIRoot");
                GameObject go = GameObject.Instantiate(prefab) as GameObject;
                go.name = prefab.name;
                uiRoot = go.GetComponent<UIRoot>();
            }

            Vector2 anchoredPos = Vector2.zero;
            Vector2 sizeDelta = Vector2.zero;
            Vector3 scale = Vector3.one;

            RectTransform rectTran = ui.CachedRectTransform;
            if(rectTran != null) {
                anchoredPos = rectTran.anchoredPosition;
                sizeDelta = rectTran.sizeDelta;
                scale = rectTran.localScale;
            }
            else {
                anchoredPos = ui.CachedTransform.localPosition;
                scale = ui.CachedTransform.localScale;
            }

            if(ui.uiType == UIType.Scene) {
                ui.CachedTransform.SetParent(uiRoot.sceneRoot);
            }
            else if(ui.uiType == UIType.Window) {
                ui.CachedTransform.SetParent(uiRoot.windowRoot);
            }
            else if(ui.uiType == UIType.Popup) {
                ui.CachedTransform.SetParent(uiRoot.popupRoot);
            }
            else if(ui.uiType == UIType.Other) {
                ui.CachedTransform.SetParent(uiRoot.otherRoot);
            }

            if(rectTran != null) {
                rectTran.anchoredPosition = anchoredPos;
                rectTran.sizeDelta = sizeDelta;
                rectTran.localScale = scale;
            }
            else {
                ui.CachedTransform.localPosition = anchoredPos;
                ui.CachedTransform.localScale = scale;
            }

        }
        void UpdateColliderMask(UIBasePanel ui)
        {
            if( ui == null || ui.colliderType == UIColliderType.None )
            {
                SetFullScreenMask(false);
            }
            else
            {
                SetFullScreenMask(true);

                Image mask = uiRoot.fullScreenMask;
                mask.transform.SetParent(uiRoot.maskRoot); // reset ui's sibling index

                if( ui.colliderType == UIColliderType.Transparent ){
                    SetMaskAlpha(mask, 0f);
                }
                else if( ui.colliderType == UIColliderType.DarkMask ){
                    SetMaskAlpha(mask, 0.5f);
                }

                if( mask.enabled )
                {
                    if(mask.transform.parent != ui.CachedTransform.parent)
                        mask.transform.SetParent(ui.CachedTransform.parent);

                    mask.transform.SetAsLastSibling();
                    ui.CachedTransform.SetAsLastSibling();
                    //int siblingIndex = ui.CachedTransform.GetSiblingIndex();
                    //mask.transform.SetSiblingIndex(siblingIndex);
                }
            }
        }

        public static void SetMaskAlpha(Image image, float alpha)
        {
            if( image == null ) return;

            alpha = Mathf.Clamp01(alpha);

            Color color = image.color;
            if( color.a != alpha ){
                color.a = alpha;
                image.color = color;
            }
        }
        public static void SetMaskState(Image image, bool value)
        {
            if( image.enabled != value ){
                image.enabled = value;
                image.raycastTarget = image.enabled;
            }
        }


        void Show_Internel(UIBasePanel ui, object args, Action<UIBasePanel> onUIReady)
        {
            if(ui == null) return;

            ui.Show(args);

            if( AddToTrackerStack(ui) )
                UpdateColliderMask(ui);

            if(onUIReady != null) onUIReady(ui);
        }

        bool AddToTrackerStack(UIBasePanel ui)
        {
            if( !IsTrackedUIType(ui.uiType) )
                return false;

            string uiName = ui.name;

            if( _uiStack.Count == 0 )
            {
                if( ui.uiType == UIType.Window ){
                    this.LogError("You must show a UIType.Scene ui before any UIType.Window on new scenes loaded");
                    return false;
                }
                else{
                    UITracker tracker = new UITracker();
                    tracker.uiScene = uiName;
                    _uiStack.Push(tracker);
                    return true;
                }
            }
            else
            {
                if( ui.uiType == UIType.Window ){
                    UITracker tracker = _uiStack.Peek();

                    if(tracker.windowStack.Contains(uiName))
                        tracker.windowStack.Remove(uiName);

                    tracker.windowStack.Add(uiName);
                }
                else{
                    // hide old scene ui
                    UITracker oldTracker = _uiStack.Peek();
                    if( oldTracker.uiScene != uiName )
                    {
                        if( _allPanels.ContainsKey(oldTracker.uiScene) )
                            _allPanels[oldTracker.uiScene].Hide();

                        for( int i = 0; i < oldTracker.windowStack.Count; i++ )
                        {
                            if( _allPanels.ContainsKey(oldTracker.windowStack[i]) )
                                _allPanels[oldTracker.windowStack[i]].Hide();
                        }

                        // add tracker of new scene
                        UITracker tracker = new UITracker();
                        tracker.uiScene = uiName;
                        _uiStack.Push(tracker);
                    }
                }
                return true;
            }
        }
        #endregion

        #region Hide UI
        private string GetTopUIName()
        {
            if(_uiStack.Count <= 0) return "";

            if( _uiStack.Count == 1 )
            {
                UITracker tracker = _uiStack.Peek();
                if( tracker.windowStack.Count > 0 ){
                    int lastIndex = tracker.windowStack.Count - 1;
                    string uiName = tracker.windowStack[lastIndex];
                    return uiName;
                }
                else{
                    return "";
                }
            }
            else
            {
                UITracker tracker = _uiStack.Peek();
                if( tracker.windowStack.Count > 0 ){
                    int lastIndex = tracker.windowStack.Count - 1;
                    string uiName = tracker.windowStack[lastIndex];
                    return uiName;
                }
                else{
                    return tracker.uiScene;
                }
            }
        }

        /// <summary>
        /// Hides the UI in the top node.
        /// </summary>
        public void PopUI()
        {
            HideUI( GetTopUIName() );
        }


        public void HideUI(string uiName)
        {
            if(_allPanels.ContainsKey(uiName))
                Hide_Internel( _allPanels[uiName] );
        }

        void Hide_Internel(UIBasePanel ui)
        {
            if(ui == null || ui.IsActive() == false) return;

            if( !IsTrackedUIType(ui.uiType) )
            {
                ui.Hide();

                if(UIReleaseCheckOnHide == true)
                    CheckToReleaseUI();

                return;
            }

            if( _uiStack.Count == 0 ){
                ui.Hide();
            }
            else if( _uiStack.Count == 1 )
            {
                UITracker tracker = _uiStack.Peek();

                if( tracker.windowStack.Count > 0 ){
                    int lastIndex = tracker.windowStack.Count - 1;
                    string uiName = tracker.windowStack[lastIndex];

                    if( ui.name == uiName ){
                        tracker.windowStack.RemoveAt(lastIndex);
                        ui.Hide();
                    }
                }
                else{
                    this.LogWarning("The only one ui in scene can't be hiden: " + ui.name);
                }
            }
            else
            {
                UITracker tracker = _uiStack.Peek();
                if( tracker.windowStack.Count > 0 ){
                    int lastIndex = tracker.windowStack.Count - 1;
                    string uiName = tracker.windowStack[lastIndex];

                    if( ui.name == uiName ){
                        tracker.windowStack.RemoveAt(lastIndex);
                        ui.Hide();
                    }
                }
                else{
                    if( ui.name == tracker.uiScene ){
                        _uiStack.Pop();  // pop tracker and hide the old uiScene
                        ui.Hide();

                        // show new tracker ui
                        tracker = _uiStack.Peek();
                        ShowUI(tracker.uiScene);

                        for( int i = 0; i < tracker.windowStack.Count; i++ )
                        {
                            ShowUI(tracker.windowStack[i]);
                        }
                    }
                }
            }

            string topUIName = GetTopUIName();
            if( _allPanels.ContainsKey(topUIName) )
                UpdateColliderMask(_allPanels[topUIName] );
            else
                UpdateColliderMask(null);

            if(UIReleaseCheckOnHide == true)
                CheckToReleaseUI();
        }
        #endregion


        #region Release UI
        public void ReleaseUI( string uiName, bool hideFirst = true )
        {
            if(_allPanels.ContainsKey(uiName))
            {
                UIBasePanel panel = _allPanels[uiName];
                if(hideFirst) {
                    Hide_Internel(panel);
                }

                Release_Internel(panel);
            }
        }

        /// <summary>
        /// Releases all UI panels of current scene except UILoadingPanel.
        /// </summary>
        public void ReleaseAllUIOfCurrentUnityScene()
        {
            ClearUITrackers();

            string[] keys = new string[_allPanels.Keys.Count];
            _allPanels.Keys.CopyTo(keys, 0);

            for(int i = 0; i < keys.Length; i++)
            {
                UIBasePanel panel = _allPanels[keys[i]];
                if( !CheckNonReleaseList(keys[i]) )
                {
                    Release_Internel(panel);
                }
            }

            Resources.UnloadUnusedAssets();

            SetFullScreenMask(false);
        }

        void Release_Internel(UIBasePanel panel)
        {
            if(panel == null) return;

            panel.Release();

            UIAssetManager.UnloadTexturesOnGameObject(panel.CachedGameObject);

            if(_allPanels.ContainsKey(panel.name))
                _allPanels.Remove(panel.name);

            UIAssetManager.UI_Destroy_GameObject(panel.CachedGameObject);
        }

        bool CheckNonReleaseList(string uiName)
        {
            return uiName == "UILoadingPanel" ||
                uiName == "UIMessageBox";
        }
        #endregion


        #region Release Strategy

        private List<UIBasePanel> hidenPanels = new List<UIBasePanel>();

        /// <summary>
        /// release hiden ui to free memory if the amount of ui is too large.
        /// </summary>
        void CheckToReleaseUI()
        {
            if( _allPanels.Count > MaxAliveUICount )
            {
                hidenPanels.Clear();

                foreach( var panel in _allPanels.Values )
                {
                    if(!panel.IsActive())
                    {
                        if(!IsInTrackerStack(panel.name))
                        {
                            // TODO: release only low-frequently used UIs, not all.
                            hidenPanels.Add(panel);

                            if( _allPanels.Count - hidenPanels.Count <= MaxAliveUICount ){
                                break;
                            }
                        }
                    }
                }

                if(hidenPanels.Count > 0)
                {
                    UIBasePanel panel = null;

                    for(int i = 0; i < hidenPanels.Count; i++)
                    {
                        panel = hidenPanels[i];
                        Debug.Log("--> release ui: " + panel.name);

                        uiRoot.StartCoroutine(ReleaseAsync(panel));
                    }
                    hidenPanels.Clear();
                }
            }
        }

        IEnumerator ReleaseAsync(UIBasePanel panel)
        {
            yield return null;

            Release_Internel(panel);
        }

        #endregion
    }
}
