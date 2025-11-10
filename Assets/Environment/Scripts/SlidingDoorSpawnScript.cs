using UnityEngine;
using Photon.Pun;
namespace Environment.Scripts {
    public class SlidingDoorSpawnScript : MonoBehaviourPun
    {
        [Header("References")]
        public Transform[] spawnPoints;
        public GameObject slidingDoorPrefab;
        public GameObject slidingDoorInstance;


        [SerializeField] private static bool UsePhoton = true;
        
        void Start()
        {
            if (UsePhoton && !PhotonNetwork.IsMasterClient) return;
            foreach (Transform spawnPoint in spawnPoints) {

                if (UsePhoton) 
                    slidingDoorInstance = PhotonNetwork.Instantiate(slidingDoorPrefab.name, spawnPoint.position, spawnPoint.rotation);
                else
                    slidingDoorInstance = Instantiate(slidingDoorPrefab, spawnPoint.position, spawnPoint.rotation);
            }
        }
    }
}
