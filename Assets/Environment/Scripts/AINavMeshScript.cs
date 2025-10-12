using System.Collections;
using Unity.AI.Navigation;
using UnityEngine;

namespace Environment.Scripts {
    public class AINavMeshScript : MonoBehaviour
    {
        public NavMeshSurface surface;      
        
        public void BuildNavMesh()
        { 
            surface.BuildNavMesh();
            Debug.Log("NavMesh built at runtime.");
        }
    }
}
