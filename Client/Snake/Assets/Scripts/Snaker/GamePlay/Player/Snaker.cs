using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GameProtocol;
using Framework;
using Random = Framework.Random;

namespace GamePlay
{
    public class Snaker : ITick
    {
        protected SnakerActor _view;
        public SnakerActor View
        {
            get{ return _view; }
            set{ _view = value; }
        }

        protected PlayerData _ownerPlayer;
        public PlayerData OwnerPlayer
        {
            get{ return _ownerPlayer; }
        }

        /// <summary>
        /// Gets the snaker type ID.
        /// </summary>
        public int ID
        {
            get{ return _ownerPlayer.snakerData.id; }
        }
        public int Length
        {
            get{ return _ownerPlayer.snakerData.length; }
        }
        public int PlayerID
        {
            get{ return (int)_ownerPlayer.playerID; }
        }
        public int TeamID
        {
            get{ return (int)_ownerPlayer.teamID; }
        }


        protected Dictionary<string, CSVValue> _data;
        public Dictionary<string, CSVValue> Data
        {
            get{ return _data; }
        }

        public int KeyStep
        {
            get{ return Mathi.Max(1, _data["KeyStep"].IntValue); }
        }
        public int EnegyPerNode
        {
            get{ return Mathi.Max(1, _data["Enegy Per Node"].IntValue); }
        }
        public int InitLength
        {
            get{ return Mathi.Max(1, _data["InitLength"].IntValue); }
        }
        public float StepSpeed
        {
            get{ return Mathi.Max(1f, _data["StepSpeed"].FloatValue); }
        }

        protected SnakerHead _head;
        public SnakerHead Head
        {
            get{ return _head; }
        }

        protected SnakerTail _tail;
        public SnakerTail Tail
        {
            get{ return _tail; }
        }

        /// <summary>
        /// Gets the position of snaker head.
        /// </summary>
        public Vector3 Position
        {
            get{ 
                if( Head != null )
                    return Head.Position;
                else
                    return Vector3.zero;
            }
        }

        protected List<SnakerComponent> _components;

        protected int _frame;
        public int CurrentFrame
        {
            get{ return _frame; }
        }


        protected Vector3 _inputDirection;
        protected Vector3 _moveDirection;
        protected float _moveSpeed;

        protected bool _isDead = false;
        public bool IsDead
        {
            get{ return _isDead; }
        }

        public bool IsReleased
        {
            get { return IsDead; }
        }

        public Vector3 MoveDirection
        {
            get{ return _moveDirection; }
        }
        public float MoveSpeed
        {
            get{ return _moveSpeed; }
        }

        // key nodes
        int enegy = 0;
        int pre_length = -1;
        int pre_count = 0;
        SnakerNode hitNode = null;


        public Snaker(PlayerData player, Vector3 initPos, float initSpeed = 0f)
        {
            _ownerPlayer = player;

            // load table
            _data = CSVTableLoader.GetTableContainer("Snaker").GetRow(player.snakerData.id.ToString());

            _head = new SnakerHead();
            _head.InitData(_data["Head"].IntValue, 0, this, initPos);
            _tail = new SnakerTail();
            _tail.InitData(_data["Tail"].IntValue, 0, this, initPos);

            _head.SetNextNode(_tail);
            _tail.SetPrevNode(_head);

            _components = new List<SnakerComponent>();

            if( _ownerPlayer.aiID > 0 ){
                SnakerAI ai = new SnakerAI(this, _ownerPlayer.aiID);
                _components.Add(ai);
            }

            _isDead = false;

            // input
            _moveSpeed = initSpeed;
            _inputDirection = new Vector3();
            _moveDirection = new Vector3(0f, 1f, 0f);

            enegy = 0;
            pre_length = -1;
            pre_count = 0;
            hitNode = null;

            // init snaker nodes
            AddNode( InitLength, initPos );
        }

        public virtual void Release()
        {
            _isDead = true;

            for(int i = 0; i < _components.Count; i++)
            {
                _components[i].Release();
            }
            _components.Clear();

            SnakerNode node = _head;
            while( node != null )
            {
                // release node...
                node.Release();

                node = node.NextNode;
            }
            _head = null;
            _tail = null;
            hitNode = null;

            enegy = 0;
            _ownerPlayer = null;

            _view = null;
        }

        internal void AddNode(int count, Vector3 initPos)
        {
            if( IsDead ) return;

            for(int i = 0; i < count; i++)
            {
                _ownerPlayer.snakerData.length++;

                SnakerNode node = new SnakerNode();
                node.InitData(_data["Body"].IntValue, _ownerPlayer.snakerData.length, this, initPos);

                // add the new node to the previous pos of _tail.
                _tail.PrevNode.SetNextNode(node);
                node.SetNextNode(_tail);
                _tail.SetPrevNode(node);
            }
        }


        public bool IsKeyNode(int index)
        {
            return index % KeyStep == 0;
        }

        /// <summary>
        /// Gets the key node count, exclude the head and tail.
        /// </summary>
        /// <returns>The key node count.</returns>
        public int GetKeyNodeCount()
        {
            if( pre_length != _ownerPlayer.snakerData.length )
            {
                int count = 0;
                for( int i = 0; i < _ownerPlayer.snakerData.length; i++ )
                {
                    if( IsKeyNode(i) )
                        count++;
                }

                pre_length = _ownerPlayer.snakerData.length;
                pre_count = count;
            }
            return pre_count;
        }


        public void MoveTo(Vector3 pos)
        {
            if( IsDead ) 
                return;

            _head.MoveTo(pos);
        }

        public virtual bool InputKey(int vkey, float arg)
        {
            switch(vkey)
            {
            case (int)GameVKey.MoveX:
                _inputDirection.x = arg;
                // 给个初始速度 
                if (_moveSpeed == 0f)
                    _moveSpeed = 1f;
            break;
            case (int)GameVKey.MoveY:
                _inputDirection.y = arg;
                // 给个初始速度 
                if (_moveSpeed == 0f)
                    _moveSpeed = 1f;
            break;
            case (int)GameVKey.SpeedUp:
                _moveSpeed = arg;
            break;
            default:
            return false;
            }
            return true;
        }

        protected void HandleMove()
        {
            for( int i = 0; i < _moveSpeed; i++ )
            {
                if( _inputDirection.x != 0f || _inputDirection.y != 0f || _inputDirection.z != 0f )
                {
                    _moveDirection.x = _inputDirection.x;
                    _moveDirection.y = _inputDirection.y;
                    _moveDirection.z = _inputDirection.z;
                }

                if( _moveDirection.x != 0f || _moveDirection.y != 0f || _moveDirection.z != 0f )
                {
                    Vector3 pos = _head.Position + _moveDirection.normalized * StepSpeed;
                    MoveTo(pos);
                }
            }
        }

        public virtual void EnterFrame(int frame)
        {
            if( _ownerPlayer == null )
                return;

            if( _frame == frame )
                return;

            for(int i = 0; i < _components.Count; i++)
            {
                _components[i].EnterFrame(frame);
            }

            HandleMove();

            CheckCollision();

            _frame = frame;
        }

        protected virtual void CheckCollision()
        {
            if( IsReleased )
                return;

            Rect bound = BattleEngine.Instance.Map.BoundRect;
            if( !bound.Contains(Head.Position.ToVector2()) )
            {
                Dead( BattleEngine.Instance.Map );
                return;
            }

            foreach(var kvs in BattleEngine.Instance.SnakerList)
            {
                if( kvs.Value != this )
                {
                    if( HitTest(this, kvs.Value, ref hitNode) )
                    {
                        Dead(kvs.Value);
                        return;
                    }
                }
            }

            foreach(Food food in BattleEngine.Instance.FoodList)
            {
                if( HitTest(this, food) )
                {
                    EatFood(food);
                }
            }
        }

        /// <summary>
        /// Snaker src collides to other.
        /// </summary>
        public static bool HitTest(Snaker src, Snaker other, ref SnakerNode hitNode, bool ignoreTeam = false)
        {
            hitNode = null;

            if( src == null || other == null || src.IsDead || other.IsDead )
                return false;

            if( ignoreTeam || src.TeamID != other.TeamID )
            {
                SnakerNode node = other.Head.NextNode; // start from Head's next node
                while( node != null )
                {
                    if( node.IsKeyNode() )
                    {
                        float distance = Vector3.Distance(src.Head.Position, node.Position);
                        if( distance <= src.Head.Radius + node.Radius ){
                            hitNode = node;
                            return true;
                        }
                    }
                    node = node.NextNode;
                }
            } 
            return false;
        }

        /// <summary>
        /// Snaker src collides to food.
        /// </summary>
        public static bool HitTest(Snaker src, Food food)
        {
            if( src == null || food == null || src.IsDead || food.IsReleased )
                return false;

            float distance = Vector3.Distance(src.Head.Position, food.Position);
            if( distance <= src.Head.Radius + food.Radius )
                return true;

            return false;
        }

        void AddEnegy(int value)
        {
            enegy += value;

            while( enegy >= EnegyPerNode )
            {
                enegy -= EnegyPerNode;

                AddNode(1, _tail.Position );
            }
        }

        void EatFood(Food food)
        {
            AddEnegy(food.GetEnegy());
            food.OnEaten(this);
        }

        public void Dead(object killer)
        {
            _isDead = true;

            Blast();

            BattleEngine.Instance.OnSnakerDead(this, killer);
        }

        protected virtual void Blast()
        {
            Framework.Random random = BattleEngine.Instance.Context.Random;

            SnakerNode node = _head;
            while( node != null)
            {
                if( node.IsKeyNode() )
                {
                    node.Blast();

                    float blastChange = node.Data["Blast Chance"].FloatValue;
                    int foodID = node.Data["Blast Food"].IntValue;
                    if( foodID > 0 && blastChange > 0f && random.Value() < blastChange )
                    {
                        Rect bound = BattleEngine.Instance.Map.BoundRect;
                        float radius = node.Radius;

                        Vector2 pos = new Vector2();
                        pos.x = node.Position.x + random.Range(-radius, radius);
                        pos.y = node.Position.y + random.Range(-radius, radius);

                        if( bound.Contains(pos) ){
                            BattleEngine.Instance.CreateFoodAt(foodID, pos.ToVector3());
                        }
                    }
                }
                node = node.NextNode;
            }
        }

    }
}
