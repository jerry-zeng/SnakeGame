using System.Collections;
using System.Collections.Generic;
using Framework;
using UnityEngine;
using GameProtocol;
using GamePlay;

namespace GamePlay
{
    public class BattleView : Framework.MonoSingleton<BattleView> 
    {
        public const int PixelsPerUnit = 1;
        public const float UnitsPerPixel = 1f / PixelsPerUnit;

        private Transform _root;
        public Transform Root
        {
            get{ 
                if( _root == null ) 
                    _root = transform;
                return _root; 
            }
        }

        private GameCamera _gameCamera;
        private GameMapView _mapView;
        private LinkedList<FoodView> _foodViewList = new LinkedList<FoodView>();
        private LinkedList<SnakerActor> _snakerActorList = new LinkedList<SnakerActor>();

        List<FoodView> removeFoodView = new List<FoodView>();
        List<SnakerActor> removeSnakerActor = new List<SnakerActor>();


        private BattleEngine battleEngine;
        private int _focusPlayerId;


        void Start()
        {
            battleEngine = BattleEngine.Instance;
            _gameCamera = GameCamera.current;

            EventManager.Instance.RegisterEvent<Snaker,object>("OnSnakerDead", OnSnakerDead);
        }

        protected override void OnDestroy()
        {
            battleEngine = null;

            ResetGameCamera();
            _gameCamera = null;

            EventManager.Instance.UnregisterEvent<Snaker,object>("OnSnakerDead", OnSnakerDead);

            base.OnDestroy();
        }

        void OnSnakerDead(Snaker snaker, object killer)
        {
            if( _focusPlayerId == snaker.PlayerID )
            {
                _focusPlayerId = 0;
                SyncGameCamera();
            }
        }

        public void FocusOnPlayer(int playerID)
        {
            _focusPlayerId = playerID;
            SyncGameCamera();
        }
        void SyncGameCamera()
        {
            if (_gameCamera != null)
                _gameCamera.SetFocusSnaker(_focusPlayerId);
        }
        void ResetGameCamera()
        {
            _focusPlayerId = 0;

            if (_gameCamera != null)
                _gameCamera.Reset();
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
         * 
         * the map view should be like:
         *   +--------------+
         *   |              |
         *   |       O      |
         *   |              |
         *   +--------------+
         */
        public Vector3 EntityToViewPos(Vector3 pos)
        {
            if( _mapView != null )
                return (pos - _mapView.ModelCenter) * UnitsPerPixel;
            else
                return pos * UnitsPerPixel;
        }

        public Rect GetMapViewBound()
        {
            if( _mapView != null ){
                return _mapView.MapViewBound;
            }
            else{
                this.LogWarning("The map view wasn't created yet!");
                return Rect.zero;
            }
        }

        float _dt;
        void Update()
        {
            if( battleEngine == null || !battleEngine.isRunning )
                return;

            if( _mapView == null )
                InitMap();

            // 只有部分玩法有暂停
            if( battleEngine.Context.isPaused )
                return;

            // 1. sync actors
            SyncActors();

            // 2. tick
            _dt = Time.deltaTime;

            // 3. update actors
            if( _mapView != null )
                _mapView.DoUpdate(_dt);

            foreach( FoodView food in _foodViewList )
            {
                if( !food.Model.IsReleased )
                    food.DoUpdate(_dt);
                else
                    removeFoodView.Add(food);
            }

            foreach( SnakerActor snaker in _snakerActorList )
            {
                if( !snaker.Model.IsReleased )
                    snaker.DoUpdate(_dt);
                else
                    removeSnakerActor.Add(snaker);
            }

            // 4 remove dead entity
            foreach(FoodView food in removeFoodView)
            {
                _foodViewList.Remove(food);
                DestroyFood(food);
            }
            removeFoodView.Clear();

            foreach( SnakerActor snaker in removeSnakerActor )
            {
                _snakerActorList.Remove(snaker);
                DestroySnaker(snaker);
            }
            removeSnakerActor.Clear();
        }

        void SyncActors()
        {
            foreach(Food food in battleEngine.FoodList)
            {
                if( !food.IsReleased && food.View == null )
                {
                    food.View = CreateFood(food);
                    _foodViewList.AddLast(food.View);
                }
            }

            foreach(var kvs in battleEngine.SnakerList)
            {
                if( !kvs.Value.IsDead && kvs.Value.View == null )
                {
                    kvs.Value.View = CreateSnaker( kvs.Value );
                    _snakerActorList.AddLast(kvs.Value.View);
                }
            }
        }


        void InitMap()
        {
            if( battleEngine.Map != null ){
                GameMap map = battleEngine.Map;

                GameObject prefab = Resources.Load<GameObject>( map.Data["Prefab"].StringValue );
                GameObject go = Instantiate<GameObject>( prefab );
                go.name = "map";
                go.transform.SetParent(Root);
                _mapView = go.EnsureComponent<GameMapView>();
                _mapView.Bind(map);
            }
        }

        private FoodView CreateFood(Food food)
        {
            if( food == null )
                return null;

            GameObject prefab = Resources.Load<GameObject>( food.Data["Prefab"].StringValue );
            GameObject go = Instantiate<GameObject>( prefab );
            go.name = prefab.name;
            go.transform.SetParent(Root);
            FoodView view = go.EnsureComponent<FoodView>();
            view.Bind(food);

            return view;
        }

        private void DestroyFood(FoodView foodView)
        {
            if( foodView != null ){
                foodView.Unbind();
                Destroy(foodView.gameObject);
            }
        }


        private SnakerActor CreateSnaker(Snaker snaker)
        {
            if( snaker == null )
                return null;

            GameObject prefab = Resources.Load<GameObject>( snaker.Data["Prefab"].StringValue );
            GameObject go = Instantiate<GameObject>( prefab );
            go.name = prefab.name + "_" + snaker.PlayerID.ToString();
            go.transform.SetParent(Root);
            SnakerActor view = go.EnsureComponent<SnakerActor>();
            view.Bind(snaker);

            return view;
        }

        private void DestroySnaker(SnakerActor actor)
        {
            if( actor != null ){
                actor.Unbind();
                Destroy(actor.gameObject);
            }
        }

    }
}
