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
    public Dictionary<GameObject, ItemSO> itemSOs;

    private GameObject currentHeldItem;

    private PhotonView _photonView;

    public void Awake()
    {
        _photonView = GetComponent<PhotonView>();
        _uiManager = GetComponent<PlayerUIManager>();
        _handGrabber = GetComponent<HandGrabberScript>();
        input = GetComponent<PlayerInputScript>();
    }

    private void Update()
    {
        if (!photonView.IsMine) return;

        // Store grabbed item in inventory
        if (input.InventoryInteraction && _handGrabber.currentItem != null)
        {
            TryStoreCurrentItem();
        }

        // Equip item from inventory to hand
        if (input.InventoryInteraction)
        {
            int index = input.InventoryIndex;
            if (index >= 0 && index < inventory.Length)
                EquipItemFromSlot(index);
        }
    }

    #region Inventory Actions

    private void TryStoreCurrentItem()
    {
        var grabbable = _handGrabber.currentItem as ItemGrabbableScript;
        if (grabbable == null) return;

        int slotIndex = input.InventoryIndex;
        if (slotIndex < 0 || slotIndex >= inventory.Length) return;
        if (inventory[slotIndex] != null) return; // slot full

        // Store item
        inventory[slotIndex] = grabbable.gameObject;
        itemSOs[grabbable.gameObject] = grabbable.data;

        // Remove it from the world
        _photonObjManager.DestroyObjectForAll(grabbable.gameObject);

        _uiManager?.UpdateInventoryUI();
    }

    private void EquipItemFromSlot(int slotIndex)
    {
        GameObject itemObject = inventory[slotIndex];
        if (itemObject == null) return;

        if (!itemSOs.TryGetValue(itemObject, out ItemSO itemData)) return;

        // Destroy any currently held item first
        if (currentHeldItem != null)
        {
            _photonObjManager.DestroyObjectForAll(currentHeldItem);
            currentHeldItem = null;
        }

        // Instantiate in hand
        string prefabName = itemData.worldPrefabName; // or heldPrefabName if you use separate prefab
        Vector3 spawnPos = _handGrabber.leftGrabOrigin.position;
        Quaternion spawnRot = _handGrabber.leftGrabOrigin.rotation;

        var obj = _photonObjManager.InstantiateObjectForAll(prefabName, spawnPos, spawnRot);
        _handGrabber.currentItem = obj.GetComponent<ItemGrabbableScript>();
        _handGrabber.RegisterAndGrabItem(_handGrabber.currentItem, _handGrabber.itemHoldingHand, spawnPos);

        // Clear slot
        inventory[slotIndex] = null;
        itemSOs.Remove(itemObject);

        _uiManager?.UpdateInventoryUI();
    }

    public void DropCurrentHeld()
    {
        if (currentHeldItem == null) return;

        // Re-spawn in world
        if (itemSOs.TryGetValue(currentHeldItem, out ItemSO itemData))
        {
            Vector3 dropPos = transform.position + transform.forward * 1f;
            _photonObjManager.InstantiateObjectForAll(itemData.worldPrefabName, dropPos, Quaternion.identity);
        }

        _photonObjManager.DestroyObjectForAll(currentHeldItem);
        currentHeldItem = null;

        _uiManager?.UpdateInventoryUI();
    }

    #endregion
}

/*
    public void Update()
    {
        if (!photonView.IsMine) return;

        if (input.InventoryInteraction && _handGrabber.currentItem != null)
        {
            // handGrabber.currentItem =   IGrabbableScript  ( a generic grabbable class )
            //ItemGrabbableScript itemGrabbable = GetComponent<>();

            ItemGrabbableScript itemGrabbable = _handGrabber.currentItem as ItemGrabbableScript;

            if (!itemGrabbable) return;

            GameObject itemObject = itemGrabbable.gameObject;

            for (int i = 0; i < inventory.Length; i++) {
                // bring in logic
                if (input.InventoryIndex == i && inventory[i] == null)
                {
                    //save a reference to the inventory for later instantiation.
                    inventory[i] = itemObject;
                    itemSOs.Add(itemObject, itemGrabbable.data);
                    DestroyObject(itemObject);
                }
            }
        }

        if (input.InventoryInteraction)
        {
            for (int i = 0; i < inventory.Length; i++)
            {

                if (input.InventoryIndex == i && inventory[i] != null)
                {
                    // bring out logic
                }
            }
        }
    }

    private void DestroyObject(GameObject obj)
    {
        _photonObjManager.DestroyObjectForAll(obj);
    }

    private void InstantiateObject(GameObject obj)
    {
        for (int i = 0; i < inventory.Length; i++)
        {
            if (inventory[i] == obj)
            {
                inventory[i] = null;
                if (!itemSOs[obj]) return;
                _photonObjManager.InstantiateObjectForAll(
                    itemSOs[obj].prefab.name,
                    transform.InverseTransformPoint(transform.localPosition),
                    transform.rotation
                );
            }
        }
    }
}
*/
