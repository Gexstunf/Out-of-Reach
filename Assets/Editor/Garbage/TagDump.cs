using UnityEditor;
using UnityEngine;

namespace Garbage {
    public class TagDump
    {
        [MenuItem("Tools/Dump All Tags")]
        public static void DumpTags()
        {
            var allTags = UnityEditorInternal.InternalEditorUtility.tags;
            Debug.Log("===== TAG DUMP START =====");
            foreach (var tag in allTags)
            {
                Debug.Log(tag);
            }
            Debug.Log("===== TAG DUMP END =====");
        }
    }
}