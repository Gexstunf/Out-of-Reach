using System;
using GlobalUtils;
using Multiplayer.Inventory;
using Multiplayer.UI;
using UI;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Characters.PlayerController.Scripts.Input
{
    public class PlayerInputScript : MonoBehaviour, PlayerLocomotionScript.IPlayerActions
    {
        
        private PlayerLocomotionScript _playerLocomotionScript;
        private readonly bool _toggleSprint = false;
        private LoggerSO _logger;
        
        #region Exposed Variables
        public Vector2 MoveInput { get; private set; }
        public Vector2 LookInput { get; private set; }
        
        public int ItemSlot { get; private set; }
        public bool JumpPressed { get; private set; }
        public bool CrouchPressed { get; private set; }
        public bool RunningPressed { get; private set; }
        public bool PropelPressed { get; private set; }

        
        public bool LeftClickPressed { get; private set; }
        public bool RightClickPressed { get; private set; }

        public bool InventoryInteraction {  get; private set; }
        public int InventoryIndex { get; private set; }

        #endregion
        
        #region Awake logic
        private void Awake()
        {
            _logger = LoggerSO.Instance;
            _playerLocomotionScript = new PlayerLocomotionScript();
            _playerLocomotionScript.Enable();

            _playerLocomotionScript.Player.Enable();
            _playerLocomotionScript.Player.SetCallbacks(this);
        }
        
        #endregion
        
        #region Enable logic 
        private void OnEnable() {
            _playerLocomotionScript.Player.Enable();
            _playerLocomotionScript.Player.SetCallbacks(this);
        }

        #endregion

        #region Late Update

        void LateUpdate()
        {
            JumpPressed = false;
            InventoryInteraction = false;
        }

        #endregion

        #region Kill logic
        
        private void OnDisable()
        {
            _playerLocomotionScript.Disable();
            _playerLocomotionScript.Player.RemoveCallbacks(this);
        }
        
        private void OnDestroy()
        {
            _playerLocomotionScript.Dispose(); 
        }
        
        #endregion
        
        #region IPlayerActions
        
        public void OnMove(InputAction.CallbackContext context)
        {
            MoveInput = context.ReadValue<Vector2>();
        }
        
        public void OnJump(InputAction.CallbackContext context)
        {
            if (context.performed)
            {
                _logger.LogMinor("Jump press");
                JumpPressed = true;
            }
        }

        public void OnLook(InputAction.CallbackContext context)
        {
            LookInput = context.ReadValue<Vector2>();
        }

        public void OnGrabLeft(InputAction.CallbackContext context)
        {
            if (context.started) {
                LeftClickPressed = true;
            } 
            else if (context.canceled) {
                LeftClickPressed = false;
            } 
        }
        
        public void OnGrabRight(InputAction.CallbackContext context) {
            if (context.started) {
                RightClickPressed = true;
            } 
            else if (context.canceled) {
                RightClickPressed = false;
            } 
        }
        
        public void OnInteract(InputAction.CallbackContext context) {
            if (!context.performed) return;

            var inv = GetComponent<PlayerInventoryPhoton>();
            if (inv == null) return;

            var ui = FindFirstObjectByType<PlayerUIManager>();
            if (ui == null) return;

            // Si mochila est� equipada en slot 4 y seleccionada
            if (inv.backpackObj != null && inv.slots[3] != null && inv.activeSlot == 3)
            {
                //ui.ToggleBackpackInventory(inv.backpackObj.GetComponent<BackpackData>(), inv);
                return;
            }

            // Buscar mochila en el suelo cerca
            Collider[] hits = Physics.OverlapSphere(transform.position, 2f, inv.itemLayer);
            foreach (var hit in hits)
            {
                var netItem = hit.GetComponentInParent<NetworkedItem>();
                if (netItem != null && netItem.itemData != null && netItem.itemData.itemType == ItemType.Backpack)
                {
                    var bd = netItem.GetComponent<BackpackData>();
                    if (bd != null)
                    {
                        //ui.ToggleBackpackInventory(bd, inv);
                    }
                    break;
                }
            }
        }


        public void OnCrouch(InputAction.CallbackContext context)
        {
            if (context.performed) {
                CrouchPressed = true;
                _logger.LogMinor("Crouch press");
            } else if (context.canceled) {
                CrouchPressed = false;
            }
        }

        public void OnPrevious(InputAction.CallbackContext context)
        {
            if (context.performed)
            {
                ItemSlot = Mathf.Max(0, ItemSlot - 1);
                //Debug.Log("Slot anterior: " + ItemSlot);
            }
        }

        public void OnNext(InputAction.CallbackContext context)
        {
            if (context.performed)
            {
                ItemSlot++;
                //Debug.Log("Slot siguiente: " + ItemSlot);
            }
        }

        public void OnSprint(InputAction.CallbackContext context) {
            if (context.performed) {
                RunningPressed = true;
                _logger.LogMinor("Sprint press!");
            } else if (context.canceled) {
                RunningPressed = false;
            }
        }

        public void OnInventory(InputAction.CallbackContext context)
        {
            if (!context.performed)
                return;

            // Detecta cuál número se presionó
            if (Keyboard.current.digit1Key.wasPressedThisFrame) InventoryIndex = 0;
            else if (Keyboard.current.digit2Key.wasPressedThisFrame) InventoryIndex = 1;
            else if (Keyboard.current.digit3Key.wasPressedThisFrame) InventoryIndex = 2;
            else if (Keyboard.current.digit4Key.wasPressedThisFrame) InventoryIndex = 3;
            else InventoryIndex = -1;

            if (InventoryIndex != -1)
                InventoryInteraction = true;
        }

        // Método llamado por el InventoryController después de procesar el input
        public void ConsumeInventoryInput()
        {
            InventoryInteraction = false;
            InventoryIndex = -1;
        }

        public void OnDrop(InputAction.CallbackContext context)
        {
            if(!context.performed) return;

            var inv = GetComponent<PlayerInventoryPhoton>();
            if (inv != null)
            {
                 inv.DropCurrent(inv.activeSlot);
            
                 var ui = FindFirstObjectByType<PlayerUIManager>();
                 if (ui != null) ui.UpdateHotbarUI();
            }
        }

        public void OnPropel(InputAction.CallbackContext context) {
            if (context.performed) {
                PropelPressed = true;
            }
            else if (context.canceled) {
                PropelPressed = false;
            }
        }

        #endregion
    }
}
