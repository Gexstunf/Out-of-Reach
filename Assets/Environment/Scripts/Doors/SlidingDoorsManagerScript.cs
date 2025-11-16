using System.Collections;
using System.Collections.Generic;
using Environment.Scripts.DungeonGeneration.CoreScripts;
using UnityEngine;

namespace Environment.Scripts.Doors {
    public class SlidingDoorsManagerScript : MonoBehaviour {
        [Header("References")] 
        [SerializeField] private DungeonGeneratorScript dungeonGenerator;
        [SerializeField] private int _doorCount = 0;
        
        public bool usePhoton = false;
        public static SlidingDoorsManagerScript Instance;
        
        [Header("Settings")]
        [SerializeField] private string parentName = "DOORS";
        
        private List<GameObject> _spawned = new();

        public void Awake() {

            if (Instance != null) {
                Destroy(gameObject);
            }
            
            Instance = this;
        }
        
        private IEnumerator Start()
        {
            yield return new WaitUntil(() => dungeonGenerator.FinishedGeneration);  
            dungeonGenerator = DungeonGeneratorScript.Instance;
            SlidingDoorSpawnScript[] spawnScripts = FindObjectsByType<SlidingDoorSpawnScript>(FindObjectsSortMode.None);

            foreach (var dScript in spawnScripts) {
                var success = dScript.SpawnSlidingDoor();
                if (success) {
                    _doorCount++;
                    _spawned.AddRange(dScript.slidingDoorInstances);
                }
                else {
                    Debug.LogWarning("Couldnt spawn a sliding door for some reason!");
                }
            }

            PlaceUnderParent(_spawned);
        }
        
        private void PlaceUnderParent(List<GameObject> list) {
            var parent = new GameObject(parentName);
            foreach (var obj in list) {
                obj.transform.SetParent(parent.transform);
            }
        }
    }
}
