using UnityEngine;

namespace Multiplayer.Inventory {
    [CreateAssetMenu(fileName = "NewItem", menuName = "Inventory/Item")]
    public class ItemSO : ScriptableObject
    {
        public GameObject prefab;
        public string itemId;
        public string worldPrefabName;
        public string heldPrefabName;
        public Sprite icon;
        public string displayName;
        public ItemType itemType;
        public ItemSize itemSize;
        public ItemSO[] internalSlots = new ItemSO[4];
    }

    public enum ItemType { Weapon, Consumable, Backpack, Interactable }
    public enum ItemSize { Small, Medium, Big, Huge }

}