
using System;
using UnityEngine;

namespace Framework
{
    /// <summary>
    /// 只在编辑器下出现，分别对应一个Loader~生成一个GameObject对象，为了方便调试！
    /// </summary>
    public class BaseLoaderDebugger : MonoBehaviour
    {
        public BaseLoader Loader;
        public int RefCount;
        public float FinishUsedTime; // 参考，完成所需时间
        public static bool IsApplicationQuit = false;
        const string bigType = "BaseLoaderDebuger";

        public static BaseLoaderDebugger Create(string type, string url, BaseLoader loader)
        {
            if (IsApplicationQuit) return null;

            Func<string> getName = () => string.Format("{0}-{1}-{2}", type, url, loader.Desc);

            var newHelpGameObject = new GameObject(getName());
            DebuggerObjectTool.SetParent(bigType, type, newHelpGameObject);
            var newHelp = newHelpGameObject.AddComponent<BaseLoaderDebugger>();
            newHelp.Loader = loader;

            loader.SetDescEvent += (newDesc) =>
            {
                if (loader.RefCount > 0)
                    newHelpGameObject.name = getName();
            };


            loader.DisposeEvent += () =>
            {
                if (!IsApplicationQuit)
                    DebuggerObjectTool.RemoveFromParent(bigType, type, newHelpGameObject);
            };

            return newHelp;
        }

        private void Update()
        {
            RefCount = Loader.RefCount;
            FinishUsedTime = Loader.CostTime;
        }

        private void OnApplicationQuit()
        {
            IsApplicationQuit = true;
        }

    }

}
