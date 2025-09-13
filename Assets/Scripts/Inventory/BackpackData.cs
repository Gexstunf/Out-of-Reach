using Photon.Pun;
using UnityEngine;

public class BackpackData : MonoBehaviourPun
{
    // Inventario por instancia (4 slots)
    public ItemSO[] internalSlots = new ItemSO[4];

    // Devuelve IDs (string) para serializar / enviar por RPC
    public string[] GetItemIds()
    {
        string[] ids = new string[internalSlots.Length];
        for (int i = 0; i < internalSlots.Length; i++)
            ids[i] = internalSlots[i] != null ? internalSlots[i].itemId : "";
        return ids;
    }

    // Inicializa desde IDs (llamado localmente o por RPC)
    public void SetFromIds(string[] ids)
    {
        if (ids == null) return;
        for (int i = 0; i < internalSlots.Length && i < ids.Length; i++)
        {
            internalSlots[i] = string.IsNullOrEmpty(ids[i]) ? null : ItemDatabase.FindById(ids[i]);
        }
    }

    // RPC para inicializar datos en todos los clientes
    [PunRPC]
    public void RPC_InitContents(string[] ids)
    {
        SetFromIds(ids);
    }

    // Intentar poner item en el primer slot libre
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

    // Sacar item de un slot
    public ItemSO RemoveItem(int index)
    {
        if (index < 0 || index >= internalSlots.Length) return null;
        ItemSO it = internalSlots[index];
        internalSlots[index] = null;
        return it;
    }
}
