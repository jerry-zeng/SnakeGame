using System.Collections;
using System.Collections.Generic;
using Framework;
using Framework.Module;
using GameProtocol;
using UnityEngine;


namespace GamePlay
{
    public class BattleEngine : ServiceModule<BattleEngine>
    {
        /// <summary>
        /// The TICk times per seconds.
        /// </summary>
        public const int TICKS_PER_SECONDS = 30;
        public static readonly float TICK_INTERVAL = 1f / (float)TICKS_PER_SECONDS;

        // params
        private GameContext _context;
        private Dictionary<int, PlayerData> _mapPlayerData = new Dictionary<int, PlayerData>();


        // game objects
        private GameMap _map;
        private LinkedList<Food> _foodList = new LinkedList<Food>();
        private Dictionary<int, Snaker> _snakerList = new Dictionary<int, Snaker>();

        List<Food> tobeAddFoodList = new List<Food>();
        List<Snaker> tobeAddSnakerList = new List<Snaker>();

        List<Food> removeFoodList = new List<Food>();
        List<int> removeSnakerList = new List<int>();

        // game var
        private int _curPlayerID = 1;
        private bool _isRunning = false;

        public bool isRunning
        {
            get{ return _isRunning; }
        }

        public GameContext Context
        {
            get{ return _context; }
        }

        public GameMap Map
        {
            get{ return _map; }
        }

        public LinkedList<Food> FoodList
        {
            get{ return _foodList; }
        }

        public Dictionary<int, Snaker> SnakerList
        {
            get{ return _snakerList; }
        }


        public int GenerateNewPlayerID()
        {
            return _curPlayerID++;
        }

        public void EnterBattle(GameParam param, IGame game)
        {
            if( _isRunning ){
                this.LogWarning("The game is running, don't run it!");
                return;
            }
            _isRunning = true;

            _map = new GameMap();
            _map.Load(param.mapID);

            _context = new GameContext(param, game);

            _foodList.Clear();
            _snakerList.Clear();
            tobeAddFoodList.Clear();
            tobeAddSnakerList.Clear();
            removeFoodList.Clear();
            removeSnakerList.Clear();

            _context.EnterFrame(0);

            this.Log("StartBattle()");
        }


        public void ExitBattle()
        {
            if( !_isRunning ){
                this.LogWarning("The game wasn't running, don't stop it!");
                return;
            }
            _isRunning = false;

            ClearBattle();

            _context.Clear();
            _context = null;
        }
        public void ClearBattle()
        {
            if (_map != null)
            {
                _map.Unload();
                _map = null;
            }

            foreach(Food food in _foodList)
            {
                food.Release();
            }
            _foodList.Clear();

            foreach(var kvs in _snakerList)
            {
                kvs.Value.Release();
            }
            _snakerList.Clear();

            tobeAddFoodList.Clear();
            tobeAddSnakerList.Clear();
            removeFoodList.Clear();
            removeSnakerList.Clear();

            _curPlayerID = 1;
        }

        public void EnterFrame(int frame)
        {
            // record new frame
            if( frame < 0 ){
                _context.EnterFrame(_context.CurrentFrame + 1);
            }
            else{
                _context.EnterFrame(frame);
            }


            // update entity and check collision themselves
            _map.EnterFrame(frame);

            foreach(var kvs in _snakerList)
            {
                kvs.Value.EnterFrame(frame);
            }

            foreach(Food food in _foodList)
            {
                if( !food.IsReleased )
                    food.EnterFrame(frame);
                else
                    removeFoodList.Add(food);
            }


            // check and delete the dead entity
            foreach(var kvs in _snakerList)
            {
                if( kvs.Value.IsReleased )
                    removeSnakerList.Add(kvs.Value.PlayerID);
            }

            foreach(Food food in removeFoodList)
            {
                RemoveFood(food);
            }
            removeFoodList.Clear();

            foreach(int playerID in removeSnakerList)
            {
                RemoveSnakerByID(playerID);
            }
            removeSnakerList.Clear();


            // add new entity
            foreach(Food food in tobeAddFoodList)
            {
                AddFood(food);
            }
            tobeAddFoodList.Clear();

            foreach(Snaker snaker in tobeAddSnakerList)
            {
                AddSnaker(snaker);
            }
            tobeAddSnakerList.Clear();
        }


        public void InputVKey(int vkey, float arg, int playerId)
        {
            if( !_isRunning ){
                return;
            }

            Snaker snaker = GetSnakerByID(playerId);
            if( snaker != null )
                snaker.InputKey(vkey, arg);
            else
                HandleOtherInput(vkey, arg, playerId);
        }

        void HandleOtherInput(int vkey, float arg, int playerId)
        {
            if ( DoVKey_CreatePlayer(vkey, arg, playerId)
                || DoVKey_RemovePlayer(vkey, arg, playerId)
                )
            {
                // Debuger.Log("BattleEngine", "HandleOtherInput: {0}, {1}, {2}", vkey, arg, playerId);
            }
        }
        bool DoVKey_CreatePlayer(int vkey, float arg, int playerId)
        {
            if (vkey == (int)GameVKey.CreatePlayer)
            {
                CreatePlayerSnaker(playerId);
                Debuger.Log("BattleEngine", "DoVKey_CreatePlayer: {0}", playerId);
                return true;
            }
            return false;
        }
        bool DoVKey_RemovePlayer(int vkey, float arg, int playerId)
        {
            if (vkey == (int)FSPVKeyBase.PLAYER_EXIT)
            {
                RemoveSnakerByID(playerId);
                Debuger.Log("BattleEngine", "DoVKey_RemovePlayer: {0}", playerId);
                return true;
            }
            return false;
        }


        #region Player
        public void RegPlayerData(PlayerData data)
        {
			_mapPlayerData[data.playerID] = data;
        }

        public PlayerData GetPlayerData(int playerId)
        {
            return _mapPlayerData.ContainsKey(playerId)? _mapPlayerData[playerId] : null;
        }
        #endregion

        #region Snaker
        public void OnSnakerDead(Snaker snaker, object killer)
        {
            Context.Game.OnPlayerDie(snaker.PlayerID);

            EventManager.Instance.SendEvent<Snaker,object>("OnSnakerDead", snaker,killer);
        }

        internal Snaker CreateRandomSnaker(int snakerID)
        {
            PlayerData player = new PlayerData();
            player.playerID = GenerateNewPlayerID();
            player.aiID = 1;
            player.teamID = player.playerID;
            player.snakerData = new SnakerData(){ id = snakerID };

            Vector3 pos = GetMapRandomPosition();

            return CreateSnaker(player, pos, 1f);
        }

        // 已经注册的玩家
        internal Snaker CreatePlayerSnaker(int playerId)
        {
             PlayerData player = GetPlayerData(playerId);
             // TODO: 可以根据阵营选择出生点
             Vector3 pos = GetMapRandomPosition();
             return CreateSnaker(player, pos);
        }

        internal Snaker CreateSnaker(PlayerData data, Vector3 pos, float speed = 0f)
        {
            if( data == null )
                return null;

            Snaker snaker = new Snaker(data, pos, speed);
            // Debuger.Log("BattleEngine", "CreateSnaker: {0}", data.playerID);
            tobeAddSnakerList.Add( snaker );

            return snaker;
        }

        internal Snaker GetSnakerByID(int playerID)
        {
            if(_snakerList.ContainsKey(playerID))
                return _snakerList[playerID];
            return null;
        }

        private void AddSnaker(Snaker snaker)
        {
            if( _snakerList.ContainsKey(snaker.PlayerID) ){
                this.LogError("You are trying to add a duplicated snaker " + snaker.PlayerID.ToString());
            }
            else{
                _snakerList[snaker.PlayerID] = snaker;
            }
        }

        private bool RemoveSnakerByID(int playerID)
        {
            if(_snakerList.ContainsKey(playerID))
                return _snakerList.Remove(playerID);
            return false;
        }
        #endregion

        #region Food
        public void OnFoodEaten(Food food, object killer)
        {
            EventManager.Instance.SendEvent<Food,object>("OnFoodEaten", food,killer);
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
        internal void CreateRandomFood(int foodID)
        {
            Food food = new Food();
            Vector3 pos = GetMapRandomPosition();
            food.InitData(foodID, pos);

            tobeAddFoodList.Add(food);
        }

        internal Vector3 GetMapRandomPosition()
        {
            Vector2 posMax = _map.OriginPosition + _map.Size * 0.9f;
            Vector2 posMin = _map.OriginPosition + _map.Size * 0.1f;

            Vector2 pos = new Vector2();

            pos.x = (float)_context.Random.Range((int)posMin.x, (int)posMax.x);
            pos.y = (float)_context.Random.Range((int)posMin.y, (int)posMax.y);
            //Debug.LogFormat("random pos {0} in range {1}~{2}", pos.ToString(), posMin.ToString(), posMax.ToString());
            return pos.ToVector3();
        }

        internal void CreateFoodAt(int foodID, Vector3 pos)
        {
            Food food = new Food();
            food.InitData(foodID, pos);

            tobeAddFoodList.Add(food);
        }

        private void AddFood(Food food)
        {
            _foodList.AddLast(food);
        }

        private bool RemoveFood(Food food)
        {
            return _foodList.Remove(food);
        }
        #endregion
    }
}
