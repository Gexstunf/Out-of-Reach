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
    public ItemSO[] slots = new ItemSO[4];
    public int activeSlot = -1;

    private GameObject currentHeldNetworkObj;
    private int currentHeldViewId = -1;

    public GameObject backpackObj; // referencia a la instancia de la mochila (networked)

    [Header("Objeto temporal")]
    public GameObject tempHeldObj;
    public ItemSO tempItemData;

    // Nuevos campos para manejar pickup temporal local y posibles contents de mochila
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
            {
                Debug.Log("[Inventory] Inicializando UI con este inventario");
                ui.InitInventory(this); // <--- asegura que Inventario se setea aquí
            }
            else
            {
                Debug.LogWarning("[Inventory] No se encontró PlayerUIManager en la escena!");
            }
        }
    }

    // ---------- PICKUP TEMPORAL ----------
    // ---------- PICKUP TEMPORAL ----------
    public void RequestPickupOnClosest(PhotonView targetItemPV)
    {
        if (targetItemPV == null) return;

        Debug.Log($"[Pickup] Requesting pickup on item '{targetItemPV.name}' (ViewID: {targetItemPV.ViewID})");

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

        Debug.Log($"[Pickup Master] Handling pickup for '{itemPV.name}' by {info.Sender.NickName}");

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
        Debug.Log($"[Pickup Master] Destroyed item '{itemPV.name}' in world");

        photonView.RPC(nameof(RPC_SpawnTempHeld), info.Sender, heldPrefabName, backpackIds);
    }

    [PunRPC]
    public void RPC_SpawnTempHeld(string heldPrefabName, string[] backpackIds)
    {
        if (!photonView.IsMine) return;

        tempHeldPrefabName = heldPrefabName;
        tempBackpackIds = backpackIds;
        tempItemData = ItemDatabase.FindByHeldPrefabName(heldPrefabName);

        Debug.Log($"[TempHeld] Spawning local preview of '{heldPrefabName}'");

        GameObject prefab = Resources.Load<GameObject>(heldPrefabName);
        if (prefab == null)
        {
            Debug.LogError($"[TempHeld] No prefab found in Resources with name '{heldPrefabName}'");
            return;
        }

        tempHeldObj = Instantiate(prefab, handSlot.position, handSlot.rotation);
        tempHeldObj.transform.SetParent(handSlot, false);
        tempHeldObj.transform.localPosition = Vector3.zero;
        tempHeldObj.transform.localRotation = Quaternion.identity;

        Debug.Log($"[TempHeld] Temp object instantiated at handSlot ({handSlot.position})");

        var pv = tempHeldObj.GetComponent<PhotonView>();
        if (pv != null) Destroy(pv);

        var rb = tempHeldObj.GetComponent<Rigidbody>();
        if (rb != null) { rb.isKinematic = true; rb.useGravity = false; }

        var bd = tempHeldObj.GetComponent<BackpackData>() ?? tempHeldObj.AddComponent<BackpackData>();
        if (tempBackpackIds != null && tempBackpackIds.Length > 0)
            bd.SetFromIds(tempBackpackIds);
        else if (tempItemData != null && tempItemData.internalSlots != null)
            bd.internalSlots = tempItemData.internalSlots;

        Debug.Log($"[TempHeld] Local temp held setup complete");
    }

    // ---------- COLOCAR OBJETO EN SLOT ----------
    public void PlaceTempHeldInSlot(int slotIndex)
    {
        if (tempHeldObj == null || tempItemData == null) return;

        if (tempItemData.itemType == ItemType.Backpack) slotIndex = 3;

        if (slots[slotIndex] != null) DropCurrent(slotIndex);

        slots[slotIndex] = tempItemData;

        Debug.Log($"[Inventory] Placing item '{tempItemData.displayName}' in slot {slotIndex}");

        if (tempItemData.itemType == ItemType.Backpack)
        {
            DestroyTempHeldLocal();
            GameObject netBack = PhotonNetwork.Instantiate(tempItemData.heldPrefabName, backSlot.position, backSlot.rotation);
            backpackObj = netBack;

            Debug.Log($"[Inventory] Networked backpack instantiated at backSlot ({backSlot.position})");

            photonView.RPC(nameof(RPC_AttachItemToPlayer),
                RpcTarget.AllBuffered,
                backpackObj.GetComponent<PhotonView>().ViewID,
                photonView.ViewID,
                "back");
        }
        else
        {
            DestroyTempHeldLocal();
            GameObject held = PhotonNetwork.Instantiate(tempItemData.heldPrefabName, handSlot.position, handSlot.rotation);
            currentHeldNetworkObj = held;
            currentHeldViewId = held.GetComponent<PhotonView>().ViewID;
            activeSlot = slotIndex;

            Debug.Log($"[Inventory] Networked item '{tempItemData.displayName}' instantiated in handSlot ({handSlot.position})");

            held.transform.SetParent(handSlot, false);
            held.transform.localPosition = Vector3.zero;
            held.transform.localRotation = Quaternion.identity;

            var rb = held.GetComponent<Rigidbody>();
            if (rb != null) { rb.isKinematic = true; rb.useGravity = false; }

            photonView.RPC(nameof(RPC_AttachItemToPlayer),
                RpcTarget.AllBuffered,
                currentHeldViewId,
                photonView.ViewID,
                "hand");
        }

        tempHeldObj = null;
        tempItemData = null;
        tempHeldPrefabName = null;
        tempBackpackIds = null;

        var ui = FindFirstObjectByType<PlayerUIManager>();
        if (ui != null) ui.UpdateInventoryUI();
        Debug.Log($"[Inventory] PlaceTempHeldInSlot finished for slot {slotIndex}");
    }

    // Destruye el tempHeldObj correctamente (si fue networked usar PhotonNetwork.Destroy, si fue local usar Destroy)
    private void DestroyTempHeldLocal()
    {
        if (tempHeldObj == null) return;

        var pv = tempHeldObj.GetComponent<PhotonView>();
        if (pv != null && pv.ViewID != 0)
        {
            Debug.Log($"[DestroyTempHeldLocal] Destroying networked tempHeldObj '{tempHeldObj.name}' (ViewID: {pv.ViewID})");
            PhotonNetwork.Destroy(tempHeldObj);
        }
        else
        {
            Debug.Log($"[DestroyTempHeldLocal] Destroying local tempHeldObj '{tempHeldObj.name}'");
            Destroy(tempHeldObj);
        }

        tempHeldObj = null;
    }


    // ---------- EQUIPAR (TOGGLE si re-seleccionas) ----------
    public void EquipFromSlot(int slotIndex)
    {
        if (!photonView.IsMine) return;
        if (slotIndex < 0 || slotIndex >= slots.Length) return;
        if (slots[slotIndex] == null)
        {
            Debug.Log($"[Inventory] Slot {slotIndex} vacío, holstering current item");
            HolsterCurrent();

            // si tenías mochila en mano, moverla a espalda
            if (backpackObj != null && backpackObj.transform.parent == handSlot)
            {
                photonView.RPC(nameof(RPC_AttachItemToPlayer),
                    RpcTarget.AllBuffered,
                    backpackObj.GetComponent<PhotonView>().ViewID,
                    photonView.ViewID,
                    "back");
            }

            activeSlot = -1; // importante para que no quede seleccionado ningún slot
            FindFirstObjectByType<PlayerUIManager>()?.UpdateInventoryUI();

            return;
        }

        ItemSO itemData = slots[slotIndex];
        Debug.Log($"[Equip] Trying to equip slot {slotIndex} -> '{itemData.displayName}'");

        if (activeSlot == slotIndex)
        {
            Debug.Log($"[Equip] Slot {slotIndex} already active, toggling off");

            if (itemData.itemType == ItemType.Backpack && backpackObj != null)
            {
                photonView.RPC(nameof(RPC_AttachItemToPlayer),
                    RpcTarget.AllBuffered,
                    backpackObj.GetComponent<PhotonView>().ViewID,
                    photonView.ViewID,
                    "back");
            }
            else HolsterCurrent();

            activeSlot = -1;
            FindFirstObjectByType<PlayerUIManager>()?.UpdateInventoryUI();
            return;
        }

        HolsterCurrent();

        if (itemData.itemType == ItemType.Backpack)
        {
            if (backpackObj != null && backpackObj.transform.parent == handSlot)
            {
                Debug.Log($"[Equip] Moving backpack to back");
                photonView.RPC(nameof(RPC_AttachItemToPlayer),
                    RpcTarget.AllBuffered,
                    backpackObj.GetComponent<PhotonView>().ViewID,
                    photonView.ViewID,
                    "back");
                activeSlot = -1;
            }
            else
            {
                Debug.Log($"[Equip] Equipping backpack to hand");
                photonView.RPC(nameof(RPC_AttachItemToPlayer),
                    RpcTarget.AllBuffered,
                    backpackObj.GetComponent<PhotonView>().ViewID,
                    photonView.ViewID,
                    "hand");
                activeSlot = slotIndex;
            }
            FindFirstObjectByType<PlayerUIManager>()?.UpdateInventoryUI();
            return;
        }

        string heldPrefabName = itemData.heldPrefabName;
        GameObject held = PhotonNetwork.Instantiate(heldPrefabName, handSlot.position, handSlot.rotation);
        currentHeldNetworkObj = held;
        currentHeldViewId = held.GetComponent<PhotonView>().ViewID;
        activeSlot = slotIndex;

        held.transform.SetParent(handSlot, false);
        held.transform.localPosition = Vector3.zero;
        held.transform.localRotation = Quaternion.identity;

        var rbHeld = held.GetComponent<Rigidbody>();
        if (rbHeld != null) { rbHeld.isKinematic = true; rbHeld.useGravity = false; }

        Debug.Log($"[Equip] Networked item '{itemData.displayName}' equipped in handSlot ({handSlot.position})");

        photonView.RPC(nameof(RPC_AttachItemToPlayer),
            RpcTarget.AllBuffered,
            currentHeldViewId,
            photonView.ViewID,
            "hand");

        FindFirstObjectByType<PlayerUIManager>()?.UpdateInventoryUI();
    }

    // ---------- HOLSTER ----------
    public void HolsterCurrent()
    {
        if (!photonView.IsMine) return;
        if (currentHeldNetworkObj != null)
        {
            Debug.Log($"[Holster] Destroying current held object '{currentHeldNetworkObj.name}'");
            PhotonNetwork.Destroy(currentHeldNetworkObj);
            currentHeldNetworkObj = null;
            currentHeldViewId = -1;
        }
    }

    // ---------- TIRAR OBJETO ----------
    public void DropCurrent(int slotIndex)
    {
        if (!photonView.IsMine) return;

        Vector3 dropPos = handSlot.position + transform.forward * 1.2f + Vector3.up * 0.3f;

        // --- Primero: mochila en mano ---
        if (backpackObj != null && backpackObj.transform.parent == handSlot)
        {
            ItemSO itemData = slots[3]; // slot 4
            var bd = backpackObj.GetComponent<BackpackData>();
            string[] ids = bd != null ? bd.GetItemIds() : new string[4];

            PhotonNetwork.Destroy(backpackObj);
            backpackObj = null;

            GameObject worldObj = PhotonNetwork.Instantiate(itemData.worldPrefabName, dropPos, Quaternion.identity);
            worldObj.GetComponent<PhotonView>()?.RPC("RPC_InitContents", RpcTarget.AllBuffered, (object)ids);

            Debug.Log($"[Drop] Backpack dropped in world at {dropPos}");

            slots[3] = null;
            activeSlot = -1;
            FindFirstObjectByType<PlayerUIManager>()?.UpdateInventoryUI();
            return;
        }

        // --- Segundo: item normal en mano ---
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

            Debug.Log($"[Drop] Item '{itemData.displayName}' dropped at {dropPos}");

            slots[slotIndex] = null;
            activeSlot = -1;
            FindFirstObjectByType<PlayerUIManager>()?.UpdateInventoryUI();
            return;
        }

        // --- Si no hay nada en mano ni mochila ---
        Debug.Log("[Drop] Nothing to drop in current slot");
    }


    // Abrir mochila del jugador (en mano o equipada)
    public void OpenBackpack()
    {
        if (backpackObj == null || slots[3] == null) return;

        var bd = backpackObj.GetComponent<BackpackData>();
        if (bd == null) return;

        var ui = FindFirstObjectByType<PlayerUIManager>();
        if (ui != null) ui.ShowBackpackInventory(bd, this); // Mostrar UI con slots internos
    }

    // Abrir mochila del mundo (item tirado)
    public void OpenBackpackWorld(NetworkedItem netItem)
    {
        if (netItem == null || netItem.itemData == null) return;

        var bd = netItem.GetComponent<BackpackData>();
        if (bd == null) return;

        var ui = FindFirstObjectByType<PlayerUIManager>();
        if (ui != null) ui.ShowBackpackInventory(bd, this);
    }

    public void StoreInBackpack(BackpackData backpack, ItemSO item, int slotIndex)
    {
        if (backpack == null) return;
        if (slotIndex < 0 || slotIndex >= backpack.internalSlots.Length) return;
        if (item == null || item.itemType == ItemType.Backpack) return;

        backpack.internalSlots[slotIndex] = item;

        // sincronizar por RPC
        var pv = backpack.GetComponent<PhotonView>();
        if (pv != null)
            pv.RPC("RPC_InitContents", RpcTarget.AllBuffered, (object)backpack.GetItemIds());

        FindFirstObjectByType<PlayerUIManager>()?.UpdateBackpackUI(backpack.internalSlots);
    }

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

        var pv = backpack.GetComponent<PhotonView>();
        if (pv != null) pv.RPC("RPC_InitContents", RpcTarget.AllBuffered, (object)backpack.GetItemIds());

        FindFirstObjectByType<PlayerUIManager>()?.UpdateBackpackUI(backpack.internalSlots);
    }


    // ---------- RPC PARA PARENT ----------
    [PunRPC]
    void RPC_AttachItemToPlayer(int itemViewID, int playerViewID, string slot)
    {
        PhotonView itemPV = PhotonView.Find(itemViewID);
        PhotonView playerPV = PhotonView.Find(playerViewID);

        if (itemPV == null || playerPV == null) return;

        var inv = playerPV.GetComponent<PlayerInventoryPhoton>();
        Transform parent = slot == "hand" ? inv.handSlot : inv.backSlot;

        Debug.Log($"[AttachItem] Attaching '{itemPV.name}' to {slot} of player '{playerPV.name}'");

        if (parent != null)
        {
            itemPV.transform.SetParent(parent, false);
            itemPV.transform.localPosition = Vector3.zero;
            itemPV.transform.localRotation = Quaternion.identity;
        }
    }
}