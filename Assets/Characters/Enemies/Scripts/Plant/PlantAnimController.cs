using Characters.Animation;
using UnityEngine;

namespace Characters.Enemies.Scripts.Plant {
    public class PlantAnimController : AnimationControllerManagerScript<PlantAnimController.EPlantStates>
    {
        // Start is called once before the first execution of Update after the MonoBehaviour is created
        [Header("References")]
        [SerializeField] private Animator plantAnimator;
        
        [Header("Visualize")]
        public bool attack;
        
        private static readonly int MediumAttack = Animator.StringToHash("Attack");
        
        public enum EPlantStates
        {
            LightAttack,
            MediumAttack,
            HeavyAttack,
            Alive,
            Dead,
            Idle,
        }
        
        protected override void SetAnimatorBool(EPlantStates plantState, bool value) {
            throw new System.NotImplementedException();
        }

        protected override void SetAnimatorTrigger(EPlantStates plantState) {
            switch (plantState) {
                case EPlantStates.LightAttack:
                    break;
                case EPlantStates.MediumAttack:
                    plantAnimator.SetTrigger(MediumAttack);
                    break; 
                case EPlantStates.HeavyAttack:
                    break;
            }
        }

        protected override void SetAnimatorFloat(EPlantStates plantState, float value) {
            throw new System.NotImplementedException();
        }
    }
}
