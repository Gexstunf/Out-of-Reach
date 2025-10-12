using UnityEngine;

namespace Characters.LifeSupportSystem {
    [CreateAssetMenu(fileName = "Attack", menuName = "Damage/Attacks")]
    public class AttackDamageSO : ScriptableObject
    {
        public enum DamageStrength {
            Heavy,
            Medium,
            Small
        }
        
        public DamageStrength damageStrength;
        public float damage;
        
        // minimum speed for damage to apply
        public float minimumSpeed;
    }
}
