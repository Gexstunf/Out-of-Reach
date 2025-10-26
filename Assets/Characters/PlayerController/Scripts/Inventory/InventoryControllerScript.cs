using Characters.PlayerController.Scripts;
using Characters.PlayerController.Scripts.Input;
using Items.Scripts;
using UnityEngine;
using Photon.Pun;
using Multiplayer.Inventory;
using System.Collections.Generic;
using Photon.Pun.Demo.PunBasics;
using UI;

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
        public GameObject itemObject; // null when stored, set when equipped
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

        // Store only data — not GameObject
        inventory[slotIndex].itemData = grabbable.data;
        inventory[slotIndex].itemObject = null;

        Debug.Log($"[Inventory] Stored '{grabbable.data?.name ?? "NULL DATA"}' at slot {slotIndex}");

        // Destroy world object
        if (_photonObjManager)
        {
            Debug.Log($"[Inventory] Destroying world object for {grabbable.name}");
            _photonObjManager.DestroyObjectForAll(grabbable.gameObject);
        }
        else
        {
            Debug.LogError("[Inventory] _photonObjManager is NULL! Cannot destroy object.");
        }

        _uiManager?.UpdateInventoryUI();
    }

    public void EquipItemFromSlot(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= inventory.Length)
            return;

        var slot = inventory[slotIndex];
        var itemData = slot.itemData;

        if (!IsValidEquipAttempt(slot, slotIndex))
            return;

        Vector3 spawnPos = _handGrabber.leftGrabOrigin.position;
        Quaternion spawnRot = _handGrabber.leftGrabOrigin.rotation;

        string prefabName = itemData.worldPrefabName ?? itemData.heldPrefabName;

        Debug.Log($"[Inventory] Attempting to equip slot {slotIndex}, item = {itemData.displayName ?? itemData.name}");

        // Destroy current held item
        if (currentHeldItem)
        {
            Debug.Log($"[Inventory] Destroying current held item {currentHeldItem.name}");
            _photonObjManager.DestroyObjectForAll(currentHeldItem);
            currentHeldItem = null;
        }

        // Spawn and grab new item
        Debug.Log($"[Inventory] Spawning prefab: {prefabName}");
        var obj = _photonObjManager.InstantiateObjectForAll(prefabName, spawnPos, spawnRot);
        _handGrabber.currentItem = obj.GetComponent<ItemGrabbableScript>();
        _handGrabber.RegisterAndGrabItem(_handGrabber.currentItem, _handGrabber.itemHoldingHand, spawnPos);

        currentHeldItem = obj;
        Debug.Log($"[Inventory] Equipped {itemData.displayName ?? itemData.name} to hand.");

        // Clear slot (item is now equipped)
        slot.itemData = null;
        slot.itemObject = null;

        _uiManager?.UpdateInventoryUI();
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