using System;
using System.Diagnostics; // <-- para Debug.Assert
using Characters.LifeSupportSystem.PlayerLifeSupport;
using Characters.PlayerController.Scripts.StateMachine.PlayerStateMachine;
using Characters.StateMachine.PlayerStateMachine;
using Characters.SystemAdaptations.Utils;
using UnityEngine;

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
            _vitalsStruct.IsUnconscious = _context.IsUnconscious;
            _vitalsStruct.IsTired = _context.IsTired;
            // _vitalsStruct.IsStarved = playerLifeSupport.IsStarved;
            // _vitalsStruct.IsHeavy = playerLifeSupport.IsHeavy;
        }

        private void CheckForVitalsEvents() {
            if (_vitalsStruct.IsTired != _context.IsTired) {
                OnTiredChanged?.Invoke(_context.IsTired);
                UnityEngine.Debug.Log("TIRED changed to: " + _context.IsTired);
            }
            
            if (_vitalsStruct.IsHeavy != _context.IsTired) {
                OnHeavyChanged?.Invoke(_context.IsTired);
            }
            
            if (_vitalsStruct.IsUnconscious != _context.IsUnconscious) {
                OnUnconsciousChanged?.Invoke(_context.IsUnconscious);
                UnityEngine.Debug.Log("UNCONSCIOUS changed to: " + _context.IsTired);
            }
            
            if (_vitalsStruct.IsStarved != _context.IsTired) {
                OnStarvedChanged?.Invoke(_context.IsTired);
            }
        }

        private void ValidateReferences()
        {
            UnityEngine.Debug.Assert(playerStateMachineScript != null, "playerStateMachineScript is null");
            UnityEngine.Debug.Assert(playerLifeSupportScript != null, "playerLifeSupportScript is null");

            if (playerStateMachineScript == null)
                UnityEngine.Debug.LogError("playerStateMachineScript is null");

            if (playerLifeSupportScript == null)
                UnityEngine.Debug.LogError("playerLifeSupportScript is null");

            if (_context == null)
            {
                UnityEngine.Debug.LogError("playerLifeSupportScript.Context is null");
                UnityEngine.Debug.Log(playerLifeSupportScript);
                UnityEngine.Debug.Log(playerLifeSupportScript.Context);
            }
        }
    }
}
