using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

namespace Framework.UI
{
    [RequireComponent(typeof(ScrollRect))]
    public class UIListView : MonoBehaviour 
    {
        public enum Direction
        {
            Horizontal,
            Vertical
        }

        protected ScrollRect _scrollRect;
        public ScrollRect ScrollRect
        {
            get{
                if(_scrollRect == null)
                    _scrollRect = GetComponent<ScrollRect>();
                return _scrollRect;
            }
        }

        public Vector2 ScrollRectSize
        {
            get{
                Vector2 scrollSize = ScrollRect.GetComponent<RectTransform>().sizeDelta;
                if(ScrollRect.viewport != null)
                    scrollSize += ScrollRect.viewport.sizeDelta;

                return scrollSize;
            }
        }

        public RectTransform ContentTransform
        {
            get{ return ScrollRect.content; }
        }
        public Vector2 ContentSize
        {
            get{ return ContentTransform.sizeDelta; }
        }

        public int Row 
        {
            get; protected set; 
        }
        public int Column
        {
            get; protected set; 
        }
        public int PooledItemCount
        {
            get; protected set; 
        }


        public Direction direction = Direction.Vertical;

        public RectTransform ItemPrefab;

        [Range(1, 20)]
        public int colOrRowCount = 1;


        protected List<UIListItem> _itemList = new List<UIListItem>();

        private IList _data;
        protected int TotalItemCount;

        protected Vector2 ItemSize;

        protected int _startIndex = 0;
        protected int _preStartIndex = 0;


        public void SetData(IList data)
        {
            IList oldData = _data;

            _data = data;
            TotalItemCount = _data.Count;

            CalculateContentSize();
            CalculatePooledItemCount();

            ClearAllItems();

            if(oldData != _data)
            {
                RepositionItems();
            }
        }

        public void Clear()
        {
            _data = null;
            TotalItemCount = 0;

            ClearAllItems();

            _startIndex = 0;
            _preStartIndex = 0;

            PooledItemCount = 0;
            Row = 0;
            Column = 0;
            ContentTransform.anchoredPosition = Vector2.zero;
        }

        protected virtual void Awake()
        {
            ItemSize = ItemPrefab.sizeDelta;
            ItemPrefab.gameObject.SetActive(false);

            ScrollRect.horizontal = (direction == Direction.Horizontal);
            ScrollRect.vertical = (direction == Direction.Vertical);
            ScrollRect.onValueChanged.AddListener(OnScroll);

            ContentTransform.pivot = ContentTransform.anchorMin = ContentTransform.anchorMax = new Vector2(0, 1);
            ContentTransform.anchoredPosition = Vector2.zero;
        }

        // update scroll rect.
        protected virtual void OnScroll(Vector2 scrollValue)
        {
            _startIndex = GetStartIndex();

            if( _startIndex != _preStartIndex )
            {
                RepositionItems();
                _preStartIndex = _startIndex;
            }

            // header and footer
            /*
            if( direction == Direction.Vertical )
            {
                float posY = ContentTransform.anchoredPosition.y;

                if( posY <= -ItemPrefab.sizeDelta.y ){
                    Debuger.Log("trigger onHeader event");
                }
                else if( posY <= -10f ){
                    Debuger.Log("press to header");
                }
                else if( posY >= (ContentSize.y - ScrollRectSize.y) + ItemPrefab.sizeDelta.y ){
                    Debuger.Log("trigger onFooter event");
                }
                else if( posY >= (ContentSize.y - ScrollRectSize.y) + 10f ){
                    Debuger.Log("press to footer");
                }
                else{
                    //Debuger.Log("reset the event");
                }
            }
            else
            {
                float posX = ContentTransform.anchoredPosition.x;

                if( posX >= ItemPrefab.sizeDelta.x ){
                    Debuger.Log("trigger onHeader event");
                }
                else if( posX >= 10f ){
                    Debuger.Log("press to header");
                }
                else if( posX <= -((ContentSize.x - ScrollRectSize.x) + ItemPrefab.sizeDelta.x) ){
                    Debuger.Log("trigger onFooter event");
                }
                else if( posX <= -((ContentSize.x - ScrollRectSize.x) + 10f) ){
                    Debuger.Log("press to footer");
                }
                else{
                    //Debuger.Log("reset the event");
                }
            }
            */
        }


        protected virtual void CalculateContentSize()
        {
            if( direction == Direction.Vertical ){
                Column = Mathf.Clamp(colOrRowCount, 1, Mathf.FloorToInt(ScrollRectSize.x/ItemSize.x) );
                Row = (TotalItemCount + Column - 1) / Column;

                ContentTransform.sizeDelta = new Vector2(Column * ItemSize.x, Row * ItemSize.y);
            }
            else{
                Row = Mathf.Clamp(colOrRowCount, 1, Mathf.FloorToInt(ScrollRectSize.y/ItemSize.y));
                Column = (TotalItemCount + Row - 1) / Row;

                ContentTransform.sizeDelta = new Vector2(Column * ItemSize.x, Row * ItemSize.y);
            }
        }

        protected virtual void CalculatePooledItemCount()
        {
            if( direction == Direction.Vertical ){
                PooledItemCount = (Mathf.FloorToInt(ScrollRectSize.y / ItemSize.y) + 2) * Column;
            }
            else{
                PooledItemCount = (Mathf.FloorToInt(ScrollRectSize.x / ItemSize.x) + 2) * Row;
            }

            PooledItemCount = Mathf.Min( PooledItemCount, TotalItemCount );
        }

        protected virtual int GetStartIndex()
        {
            if( direction == Direction.Vertical ){
                // top -> bottom, when y < 0, the first one always shows up.
                if( ContentTransform.anchoredPosition.y < 0f )
                    return 0;
                
                float height = Mathf.Abs(ContentTransform.anchoredPosition.y);
                return Mathf.FloorToInt(height / ItemSize.y) * Column;
            }
            else{
                if( ContentTransform.anchoredPosition.x > 0f )
                    return 0;

                float width = Mathf.Abs(ContentTransform.anchoredPosition.x);
                return Mathf.FloorToInt(width / ItemSize.x) * Row;
            }
        }

        protected virtual Vector2 GetItemAnchorPostion(int index)
        {
            if( direction == Direction.Vertical ){
                return new Vector2( (index % Column) * ItemSize.x, -(index / Column * ItemSize.y) );
            }
            else{
                return new Vector2( (index / Row) * ItemSize.x, -(index % Row) * ItemSize.y );
            }
        }


        protected virtual UIListItem GetItem(int index)
        {
            if( index < _itemList.Count )
            {
                return _itemList[index];
            }
            else
            {
                RectTransform newItem = CreateItem();
                newItem.gameObject.SetActive(true);
                newItem.name = "Item " + index.ToString();

                UIListItem item = newItem.GetComponent<UIListItem>();
                _itemList.Add(item);

                return item;
            }
        }

        protected virtual RectTransform CreateItem()
        {
            RectTransform trans = Instantiate( ItemPrefab ) as RectTransform;
            trans.SetParent(ContentTransform);
            trans.pivot = trans.anchorMin = trans.anchorMax = new Vector2(0, 1);

            trans.localRotation = ItemPrefab.transform.localRotation;
            trans.localScale = ItemPrefab.transform.localScale;
            trans.localPosition = Vector3.zero;

            return trans;
        }

        protected virtual void DestroyItem(GameObject go)
        {
            if(go == null) return;

            go.transform.SetParent(null);
            Destroy(go);
        }

        protected virtual void ClearAllItems()
        {
            for( int i = 0; i < _itemList.Count; i++ )
            {
                DestroyItem(_itemList[i].gameObject);
            }
            _itemList.Clear();
        }

        public virtual void RepositionItems()
        {
            if( _startIndex > _data.Count - PooledItemCount )
                return;

            for( int i = _startIndex; i < _startIndex + PooledItemCount; ++i )
            {
                if( i < 0 || i >= _data.Count )
                    continue;
                
                UIListItem item = GetItem(i - _startIndex);  // 需要减去_startIndex，因为GetItem最大只能是PooledItemCount-1
                item.CachedGameObject.name = "Item " + i.ToString();
                item.CachedRectTransform.anchoredPosition = GetItemAnchorPostion(i);
                item.UpdateItem(i, _data[i]);
            }
        }


        public virtual void ScrollTo(int index)
        {
            Debug.LogWarning("This function was not implemented yet");

            if( index < 0 || index > _data.Count - 1 )
                return;

            /*
            Vector2 itemPosition = GetItemAnchorPostion(index);
            Vector2 contentPos;

            if( direction == Direction.Vertical ){
                
            }
            else{
                
            }
            */
        }
    }

}
