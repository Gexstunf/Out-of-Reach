using System;
using Items.Scripts;
using Multiplayer.Inventory;
using Photon.Pun;
using UnityEngine;
using UnityEngine.Serialization;

namespace Environment.Scripts {
    public class ItemSpawnScript : MonoBehaviour
    {
        [Header("Settings")]
        public ItemSize maxItemSize = ItemSize.Medium;
        public ItemType[] allowedTypes;
        public ItemSO[] specialItems;

        private ItemSpawnManagerScript _itemManager;
        private ItemSO _itemData;
        private bool _failedSpawn;
        
        public GameObject ItemInstance { get; private set; }
        
        public bool TrySpawnObject() {
            _itemManager = ItemSpawnManagerScript.Instance;
            _itemData = _itemManager.ChooseItem(maxItemSize, allowedTypes);
            if (specialItems.Length != 0) {
                int random = UnityEngine.Random.Range(0, specialItems.Length);
                _itemData = specialItems[random];
            };
            if (_itemData == null) return false;

            float spawnRandomNum = UnityEngine.Random.Range(0F, 1F);
            if (spawnRandomNum < _itemData.spawnChance) {
                
                if (_itemManager.usePhoton) 
                    ItemInstance = PhotonNetwork.Instantiate(_itemData.prefab.name, transform.position, transform.rotation);
                else 
                    ItemInstance = Instantiate(_itemData.prefab, transform.position, transform.rotation);
                
                return true;
            }
            
            _failedSpawn = true;
            return false;
        }
        
        private void Reset() {
            allowedTypes = new[] { ItemType.Weapon, ItemType.Consumable };
        }

        private void OnDrawGizmos() {
            if (_itemData != null && !_failedSpawn) return;
            
            Gizmos.color = _failedSpawn ? Color.yellow : Color.red;
            Gizmos.DrawWireSphere(transform.position, 0.4f);
        }
    }
}
