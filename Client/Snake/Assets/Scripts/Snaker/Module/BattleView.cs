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

        private GameMapView _mapView;
        private LinkedList<FoodView> _foodViewList = new LinkedList<FoodView>();
        private LinkedList<SnakerActor> _snakerActorList = new LinkedList<SnakerActor>();

        List<FoodView> removeFoodView = new List<FoodView>();
        List<SnakerActor> removeSnakerActor = new List<SnakerActor>();

        private int _focusPlayerID = 0;
        public int FocusPlayerID
        {
            get{ return _focusPlayerID; }
        }

        private BattleEngine battleEngine;

        bool _paused = false;
        float _nextTickTime = 0f;
        int _frame = 0;
        float _dt = 0f;

        public int CurrentFrame
        {
            get{ return _frame; }
        }


        void Start()
        {
            GameInput.onVKey = OnVKey;

            _paused = false;
            _nextTickTime = 0f;
            _frame = 0;

            battleEngine = BattleEngine.Instance;

            EventManager.Instance.RegisterEvent<Snaker,object>("OnSnakerDead", OnSnakerDead);
            EventManager.Instance.RegisterEvent<Food,object>("OnFoodEaten", OnFoodEaten);

            GameInput.DisableInput();
        }

        protected override void OnDestroy()
        {
            battleEngine = null;

            EventManager.Instance.UnregisterEvent<Snaker,object>("OnSnakerDead", OnSnakerDead);
            EventManager.Instance.UnregisterEvent<Food,object>("OnFoodEaten", OnFoodEaten);

            base.OnDestroy();
        }

        public void OnPlayerReady()
        {
            var userData = UserManager.Instance.UserData;
            int snakerID = UserManager.Instance.CurrentSnakerID;

            PlayerData player = new PlayerData();
            player.playerID = battleEngine.GenerateNewPlayerID();
            player.teamID = 1;
            player.snakerData = new SnakerData(){id = snakerID, length = 0 };
            player.aiID = 0;
            player.userID = userData.id;
            player.userName = userData.userName;

            Vector3 pos = battleEngine.Map.BoundRect.center.ToVector3();
            battleEngine.CreateSnaker(player, pos);

            GameInput.EnableInput();

            FocusOnPlayer(player.playerID);

            if(!battleEngine.IsRunning)
                battleEngine.StartRunning();
        }

        public void FocusOnPlayer(int playerID)
        {
            _focusPlayerID = playerID;
        }

        public void PauseGame()
        {
            _paused = true;

            GameInput.DisableInput();
        }
        public void ResumeGame()
        {
            _paused = false;

            GameInput.EnableInput();
        }

        void OnVKey(int vkey, float arg)
        {
            BattleEngine.Instance.InputVKey((int)vkey, arg, _focusPlayerID);
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

        private void OnSnakerDead(Snaker snaker, object killer)
        {
            if( snaker != null && snaker.PlayerID == _focusPlayerID )
            {
                _focusPlayerID = 0;
                GameInput.DisableInput();

                Scheduler.Schedule( 0.5f, ()=>
                {
                    MessageBox.Show("Revive or Quit this game?",
                                    "Quit", ()=>{
                        BattleEngine.Instance.EndBattle();
                        MessageBox.Hide();
                        EventManager.Instance.SendEvent(EventDef.OnLeaveBattle);
                    },
                                    "Revive", ()=>{
                        OnPlayerReady();
                        MessageBox.Hide();
                    });
                } );

            }
        }

        private void OnFoodEaten(Food food, object killer)
        {
            
        }


        void Update()
        {
            if( battleEngine == null || !battleEngine.IsRunning )
                return;

            if( _mapView == null )
                InitMap();

            if( _paused )
                return;

            // 1. sync actors
            SyncActors();

            // 2. tick
            _dt = Time.deltaTime;

            _nextTickTime -= _dt;

            if( _nextTickTime <= 0f )
            {
                _frame++;

                Tick();

                while(_nextTickTime <= 0f){
                    _nextTickTime += BattleEngine.TICK_INTERVAL;
                }
            }

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

        void Tick()
        {
            battleEngine.EnterFrame(_frame);
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
