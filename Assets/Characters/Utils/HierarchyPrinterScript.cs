using System.Text;
using UnityEngine;

namespace Characters.Utils {
    public class HierarchyPrinterScript : MonoBehaviour
    {
    
        public Transform root; 
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

            foreach (Transform child in parent)
            {
                BuildHierarchyString(child, depth + 1, sb);
            }
        }
    }
}
