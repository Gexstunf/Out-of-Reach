using System.Collections;
using Photon.Pun;
using UnityEngine;
using UI;
using Characters.PlayerController.Scripts.Input;

public class PlayerInventoryPhoton : MonoBehaviourPun
{
    [Header("Referencias")]
    public PlayerInputScript inputScript;
    public Transform handSlot;
    public Transform backSlot;
    public LayerMask itemLayer;
    public float dropForce = 3f;

    [Header("Inventario")]
    public ItemSO[] slots = new ItemSO[4];
    public int activeSlot = -1;

    private GameObject currentHeldNetworkObj;
    private int currentHeldViewId = -1;

    public GameObject backpackObj; // referencia a la mochila

    [Header("Objeto temporal")]
    public GameObject tempHeldObj;
    public ItemSO tempItemData;

    void Start()
    {
        if (inputScript == null)
            inputScript = GetComponent<PlayerInputScript>();

        if (photonView.IsMine)
        {
            var ui = FindFirstObjectByType<PlayerUIManager>();
            if (ui != null) ui.InitInventory(this);
        }
    }

    // ---------- PICKUP TEMPORAL ----------
    public void RequestPickupOnClosest(PhotonView targetItemPV)
    {
        if (targetItemPV == null) return;

        var netItem = targetItemPV.GetComponent<NetworkedItem>();
        if (netItem == null || netItem.itemData == null) return;

        photonView.RPC(nameof(RPC_RequestPickup_Master), RpcTarget.MasterClient, targetItemPV.ViewID);
    }

    [PunRPC]
    public void RPC_RequestPickup_Master(int itemViewID, PhotonMessageInfo info)
    {
        if (!PhotonNetwork.IsMasterClient) return;

        PhotonView itemPV = PhotonView.Find(itemViewID);
        if (itemPV == null) return;

        var netItem = itemPV.GetComponent<NetworkedItem>();
        if (netItem == null || netItem.itemData == null) return;

        string heldPrefabName = netItem.GetHeldPrefabName();
        PhotonNetwork.Destroy(itemPV.gameObject);

        photonView.RPC(nameof(RPC_SpawnTempHeld), info.Sender, heldPrefabName);
    }

    [PunRPC]
    public void RPC_SpawnTempHeld(string heldPrefabName)
    {
        if (!photonView.IsMine) return;

        tempItemData = ItemDatabase.FindByHeldPrefabName(heldPrefabName);
        tempHeldObj = PhotonNetwork.Instantiate(heldPrefabName, Vector3.zero, Quaternion.identity); // No mostrar aún
        tempHeldObj.SetActive(false); // oculto hasta colocar en slot
    }

    // ---------- COLOCAR OBJETO EN SLOT ----------
    public void PlaceTempHeldInSlot(int slotIndex)
    {
        if (tempHeldObj == null || tempItemData == null) return;

        // La mochila siempre va al último slot
        if (tempItemData.itemType == ItemType.Backpack) slotIndex = 3;

        // Si ya hay algo en ese slot, lo tiramos
        if (slots[slotIndex] != null) DropCurrent(slotIndex);

        slots[slotIndex] = tempItemData;

        if (tempItemData.itemType == ItemType.Backpack)
        {
            backpackObj = tempHeldObj;
            // Equipar mochila en la espalda
            photonView.RPC(nameof(RPC_AttachItemToPlayer), RpcTarget.AllBuffered,
                backpackObj.GetComponent<PhotonView>().ViewID,
                photonView.ViewID,
                "back");
        }
        else
        {
            // Guardamos referencia y lo equipamos directamente
            currentHeldNetworkObj = tempHeldObj;
            currentHeldViewId = tempHeldObj.GetComponent<PhotonView>().ViewID;
            activeSlot = slotIndex;

            photonView.RPC(nameof(RPC_AttachItemToPlayer), RpcTarget.AllBuffered,
                currentHeldViewId,
                photonView.ViewID,
                "hand");
        }

        // Limpiar temp
        tempHeldObj = null;
        tempItemData = null;

        var ui = FindFirstObjectByType<PlayerUIManager>();
        if (ui != null) ui.UpdateInventoryUI();
    }

    // ---------- EQUIPAR ----------
    public void EquipFromSlot(int slotIndex)
    {
        if (!photonView.IsMine) return;
        if (slots[slotIndex] == null) return;

        ItemSO itemData = slots[slotIndex];

        // --- MOCHILA ---
        if (itemData.itemType == ItemType.Backpack)
        {
            if (backpackObj == null) return;

            // toggle entre mano y espalda
            if (backpackObj.transform.parent == handSlot)
            {
                photonView.RPC(nameof(RPC_AttachItemToPlayer),
                    RpcTarget.AllBuffered,
                    backpackObj.GetComponent<PhotonView>().ViewID,
                    photonView.ViewID,
                    "back");
                activeSlot = -1;
            }
            else
            {
                photonView.RPC(nameof(RPC_AttachItemToPlayer),
                    RpcTarget.AllBuffered,
                    backpackObj.GetComponent<PhotonView>().ViewID,
                    photonView.ViewID,
                    "hand");
                activeSlot = slotIndex;
            }
            return;
        }

        // --- ITEM NORMAL ---
        // si ya hay algo en mano, destruirlo
        HolsterCurrent();

        // spawn nuevo
        string heldPrefabName = slots[slotIndex].heldPrefabName;
        GameObject held = PhotonNetwork.Instantiate(heldPrefabName, handSlot.position, handSlot.rotation);

        currentHeldNetworkObj = held;
        currentHeldViewId = held.GetComponent<PhotonView>().ViewID;
        activeSlot = slotIndex;

        photonView.RPC(nameof(RPC_AttachItemToPlayer),
            RpcTarget.AllBuffered,
            currentHeldViewId,
            photonView.ViewID,
            "hand");

        var ui = FindFirstObjectByType<PlayerUIManager>();
        if (ui != null) ui.UpdateInventoryUI();
    }

    // ---------- HOLSTER ----------
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

    // ---------- TIRAR OBJETO ----------
    public void DropCurrent(int slotIndex)
    {
        if (!photonView.IsMine) return;
        if (slots[slotIndex] == null) return;

        ItemSO itemData = slots[slotIndex];
        GameObject objToDrop = null;

        if (itemData.itemType == ItemType.Backpack)
        {
            if (backpackObj == null) return;
            objToDrop = backpackObj;
            backpackObj = null;
        }
        else
        {
            if (currentHeldNetworkObj == null) return;
            objToDrop = currentHeldNetworkObj;
            currentHeldNetworkObj = null;
            currentHeldViewId = -1;
        }

        slots[slotIndex] = null;
        activeSlot = -1;

        if (objToDrop != null)
        {
            Vector3 dropPos = handSlot.position + transform.forward * 1.2f + Vector3.up * 0.3f;
            PhotonNetwork.Destroy(objToDrop);

            GameObject worldObj = PhotonNetwork.Instantiate(itemData.worldPrefabName, dropPos, Quaternion.identity);
            Rigidbody rb = worldObj.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.isKinematic = false;
                rb.AddForce(transform.forward * dropForce + Vector3.up * 1.2f, ForceMode.Impulse);
            }
        }
    }

    // ---------- ABRIR MOCHILA ----------
    public void OpenBackpack()
    {
        if (backpackObj == null || slots[3] == null) return;
        ItemSO backpackData = slots[3];

        var ui = FindFirstObjectByType<PlayerUIManager>();
        if (ui != null) ui.ShowBackpackInventory(backpackData.internalSlots, this);
    }

    public void OpenBackpackWorld(NetworkedItem netItem)
    {
        if (netItem.itemData == null) return;

        var ui = FindFirstObjectByType<PlayerUIManager>();
        if (ui != null) ui.ShowBackpackInventory(netItem.itemData.internalSlots, this);
    }

    // ---------- SACAR OBJETOS DE MOCHILA ----------
    public void DropFromBackpack(int slotIndex)
    {
        if (slots[3] == null) return; // mochila no equipada
        ItemSO backpackData = slots[3];
        if (backpackData.internalSlots[slotIndex] == null) return;

        ItemSO item = backpackData.internalSlots[slotIndex];
        backpackData.internalSlots[slotIndex] = null;

        Vector3 dropPos = handSlot.position + transform.forward * 1.2f + Vector3.up * 0.3f;
        GameObject worldObj = PhotonNetwork.Instantiate(item.worldPrefabName, dropPos, Quaternion.identity);
        Rigidbody rb = worldObj.GetComponent<Rigidbody>();
        if (rb != null) rb.AddForce(transform.forward * 3f + Vector3.up * 1.2f, ForceMode.Impulse);

        var ui = FindFirstObjectByType<PlayerUIManager>();
        if (ui != null) ui.UpdateBackpackUI(backpackData.internalSlots);
    }

    // ---------- RPC PARA PARENT ----------
    [PunRPC]
    void RPC_AttachItemToPlayer(int itemViewID, int playerViewID, string slot)
    {
        PhotonView itemPV = PhotonView.Find(itemViewID);
        PhotonView playerPV = PhotonView.Find(playerViewID);

        if (itemPV == null || playerPV == null) return;

        var inv = playerPV.GetComponent<PlayerInventoryPhoton>();
        Transform parent = null;

        if (slot == "hand") parent = inv.handSlot;
        if (slot == "back") parent = inv.backSlot;

        if (parent != null)
        {
            itemPV.transform.SetParent(parent, false);
            itemPV.transform.localPosition = Vector3.zero;
            itemPV.transform.localRotation = Quaternion.identity;
        }
    }
}
