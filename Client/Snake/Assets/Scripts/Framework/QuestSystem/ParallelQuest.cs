
namespace Framework.QuestSystem
{
    /*
    *    平行任务，所有子任务同时执行.
    */
    public abstract class ParallelQuest : CompositeQuest
    {

        protected override void OnStart()
        {
            base.OnStart();

            foreach(Quest quest in _subQuests)
            {
                quest.Start();
                OnSubQuestStart(quest);
            }
        }

        protected override void OnSubQuestStart(Quest quest)
        {
            
        }

        protected override void CheckCompleted()
        {
            foreach(Quest quest in _subQuests)
            {
                if( quest.HasCompleted() ) 
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
