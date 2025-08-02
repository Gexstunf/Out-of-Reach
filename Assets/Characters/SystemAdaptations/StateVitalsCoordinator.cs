using System;
using Characters.LifeSupportSystem.PlayerLifeSupport;
using Characters.PlayerController.Scripts.StateMachine.PlayerStateMachine;
using Characters.SystemAdaptations.Utils;
using UnityEngine;

namespace Characters.SystemAdaptations {
    public class StateVitalsCoordinator : MonoBehaviour
    {
        public PlayerStateMachineScript playerStateMachine;
        public PlayerLifeSupportScript playerLifeSupport;

        private MovementStatesStructScript _movementStruct;
        private VitalsStructScript _vitalsStruct;

        public void Awake() {
            playerStateMachine = GetComponent<PlayerStateMachineScript>();
            playerLifeSupport = GetComponent<PlayerLifeSupportScript>();
            
            _movementStruct = new MovementStatesStructScript(
                    isRunning: false,
                    isJumping: false,
                    isWalking: false,
                    isClimbing: false,
                    isIdle: false
                );
            
            _vitalsStruct = new VitalsStructScript(
                    isStarved: false,
                    isUnconscious: false,
                    isHeavy: false,
                    isTired: false
                );
        }

        public void Update() {
            HandleStateMachine();
            playerLifeSupport.Context.SetMovementStates(_movementStruct);
            HandleVitals();
            playerStateMachine.Context.SetVitalStates(_vitalsStruct);
        }

        private void HandleStateMachine() {
            _movementStruct.IsRunning = playerStateMachine.IsRunning;
            _movementStruct.IsJumping = playerStateMachine.IsJumping;
            _movementStruct.IsWalking = playerStateMachine.IsWalking;
            _movementStruct.IsIdle = playerStateMachine.IsIdle;
        }
        
        private void HandleVitals() {
            // _vitalsStruct.IsUnconscious = playerLifeSupport.IsUnconscious;
            // _vitalsStruct.IsTired = playerLifeSupport.IsTired;
            // _vitalsStruct.IsStarved = playerLifeSupport.IsStarved;
            // __vitalsStruct.IsHeavy = playerLifeSupport.IsHeavy;
        }
        
    }
}
