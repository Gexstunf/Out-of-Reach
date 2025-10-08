using UnityEngine;

namespace Environment.Scripts {
    public class SlidingDoorSpawnScript : MonoBehaviour
    {
        [Header("References")]
        public Transform[] spawnPoints;
        public GameObject slidingDoorPrefab;
        public GameObject slidingDoorInstance;
        
        void Start()
        {
            foreach (Transform spawnPoint in spawnPoints) {
                slidingDoorInstance = Instantiate(slidingDoorPrefab, spawnPoint.position, spawnPoint.rotation);
            }
        }
    }
}
