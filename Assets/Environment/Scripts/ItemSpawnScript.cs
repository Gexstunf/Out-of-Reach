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
        
        public void Awake() {
            itemDatabase = Resources.Load<ItemDatabaseSO>("Databases/ItemDatabase");
            _itemManager = new ItemSpawnManagerScript(itemDatabase);
        }


        public void Start() {
            _itemData = _itemManager.ChooseItem(maxItemSize, allowedTypes);

            if (_itemData == null) {
                Debug.Log("No data");
            }
            else {
                Instantiate(_itemData.prefab, transform.position, transform.rotation);
            }
        }
        
        private void Reset() {
            allowedTypes = new ItemType[] { ItemType.Weapon, ItemType.Consumable };
        }

    }
}
