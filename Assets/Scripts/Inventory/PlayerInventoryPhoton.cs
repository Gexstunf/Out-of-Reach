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

        // Si es mochila, extraer sus contents antes de destruir
        string[] backpackIds = null;
        if (netItem.itemData.itemType == ItemType.Backpack)
        {
            var bd = itemPV.GetComponent<BackpackData>();
            if (bd != null) backpackIds = bd.GetItemIds();
        }

        PhotonNetwork.Destroy(itemPV.gameObject);

        // Llamar al cliente que pidió pickup pasándole las IDs (puede ser null)
        photonView.RPC(nameof(RPC_SpawnTempHeld), info.Sender, heldPrefabName, backpackIds);
    }

    [PunRPC]
    public void RPC_SpawnTempHeld(string heldPrefabName, string[] backpackIds)
    {
        if (!photonView.IsMine) return;

        tempItemData = ItemDatabase.FindByHeldPrefabName(heldPrefabName);
        tempHeldObj = PhotonNetwork.Instantiate(heldPrefabName, Vector3.zero, Quaternion.identity);
        // ocultalo hasta colocarlo
        tempHeldObj.SetActive(false);

        // Si vino contenido (era una mochila), inicializá BackpackData en el tempHeldObj
        if (backpackIds != null && backpackIds.Length > 0)
        {
            var bd = tempHeldObj.GetComponent<BackpackData>();
            if (bd == null) bd = tempHeldObj.AddComponent<BackpackData>();

            // Llamamos RPC en la propia PV del objeto para sincronizar en todos
            var pv = tempHeldObj.GetComponent<PhotonView>();
            if (pv != null)
            {
                pv.RPC("RPC_InitContents", RpcTarget.AllBuffered, (object)backpackIds);
            }
        }
        else
        {
            // si querés plantilla por defecto desde ItemSO:
            if (tempItemData != null && tempItemData.defaultInternalSlots != null && tempItemData.defaultInternalSlots.Length > 0)
            {
                var bd = tempHeldObj.GetComponent<BackpackData>() ?? tempHeldObj.AddComponent<BackpackData>();
                for (int i = 0; i < bd.internalSlots.Length && i < tempItemData.defaultInternalSlots.Length; i++)
                    bd.internalSlots[i] = tempItemData.defaultInternalSlots[i];
                // opcional: sincronizar con pv.RPC(...)
                var pv = tempHeldObj.GetComponent<PhotonView>();
                if (pv != null) pv.RPC("RPC_InitContents", RpcTarget.AllBuffered, (object)bd.GetItemIds());
            }
        }
    }

    // ---------- COLOCAR OBJETO EN SLOT ----------
    public void PlaceTempHeldInSlot(int slotIndex)
    {
        if (tempHeldObj == null || tempItemData == null) return;

        if (tempItemData.itemType == ItemType.Backpack) slotIndex = 3;

        if (slots[slotIndex] != null) DropCurrent(slotIndex);

        slots[slotIndex] = tempItemData;

        if (tempItemData.itemType == ItemType.Backpack)
        {
            backpackObj = tempHeldObj;
            // asegurar que BackpackData existe (debería estar)
            var bd = backpackObj.GetComponent<BackpackData>();
            if (bd == null) bd = backpackObj.AddComponent<BackpackData>();
            // parent por defecto a espalda
            backpackObj.SetActive(true);
            backpackObj.transform.SetParent(backSlot, false);
            backpackObj.transform.localPosition = Vector3.zero;
            backpackObj.transform.localRotation = Quaternion.identity;

            // sincronizamos parent para todos
            photonView.RPC(nameof(RPC_AttachItemToPlayer),
                RpcTarget.AllBuffered,
                backpackObj.GetComponent<PhotonView>().ViewID,
                photonView.ViewID,
                "back");
        }
        else
        {
            // item normal → equipar en mano
            currentHeldNetworkObj = tempHeldObj;
            currentHeldViewId = tempHeldObj.GetComponent<PhotonView>().ViewID;
            activeSlot = slotIndex;

            tempHeldObj.SetActive(true);
            tempHeldObj.transform.SetParent(handSlot, false);
            tempHeldObj.transform.localPosition = Vector3.zero;
            tempHeldObj.transform.localRotation = Quaternion.identity;

            photonView.RPC(nameof(RPC_AttachItemToPlayer),
                RpcTarget.AllBuffered,
                currentHeldViewId,
                photonView.ViewID,
                "hand");
        }

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
            // obtener ids desde backpackObj
            var bd = backpackObj.GetComponent<BackpackData>();
            string[] ids = bd != null ? bd.GetItemIds() : new string[4];

            // destruir la instancia local (held) en la escena
            PhotonNetwork.Destroy(backpackObj);
            backpackObj = null;

            // instanciar world prefab y enviar los ids para inicializar
            GameObject worldObj = PhotonNetwork.Instantiate(itemData.worldPrefabName, dropPos, Quaternion.identity);
            var worldPV = worldObj.GetComponent<PhotonView>();
            if (worldPV != null)
            {
                worldPV.RPC("RPC_InitContents", RpcTarget.AllBuffered, (object)ids);
            }
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
