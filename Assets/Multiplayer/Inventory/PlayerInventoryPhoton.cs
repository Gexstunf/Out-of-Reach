using System.Collections;
using Photon.Pun;
using UnityEngine;
using UI;
using Characters.PlayerController.Scripts.Input;
using Multiplayer.Inventory;

public class PlayerInventoryPhoton : MonoBehaviourPun
{
    [Header("Referencias")]
    public PlayerInputScript inputScript;
    public Transform handSlot;
    public Transform backSlot;
    public LayerMask itemLayer;
    public float dropForce = 3f;

    [Header("Inventario")]
    public ItemSO[] slots = new ItemSO[4]; // Slots 0-2 normales, slot 3 mochila
    public int activeSlot = -1;

    private GameObject currentHeldNetworkObj;
    private int currentHeldViewId = -1;

    public GameObject backpackObj; // referencia a la mochila networked

    [Header("Objeto temporal")]
    public GameObject tempHeldObj;
    public ItemSO tempItemData;

    private string tempHeldPrefabName;
    private string[] tempBackpackIds;

    void Start()
    {
        if (inputScript == null)
            inputScript = GetComponent<PlayerInputScript>();

        if (photonView.IsMine)
        {
            var ui = FindFirstObjectByType<PlayerUIManager>();
            if (ui != null)
                ui.InitInventory(this);
            else
                Debug.LogWarning("[Inventory] No se encontró PlayerUIManager en la escena!");
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
        string[] backpackIds = null;

        if (netItem.itemData.itemType == ItemType.Backpack)
        {
            var bd = itemPV.GetComponent<BackpackData>();
            if (bd != null) backpackIds = bd.GetItemIds();
        }

        PhotonNetwork.Destroy(itemPV.gameObject);
        photonView.RPC(nameof(RPC_SpawnTempHeld), info.Sender, heldPrefabName, backpackIds);
    }

    public void Pickup(ItemSO item)
    {
        if (item == null) return;

        Debug.Log($"[Inventory] Intentando recoger item '{item.displayName}'");

        if (item.itemType == ItemType.Backpack)
        {
            Debug.Log("[Inventory] Es una mochila, slot reservado 3");

            // Guardar en slot 4 (índice 3)
            slots[3] = item;

            // Instanciar en mano
            GameObject held = PhotonNetwork.Instantiate(item.heldPrefabName, handSlot.position, handSlot.rotation);
            currentHeldNetworkObj = held;
            currentHeldViewId = held.GetComponent<PhotonView>().ViewID;
            activeSlot = 3;
            backpackObj = held; // guardamos la referencia a la mochila

            held.transform.SetParent(handSlot, false);
            held.transform.localPosition = Vector3.zero;
            held.transform.localRotation = Quaternion.identity;

            var rb = held.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.isKinematic = true;
                rb.useGravity = false;
            }

            Debug.Log($"[Inventory] Mochila recogida y equipada en mano. ViewID={currentHeldViewId}");

            // RPC para que todos vean la mochila en la mano
            photonView.RPC(nameof(RPC_AttachItemToPlayer), RpcTarget.AllBuffered,
                currentHeldViewId, photonView.ViewID, "hand");

            var ui = FindFirstObjectByType<PlayerUIManager>();
            if (ui != null) ui.UpdateInventoryUI();

            return;
        }

        // Si no es mochila, agregamos al inventario normal
        TryAddItem(item);
    }

    private bool TryAddItem(ItemSO item)
    {
        if (item == null) return false;

        // Slot 3 exclusivo para mochilas
        if (item.itemType == ItemType.Backpack)
        {
            if (slots[3] == null)
            {
                slots[3] = item;
                EquipFromSlot(3);
                FindFirstObjectByType<PlayerUIManager>()?.UpdateInventoryUI();
                return true;
            }
            else
            {
                Debug.LogWarning("[Inventory] Slot de mochila ya está ocupado.");
                return false;
            }
        }

        // Para items normales, buscar primer slot vacío
        for (int i = 0; i < slots.Length; i++)
        {
            if (i == 3) continue; // slot mochila
            if (slots[i] == null)
            {
                slots[i] = item;
                EquipFromSlot(i);
                FindFirstObjectByType<PlayerUIManager>()?.UpdateInventoryUI();
                return true;
            }
        }

        Debug.LogWarning($"[Inventory] No hay slot disponible para '{item.displayName}'.");
        return false;
    }

    private void SpawnHeldItem(string prefabName, Transform parentSlot)
    {
        if (string.IsNullOrEmpty(prefabName) || parentSlot == null) return;

        GameObject held = PhotonNetwork.Instantiate(prefabName, parentSlot.position, parentSlot.rotation);
        currentHeldNetworkObj = held;
        currentHeldViewId = held.GetComponent<PhotonView>().ViewID;

        held.transform.SetParent(parentSlot, false);
        held.transform.localPosition = Vector3.zero;
        held.transform.localRotation = Quaternion.identity;

        var rb = held.GetComponent<Rigidbody>();
        if (rb != null) { rb.isKinematic = true; rb.useGravity = false; }

        photonView.RPC(nameof(RPC_AttachItemToPlayer), RpcTarget.AllBuffered, currentHeldViewId, photonView.ViewID, parentSlot == handSlot ? "hand" : "back");
    }

    [PunRPC]
    public void RPC_SpawnTempHeld(string heldPrefabName, string[] backpackIds)
    {
        if (!photonView.IsMine) return;

        tempHeldPrefabName = heldPrefabName;
        tempBackpackIds = backpackIds;
        tempItemData = ItemDatabase.FindByHeldPrefabName(heldPrefabName);

        if (tempItemData == null)
        {
            Debug.LogError("[TempHeld] No se encontró ItemSO para " + heldPrefabName);
            return;
        }

        Debug.Log($"[TempHeld] Spawning local preview of '{heldPrefabName}'");

        GameObject prefab = Resources.Load<GameObject>(heldPrefabName);
        if (prefab == null)
        {
            Debug.LogError("[TempHeld] No prefab found in Resources with name " + heldPrefabName);
            return;
        }

        tempHeldObj = Instantiate(prefab, handSlot.position, handSlot.rotation);
        tempHeldObj.transform.SetParent(handSlot, false);
        tempHeldObj.transform.localPosition = Vector3.zero;
        tempHeldObj.transform.localRotation = Quaternion.identity;

        var pv = tempHeldObj.GetComponent<PhotonView>();
        if (pv != null) Destroy(pv);

        var rb = tempHeldObj.GetComponent<Rigidbody>();
        if (rb != null) { rb.isKinematic = true; rb.useGravity = false; }

        var bd = tempHeldObj.GetComponent<BackpackData>() ?? tempHeldObj.AddComponent<BackpackData>();
        if (tempBackpackIds != null && tempBackpackIds.Length > 0)
            bd.SetFromIds(tempBackpackIds);
        else if (tempItemData.internalSlots != null)
            bd.internalSlots = tempItemData.internalSlots;

        Debug.Log("[TempHeld] Local temp held setup complete");

        // **Si es mochila, guardamos en slot 4 y lo equipamos**
        if (tempItemData.itemType == ItemType.Backpack)
        {
            slots[3] = tempItemData;
            backpackObj = tempHeldObj;
            activeSlot = 3;
            Debug.Log("[Inventory] Mochila guardada en slot 4 y equipada en la mano");
        }
    }


    public void PlaceTempHeldInSlot(int slotIndex)
    {
        if (tempHeldObj == null || tempItemData == null) return;

        if (tempItemData.itemType == ItemType.Backpack) slotIndex = 3;

        if (slots[slotIndex] != null) DropCurrent(slotIndex);

        slots[slotIndex] = tempItemData;

        if (tempItemData.itemType == ItemType.Backpack)
        {
            DestroyTempHeldLocal();
            SpawnHeldItem(tempItemData.heldPrefabName, backSlot);
            backpackObj = currentHeldNetworkObj;
        }
        else
        {
            DestroyTempHeldLocal();
            SpawnHeldItem(tempItemData.heldPrefabName, handSlot);
            activeSlot = slotIndex;
        }

        tempHeldObj = null;
        tempItemData = null;
        tempHeldPrefabName = null;
        tempBackpackIds = null;

        FindFirstObjectByType<PlayerUIManager>()?.UpdateInventoryUI();
    }

    private void DestroyTempHeldLocal()
    {
        if (tempHeldObj == null) return;
        var pv = tempHeldObj.GetComponent<PhotonView>();
        if (pv != null && pv.ViewID != 0)
            PhotonNetwork.Destroy(tempHeldObj);
        else
            Destroy(tempHeldObj);

        tempHeldObj = null;
    }

    public void EquipFromSlot(int slotIndex)
    {
        if (!photonView.IsMine) return;
        if (slotIndex < 0 || slotIndex >= slots.Length) return;

        ItemSO itemData = slots[slotIndex];
        if (itemData == null)
        {
            HolsterCurrent();
            activeSlot = -1;
            return;
        }

        if (activeSlot == slotIndex)
        {
            if (itemData.itemType == ItemType.Backpack && backpackObj != null)
                photonView.RPC(nameof(RPC_AttachItemToPlayer), RpcTarget.AllBuffered, backpackObj.GetComponent<PhotonView>().ViewID, photonView.ViewID, "back");
            else HolsterCurrent();

            activeSlot = -1;
            FindFirstObjectByType<PlayerUIManager>()?.UpdateInventoryUI();
            return;
        }

        HolsterCurrent();

        if (itemData.itemType == ItemType.Backpack)
        {
            if (backpackObj == null) return; // no hay mochila instanciada aún

            // Si está en la mano, pasar a espalda
            if (backpackObj.transform.parent == handSlot)
            {
                photonView.RPC(nameof(RPC_AttachItemToPlayer),
                    RpcTarget.AllBuffered,
                    backpackObj.GetComponent<PhotonView>().ViewID,
                    photonView.ViewID,
                    "back");
            }
            else // si está en la espalda, pasar a mano
            {
                photonView.RPC(nameof(RPC_AttachItemToPlayer),
                    RpcTarget.AllBuffered,
                    backpackObj.GetComponent<PhotonView>().ViewID,
                    photonView.ViewID,
                    "hand");
            }

            activeSlot = 3; // siempre queda como slot activo
            FindFirstObjectByType<PlayerUIManager>()?.UpdateInventoryUI();
            return;
        }

        SpawnHeldItem(itemData.heldPrefabName, handSlot);
        activeSlot = slotIndex;
        FindFirstObjectByType<PlayerUIManager>()?.UpdateInventoryUI();
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

        Vector3 dropPos = handSlot.position + transform.forward * 1.2f + Vector3.up * 0.3f;

        // Mochila en mano
        if (backpackObj != null && backpackObj.transform.parent == handSlot)
        {
            ItemSO itemData = slots[3];
            var bd = backpackObj.GetComponent<BackpackData>();
            string[] ids = bd != null ? bd.GetItemIds() : new string[4];

            PhotonNetwork.Destroy(backpackObj);
            backpackObj = null;

            GameObject worldObj = PhotonNetwork.Instantiate(itemData.worldPrefabName, dropPos, Quaternion.identity);
            worldObj.GetComponent<PhotonView>()?.RPC("RPC_InitContents", RpcTarget.AllBuffered, (object)ids);

            slots[3] = null;
            activeSlot = -1;
            FindFirstObjectByType<PlayerUIManager>()?.UpdateInventoryUI();
            return;
        }

        // Item normal
        if (currentHeldNetworkObj != null)
        {
            ItemSO itemData = slots[slotIndex];
            GameObject objToDrop = currentHeldNetworkObj;
            currentHeldNetworkObj = null;
            currentHeldViewId = -1;

            PhotonNetwork.Destroy(objToDrop);

            GameObject worldObj = PhotonNetwork.Instantiate(itemData.worldPrefabName, dropPos, Quaternion.identity);
            Rigidbody rb = worldObj.GetComponent<Rigidbody>();
            if (rb != null) rb.AddForce(transform.forward * dropForce + Vector3.up * 1.2f, ForceMode.Impulse);

            slots[slotIndex] = null;
            activeSlot = -1;
            FindFirstObjectByType<PlayerUIManager>()?.UpdateInventoryUI();
        }
    }

    public void PickupBackpack(GameObject backpackObj)
    {
        var netItem = backpackObj.GetComponent<NetworkedItem>();
        if (netItem == null || netItem.itemData == null) return;

        if (slots[3] == null) // slot 4 vacío
        {
            slots[3] = netItem.itemData; // guardo el ItemSO
            activeSlot = 3;
            EquipFromSlot(3);

            Debug.Log("[Inventory] Mochila recogida y equipada en slot 4.");
        }
        else
        {
            Debug.Log("[Inventory] Slot 4 ocupado, no se pudo recoger mochila.");
        }
    }


    public void OpenBackpack()
    {
        if (backpackObj == null || slots[3] == null) return;
        var bd = backpackObj.GetComponent<BackpackData>();
        if (bd == null) return;
        FindFirstObjectByType<PlayerUIManager>()?.ShowBackpackInventory(bd, this);
    }

    public void StoreInBackpack(BackpackData backpack, ItemSO item, int slotIndex)
    {
        if (backpack == null || item == null || slotIndex < 0 || slotIndex >= backpack.internalSlots.Length) return;
        if (item.itemType == ItemType.Backpack) return;

        backpack.internalSlots[slotIndex] = item;

        var pv = backpack.GetComponent<PhotonView>();
        if (pv != null) pv.RPC("RPC_InitContents", RpcTarget.AllBuffered, (object)backpack.GetItemIds());

        FindFirstObjectByType<PlayerUIManager>()?.UpdateBackpackUI(backpack.internalSlots);
    }

    public void DropFromBackpack(BackpackData backpack, int slotIndex)
    {
        if (backpack == null || slotIndex < 0 || slotIndex >= backpack.internalSlots.Length) return;
        ItemSO item = backpack.internalSlots[slotIndex];
        if (item == null) return;

        backpack.internalSlots[slotIndex] = null;

        Vector3 dropPos = handSlot.position + transform.forward * 1.2f + Vector3.up * 0.3f;
        GameObject worldObj = PhotonNetwork.Instantiate(item.worldPrefabName, dropPos, Quaternion.identity);
        Rigidbody rb = worldObj.GetComponent<Rigidbody>();
        if (rb != null) rb.AddForce(transform.forward * 3f + Vector3.up * 1.2f, ForceMode.Impulse);

        var pv = backpack.GetComponent<PhotonView>();
        if (pv != null) pv.RPC("RPC_InitContents", RpcTarget.AllBuffered, (object)backpack.GetItemIds());

        FindFirstObjectByType<PlayerUIManager>()?.UpdateBackpackUI(backpack.internalSlots);
    }

    [PunRPC]
    void RPC_AttachItemToPlayer(int itemViewID, int playerViewID, string slot)
    {
        PhotonView itemPV = PhotonView.Find(itemViewID);
        PhotonView playerPV = PhotonView.Find(playerViewID);

        if (itemPV == null || playerPV == null)
        {
            Debug.LogWarning("[AttachItem] No se encontró item o player PV");
            return;
        }

        var inv = playerPV.GetComponent<PlayerInventoryPhoton>();
        Transform parent = slot == "hand" ? inv.handSlot : inv.backSlot;

        Debug.Log($"[AttachItem] {itemPV.name} se va a parentear a {slot} de {playerPV.name}");

        if (parent != null)
        {
            itemPV.transform.SetParent(parent, false);
            itemPV.transform.localPosition = Vector3.zero;
            itemPV.transform.localRotation = Quaternion.identity;
        }
    }

}
