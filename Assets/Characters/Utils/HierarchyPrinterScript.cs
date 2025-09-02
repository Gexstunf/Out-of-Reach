using System.Text;
using UnityEngine;

namespace Characters.Utils {
    public class HierarchyPrinterScript : MonoBehaviour
    {
        [Header("Settings")]
        public Transform root; 
        public bool printComponents = false;

        [ContextMenu("Print Hierarchy")]
        void PrintHierarchy()
        {
            if (root == null)
            {
                Debug.LogWarning("Root not assigned!");
                return;
            }

            StringBuilder sb = new StringBuilder();
            BuildHierarchyString(root, 0, sb);
            Debug.Log(sb.ToString());
        }

        void BuildHierarchyString(Transform parent, int depth, StringBuilder sb)
        {
            string indent = new string(' ', depth * 2); // 2 spaces per depth
            sb.AppendLine(indent + "• " + parent.name);

            if (printComponents)
            {
                Component[] components = parent.GetComponents<Component>();
                foreach (var comp in components)
                {
                    if (comp == null) continue;
                    sb.AppendLine(indent + "   - " + comp.GetType().Name);
                }
            }

            foreach (Transform child in parent)
            {
                BuildHierarchyString(child, depth + 1, sb);
            }
        }
    }
}