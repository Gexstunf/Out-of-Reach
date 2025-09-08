using System;
using UnityEngine;

namespace Characters.PlayerController.Scripts {
    public class CharAnimControllerScript : MonoBehaviour
    {
        private static readonly int Speed = Animator.StringToHash("Speed");
        [SerializeField] private Animator animator;
        [SerializeField] private Rigidbody rigidbody;

        public void Update() {
            animator.SetFloat(Speed, rigidbody.linearVelocity.magnitude);
        }
    }
}
