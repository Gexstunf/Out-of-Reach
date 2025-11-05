using System.Collections.Generic;
using Characters.PlayerController.Scripts.Input;
using Items.Scripts;
using Multiplayer.Inventory;
using Multiplayer.UI;
using Photon.Pun;
using UI;
using UnityEngine;

namespace Characters.PlayerController.Scripts.Inventory {
    public class InventoryControllerScript : MonoBehaviourPun
    {
        [Header("References")]
        [SerializeField] private HandGrabberScript _handGrabber;
        [SerializeField] private PhotonObjectManagerScript _photonObjManager;
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
            public ItemGrabbableScript itemGrabbableScript;
            public ItemInteractionScript itemInteractionScript;
            public ItemSO itemData;
            public GameObject itemObject;
        }
    
        public void Awake()
        {
            _photonView = GetComponent<PhotonView>();
            _handGrabber = GetComponent<HandGrabberScript>();
            input = GetComponent<PlayerInputScript>();
            _photonObjManager = PhotonObjectManagerScript.Instance;

            if (_uiManager == null)
                _uiManager = GetComponent<PlayerUIManager>();

            // if (debug) Debug.Log($"[Inventory] Awake(): references -> " +
            //                      $"PhotonView={_photonView != null}, " +
            //                      $"HandGrabber={_handGrabber != null}, " +
            //                      $"Input={input != null}, " +
            //                      $"UI={_uiManager != null}");


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
            inventory[slotIndex].itemObject = grabbable.gameObject;
            inventory[slotIndex].itemGrabbableScript = grabbable;
            inventory[slotIndex].itemInteractionScript = grabbable.interactionScript;
            _handGrabber.SetInteractWithGrabbable(false);
            

            if (_photonObjManager)
            {
                //if (debug) Debug.Log($"[Inventory] Destroying world object for {grabbable.name}");
                //_photonObjManager.DestroyObjectForAll(grabbable.gameObject);
                //_handGrabber.currentItem = null;
                HideItem(grabbable.gameObject);
            }
            else
            {
                Debug.LogError("[Inventory] _photonObjManager is NULL! Cannot destroy object.");
            }
            
            currentHeldItem = null; // Liberamos referencia global
            _uiManager?.UpdateHotbarUI();

            //if (debug) Debug.Log($"[Inventory] Stored '{grabbable.data?.name ?? "NULL DATA"}' at slot {slotIndex}");
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

            // Instanciamos un nuevo objeto para el item en la mano
            Vector3 spawnPos = _handGrabber.leftGrabOrigin.position;
            Quaternion spawnRot = _handGrabber.leftGrabOrigin.rotation;

            //string prefabName = itemData.worldPrefabName ?? itemData.heldPrefabName;
            //var obj = _photonObjManager.InstantiateObjectForAll(prefabName, spawnPos, spawnRot);
            //_photonObjManager.TransferOwnership(obj, PhotonNetwork.LocalPlayer);

            // HandGrabber lo agarra
            _handGrabber.currentItem = slot.itemGrabbableScript;
            ShowItem(slot.itemObject, spawnPos, transform.rotation);
            
            if (slot.itemInteractionScript) _handGrabber.SetInteractWithGrabbable(true);
            
            _handGrabber.RegisterAndGrabItemLeftHand(slot.itemGrabbableScript, spawnPos);
            //_handGrabber.GrabNetworkedObjectFromInventory(obj);
        
            //currentHeldItem = obj;
            currentHeldItem = slot.itemObject;
            // ✅ Limpiamos el slot, sin tocar otros objetos
            slot.itemData = null;
            slot.itemObject = null;
            slot.itemInteractionScript = null;

            _uiManager?.UpdateHotbarUI();

            //if (debug) Debug.Log($"[Inventory] Equipped {itemData.displayName ?? itemData.name} via HandGrabber inventory flow.");
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
        
        private void HideItem(GameObject item) {
            item.transform.position = Vector3.zero;
            item.SetActive(false);
        }
        
        private void ShowItem(GameObject item, Vector3 showPosition, Quaternion rotation) {
            item.transform.position = showPosition;
            item.transform.rotation = rotation;
            item.SetActive(true);
        }
        
        #endregion
    }
}