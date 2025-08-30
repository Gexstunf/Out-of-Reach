using Photon.Pun;
using UnityEngine;

public class NetworkedItem : MonoBehaviourPun
{
    public ItemSO itemData;

    public string GetWorldPrefabName() => itemData != null ? itemData.worldPrefabName : gameObject.name;
    public string GetHeldPrefabName() => itemData != null ? itemData.heldPrefabName : null;
}
