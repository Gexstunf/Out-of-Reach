using Multiplayer.Inventory;
using Photon.Pun;
using UnityEngine;

public class BackpackData : MonoBehaviourPun
{
    public ItemSO[] internalSlots = new ItemSO[4];

    void Awake()
    {
        if (internalSlots == null || internalSlots.Length == 0)
            internalSlots = new ItemSO[4];
    }

    public string[] GetItemIds()
    {
        string[] ids = new string[internalSlots.Length];
        for (int i = 0; i < internalSlots.Length; i++)
            ids[i] = internalSlots[i] != null ? internalSlots[i].itemId : "";
        return ids;
    }

    public void SetFromIds(string[] ids)
    {
        if (ids == null) return;
        for (int i = 0; i < internalSlots.Length && i < ids.Length; i++)
        {
            internalSlots[i] = string.IsNullOrEmpty(ids[i]) ? null : ItemDatabase.FindById(ids[i]);
        }
    }

    [PunRPC]
    public void RPC_InitContents(string[] ids)
    {
        SetFromIds(ids);
    }

    public bool TryAddItem(ItemSO item)
    {
        if (item == null || item.itemType == ItemType.Backpack) return false;
        for (int i = 0; i < internalSlots.Length; i++)
        {
            if (internalSlots[i] == null)
            {
                internalSlots[i] = item;
                return true;
            }
        }
        return false;
    }

    public ItemSO RemoveItem(int index)
    {
        if (index < 0 || index >= internalSlots.Length) return null;
        ItemSO it = internalSlots[index];
        internalSlots[index] = null;
        return it;
    }
}
