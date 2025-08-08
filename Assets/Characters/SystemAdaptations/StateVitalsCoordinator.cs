using System;
using Characters.LifeSupportSystem.PlayerLifeSupport;
using Characters.PlayerController.Scripts.StateMachine.PlayerStateMachine;
using Characters.SystemAdaptations.Utils;
using UnityEngine;
using UnityEngine.Assertions;
using Assert = NUnit.Framework.Assert;

namespace Characters.SystemAdaptations {
    [RequireComponent(typeof(PlayerStateMachineScript))]
    [RequireComponent(typeof(PlayerLifeSupportScript))]
    public class StateVitalsCoordinator : MonoBehaviour
    {
        public PlayerStateMachineScript playerStateMachineScript;
        public PlayerLifeSupportScript playerLifeSupportScript;

        private MovementStatesStructScript _movementStruct;
        private VitalsStructScript _vitalsStruct;
        
        
        public event Action<bool> OnTiredChanged;
        public event Action<bool> OnHeavyChanged;
        public event Action<bool> OnStarvedChanged;
        public event Action<bool> OnUnconsciousChanged;

        private PlayerLifeSupportContextScript _context;
        
        public void Start() {
            playerStateMachineScript = GetComponent<PlayerStateMachineScript>();
            playerLifeSupportScript = GetComponent<PlayerLifeSupportScript>();
            
            _context = playerLifeSupportScript.Context;
            
            _movementStruct = new MovementStatesStructScript(
                    isRunning: false,
                    isJumping: false,
                    isWalking: false,
                    isClimbing: false,
                    isIdle: false,
                    isMoving: false,
                    isFalling: false
                );
            
            _vitalsStruct = new VitalsStructScript(
                    isStarved: false,
                    isUnconscious: false,
                    isHeavy: false,
                    isTired: false
                );

            ValidateReferences();
        }

        public void Update() {
            HandleStateMachine();
            playerLifeSupportScript.Context.SetMovementStates(_movementStruct);
            CheckForVitalsEvents();
            HandleVitals();
        }

        private void HandleStateMachine() {
            _movementStruct.IsRunning = playerStateMachineScript.IsRunning;
            _movementStruct.IsJumping = playerStateMachineScript.IsJumping;
            _movementStruct.IsWalking = playerStateMachineScript.IsWalking;
            _movementStruct.IsIdle = playerStateMachineScript.IsIdle;
            _movementStruct.IsMoving = playerStateMachineScript.IsMoving;
            _movementStruct.IsFalling = playerStateMachineScript.IsFalling;
        }
        
        private void HandleVitals() {
            // we first check for any events on vitals before updating the struct
            
            _vitalsStruct.IsUnconscious = _context.IsUnconscious;
            _vitalsStruct.IsTired = _context.IsTired;
            // _vitalsStruct.IsStarved = playerLifeSupport.IsStarved;
            // __vitalsStruct.IsHeavy = playerLifeSupport.IsHeavy;
        }

        private void CheckForVitalsEvents() {
            // any discrepancies here, mean that the last frame vars != to the current frame vars (meaning the vital changed)
            if (_vitalsStruct.IsTired != _context.IsTired) {
                OnTiredChanged?.Invoke(_context.IsTired);
                Debug.Log("TIRED changed to: " + _context.IsTired);
            }
            
            if (_vitalsStruct.IsHeavy != _context.IsTired) {
                OnHeavyChanged?.Invoke(_context.IsTired);
            }
            
            if (_vitalsStruct.IsUnconscious != _context.IsUnconscious) {
                OnUnconsciousChanged?.Invoke(_context.IsUnconscious);
                Debug.Log("UNCONSCIOUS changed to: " + _context.IsTired);
            }
            
            if (_vitalsStruct.IsStarved != _context.IsTired) {
                OnStarvedChanged?.Invoke(_context.IsTired);
            }
        }

        private void ValidateReferences() {
            Assert.IsNotNull(playerStateMachineScript, "playerStateMachineScript Is null");
            Assert.IsNotNull(playerLifeSupportScript, "playerLifeSupportScript Is null");
            Assert.IsNotNull(_vitalsStruct, "vitalsStruct Is null");
            Assert.IsNotNull(_movementStruct, "movementStruct Is null");

            if (playerStateMachineScript == null)
                Debug.LogError("playerStateMachineScript is null");

            if (playerLifeSupportScript == null)
                Debug.LogError("playerLifeSupportScript is null");

            if (_context == null) {
                Debug.LogError("playerLifeSupportScript.Context is null");
                Debug.Log(playerLifeSupportScript);
                Debug.Log(playerLifeSupportScript.Context);
            }
        }
    }
}
