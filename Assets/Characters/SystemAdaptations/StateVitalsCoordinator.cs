using System;
using System.Diagnostics; // <-- para Debug.Assert
using Characters.LifeSupportSystem.PlayerLifeSupport;
using Characters.PlayerController.Scripts.StateMachine.PlayerStateMachine;
using Characters.StateMachine.PlayerStateMachine;
using Characters.SystemAdaptations.Utils;
using UnityEngine;
using Debug = UnityEngine.Debug;

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

            if (playerLifeSupportScript.Context == null)
            {
                Debug.LogWarning("Context no inicializado para este jugador (no es local)");
                return; // no hacemos nada para jugadores remotos
            }

            _context = playerLifeSupportScript.Context;

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

        private void Update()
        {
            if (playerLifeSupportScript == null) return;

            if (_context == null)
            {
                if (playerLifeSupportScript.Context != null)
                    _context = playerLifeSupportScript.Context;
                else
                    return; // todavía no inicializado, esperar
            }

            HandleStateMachine();
            _context.SetMovementStates(_movementStruct);
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

        private void HandleVitals()
        {
            if (_context == null) return;

            _vitalsStruct.IsUnconscious = _context.IsUnconscious;
            _vitalsStruct.IsTired = _context.IsTired;
            // otros vitals...
        }

        private void CheckForVitalsEvents()
        {
            if (_context == null) return;

            if (_vitalsStruct.IsTired != _context.IsTired)
            {
                OnTiredChanged?.Invoke(_context.IsTired);
                UnityEngine.Debug.Log("TIRED changed to: " + _context.IsTired);
                _vitalsStruct.IsTired = _context.IsTired; // actualizar struct
            }

            if (_vitalsStruct.IsHeavy != _context.IsHeavy)
            {
                OnHeavyChanged?.Invoke(_context.IsHeavy);
                _vitalsStruct.IsHeavy = _context.IsHeavy;
            }

            if (_vitalsStruct.IsUnconscious != _context.IsUnconscious)
            {
                OnUnconsciousChanged?.Invoke(_context.IsUnconscious);
                _vitalsStruct.IsUnconscious = _context.IsUnconscious;
            }

            if (_vitalsStruct.IsStarved != _context.IsStarved)
            {
                OnStarvedChanged?.Invoke(_context.IsStarved);
                _vitalsStruct.IsStarved = _context.IsStarved;
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
