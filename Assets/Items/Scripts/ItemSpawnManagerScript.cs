using System.Collections.Generic;
using System.Linq;
using Multiplayer.Inventory;
using UnityEngine;

namespace Items.Scripts {
    public class ItemSpawnManagerScript
    {
        [SerializeField] private ItemDatabaseSO itemDatabase;

        public ItemSpawnManagerScript(ItemDatabaseSO database) {
            itemDatabase = database;
        }
        
        public ItemSO ChooseItem(ItemSize maxSize, ItemType[] itemTypes) {
            List<ItemSO> validItems = itemDatabase.allItems
                .Where(i => i.itemSize <= maxSize && itemTypes.Contains(i.itemType))
                .ToList(); 

            if (validItems.Count == 0) return null;

            ItemSO chosen = validItems[Random.Range(0, validItems.Count)];

            return chosen;
        }
    }
}
