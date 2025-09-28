using Multiplayer.Inventory;
using UnityEngine;

namespace Items.Scripts {
    [CreateAssetMenu(fileName = "ItemDatabase", menuName = "Inventory/ItemDatabase")]
    public class ItemDatabaseSO : ScriptableObject {
        public ItemSO[] allItems;
    }
}