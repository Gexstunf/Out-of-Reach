using System;
using Characters.PlayerController.Scripts.Input;
using Characters.StateMachine.PlayerStateMachine;
using UnityEngine;

namespace Characters.PlayerController.Scripts {
    public class CharAnimControllerScript : MonoBehaviour
    {
        private static readonly int Move = Animator.StringToHash("Input");
        private static readonly int Crouch = Animator.StringToHash("Crouch");
        private static readonly int Jumping = Animator.StringToHash("Jumping");
        private static readonly int Falling = Animator.StringToHash("Falling");

        [SerializeField] private Animator animator;
        [SerializeField] private Rigidbody rb;
        
        [Header("References")]
        [SerializeField] private PlayerInputScript playerInput;
        [SerializeField] private PlayerStateMachineScript playerStateMachineScript;

        public void Update() {
            animator.SetFloat(Move, playerInput.MoveInput.magnitude);
            animator.SetBool(Crouch, playerInput.CrouchPressed);
            animator.SetBool(Jumping, playerStateMachineScript.IsJumping);
            animator.SetBool(Falling, playerStateMachineScript.IsFalling);
        }
    }
}
