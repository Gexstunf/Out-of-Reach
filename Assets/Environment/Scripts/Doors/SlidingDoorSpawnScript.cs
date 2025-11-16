using Photon.Pun;
using UnityEngine;

namespace Environment.Scripts.Doors {
    public class SlidingDoorSpawnScript : MonoBehaviourPun
    {
        [Header("References")]
        public Transform[] spawnPoints;
        public GameObject slidingDoorPrefab;
        public GameObject[] slidingDoorInstances;
        
        public bool SpawnSlidingDoor() {
            slidingDoorInstances = new GameObject[spawnPoints.Length];

            for (int i = 0; i < spawnPoints.Length; i++) {
                Transform spawnPoint = spawnPoints[i];
                GameObject inst;

                if (SlidingDoorsManagerScript.Instance.usePhoton)
                    inst = PhotonNetwork.Instantiate(slidingDoorPrefab.name, spawnPoint.position, spawnPoint.rotation);
                else
                    inst = Instantiate(slidingDoorPrefab, spawnPoint.position, spawnPoint.rotation);

                slidingDoorInstances[i] = inst;
            }

            foreach (var door in slidingDoorInstances) {
                if (door == null) return false;
            }
            
            return true;
        }
    }
}
