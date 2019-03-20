
using System.Collections.Generic;

namespace Framework.QuestSystem
{
    /*
    *    组合任务composited quest
    */
    public abstract class CompositeQuest : Quest
    {
        protected List<Quest> _subQuests = null;

        protected override void OnInit()
        {
            base.OnInit();

            InitSubQuests();
        }

        // 子类必须实现该方法.
        protected abstract void InitSubQuests();

        protected abstract void OnSubQuestStart(Quest quest);

        protected abstract void CheckCompleted();

        protected abstract void OnSubQuestCompleted(Quest quest);
    }
}
