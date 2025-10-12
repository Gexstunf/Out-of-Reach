using System;
using Items.Scripts;
using Multiplayer.Inventory;
using UnityEngine;
using UnityEngine.Serialization;

namespace Environment.Scripts {
    public class ItemSpawnScript : MonoBehaviour
    {
        [Header("Settings")] 
        public ItemDatabaseSO itemDatabase;
        
        [Header("Spawn Properties")]
        public ItemSize maxItemSize = ItemSize.Medium;
        public ItemType[] allowedTypes;

        [HideInInspector] public bool isOccupied = false;

        private ItemSpawnManagerScript _itemManager;
        private ItemSO _itemData;
        
        private bool _hasData = true;
        private bool _failedSpawnChance;
        
        public void Awake() {
            itemDatabase = Resources.Load<ItemDatabaseSO>("Databases/ItemDatabase");
            _itemManager = new ItemSpawnManagerScript(itemDatabase);
        }


        public void Start() {
            _itemData = _itemManager.ChooseItem(maxItemSize, allowedTypes);

            if (_itemData == null) {
                _hasData = false;
                return;
            }
            
            float spawnRandomNum = UnityEngine.Random.Range(0f, 1f);

            if (spawnRandomNum < _itemData.spawnChance) {
                Instantiate(_itemData.prefab, transform.position, transform.rotation);
            }
            else {
                Debug.Log("Failed spawn chance");
                _failedSpawnChance = true;
            }
        }
        
        private void Reset() {
            allowedTypes = new ItemType[] { ItemType.Weapon, ItemType.Consumable };
        }

        private void OnDrawGizmos() {
            if (_hasData && !_failedSpawnChance) return;
            
            Gizmos.color = _failedSpawnChance ? Color.yellow : Color.red;
            Gizmos.DrawWireSphere(transform.position, 0.4f);
        }
    }
}
