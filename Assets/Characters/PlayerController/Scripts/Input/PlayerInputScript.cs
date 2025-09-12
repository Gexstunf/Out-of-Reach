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

        #region Disable logic
        private void OnDisable()
        {
            _playerLocomotionScript.Player.Disable();
            _playerLocomotionScript.Player.RemoveCallbacks(this);
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
            throw new System.NotImplementedException();
        }

        public void OnCrouch(InputAction.CallbackContext context)
        {
            throw new System.NotImplementedException();
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

            var key = context.control;

            if (key == Keyboard.current.digit1Key) ItemSlot = 0;
            else if (key == Keyboard.current.digit2Key) ItemSlot = 1;
            else if (key == Keyboard.current.digit3Key) ItemSlot = 2;
            else if (key == Keyboard.current.digit4Key) ItemSlot = 3;

            Debug.Log("Cambio a slot " + ItemSlot);

            // Equipar directamente
            var inv = GetComponent<PlayerInventoryPhoton>();
            if (inv != null)
            {
                inv.EquipFromSlot(ItemSlot);
            }
        }

        public void OnDrop(InputAction.CallbackContext context)
        {
            if(!context.performed) return;

            var inv = GetComponent<PlayerInventoryPhoton>();
            if (inv != null)
            {
                inv.DropCurrent(inv.activeSlot);

                var ui = FindFirstObjectByType<PlayerUIManager>();
                if (ui != null) ui.UpdateInventoryUI();
            }
        }

        #endregion
    }
}
