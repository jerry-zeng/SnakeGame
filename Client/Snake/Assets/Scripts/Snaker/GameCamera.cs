using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Framework;

namespace GamePlay
{
    [RequireComponent(typeof(Camera))]
    public class GameCamera : MonoBehaviour 
    {
        public static GameCamera current;

        public float moveSmooth = 10;
        private Transform _tran;
        private Camera _camera;
        private Vector2 _viewSize;

        private Snaker _focusSnaker;

        void Awake()
        {
            current = this;
        }

        void Start()
        {
            _tran = transform;
            _camera = GetComponent<Camera>();
            _camera.orthographic = true;

            float height = _camera.orthographicSize;
            float width = (float)Screen.width / (float)Screen.height * height;
            _viewSize = new Vector2(width, height);

            EventManager.Instance.RegisterEvent<Snaker,object>("OnSnakerDead", OnSnakerDead);
        }

        void OnDesotroy()
        {
            current = null;

            EventManager.Instance.UnregisterEvent<Snaker,object>("OnSnakerDead", OnSnakerDead);
        }

        void OnSnakerDead(Snaker snaker, object killer)
        {
            if( snaker != null && _focusSnaker != null && 
               snaker.PlayerID == _focusSnaker.PlayerID )
            {
                _focusSnaker = null;
            }
        }

        public void SetFocusSnaker(Snaker snaker)
        {
            _focusSnaker = snaker;
        }

        void LateUpdate()
        {
            if( BattleEngine.Instance.IsRunning )
            {
                if(_focusSnaker == null)
                    _focusSnaker = BattleEngine.Instance.GetSnakerByID(BattleView.Instance.FocusPlayerID);

                if( _focusSnaker != null )
                {
                    Vector3 pos = BattleView.Instance.EntityToViewPos(_focusSnaker.Head.Position);
                    if( _tran.localPosition != pos )
                    {
                        Vector3 targetPos = _tran.localPosition;
                        targetPos.x = pos.x;
                        targetPos.y = pos.y;
                        _tran.localPosition = ClampPosition(targetPos);
                        //_tran.localPosition = Vector3.MoveTowards(_tran.localPosition, targetPos, Time.deltaTime * moveSmooth);
                    }
                }
            }
        }

        Vector3 ClampPosition(Vector3 pos)
        {
            Rect bound = BattleView.Instance.GetMapViewBound();
            pos.x = Mathf.Clamp(pos.x, bound.xMin + _viewSize.x, bound.xMax - _viewSize.x);
            pos.y = Mathf.Clamp(pos.y, bound.yMin + _viewSize.y, bound.yMax - _viewSize.y);
            return pos;
        }
    }
}
