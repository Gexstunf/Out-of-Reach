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
        
        public enum EPlantStates
        {
            SweepAttack,
            MediumAttack,
            HeavyAttack,
            Alive,
            Dead,
            Idle,
        }
        
        protected override void SetAnimatorBool(EPlantStates plantState, bool value) {
            plantAnimator.SetBool(plantState.ToString(), value);
        }

        protected override void SetAnimatorTrigger(EPlantStates plantState) {
            plantAnimator.SetTrigger(plantState.ToString());
        }

        protected override void SetAnimatorFloat(EPlantStates plantState, float value) {
            plantAnimator.SetFloat(plantState.ToString(), value);
        }
    }
}
