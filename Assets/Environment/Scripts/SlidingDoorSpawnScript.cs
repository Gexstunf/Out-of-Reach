using System.Linq;
using UnityEngine;
using Photon.Pun;
using UnityEngine.Serialization;

namespace Environment.Scripts {
    public class SlidingDoorSpawnScript : MonoBehaviourPun
    {
        [Header("References")]
        public Transform[] spawnPoints;
        public GameObject slidingDoorPrefab;
        public GameObject[] slidingDoorInstances;


        [SerializeField] private static bool UsePhoton = false;
        
        void Start() {
            slidingDoorInstances = new GameObject[spawnPoints.Length];

            if (UsePhoton && !PhotonNetwork.IsMasterClient)
                return;

            for (int i = 0; i < spawnPoints.Length; i++) {
                Transform spawnPoint = spawnPoints[i];
                GameObject inst;

                if (UsePhoton)
                    inst = PhotonNetwork.Instantiate(slidingDoorPrefab.name, spawnPoint.position, spawnPoint.rotation);
                else
                    inst = Instantiate(slidingDoorPrefab, spawnPoint.position, spawnPoint.rotation);

                slidingDoorInstances[i] = inst;
            }
        }
    }
}
