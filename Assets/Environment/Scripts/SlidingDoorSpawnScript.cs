using UnityEngine;

namespace Environment.Scripts {
    public class SlidingDoorSpawnScript : MonoBehaviour
    {
        [Header("References")]
        public Transform[] spawnPoints;
        public GameObject slidingDoorPrefab;
        
        void Start()
        {
            foreach (Transform spawnPoint in spawnPoints) {
                Instantiate(slidingDoorPrefab, spawnPoint.position, spawnPoint.rotation);
            }
        }
    }
}
