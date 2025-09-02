using System.Collections;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

public class PlayerInventoryPhoton : MonoBehaviourPun
{
    [Header("Inventario")]
    public ItemSO[] slots = new ItemSO[4];
    public int activeSlot = 0;

    [Header("Mano")]
    public Transform handSlot;
    private GameObject currentHeldNetworkObj;
    private int currentHeldViewId = -1;

    [Header("Referencias")]
    public float dropForce = 3f;

    private GameObject currentItemInHand;

    public void EquipItem(int slot)
    {
        ClearHand();

        if (slot >= 0 && slot < slots.Length && slots[slot] != null)
        {
            currentItemInHand = Instantiate(slots[slot], handSlot);
            currentItemInHand.transform.localPosition = Vector3.zero;
            currentItemInHand.transform.localRotation = Quaternion.identity;
        }
    }

    public void ClearHand()
    {
        if (currentItemInHand != null)
        {
            Destroy(currentItemInHand);
            currentItemInHand = null;
        }
    }

    void Start()
    {
        if (photonView.IsMine)
        {
            var ui = FindFirstObjectByType<PlayerUIManager>();
            if (ui != null)
            {
                ui.InitInventory(this);
            }
        }
    }

    public void RequestPickupOnClosest(PhotonView targetItemPV)
    {
        if (targetItemPV == null) return;

        photonView.RPC(nameof(RPC_RequestPickup_Master), RpcTarget.MasterClient, targetItemPV.ViewID, activeSlot);
    }

    [PunRPC]
    public void RPC_RequestPickup_Master(int itemViewID, int slotIndex, PhotonMessageInfo info)
    {
        if (!PhotonNetwork.IsMasterClient) return;

        PhotonView itemPV = PhotonView.Find(itemViewID);
        if (itemPV == null)
        {
            return;
        }

        var netItem = itemPV.GetComponent<NetworkedItem>();
        if (netItem == null || netItem.itemData == null)
        {
            Debug.LogWarning("MasterClient: item sin NetworkedItem o itemData.");
            return;
        }

        string heldPrefabName = netItem.GetHeldPrefabName();

        PhotonNetwork.Destroy(itemPV.gameObject);

        photonView.RPC(nameof(RPC_SpawnHeldOnPlayer), info.Sender, heldPrefabName, slotIndex);
    }

    [PunRPC]
    public void RPC_SpawnHeldOnPlayer(string heldPrefabName, int slotIndex, PhotonMessageInfo info)
    {
        if (!photonView.IsMine) return;

        Vector3 spawnPos = handSlot.position;
        Quaternion spawnRot = handSlot.rotation;
        GameObject held = PhotonNetwork.Instantiate(heldPrefabName, spawnPos, spawnRot);
        held.transform.SetParent(handSlot, true);

        currentHeldNetworkObj = held;
        currentHeldViewId = held.GetComponent<PhotonView>()?.ViewID ?? -1;

        ItemSO itemData = ItemDatabase.FindByHeldPrefabName(heldPrefabName);
        slots[slotIndex] = itemData;
    }

    public void EquipFromSlot(int slotIndex)
    {
        if (!photonView.IsMine) return;
        if (slots[slotIndex] == null) return;

        if (currentHeldNetworkObj != null)
        {
            HolsterCurrent();
        }

        string heldPrefabName = slots[slotIndex].heldPrefabName;
        GameObject held = PhotonNetwork.Instantiate(heldPrefabName, handSlot.position, handSlot.rotation);
        held.transform.SetParent(handSlot, true);
        currentHeldNetworkObj = held;
        currentHeldViewId = held.GetComponent<PhotonView>()?.ViewID ?? -1;
    }

    public void HolsterCurrent()
    {
        if (!photonView.IsMine) return;
        if (currentHeldNetworkObj != null)
        {
            PhotonNetwork.Destroy(currentHeldNetworkObj);
            currentHeldNetworkObj = null;
            currentHeldViewId = -1;
        }
    }

    public void DropCurrent(int slotIndex)
    {
        if (!photonView.IsMine) return;
        if (slots[slotIndex] == null) return;
        if (currentHeldNetworkObj == null) return;

        var item = slots[slotIndex];
        string worldPrefabName = item.worldPrefabName;

        PhotonNetwork.Destroy(currentHeldNetworkObj);
        currentHeldNetworkObj = null;
        currentHeldViewId = -1;

        Vector3 dropPos = handSlot.position + transform.forward * 1.2f + Vector3.up * 0.3f;
        GameObject worldObj = PhotonNetwork.Instantiate(worldPrefabName, dropPos, Quaternion.identity);

        Rigidbody rb = worldObj.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = false;
            rb.AddForce(transform.forward * dropForce + Vector3.up * 1.2f, ForceMode.Impulse);
        }

        slots[slotIndex] = null;
    }
}
