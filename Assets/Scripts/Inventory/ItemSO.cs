using UnityEngine;

[CreateAssetMenu(fileName = "New Item", menuName = "Inventory/Item")]
public class ItemSO : ScriptableObject
{
    public string itemId;
    public string worldPrefabName;
    public string heldPrefabName;
    public Sprite icon;
    public string displayName;
}
