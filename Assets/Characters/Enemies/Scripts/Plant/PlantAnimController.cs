using UnityEngine;

namespace Characters.Enemies.Scripts.Plant {
    public class PlantAnimController : MonoBehaviour
    {
        // Start is called once before the first execution of Update after the MonoBehaviour is created
        [Header("References")]
        [SerializeField] private Animator plantAnimator;

        [Header("Visualize")]
        public bool attack;
        
        private static readonly int Attack = Animator.StringToHash("Attack");

        // Update is called once per frame
        void Update()
        {
            if (attack) {
                attack = false;
                plantAnimator.SetTrigger(Attack);
            }
            else {
                plantAnimator.ResetTrigger(Attack);
            }
        }
    }
}
