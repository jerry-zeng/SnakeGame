using System.Collections;
using System.Collections.Generic;
using Framework;

namespace GamePlay
{
    public class MapScript 
    {
        protected GameMap _owner;

        int TickInterval_Food = BattleEngine.TICKS_PER_SECONDS;
        int TickInterval_Snaker = BattleEngine.TICKS_PER_SECONDS * 5;
        int Threshold_Food = 30;
        int Threshold_Snaker = 5;
        List<int> randomFoodList;
        List<int> randomSnakerList;

        int nextTickFrame_Food = 0;
        int nextTickFrame_Snaker = 0;

        public MapScript(GameMap owner)
        {
            _owner = owner;

            Threshold_Food = _owner.Data["Food Threshold"].IntValue;
            TickInterval_Food = (int)(BattleEngine.TICKS_PER_SECONDS * _owner.Data["Food Interval"].FloatValue);
            Threshold_Snaker = _owner.Data["Snaker Threshold"].IntValue;
            TickInterval_Snaker = (int)(BattleEngine.TICKS_PER_SECONDS * _owner.Data["Snaker Interval"].FloatValue);
        
            randomFoodList = new List<int>(_owner.Data["Food List"].IntArrayValue);
            randomSnakerList = new List<int>(_owner.Data["Snaker List"].IntArrayValue);

            if( randomFoodList.Count == 0 )
                this.LogError("No food to generate?");
            if( randomSnakerList.Count == 0 )
                this.LogError("No snaker ai to generate?");
        }

        public virtual void Release()
        {
            _owner = null;
            nextTickFrame_Food = 0;
            nextTickFrame_Snaker = 0;
        }

        public virtual void EnterFrame(int frame)
        {
            if( _owner == null )
                return;

            if( nextTickFrame_Food == 0 || frame >= nextTickFrame_Food )
            {
                nextTickFrame_Food = frame + TickInterval_Food;

                // generate food
                if( randomFoodList.Count > 0 && BattleEngine.Instance.FoodList.Count < Threshold_Food )
                {
                    int index = BattleEngine.Instance.Context.Random.Range(0, randomFoodList.Count);
                    BattleEngine.Instance.CreateRandomFood(randomFoodList[index]);
                }
            }

            if( nextTickFrame_Snaker == 0 || frame >= nextTickFrame_Snaker )
            {
                nextTickFrame_Snaker = frame + TickInterval_Snaker;

                // generate snaker ai
                if( randomSnakerList.Count > 0 && BattleEngine.Instance.SnakerList.Count < Threshold_Snaker )
                {
                    int index = BattleEngine.Instance.Context.Random.Range(0, randomSnakerList.Count);
                    BattleEngine.Instance.CreateRandomSnaker(randomSnakerList[index]);
                }

            }
        }
    }
}
