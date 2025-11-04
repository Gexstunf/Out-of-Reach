using System;
using Characters.LifeSupportSystem;
using UnityEngine;

namespace Characters.Enemies.Scripts {
    [Serializable] 
    public class AttackDefinitionScript {
        [Tooltip("Name of the animation state enum value. Must match the enum name used by the enemy's AnimationControllerManager.")]
        public string animationStateName;

        public AttackDamageSO damageSO;
        [Min(0f)] public float range = 2f;
        [Min(0f)] public float cooldown = 1.5f;

        [Tooltip("Optional weight for selection when several attacks are valid. 0 = never selected if other >0.")]
        [Min(0f)] public float selectionWeight = 1f;
    }
}

