using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GamePlay
{
    public class Food : Entity, ITick
    {
        protected FoodView _view = null;
        public FoodView View
        {
            get{ return _view; }
            set{ _view = value; }
        }

        protected Dictionary<string, CSVValue> _data;
        public Dictionary<string, CSVValue> Data
        {
            get{ return _data; }
        }

        protected bool _isReleased = false;

        public bool IsReleased
        {
            get { return _isReleased; }
        }

        protected int _enegy = 1;

        public void Release()
        {
            _isReleased = true;

            _view = null;
        }


        public virtual void InitData(int id, Vector3 pos)
        {
            _id = id;
            _isReleased = false;
            _prePosition = _position = pos;

            _data = CSVTableLoader.GetTableContainer("Food").GetRow(_id.ToString());
            _enegy = _data["Enegy"].IntValue;
            _radius = _data["Radius"].IntValue;
        }

        public virtual void SetPosition(Vector3 pos)
        {
            _prePosition = _position;
            _position = pos;
        }

        public virtual void EnterFrame(int frame)
        {
            
        }

        public void OnEaten(object killer)
        {
            Release();

            BattleEngine.Instance.OnFoodEaten(this, killer);
        }

        public int GetEnegy()
        {
            return _enegy;
        }
    }
}
