
namespace Framework.QuestSystem
{
    /*
    *    链状任务，所有子任务按顺序一个一个执行.
    */
    public abstract class SerialQuest : CompositeQuest 
    {
        protected int _curQuestIndex = -1;
        public int CurrentQuestIndex { get{ return _curQuestIndex; } }


        protected override void OnStart()
        {
            base.OnStart();

            _curQuestIndex = 0;

            Quest quest = _subQuests[_curQuestIndex];
            quest.Start();
            OnSubQuestStart(quest);
        }


        protected override void OnSubQuestStart(Quest quest)
        {
            
        }

        protected override void CheckCompleted()
        {
            for(int i = _curQuestIndex; i < _subQuests.Count; i++)
            {
                Quest quest = _subQuests[i];
                if( quest.HasCompleted() || quest.HasStarted() ) 
                    continue;
                
                quest.Start();
                _curQuestIndex = i;
                OnSubQuestStart(quest);
                return;
            }

            Complete();
        }

        protected override void OnSubQuestCompleted(Quest quest)
        {
            CheckCompleted();
        }
    }
}
