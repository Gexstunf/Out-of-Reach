using System.Collections;
using Photon.Pun;
using UnityEngine;
using UI;
using Characters.PlayerController.Scripts.Input;
using Multiplayer.Inventory;

public class PlayerInventoryPhoton : MonoBehaviourPun
{
    [Header("Referencias")]
    [SerializeField] public PlayerInputScript inputScript;
    [SerializeField] private PlayerUIManager playerUI;
    [SerializeField] public Transform handSlot;
    [SerializeField] public Transform backSlot;
    [SerializeField] public LayerMask itemLayer;
    [SerializeField] public float dropForce = 3f;

    [Header("Inventario")]
    [SerializeField] public ItemSO[] slots = new ItemSO[4]; // Slots 0-2 normales, slot 3 mochila
    [SerializeField] public int activeSlot = -1;

    private GameObject currentHeldNetworkObj;
    private int currentHeldViewId = -1;

    [SerializeField] public GameObject backpackObj; // referencia a la mochila networked

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
            if (playerUI == null)
                playerUI = GetComponentInChildren<PlayerUIManager>();

            if (playerUI != null)
                playerUI.InitInventory(this);
            else
                Debug.LogWarning("[Inventory] No se encontró PlayerUIManager en la jerarquía del jugador!");
        }
    }

    private void SpawnHeldItem(string prefabName, Transform parentSlot)
    {
        if (string.IsNullOrEmpty(prefabName) || parentSlot == null) return;

        if (currentHeldNetworkObj != null)
        {
            Debug.LogWarning("[SpawnHeldItem] Ya hay un objeto sostenido, destruyendo antes de instanciar uno nuevo.");
            PhotonNetwork.Destroy(currentHeldNetworkObj);
            currentHeldNetworkObj = null;
        }

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

        playerUI?.UpdateInventoryUI();
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
            playerUI?.UpdateInventoryUI();
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
            playerUI?.UpdateInventoryUI();
            return;
        }

        SpawnHeldItem(itemData.heldPrefabName, handSlot);
        activeSlot = slotIndex;
        playerUI?.UpdateInventoryUI();
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
            playerUI?.UpdateInventoryUI();
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
            playerUI?.UpdateInventoryUI();
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
        playerUI?.ShowBackpackInventory(bd, this);
    }

    public void StoreInBackpack(BackpackData backpack, ItemSO item, int slotIndex)
    {
        if (backpack == null || item == null || slotIndex < 0 || slotIndex >= backpack.internalSlots.Length) return;
        if (item.itemType == ItemType.Backpack) return;

        backpack.internalSlots[slotIndex] = item;

        var pv = backpack.GetComponent<PhotonView>();
        if (pv != null) pv.RPC("RPC_InitContents", RpcTarget.AllBuffered, (object)backpack.GetItemIds());

        playerUI?.UpdateBackpackUI(backpack.internalSlots);
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

        playerUI?.UpdateBackpackUI(backpack.internalSlots);
    }

    [PunRPC]
    void RPC_AttachItemToPlayer(int itemViewID, int playerViewID, string slot)
    {
        StartCoroutine(AttachWhenReady(itemViewID, playerViewID, slot));
    }

    private IEnumerator AttachWhenReady(int itemViewID, int playerViewID, string slot)
    {
        PhotonView itemPV = null;
        PhotonView playerPV = null;

        yield return new WaitUntil(() =>
        {
            itemPV = PhotonView.Find(itemViewID);
            playerPV = PhotonView.Find(playerViewID);
            return itemPV != null && playerPV != null;
        });

        var inv = playerPV.GetComponent<PlayerInventoryPhoton>();
        Transform parent = slot == "hand" ? inv.handSlot : inv.backSlot;

        if (parent != null)
        {
            itemPV.transform.SetParent(parent, false);
            itemPV.transform.localPosition = Vector3.zero;
            itemPV.transform.localRotation = Quaternion.identity;
            Debug.Log($"[AttachItem] {itemPV.name} parented to {slot} of {playerPV.name}");
        }
    }

    public void NotifyItemGrabbed(ItemSO itemData, GameObject worldObj)
    {
        if (!photonView.IsMine) return;
        if (tempHeldObj != null) return; // ya hay un temporal

        tempItemData = itemData;
        tempHeldObj = worldObj;

        // Desactiva el objeto físico localmente (sin destruirlo aún)
        worldObj.SetActive(false);

        // Envía la orden a todos los clientes para spawnear la versión temporal local
        photonView.RPC(nameof(RPC_SpawnTempHeld), RpcTarget.AllBuffered, itemData.heldPrefabName, new string[0]);
    }
}
