using System.Collections.Generic;
using UnityEngine;

public class ItemDatabase : MonoBehaviour
{
    public static ItemDatabase Instance;
    public ItemSO[] allItems;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public static ItemSO FindByHeldPrefabName(string heldName)
    {
        if (Instance == null) return null;
        foreach (var it in Instance.allItems)
            if (it != null && it.heldPrefabName == heldName) return it;
        return null;
    }

    public static ItemSO FindByWorldPrefabName(string worldName)
    {
        if (Instance == null) return null;
        foreach (var it in Instance.allItems)
            if (it != null && it.worldPrefabName == worldName) return it;
        return null;
    }
}
