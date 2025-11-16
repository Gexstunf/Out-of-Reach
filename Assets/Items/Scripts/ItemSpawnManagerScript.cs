using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Environment.Scripts;
using Environment.Scripts.DungeonGeneration.CoreScripts;
using Multiplayer.Inventory;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

namespace Items.Scripts {
    public class ItemSpawnManagerScript : MonoBehaviour {
        [Header("References")] 
        [SerializeField] private DungeonGeneratorScript dungeonGenerator;
        [SerializeField] private ItemDatabaseSO _itemDatabase;
        public static ItemSpawnManagerScript Instance;
        
        [Header("Settings")] 
        [SerializeField] private string parentName = "ITEMS";
        public bool usePhoton;

        [Header("Visualize")] 
        [SerializeField] private int _itemCount;

        private List<GameObject> _spawned = new();
        
        public void Awake() {

            if (Instance != null) {
                Destroy(gameObject);
            }
            
            _itemDatabase = Resources.Load<ItemDatabaseSO>("Databases/ItemDatabase");
            Instance = this;
        }

        public IEnumerator Start() {
            dungeonGenerator = DungeonGeneratorScript.Instance;
            yield return new WaitUntil(() => dungeonGenerator.FinishedGeneration);
            ItemSpawnScript[] spawnScripts = FindObjectsByType<ItemSpawnScript>(FindObjectsSortMode.None);

            foreach (var spawn in spawnScripts) {
                var success = spawn.TrySpawnObject();
                if (success) {
                    _itemCount++;
                    _spawned.Add(spawn.ItemInstance);
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

        public ItemSO ChooseItem(ItemSize maxSize, ItemType[] itemTypes) {
            List<ItemSO> validItems = _itemDatabase.allItems
                .Where(i => i.itemSize <= maxSize && itemTypes.Contains(i.itemType))
                .ToList(); 

            if (validItems.Count == 0) return null;

            ItemSO chosen = validItems[Random.Range(0, validItems.Count)];

            return chosen;
        }
    }
}
