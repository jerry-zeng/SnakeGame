using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GamePlay
{
    public class GameMap : ITick
    {
        protected GameMapView _view;
        public GameMapView View
        {
            get{ return _view; }
            set{ _view = value; }
        }

        protected bool _isReleased = false;

        public bool IsReleased
        {
            get { return _isReleased; }
        }

        protected int _id;
        public int ID
        {
            get{ return _id; }
        }

        protected MapScript _script;
        protected int _frame;

        protected Dictionary<string, CSVValue> _data;
        public Dictionary<string, CSVValue> Data
        {
            get{ return _data; }
        }

        /*
         * the map is like:
         *   +--------------+
         *   |              |
         *   |              | height
         *   |              |
         *   O--------------+
         *        width
         *   O is the origin position
         */

        protected Vector2 _originPos;
        public Vector2 OriginPosition
        {
            get{ return _originPos; }
        }

        protected Vector2 _size;
        public Vector2 Size
        {
            get{ return _size; }
        }

        protected Rect _rect;
        public Rect BoundRect
        {
            get{ return _rect; }
        }

        public void Load(int id)
        {
            _id = id;
            _isReleased = false;

            _data = CSVTableLoader.GetTableContainer("Map").GetRow(_id.ToString());

            _originPos = new Vector2();
            _size = new Vector2(_data["Width"].FloatValue, _data["Height"].FloatValue);
            _rect = new Rect(_originPos, _size);

            _script = new MapScript(this);
        }

        public void Unload()
        {
            _script.Release();
            _script = null;

            _isReleased = true;

            _view = null;
        }

        public void Release()
        {
            Unload();
        }

        public void EnterFrame(int frame)
        {
            if( _frame == frame )
                return;

            if( _script != null )
                _script.EnterFrame(frame);

            _frame = frame;
        }
    }
}
