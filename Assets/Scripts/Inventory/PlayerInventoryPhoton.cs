using System.Collections;
using Characters.PlayerController.Scripts.Input;
using Photon.Pun;
using Photon.Realtime;
using UI;
using UnityEngine;

public class PlayerInventoryPhoton : MonoBehaviourPun
{
    [Header("Referencia")]
    public PlayerInputScript inputScript;

    [Header("Inventario")]
    public ItemSO[] slots = new ItemSO[4];
    public int activeSlot = 0;

    [Header("Mano")]
    public Transform handSlot;
    private GameObject currentHeldNetworkObj;
    private int currentHeldViewId = -1;

    [Header("Referencias")]
    public float dropForce = 3f;

    void Start()
    {
        if (inputScript == null)
            inputScript = GetComponent<PlayerInputScript>();

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

        if (slots[inputScript.ItemSlot] != null)
        {
            Debug.Log("El slot " + inputScript.ItemSlot + " ya est� ocupado.");
            return;
        }

        photonView.RPC(nameof(RPC_RequestPickup_Master), RpcTarget.MasterClient, targetItemPV.ViewID, inputScript.ItemSlot);
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

        if (slots[slotIndex] != null && currentHeldNetworkObj != null)
        {
            PhotonNetwork.Destroy(currentHeldNetworkObj);
            currentHeldNetworkObj = null;
            currentHeldViewId = -1;
        }

        Vector3 spawnPos = handSlot.position;
        Quaternion spawnRot = handSlot.rotation;
        GameObject held = PhotonNetwork.Instantiate(heldPrefabName, spawnPos, spawnRot);
        held.transform.SetParent(handSlot, true);

        Rigidbody rb = held.GetComponent<Rigidbody>();
        if(rb != null)
        {
            rb.isKinematic = true;
            rb.useGravity = false;
        }

        currentHeldNetworkObj = held;
        currentHeldViewId = held.GetComponent<PhotonView>()?.ViewID ?? -1;

        ItemSO itemData = ItemDatabase.FindByHeldPrefabName(heldPrefabName);
        slots[slotIndex] = itemData;

        activeSlot = slotIndex;

        var ui = FindFirstObjectByType<PlayerUIManager>();
        if (ui != null) ui.UpdateInventoryUI();
    }

    public void EquipFromSlot(int slotIndex)
    {
        if (!photonView.IsMine) return;

        if (slots[slotIndex] == null)
        {
            HolsterCurrent();
            activeSlot = slotIndex;

            var ui2 = FindFirstObjectByType<PlayerUIManager>();
            if (ui2 != null) ui2.UpdateInventoryUI();
            return;
        }

        if (activeSlot == slotIndex && currentHeldNetworkObj != null)
            return;

        HolsterCurrent();

        string heldPrefabName = slots[slotIndex].heldPrefabName;
        GameObject held = PhotonNetwork.Instantiate(heldPrefabName, handSlot.position, handSlot.rotation);
        held.transform.SetParent(handSlot, true);

        Rigidbody rb = held.GetComponent<Rigidbody>();
        if(rb != null)
        {
            rb.isKinematic = true;
            rb.useGravity = false;
        }

        currentHeldNetworkObj = held;
        currentHeldViewId = held.GetComponent<PhotonView>()?.ViewID ?? -1;

        activeSlot = slotIndex;

        var ui = FindFirstObjectByType<PlayerUIManager>();
        if (ui != null) ui.UpdateInventoryUI();
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
