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

    public GameObject backpackObj; // referencia a la instancia de la mochila (held/back)

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
        // instantiate in world for this client and put into hand immediately (visible)
        tempHeldObj = PhotonNetwork.Instantiate(heldPrefabName, handSlot.position, handSlot.rotation);
        tempHeldObj.transform.SetParent(handSlot, false);

        // ensure physics / kinematic for held visuals (optional)
        var rb = tempHeldObj.GetComponent<Rigidbody>();
        if (rb != null) { rb.isKinematic = true; rb.useGravity = false; }

        // initialize BackpackData if needed
        var pv = tempHeldObj.GetComponent<PhotonView>();
        var bd = tempHeldObj.GetComponent<BackpackData>() ?? tempHeldObj.AddComponent<BackpackData>();

        if (backpackIds != null && backpackIds.Length > 0)
        {
            if (pv != null)
                pv.RPC("RPC_InitContents", RpcTarget.AllBuffered, (object)backpackIds);
            else
                bd.SetFromIds(backpackIds);
        }
        else
        {
            if (tempItemData != null && tempItemData.internalSlots != null)
            {
                for (int i = 0; i < bd.internalSlots.Length && i < tempItemData.internalSlots.Length; i++)
                    bd.internalSlots[i] = tempItemData.internalSlots[i];
                if (pv != null) pv.RPC("RPC_InitContents", RpcTarget.AllBuffered, (object)bd.GetItemIds());
            }
        }

        Debug.Log($"[Inventory] Spawned temp held '{heldPrefabName}' ({(tempItemData != null ? tempItemData.displayName : "null")})");
    }

    // ---------- COLOCAR OBJETO EN SLOT ----------
    public void PlaceTempHeldInSlot(int slotIndex)
    {
        if (tempHeldObj == null || tempItemData == null) return;

        if (tempItemData.itemType == ItemType.Backpack) slotIndex = 3;

        // Si ya hay algo en ese slot, lo tiramos
        if (slots[slotIndex] != null) DropCurrent(slotIndex);

        // Guardamos ItemSO en slots
        slots[slotIndex] = tempItemData;

        if (tempItemData.itemType == ItemType.Backpack)
        {
            // la instancia tempHeldObj contiene BackpackData (inicializada en spawn)
            backpackObj = tempHeldObj;
            var bd = backpackObj.GetComponent<BackpackData>() ?? backpackObj.AddComponent<BackpackData>();

            backpackObj.transform.SetParent(backSlot, false);
            backpackObj.transform.localPosition = Vector3.zero;
            backpackObj.transform.localRotation = Quaternion.identity;

            // MUY IMPORTANTE → Activarla después de parentarla
            backpackObj.SetActive(true);

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

        // instanciar y equipar
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

        // declarar dropPos aquí (antes de usar)
        Vector3 dropPos = handSlot.position + transform.forward * 1.2f + Vector3.up * 0.3f;

        if (itemData.itemType == ItemType.Backpack)
        {
            if (backpackObj == null) return;

            // obtener ids desde backpackObj (si existe BackpackData)
            var bd = backpackObj.GetComponent<BackpackData>();
            string[] ids = bd != null ? bd.GetItemIds() : new string[4];

            // destruir la instancia visual (held/back)
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
            GameObject objToDrop = currentHeldNetworkObj;
            currentHeldNetworkObj = null;
            currentHeldViewId = -1;

            PhotonNetwork.Destroy(objToDrop);

            GameObject worldObj = PhotonNetwork.Instantiate(itemData.worldPrefabName, dropPos, Quaternion.identity);
            Rigidbody rb = worldObj.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.isKinematic = false;
                rb.AddForce(transform.forward * dropForce + Vector3.up * 1.2f, ForceMode.Impulse);
            }
        }

        // limpiar el slot
        slots[slotIndex] = null;
        activeSlot = -1;

        var ui = FindFirstObjectByType<PlayerUIManager>();
        if (ui != null) ui.UpdateInventoryUI();
    }

    // ---------- ABRIR MOCHILA ----------
    public void OpenBackpack()
    {
        if (backpackObj == null || slots[3] == null) return;

        var bd = backpackObj.GetComponent<BackpackData>();
        if (bd == null) return;

        var ui = FindFirstObjectByType<PlayerUIManager>();
        if (ui != null) ui.ShowBackpackInventory(bd, this);
    }

    // Guardar en mochila (usa BackpackData)
    public void StoreInBackpack(BackpackData backpack, ItemSO item, int slotIndex)
    {
        if (backpack == null) return;
        if (slotIndex < 0 || slotIndex >= backpack.internalSlots.Length) return;
        if (item == null || item.itemType == ItemType.Backpack) return; // no meter mochilas dentro de mochilas

        backpack.internalSlots[slotIndex] = item;

        // sincronizar (opcional): RPC sobre la PV de la mochila
        var pv = backpack.GetComponent<PhotonView>();
        if (pv != null)
            pv.RPC("RPC_InitContents", RpcTarget.AllBuffered, (object)backpack.GetItemIds());

        ClearTempHeld();

        var ui = FindFirstObjectByType<PlayerUIManager>();
        if (ui != null) ui.UpdateBackpackUI(backpack.internalSlots);
    }

    // Limpiar temp (destruye el tempHeldObj si existe)
    public void ClearTempHeld()
    {
        if (tempHeldObj != null)
        {
            PhotonNetwork.Destroy(tempHeldObj);
            tempHeldObj = null;
        }
        tempItemData = null;
    }

    // Abrir mochila en mundo
    public void OpenBackpackWorld(NetworkedItem netItem)
    {
        if (netItem == null || netItem.itemData == null) return;

        var worldPV = netItem.GetComponent<PhotonView>();
        var bd = netItem.GetComponent<BackpackData>();
        if (bd == null && worldPV != null)
        {
            // si no tiene componente local, puede que se inicialice por RPC; intentar leer luego
            bd = netItem.GetComponent<BackpackData>();
        }

        var ui = FindFirstObjectByType<PlayerUIManager>();
        if (ui != null && bd != null) ui.ShowBackpackInventory(bd, this);
    }

    // Sacar objetos de mochila al mundo (usando BackpackData)
    public void DropFromBackpack(BackpackData backpack, int slotIndex)
    {
        if (backpack == null) return;
        if (slotIndex < 0 || slotIndex >= backpack.internalSlots.Length) return;
        if (backpack.internalSlots[slotIndex] == null) return;

        ItemSO item = backpack.internalSlots[slotIndex];
        backpack.internalSlots[slotIndex] = null;

        Vector3 dropPos = handSlot.position + transform.forward * 1.2f + Vector3.up * 0.3f;
        GameObject worldObj = PhotonNetwork.Instantiate(item.worldPrefabName, dropPos, Quaternion.identity);
        Rigidbody rb = worldObj.GetComponent<Rigidbody>();
        if (rb != null) rb.AddForce(transform.forward * 3f + Vector3.up * 1.2f, ForceMode.Impulse);

        // sincronizar contents
        var pv = backpack.GetComponent<PhotonView>();
        if (pv != null) pv.RPC("RPC_InitContents", RpcTarget.AllBuffered, (object)backpack.GetItemIds());

        var ui = FindFirstObjectByType<PlayerUIManager>();
        if (ui != null) ui.UpdateBackpackUI(backpack.internalSlots);
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

        Debug.Log($"[AttachItem] {itemPV.name} -> {slot} del jugador {playerPV.name}");

        if (parent != null)
        {
            itemPV.transform.SetParent(parent, false);
            itemPV.transform.localPosition = Vector3.zero;
            itemPV.transform.localRotation = Quaternion.identity;
        }
    }
}
