using Characters.ActiveRagdollSystem;
using UnityEngine;
using UnityEngine.Serialization;

namespace Characters.Enemies.Scripts.Bat {
    public class BatActiveRagdollScript : MonoBehaviour
    {
    
        [Header("References")] 
        [SerializeField] private ActiveRagdollCoreScript _arCoreScript;
    
        [Header("Revival settings")] 
        [SerializeField] private float _smoothLockDuration = 6f;
        [SerializeField] private float _lockSpring = 60f;
        [SerializeField] private float _lockDamper = 60f;
        [SerializeField] private float _initialClearance = 2f;

    
        [Header("Visualize")]
        public bool alive = true;
    
        private bool _previousAlive = true;
        private RevivalParams _revivalParams;
        private DeathParams _deathParams;


        private void Start() {
            _arCoreScript = gameObject.GetComponent<ActiveRagdollCoreScript>();
        
            _deathParams = new DeathParams {
                AllowLimitedMovement = false
            };
        }

        void FixedUpdate() {
            if (_previousAlive != alive) {
                ActiveRagdollCoreScript.StabilizerMode mode = alive ? ActiveRagdollCoreScript.StabilizerMode.Reviving : ActiveRagdollCoreScript.StabilizerMode.Dead;

                if (alive) {
                    _revivalParams = new RevivalParams {
                        UseClearance = false,
                        Damper = _lockDamper,
                        Duration = _smoothLockDuration,
                        AngularXEnd = ConfigurableJointMotion.Free,
                        AngularYEnd = ConfigurableJointMotion.Free,
                        AngularZEnd = ConfigurableJointMotion.Free,
                        YMotionStart = ConfigurableJointMotion.Locked,
                    };
                
                    _arCoreScript.SetStabilizerMode(mode, _revivalParams);
                }
                else {
                    _arCoreScript.SetStabilizerMode(mode, _deathParams);
                }
                _previousAlive = alive;
            } 
        }
    }
}
