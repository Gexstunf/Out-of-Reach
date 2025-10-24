using System.Collections;
using Photon.Pun;
using UnityEngine;
using UI;
using Characters.PlayerController.Scripts.Input;
using Multiplayer.Inventory;
using Characters.PlayerController.Scripts;

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

            if (playerUI != null) ;
            //playerUI.InitInventory(this);
            else
                Debug.LogWarning("[Inventory] No se encontró PlayerUIManager en la jerarquía del jugador!");
        }
    }

    private void SpawnHeldItem(string prefabName, Transform parentSlot)
    {
        if (string.IsNullOrEmpty(prefabName)) return;

        if (currentHeldNetworkObj != null)
        {
            Debug.LogWarning("[SpawnHeldItem] Ya hay un objeto sostenido, destruyendo antes de instanciar uno nuevo.");
            PhotonNetwork.Destroy(currentHeldNetworkObj);
            currentHeldNetworkObj = null;
        }

        // spawn ligeramente delante del jugador
        Vector3 spawnPos = transform.position + transform.forward * 0.7f + Vector3.up * 1.3f;
        Quaternion spawnRot = transform.rotation;

        GameObject held = PhotonNetwork.Instantiate(prefabName, spawnPos, spawnRot);
        currentHeldNetworkObj = held;
        var pv = held.GetComponent<PhotonView>();
        currentHeldViewId = pv != null ? pv.ViewID : -1;

        // Si vienen solicitud para parentar a la "espalda", lo parentamos; para "mano" NO parentamos
        if (parentSlot != null && parentSlot == backSlot)
        {
            held.transform.SetParent(parentSlot, false);
            held.transform.localPosition = Vector3.zero;
            held.transform.localRotation = Quaternion.identity;
            // sincroniza RPC indicando back
            photonView.RPC(nameof(RPC_AttachItemToPlayer), RpcTarget.AllBuffered, currentHeldViewId, photonView.ViewID, "back");
        }
        else
        {
            // no parentear en mano: sólo avisamos en red que el item está "en mano" para representación si hace falta
            photonView.RPC(nameof(RPC_AttachItemToPlayer), RpcTarget.AllBuffered, currentHeldViewId, photonView.ViewID, "hand");
        }

        // Asegurarse que tenga Rigidbody interactivo (no kinematic) para que HandGrabber pueda crear joint y moverlo
        var rb = held.GetComponent<Rigidbody>();
        if (rb != null) { rb.isKinematic = false; rb.useGravity = true; }

        // Actualizar UI
        playerUI?.UpdateInventoryUI();

        // --- HANDGRABBER HOOK: pedir al grabber que lo agarre físicamente ---
        var grabber = GetComponent<HandGrabberScript>();
        if (grabber != null)
        {
            var grabbable = held.GetComponent<IGrabbableScript>();
            if (grabbable != null)
            {
                grabber.GrabNetworkedObject(held);
            }
        }
    }


    [PunRPC]
    public void RPC_SpawnTempHeld(string heldPrefabName, string[] backpackIds)
    {
        Debug.Log($"📥 [RPC_SpawnTempHeld] Recibido RPC en {gameObject.name}, prefab={heldPrefabName}");

        if (!photonView.IsMine)
        {
            Debug.Log($"🚫 [RPC_SpawnTempHeld] Ignorado porque no es mío ({gameObject.name})");
            return;
        }

        if (string.IsNullOrEmpty(heldPrefabName))
        {
            Debug.LogError("❌ [RPC_SpawnTempHeld] heldPrefabName vacío");
            return;
        }

        tempItemData = ItemDatabase.FindByHeldPrefabName(heldPrefabName);
        if (tempItemData == null)
        {
            Debug.LogError($"❌ [RPC_SpawnTempHeld] No se encontró ItemSO para {heldPrefabName}");
            return;
        }

        GameObject prefab = Resources.Load<GameObject>(heldPrefabName);
        if (prefab == null)
        {
            Debug.LogError($"❌ [RPC_SpawnTempHeld] No se encontró prefab en Resources: {heldPrefabName}");
            return;
        }

        // Determinar posición de spawn cerca de la mano
        Vector3 handHeight = transform.position + Vector3.up * 1.0f; // ajustar según tu rig
        Vector3 spawnPos = handHeight + transform.forward * 0.5f;

        tempHeldObj = Instantiate(prefab, spawnPos, transform.rotation);

        // Lo agarra físicamente el HandGrabber usando el flujo networked
        var grabber = GetComponent<HandGrabberScript>();
        if (grabber != null)
        {
            grabber.GrabNetworkedObject(tempHeldObj);
        }

        Debug.Log($"✅ [RPC_SpawnTempHeld] Prefab temporal instanciado y agarrado físicamente ({prefab.name})");
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
            SpawnHeldItem(tempItemData.heldPrefabName, null);
            backpackObj = currentHeldNetworkObj;
        }
        else
        {
            DestroyTempHeldLocal();
            SpawnHeldItem(tempItemData.heldPrefabName, null);
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

        SpawnHeldItem(itemData.heldPrefabName, null);
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

        Vector3 dropPos = transform.position + transform.forward * 1.2f + Vector3.up * 0.3f;

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

            // Soltar físicamente
            var grabber = GetComponent<Characters.PlayerController.Scripts.HandGrabberScript>();
            if (grabber != null)
            {
                grabber.OnItemReleased();
            }
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
        StartCoroutine(AttachWhenReady(itemViewID, playerViewID, "hand"));
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
        if (inv == null)
        {
            yield break;
        }

        // Si es "back", parentamos al backSlot para la representación en espalda.
        if (slot == "back" && inv.backSlot != null)
        {
            itemPV.transform.SetParent(inv.backSlot, false);
            itemPV.transform.localPosition = Vector3.zero;
            itemPV.transform.localRotation = Quaternion.identity;
            Debug.Log($"[AttachItem] {itemPV.name} parented to back of {playerPV.name}");
        }
        else
        {
            // Si es "hand" NO parentear; mantener en mundo. El HandGrabber local del player debe ya haber agarrado
            // la instancia local o la instancia puede ser agarrada por el grabber cuando sea necesario.
            Debug.Log($"[AttachItem] {itemPV.name} left unparented for hand on {playerPV.name}");
        }
    }


    public void NotifyItemGrabbed(ItemSO itemData, GameObject worldObj)
    {
        Debug.Log($"[NotifyItemGrabbed] photonView.IsMine={photonView.IsMine} para {gameObject.name}");
        if (!photonView.IsMine)
        {
            Debug.LogWarning("[NotifyItemGrabbed] No es mío, se ignora.");
            return;
        }

        if (tempHeldObj != null)
        {
            Debug.LogWarning("[NotifyItemGrabbed] Ya hay un objeto temporal, se cancela.");
            return;
        }

        if (itemData == null || worldObj == null)
        {
            Debug.LogError("[NotifyItemGrabbed] Parámetros inválidos (itemData o worldObj nulos)");
            return;
        }

        Debug.Log($"[NotifyItemGrabbed] Ejecutando para {itemData.name}, prefab={itemData.heldPrefabName}");

        tempItemData = itemData;
        tempHeldObj = worldObj;

        // Desactiva localmente el objeto del mundo (sin destruirlo todavía)
        if (worldObj.activeSelf)
        {
            worldObj.SetActive(false);
            Debug.Log($"[NotifyItemGrabbed] Objeto físico desactivado: {worldObj.name}");
        }

        // Enviar el RPC correctamente con un array vacío
        photonView.RPC(nameof(RPC_SpawnTempHeld), RpcTarget.AllBuffered, itemData.heldPrefabName, new string[0]);

        // Si el jugador es el dueño del objeto físico en Photon, destruirlo en red
        PhotonView objPV = worldObj.GetComponent<PhotonView>();
        if (objPV != null && objPV.IsMine)
        {
            PhotonNetwork.Destroy(worldObj);
            Debug.Log($"[NotifyItemGrabbed] Destruido objeto del mundo en red: {worldObj.name}");
        }
    }
}
