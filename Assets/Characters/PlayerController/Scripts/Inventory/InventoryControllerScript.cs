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
    public GameObject[] inventory;
    public Dictionary<GameObject, ItemSO> itemSOs = new Dictionary<GameObject, ItemSO>();

    private GameObject currentHeldItem;

    private PhotonView _photonView;

    [Header("Debug")]
    public bool debug = true;

    public void Awake()
    {
        _photonView = GetComponent<PhotonView>();
        _handGrabber = GetComponent<HandGrabberScript>();
        input = GetComponent<PlayerInputScript>();

        if (_uiManager == null)
            _uiManager = GetComponent<PlayerUIManager>();

        if (_uiManager != null)
            _uiManager.InitInventory(this);

        if (debug) Debug.Log($"[Inventory] Awake(): references -> " +
            $"PhotonView={_photonView != null}, " +
            $"HandGrabber={_handGrabber != null}, " +
            $"Input={input != null}, " +
            $"UI={_uiManager != null}");
    }

    private void Update()
    {
        if (!photonView.IsMine) return;

        // Debug the index
        if (debug && input.InventoryInteraction)
            Debug.Log($"[Inventory] InventoryInteraction triggered. Index = {input.InventoryIndex}, " +
                      $"CurrentItem = {_handGrabber.currentItem}, " +
                      $"IsMine = {photonView.IsMine}");

        // Store grabbed item
        if (input.InventoryInteraction && _handGrabber.currentItem != null)
        {
            TryStoreCurrentItem();
        }
        // Equip from slot
        else if (input.InventoryInteraction)
        {
            int index = input.InventoryIndex;
            if (index >= 0 && index < inventory.Length)
                EquipItemFromSlot(index);
            else if (debug)
                Debug.LogWarning($"[Inventory] Invalid index ({index}) - can't equip.");
        }
    }

    #region Inventory Actions

    private void TryStoreCurrentItem()
    {
        var grabbable = _handGrabber.currentItem as ItemGrabbableScript;
        if (grabbable == null)
        {
            Debug.LogWarning("[Inventory] Item grabbable is null, can't try to store.");
            return;
        }

        int slotIndex = input.InventoryIndex;
        if (slotIndex < 0 || slotIndex >= inventory.Length)
        {
            Debug.LogWarning($"[Inventory] Slot index {slotIndex} out of range.");
            return;
        }

        if (inventory[slotIndex] != null)
        {
            Debug.LogWarning($"[Inventory] Inventory slot {slotIndex} already occupied by {inventory[slotIndex].name}");
            return;
        }

        Debug.Log($"[Inventory] Storing item '{grabbable.gameObject.name}' with data '{grabbable.data?.name ?? "NULL DATA"}' at slot {slotIndex}");

        // Store item
        inventory[slotIndex] = grabbable.gameObject;
        try
        {
            itemSOs[grabbable.gameObject] = grabbable.data;
            Debug.Log($"[Inventory] Added {grabbable.gameObject.name} -> {grabbable.data?.name ?? "NULL"} to itemSOs dictionary. Total count: {itemSOs.Count}");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[Inventory] Failed to add to dictionary: {e.Message}");
        }

        // Remove from world
        if (_photonObjManager != null)
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
        GameObject itemObject = inventory[slotIndex];
        if (itemObject == null)
        {
            Debug.LogWarning($"[Inventory] Tried to equip slot {slotIndex} but it’s empty.");
            return;
        }

        Debug.Log($"[Inventory] Attempting to equip slot {slotIndex}, item = {itemObject.name}");

        if (!itemSOs.TryGetValue(itemObject, out ItemSO itemData))
        {
            Debug.LogError($"[Inventory] No ItemSO found for {itemObject.name}! itemSOs.Count = {itemSOs.Count}");
            return;
        }

        // Destroy current held item
        if (currentHeldItem != null)
        {
            Debug.Log($"[Inventory] Destroying current held item {currentHeldItem.name}");
            _photonObjManager.DestroyObjectForAll(currentHeldItem);
            currentHeldItem = null;
        }

        string prefabName = itemData.worldPrefabName ?? itemData.heldPrefabName;
        Debug.Log($"[Inventory] Spawning prefab: {prefabName}");

        if (string.IsNullOrEmpty(prefabName))
        {
            Debug.LogError($"[Inventory] Prefab name is null for {itemData.name}");
            return;
        }

        if (_handGrabber == null || _handGrabber.leftGrabOrigin == null)
        {
            Debug.LogError("[Inventory] HandGrabber or its grab origin is null.");
            return;
        }

        Vector3 spawnPos = _handGrabber.leftGrabOrigin.position;
        Quaternion spawnRot = _handGrabber.leftGrabOrigin.rotation;

        var obj = _photonObjManager.InstantiateObjectForAll(prefabName, spawnPos, spawnRot);
        if (obj == null)
        {
            Debug.LogError($"[Inventory] Failed to instantiate {prefabName}");
            return;
        }

        _handGrabber.currentItem = obj.GetComponent<ItemGrabbableScript>();
        if (_handGrabber.currentItem == null)
            Debug.LogError($"[Inventory] Spawned object {prefabName} missing ItemGrabbableScript!");

        _handGrabber.RegisterAndGrabItem(_handGrabber.currentItem, _handGrabber.itemHoldingHand, spawnPos);

        Debug.Log($"[Inventory] Equipped {itemData.displayName ?? itemData.name} to hand.");

        inventory[slotIndex] = null;
        itemSOs.Remove(itemObject);
        _uiManager?.UpdateInventoryUI();
    }

    #endregion
}