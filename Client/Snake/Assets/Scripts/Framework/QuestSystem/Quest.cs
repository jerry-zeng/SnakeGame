
namespace Framework.QuestSystem
{
    /*
    *    任务基类base quest class
    */
    public class Quest 
    {
        public enum QuestState
        {
            Inited = 0,
            Started,
            Completed
        }

        protected QuestState _state;
        public QuestState State{ get{ return _state; } }

        protected string _type;
        public string Type{ get{ return _type; } }

        protected int _id;
        public int ID{ get{ return _id; } protected set{ _id = value; } }

        public Quest()
        {
            _id = 0;
            _type = "unknown";

            OnInit();
        }

        public Quest(int id, string type)
        {
            _id = id;
            _type = type;

            OnInit();
        }

        protected virtual void OnInit()
        {
            _state = QuestState.Inited;
        }

        public bool HasStarted()
        {
            return _state == QuestState.Started;
        }

        public void Start()
        {
            if( !HasStarted() && !HasCompleted() )
            {
                _state = QuestState.Started;
                OnStart();
            }
            else
            {
                this.LogError("This quest has started or already completed, but you want to start it.");
            }
        }

        protected virtual void OnStart()
        {
            this.Log( "({0})Quest {1} started.", _type.ToString(), _id );
        }


        public bool HasCompleted()
        {
            return _state == QuestState.Completed;
        }

        public void Complete()
        {
            if( HasStarted() && !HasCompleted() )
            {
                _state = QuestState.Completed;
                OnCompleted();
            }
            else
            {
                this.LogError("This quest hasn't started or already completed, but you want to complete it.");
            }
        }

        protected virtual void OnCompleted()
        {
            this.Log( "({0})Quest {1} completed.", _type.ToString(), _id );
        }
    }
}
