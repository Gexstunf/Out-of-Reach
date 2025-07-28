using System;
using Characters.PlayerController.Scripts.StateMachine.PlayerStateMachine.ConcreteStates;
using UnityEngine;

namespace Characters.PlayerController.Scripts.StateMachine.PlayerStateMachine
{
    public class PlayerStateMachineScript : StateManagerScript<PlayerStateMachineScript.EPlayerStates>
    {

        [Header("Player State References")] 
        [SerializeField] private Rigidbody _rb;
        [SerializeField] private CapsuleCollider _collider;
        
        
        private PlayerStateContextScript _context;
        
        public enum EPlayerStates
        {
            Falling,
            Jumping,
            Running,
            Walking,
            Idle,
        }
        
        private void Awake()
        {
            _context = new PlayerStateContextScript(_rb, _collider);
            InitializeStates();
        }

        private void InitializeStates()
        {
            States.Add(EPlayerStates.Falling, new FallingStateScript(_context, EPlayerStates.Falling));
            States.Add(EPlayerStates.Jumping, new JumpingStateScript(_context, EPlayerStates.Jumping));
            States.Add(EPlayerStates.Walking, new WalkingStateScript(_context, EPlayerStates.Walking));
            States.Add(EPlayerStates.Idle, new IdleStateScript(_context, EPlayerStates.Idle));
            
            CurrentState = States[EPlayerStates.Idle];
        }
    }
}
