using Characters.PlayerController.Scripts;
using Characters.PlayerController.Scripts.Input;
using Items.Scripts;
using UnityEngine;
using Photon.Pun;
using Multiplayer.Inventory;
using System.Collections.Generic;
using Photon.Pun.Demo.PunBasics;
using UI;
using System.Collections;

public class InventoryControllerScript : MonoBehaviourPun
{
    [Header("References")]
    [SerializeField] private HandGrabberScript _handGrabber;
    [SerializeField] private PhotonObjectManagerScript _photonObjManager;
    [SerializeField] private Transform handSlot;
    [SerializeField] private PlayerUIManager _uiManager;

    public PlayerInputScript input;
    public InventorySlot[] inventory;
    public Dictionary<GameObject, ItemSO> itemSOs = new Dictionary<GameObject, ItemSO>();

    private GameObject currentHeldItem;
    private PhotonView _photonView;

    [Header("Debug")]
    public bool debug = true;
    
    [System.Serializable]
    public class InventorySlot {
        public ItemSO itemData;
        [HideInInspector] public GameObject itemObject;
    }
    
    public void Awake()
    {
        _photonView = GetComponent<PhotonView>();
        _handGrabber = GetComponent<HandGrabberScript>();
        input = GetComponent<PlayerInputScript>();
        _photonObjManager = PhotonObjectManagerScript.Instance;

        if (_uiManager == null)
            _uiManager = GetComponent<PlayerUIManager>();

        if (_uiManager != null)
            _uiManager.InitInventory(this);

        if (debug) Debug.Log($"[Inventory] Awake(): references -> " +
            $"PhotonView={_photonView != null}, " +
            $"HandGrabber={_handGrabber != null}, " +
            $"Input={input != null}, " +
            $"UI={_uiManager != null}");


        inventory = new InventorySlot[4];
        for (int i = 0; i < inventory.Length; i++)
            inventory[i] = new InventorySlot();    
    }
    
    private void Update()
    {
        if (!_photonView || !_photonView.IsMine || !input)
            return;

        if (!input.InventoryInteraction)
            return; // Nada que hacer este frame

        // se asume que InventoryInteraction = true
        if (_handGrabber.currentItem) {
            TryStoreCurrentItem();
        }
        else {
            EquipItemFromSlot(input.InventoryIndex);
        }

        // Muy importante: consumir el input después de usarlo
        //input.ConsumeInventoryInput();
    }


    #region Inventory Actions

    private void TryStoreCurrentItem()
    {
        var grabbable = _handGrabber.currentItem;
        var slotIndex = input.InventoryIndex;

        if (!IsValidStoreAttempt(grabbable, slotIndex))
            return;

        // ✅ Guardamos solo el ItemSO, nunca el GameObject físico
        inventory[slotIndex].itemData = grabbable.data;
        inventory[slotIndex].itemObject = null;

        if (_photonObjManager)
        {
            if (debug) Debug.Log($"[Inventory] Destroying world object for {grabbable.name}");
            _photonObjManager.DestroyObjectForAll(grabbable.gameObject);
        }
        else
        {
            Debug.LogError("[Inventory] _photonObjManager is NULL! Cannot destroy object.");
        }

        currentHeldItem = null; // Liberamos referencia global

        _uiManager?.UpdateInventoryUI();

        if (debug) Debug.Log($"[Inventory] Stored '{grabbable.data?.name ?? "NULL DATA"}' at slot {slotIndex}");
    }

    public void EquipItemFromSlot(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= inventory.Length)
            return;

        var slot = inventory[slotIndex];
        var itemData = slot.itemData;

        if (!IsValidEquipAttempt(slot, slotIndex))
            return;

        if (debug) Debug.Log($"[Inventory] Equipando slot {slotIndex}: {itemData.displayName ?? itemData.name}");

        // Instanciamos un nuevo objeto para el item
        Vector3 spawnPos = _handGrabber.leftGrabOrigin.position;
        Quaternion spawnRot = _handGrabber.leftGrabOrigin.rotation;

        string prefabName = itemData.worldPrefabName ?? itemData.heldPrefabName;
        var obj = _photonObjManager.InstantiateObjectForAll(prefabName, spawnPos, spawnRot);
        _photonObjManager.TransferOwnership(obj, PhotonNetwork.LocalPlayer);

        // HandGrabber lo agarra
        _handGrabber.GrabNetworkedObjectFromInventory(obj);

        currentHeldItem = obj;

        // ✅ Limpiamos el slot, sin tocar otros objetos
        slot.itemData = null;
        slot.itemObject = null;

        _uiManager?.UpdateInventoryUI();

        if (debug) Debug.Log($"[Inventory] Equipped {itemData.displayName ?? itemData.name} via HandGrabber inventory flow.");
    }

    #endregion

    #region Helpers 
    private bool IsValidStoreAttempt(ItemGrabbableScript grabbable, int slotIndex)
    {
        if (!grabbable)
        {
            Debug.LogWarning("[Inventory] Item grabbable is null, can't try to store.");
            return false;
        }

        if (slotIndex < 0 || slotIndex >= inventory.Length)
        {
            Debug.LogWarning($"[Inventory] Slot index {slotIndex} out of range.");
            return false;
        }

        if (inventory[slotIndex].itemData)
        {
            Debug.LogWarning($"[Inventory] Inventory slot {slotIndex} already occupied by {inventory[slotIndex].itemData.name}");
            return false;
        }

        return true;
    }

    private bool IsValidEquipAttempt(InventorySlot slot, int slotIndex)
    {
        if (slot == null)
        {
            Debug.LogWarning($"[Inventory] Tried to equip slot {slotIndex} but it's null.");
            return false;
        }

        if (!slot.itemData)
        {
            Debug.LogWarning($"[Inventory] Tried to equip slot {slotIndex} but it’s empty.");
            return false;
        }

        return true;
    }
    #endregion
}