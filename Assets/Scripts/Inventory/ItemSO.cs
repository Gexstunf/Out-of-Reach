using UnityEngine;

[CreateAssetMenu(fileName = "New Item", menuName = "Inventory/Item")]
public class ItemSO : ScriptableObject
{
    public string itemId;
    public string worldPrefabName;
    public string heldPrefabName;
    public Sprite icon;
    public string displayName;
    public ItemType itemType;

    public ItemSO[] internalSlots = new ItemSO[4];
}

public enum ItemType { Weapon, Consumable, Backpack, Other }
