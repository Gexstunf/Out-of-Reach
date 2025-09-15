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

        // Guardamos los datos temporales (no networked todavía)
        tempHeldPrefabName = heldPrefabName;
        tempBackpackIds = backpackIds;
        tempItemData = ItemDatabase.FindByHeldPrefabName(heldPrefabName);

        // --- INSTANTIATE LOCAL VISUAL (no PhotonNetwork.Instantiate) ---
        GameObject prefab = Resources.Load<GameObject>(heldPrefabName);
        if (prefab == null)
        {
            Debug.LogError($"[Inventory] No se encontró prefab en Resources con nombre '{heldPrefabName}'");
            return;
        }

        // Instanciamos localmente para que el jugador lo vea en la mano mientras elige el slot
        tempHeldObj = Instantiate(prefab, handSlot.position, handSlot.rotation);
        tempHeldObj.transform.SetParent(handSlot, false);

        // Evitar que el objeto local intente registrarse en PUN (si el prefab tiene PhotonView)
        var pv = tempHeldObj.GetComponent<PhotonView>();
        if (pv != null) Destroy(pv);

        var rb = tempHeldObj.GetComponent<Rigidbody>();
        if (rb != null) { rb.isKinematic = true; rb.useGravity = false; }

        // Inicializar BackpackData localmente para la UI/local preview
        var bd = tempHeldObj.GetComponent<BackpackData>() ?? tempHeldObj.AddComponent<BackpackData>();
        if (tempBackpackIds != null && tempBackpackIds.Length > 0)
        {
            bd.SetFromIds(tempBackpackIds);
        }
        else if (tempItemData != null && tempItemData.internalSlots != null)
        {
            bd.internalSlots = new ItemSO[tempItemData.internalSlots.Length];
            for (int i = 0; i < bd.internalSlots.Length && i < tempItemData.internalSlots.Length; i++)
                bd.internalSlots[i] = tempItemData.internalSlots[i];
        }

        Debug.Log($"[Inventory] Spawned LOCAL temp held '{heldPrefabName}' ({(tempItemData != null ? tempItemData.displayName : "null")})");
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

        // IMPORTANTE: ahora al colocar realmente creamos la instancia NETWORKED (PhotonNetwork.Instantiate)
        if (tempItemData.itemType == ItemType.Backpack)
        {
            // Destruir preview local
            DestroyTempHeldLocal();

            // Instanciamos el prefab "held" pero networked para que todos vean la mochila en el jugador
            GameObject netBack = PhotonNetwork.Instantiate(tempItemData.heldPrefabName, backSlot.position, backSlot.rotation);
            backpackObj = netBack;
            var bd = backpackObj.GetComponent<BackpackData>() ?? backpackObj.AddComponent<BackpackData>();

            // Inicializar contents: preferimos usar los ids que recibimos al pickear
            var netPV = backpackObj.GetComponent<PhotonView>();
            if (tempBackpackIds != null && tempBackpackIds.Length > 0)
            {
                if (netPV != null)
                    netPV.RPC("RPC_InitContents", RpcTarget.AllBuffered, (object)tempBackpackIds);
                else
                    bd.SetFromIds(tempBackpackIds);
            }
            else
            {
                if (tempItemData != null && tempItemData.internalSlots != null)
                {
                    bd.internalSlots = new ItemSO[tempItemData.internalSlots.Length];
                    for (int i = 0; i < bd.internalSlots.Length && i < tempItemData.internalSlots.Length; i++)
                        bd.internalSlots[i] = tempItemData.internalSlots[i];

                    if (netPV != null) netPV.RPC("RPC_InitContents", RpcTarget.AllBuffered, (object)bd.GetItemIds());
                }
            }

            backpackObj.transform.SetParent(backSlot, false);
            backpackObj.transform.localPosition = Vector3.zero;
            backpackObj.transform.localRotation = Quaternion.identity;

            photonView.RPC(nameof(RPC_AttachItemToPlayer),
                RpcTarget.AllBuffered,
                backpackObj.GetComponent<PhotonView>().ViewID,
                photonView.ViewID,
                "back");
        }
        else
        {
            // item normal → instanciar networked y equipar en mano
            DestroyTempHeldLocal();

            string heldPrefabName = tempItemData.heldPrefabName;
            GameObject held = PhotonNetwork.Instantiate(heldPrefabName, handSlot.position, handSlot.rotation);

            currentHeldNetworkObj = held;
            currentHeldViewId = held.GetComponent<PhotonView>().ViewID;
            activeSlot = slotIndex;

            // asegurar física visual (opcional)
            var rb = held.GetComponent<Rigidbody>();
            if (rb != null) { rb.isKinematic = true; rb.useGravity = false; }

            photonView.RPC(nameof(RPC_AttachItemToPlayer),
                RpcTarget.AllBuffered,
                currentHeldViewId,
                photonView.ViewID,
                "hand");
        }

        // Limpiar temp data
        tempHeldObj = null;
        tempItemData = null;
        tempHeldPrefabName = null;
        tempBackpackIds = null;

        var ui = FindFirstObjectByType<PlayerUIManager>();
        if (ui != null) ui.UpdateInventoryUI();
    }

    // ---------- EQUIPAR (TOGGLE si re-seleccionas) ----------
    public void EquipFromSlot(int slotIndex)
    {
        if (!photonView.IsMine) return;
        if (slotIndex < 0 || slotIndex >= slots.Length) return;
        if (slots[slotIndex] == null) return;

        ItemSO itemData = slots[slotIndex];

        // Si el slot ya está activo (lo tienes en la mano), re-seleccionarlo -> dejar manos vacías
        if (activeSlot == slotIndex)
        {
            if (itemData.itemType == ItemType.Backpack)
            {
                if (backpackObj == null) return;
                // mover a espalda (manos vacías) — esto deja la mochila en el jugador pero no en la mano
                photonView.RPC(nameof(RPC_AttachItemToPlayer),
                    RpcTarget.AllBuffered,
                    backpackObj.GetComponent<PhotonView>().ViewID,
                    photonView.ViewID,
                    "back");
                activeSlot = -1;
            }
            else
            {
                // destruir el objeto en mano (networked)
                HolsterCurrent();
                activeSlot = -1;
            }

            var uiToggle = FindFirstObjectByType<PlayerUIManager>();
            if (uiToggle != null) uiToggle.UpdateInventoryUI();
            return;
        }

        // --- SI NO ESTABA ACTIVO, equipar ---
        // --- MOCHILA ---
        if (itemData.itemType == ItemType.Backpack)
        {
            if (backpackObj == null)
            {
                // si no existe instancia networked aún, crearla en la espalda
                GameObject netBack = PhotonNetwork.Instantiate(itemData.heldPrefabName, backSlot.position, backSlot.rotation);
                backpackObj = netBack;

                // (Si tienes un sistema para guardar contenidos por slot, deberías inicializarlos aquí)
                // por ahora dejamos la mochila en la espalda
                photonView.RPC(nameof(RPC_AttachItemToPlayer),
                    RpcTarget.AllBuffered,
                    backpackObj.GetComponent<PhotonView>().ViewID,
                    photonView.ViewID,
                    "back");
                activeSlot = -1;

                var ui1 = FindFirstObjectByType<PlayerUIManager>();
                if (ui1 != null) ui1.UpdateInventoryUI();
                return;
            }

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
            var ui2 = FindFirstObjectByType<PlayerUIManager>();
            if (ui2 != null) ui2.UpdateInventoryUI();
            return;
        }

        // --- ITEM NORMAL ---
        // si ya hay algo en mano, destruirlo
        HolsterCurrent();

        // instanciar y equipar (networked)
        string heldPrefabName = slots[slotIndex].heldPrefabName;
        GameObject held = PhotonNetwork.Instantiate(heldPrefabName, handSlot.position, handSlot.rotation);

        currentHeldNetworkObj = held;
        currentHeldViewId = held.GetComponent<PhotonView>().ViewID;
        activeSlot = slotIndex;

        // asegurar física visual (opcional)
        var rbHeld = held.GetComponent<Rigidbody>();
        if (rbHeld != null) { rbHeld.isKinematic = true; rbHeld.useGravity = false; }

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

            // destruir la instancia visual (networked)
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

        // limpiar el slot (sigue vaciándose el slot)
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
        DestroyTempHeldLocal();
        tempHeldObj = null;
        tempItemData = null;
        tempHeldPrefabName = null;
        tempBackpackIds = null;
    }

    // helper: destruye el tempHeldObj correctamente (si fue networked usar PhotonNetwork.Destroy, si fue local usar Destroy)
    private void DestroyTempHeldLocal()
    {
        if (tempHeldObj == null) return;
        var pv = tempHeldObj.GetComponent<PhotonView>();
        if (pv != null && pv.ViewID != 0)
            PhotonNetwork.Destroy(tempHeldObj);
        else
            Destroy(tempHeldObj);
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
