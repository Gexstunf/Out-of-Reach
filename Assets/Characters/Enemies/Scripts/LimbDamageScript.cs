using System;
using UnityEngine;
using UnityEngine.Serialization;

namespace Characters.Enemies.Scripts {
    public class LimbDamageScript : MonoBehaviour, ILimbDamageScript {
        public AttackScript HostAttackScript { get; private set; }
        
        public void SetHostAttackScript(AttackScript host)
        {
            HostAttackScript = host;
        }
        
        private void OnCollisionEnter(Collision other)
        {
            // optional / maybe some logic
        }
    }
}
