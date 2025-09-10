using System;
using Characters.PlayerController.Scripts.Input;
using UnityEngine;

namespace Characters.PlayerController.Scripts {
    public class CharAnimControllerScript : MonoBehaviour
    {
        private static readonly int Move = Animator.StringToHash("Input");
        [SerializeField] private Animator animator;
        [SerializeField] private Rigidbody rigidbody;
        [SerializeField] private PlayerInputScript playerInput;

        public void Update() {
            animator.SetFloat(Move, playerInput.MoveInput.magnitude);
        }
    }
}
