using UnityEngine;

namespace Framework
{
    /// <summary>
    /// 对XXXLoader的结果Asset进行Debug显示
    /// </summary>
    public class LoadedAssetDebugger : MonoBehaviour
    {
        public string memorySize;
        public Object theObject;
        public string type;

        private const string bigType = "LoadedAssetDebugger";
        private bool isRemoveFromParent = false;

        public static LoadedAssetDebugger Create(string type, string uniqueKey, Object theObject)
        {
            if (BaseLoaderDebugger.IsApplicationQuit) return null;

            //simplified uniqueKey
            uniqueKey = uniqueKey.Replace(AssetConfig.GetWritablePath(), "").Replace(AssetConfig.GetStreamingAssetsPath(), "").Replace(AssetConfig.GameAssetsFolder, "");

            // create a LoadedAssetDebugger
            GameObject newHelpGameObject = new GameObject(uniqueKey);
            var newHelp = newHelpGameObject.AddComponent<LoadedAssetDebugger>();
            newHelp.type = type;
            newHelp.theObject = theObject;
            newHelp.memorySize = string.Format("{0:F5}KB",
#if UNITY_2018_1_OR_NEWER
                UnityEngine.Profiling.Profiler.GetRuntimeMemorySizeLong(theObject) / 1024f
#elif UNITY_5_5 || UNITY_2017_1_OR_NEWER
				UnityEngine.Profiling.Profiler.GetRuntimeMemorySize(theObject) / 1024f
#else
                UnityEngine.Profiler.GetRuntimeMemorySize(theObject) / 1024f
#endif
			);

            // add to hierarchy
            DebuggerObjectTool.SetParent(bigType, type, newHelpGameObject);

            return newHelp;
        }

        private void Update()
        {
            if (theObject == null && !isRemoveFromParent)
            {
                DebuggerObjectTool.RemoveFromParent(bigType, type, gameObject);
                isRemoveFromParent = true;
            }
        }

        // 可供调试删资源
        private void OnDestroy()
        {
            if (!isRemoveFromParent)
            {
                DebuggerObjectTool.RemoveFromParent(bigType, type, gameObject);
                isRemoveFromParent = true;
            }
        }
    }
}
