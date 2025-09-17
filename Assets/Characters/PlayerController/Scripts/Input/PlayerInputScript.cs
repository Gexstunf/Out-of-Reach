using System;
using UI;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Characters.PlayerController.Scripts.Input
{
    public class PlayerInputScript : MonoBehaviour, PlayerLocomotionScript.IPlayerActions
    {
        private PlayerLocomotionScript _playerLocomotionScript;
        private readonly bool _toggleSprint = false;
        
        #region Exposed Variables
        public Vector2 MoveInput { get; private set; }
        public Vector2 LookInput { get; private set; }
        
        public int ItemSlot { get; private set; }
        public bool JumpPressed { get; private set; }
        public bool CrouchPressed { get; private set; }
        
        public bool RunningPressed { get; private set; }
        
        #endregion
        
        #region Awake logic
        private void Awake()
        {
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
                Debug.Log("Jump press");
                JumpPressed = true;
            }
        }

        public void OnLook(InputAction.CallbackContext context)
        {
            LookInput = context.ReadValue<Vector2>();
        }

        public void OnAttack(InputAction.CallbackContext context)
        {
            throw new System.NotImplementedException();
        }

        public void OnInteract(InputAction.CallbackContext context)
        {
            if (!context.performed) return;

            /*var inv = GetComponent<PlayerInventoryPhoton>();
            if (inv == null) return;

            // Abrir mochila en mano
            if (inv.backpackObj != null && inv.slots[3] != null && inv.activeSlot == 3)
            {
                inv.OpenBackpack();
                return;
            }

            // Buscar mochila tirada cerca
            Collider[] hits = Physics.OverlapSphere(transform.position, 2f, inv.itemLayer);
            foreach (var hit in hits)
            {
                var netItem = hit.GetComponentInParent<NetworkedItem>();
                if (netItem != null && netItem.itemData != null && netItem.itemData.itemType == ItemType.Backpack)
                {
                    inv.OpenBackpackWorld(netItem);
                    break;
                }
            }*/
        }

        public void OnCrouch(InputAction.CallbackContext context)
        {
            if (context.performed) {
                CrouchPressed = true;
                Debug.Log("Crouch press");
            } else if (context.canceled) {
                CrouchPressed = false;
            }
        }

        public void OnPrevious(InputAction.CallbackContext context)
        {
            if (context.performed)
            {
                ItemSlot = Mathf.Max(0, ItemSlot - 1);
                Debug.Log("Slot anterior: " + ItemSlot);
            }
        }

        public void OnNext(InputAction.CallbackContext context)
        {
            if (context.performed)
            {
                ItemSlot++;
                Debug.Log("Slot siguiente: " + ItemSlot);
            }
        }

        public void OnSprint(InputAction.CallbackContext context) {
            if (context.performed) {
                RunningPressed = true;
                Debug.Log("Sprint press!");
            } else if (context.canceled) {
                RunningPressed = false;
            }
        }

        public void OnInventory(InputAction.CallbackContext context) {
            if (!context.performed) return;
            /*var inv = GetComponent<PlayerInventoryPhoton>();
            if (inv == null) return;

            if (Keyboard.current.digit1Key.wasPressedThisFrame)
            {
                if (inv.tempHeldObj != null) inv.PlaceTempHeldInSlot(0);
                else inv.EquipFromSlot(0);
            }
            else if (Keyboard.current.digit2Key.wasPressedThisFrame)
            {
                if (inv.tempHeldObj != null) inv.PlaceTempHeldInSlot(1);
                else inv.EquipFromSlot(1);
            }
            else if (Keyboard.current.digit3Key.wasPressedThisFrame)
            {
                if (inv.tempHeldObj != null) inv.PlaceTempHeldInSlot(2);
                else inv.EquipFromSlot(2);
            }
            else if (Keyboard.current.digit4Key.wasPressedThisFrame)
            {
                if (inv.tempHeldObj != null) inv.PlaceTempHeldInSlot(3);
                else inv.EquipFromSlot(3);
            }*/
        }

        public void OnDrop(InputAction.CallbackContext context)
        {
            if(!context.performed) return;

            // var inv = GetComponent<PlayerInventoryPhoton>();
            // if (inv != null)
            // {
            //     inv.DropCurrent(inv.activeSlot);
            //
            //     var ui = FindFirstObjectByType<PlayerUIManager>();
            //     if (ui != null) ui.UpdateInventoryUI();
            // }
        }

        #endregion
    }
}
