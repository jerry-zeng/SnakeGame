using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GameProtocol;

namespace GamePlay
{
    public class SnakerAI : SnakerComponent
    {
        public enum State
        {
            Wait = 0,
            Wander,
            SearchFood,
        }

        protected int _aiType;
        protected State _curState;
        protected int curStateEndFrame;
        protected int _curFrame;
        protected Food _curTarget;
        float tempNearestDistance = float.MaxValue;
        int _preChangeDirFrame = 0;

        int ChangeDirectionInterval = BattleEngine.TICKS_PER_SECONDS * 1;
        float SearchFoodRange = 150f;
        GameMap map;
        Vector3 tempVector;
        Vector3 nextPos;
        SnakerNode hitNode;


        public SnakerAI(Snaker owner) : base(owner)
        {
            _aiType = 0;
            _curState = State.Wait;
            _curFrame = 0;
            curStateEndFrame = 0;
        } 

        public SnakerAI(Snaker owner, int aiType) : base(owner)
        {
            _aiType = aiType;
            _curState = State.Wait;
            _curFrame = 0;
            curStateEndFrame = 0;
            _curTarget = null;
        }


        public override void Release()
        {
            base.Release();
            _aiType = 0;
            _curState = State.Wait;

            _owner = null;
            map = null;
            hitNode = null;
        }

        public override void EnterFrame(int frame)
        {
            if( _owner == null || _owner.IsDead )
                return;

            _curFrame = frame;

            switch( _curState )
            {
            case State.Wait:
                if( _curFrame >= curStateEndFrame ){
                    Wander(BattleEngine.TICKS_PER_SECONDS * 5);
                }
            break;

            case State.Wander:
                if( _curFrame >= curStateEndFrame )
                {
                    SearchFood(BattleEngine.TICKS_PER_SECONDS * 10);
                }
                else
                {
                    if( _preChangeDirFrame == 0 || _curFrame - _preChangeDirFrame >= ChangeDirectionInterval )
                    {
                        Framework.Random random = BattleEngine.Instance.Context.Random;
                        if( random.Value() < 0.5f ){
                            _owner.InputKey((int)GameVKey.MoveX, random.Range(-1f, 1f));
                        }
                        if( random.Value() < 0.5f ){
                            _owner.InputKey((int)GameVKey.MoveY, random.Range(-1f, 1f));
                        }
                        RecordChangeDirectionFrame();
                    }
                }
            break;

            case State.SearchFood:
                if( _curFrame >= curStateEndFrame )
                {
                    ResetTarget();
                    Wander(BattleEngine.TICKS_PER_SECONDS * 10);
                }
                else
                {
                    if( _curTarget == null || _curTarget.IsReleased )
                    {
                        FindNearestFood();

                        if( _curTarget == null || _curTarget.IsReleased ){
                            ResetTarget();
                            Wander(BattleEngine.TICKS_PER_SECONDS * 5);
                        }
                        else{
                            Vector3 dir = _curTarget.Position - _owner.Head.Position;
                            dir = dir.normalized;

                            _owner.InputKey( (int)GameVKey.MoveX, dir.x );
                            _owner.InputKey( (int)GameVKey.MoveY, dir.y );

                            RecordChangeDirectionFrame();
                        }
                    }
                }
            break;
            default:
            break;
            }

            CheckMoveDirection();
        }

        protected Vector3 GetNextPosition(Vector3 moveDir)
        {
            Vector3 next = _owner.Head.Position;
            for( int i = 0; i < _owner.MoveSpeed; i++ )
            {
                next = nextPos + moveDir.normalized * _owner.StepSpeed;
            }
            return next;
        }

        protected bool CheckNextOutBound( Vector3 nextPos, ref Vector3 outValue )
        {
            outValue.x = outValue.y = outValue.z = 0f;

            map = BattleEngine.Instance.Map;

            if( nextPos.x < map.BoundRect.xMin )
                outValue.x = nextPos.x - map.BoundRect.xMin;
            if( nextPos.x > map.BoundRect.xMax )
                outValue.x = nextPos.x - map.BoundRect.xMax;

            if( nextPos.y < map.BoundRect.yMin )
                outValue.y = nextPos.y - map.BoundRect.yMin;
            if( nextPos.y > map.BoundRect.yMax )
                outValue.y = nextPos.y - map.BoundRect.yMax;

            return outValue.x != 0f || outValue.y != 0f || outValue.z != 0f;
        }

        protected bool CheckNextHitSnaker( Vector3 nextPos, ref Vector3 outValue )
        {
            outValue.x = outValue.y = outValue.z = 0f;

            foreach(var kvs in BattleEngine.Instance.SnakerList)
            {
                if( kvs.Value != _owner )
                {
                    if( Snaker.HitTest(_owner, kvs.Value, ref hitNode) )
                    {
                        outValue = nextPos - hitNode.Position;
                        return true;
                    }
                }
            }
            return false;
        }

        protected void CheckMoveDirection()
        {
            nextPos = GetNextPosition( _owner.MoveDirection );

            if( CheckNextOutBound(nextPos, ref tempVector) || CheckNextHitSnaker(nextPos, ref tempVector) )
            {
                float outX = tempVector.x;
                float outY = tempVector.y;

                if( outX != 0f && outY == 0f ) // x dir out
                {
                    _owner.InputKey( (int)GameVKey.MoveX, -_owner.MoveDirection.x );
                }
                else if( outX == 0f && outY != 0f ) // y dir out
                {
                    _owner.InputKey( (int)GameVKey.MoveY, -_owner.MoveDirection.y );
                }
                else // both
                {
                    _owner.InputKey( (int)GameVKey.MoveX, -_owner.MoveDirection.x );
                    _owner.InputKey( (int)GameVKey.MoveY, -_owner.MoveDirection.y );
                }

                RecordChangeDirectionFrame();
            }

        }

        protected void FindNearestFood()
        {
            foreach(Food food in BattleEngine.Instance.FoodList)
            {
                if( food.IsReleased ) continue;

                float distance = Vector3.Distance(_owner.Head.Position, food.Position);
                if( distance <= SearchFoodRange )
                {
                    if( _curTarget == null ){
                        _curTarget = food;
                        tempNearestDistance = distance;
                    }
                    else{
                        if( distance < tempNearestDistance ){
                            _curTarget = food;
                            tempNearestDistance = distance;
                        }
                    }
                }
            }

        }

        protected void Wander(int frames)
        {
            _curState = State.Wander;
            curStateEndFrame = _curFrame + frames;
        }

        protected void SearchFood(int frames)
        {
            _curState = State.SearchFood;
            curStateEndFrame = _curFrame + frames;
        }
        protected void ResetTarget()
        {
            _curTarget = null;
            tempNearestDistance = float.MaxValue;
        }

        protected void RecordChangeDirectionFrame()
        {
            _preChangeDirFrame = _curFrame;
        }
    }
}
