using Characters.ActiveRagdollSystem;
using UnityEngine;
using UnityEngine.Serialization;

namespace Characters.Enemies.Scripts.Plant {
    public class PlantActiveRagdollScript : MonoBehaviour {
    
        [Header("References")] 
        [SerializeField] private ActiveRagdollCoreScript _arCoreScript;
    
        [Header("Revival settings")] 
        [SerializeField] private float _smoothLockDuration = 6f;
        [SerializeField] private float _lockSpring = 60f;
        [SerializeField] private float _lockDamper = 60f;
        [SerializeField] private float _initialClearance = 2f;

    
        [Header("Visualize & debug")]
        [Tooltip("This HAS to be true for these options to work.")] public bool debug = true;
        public bool debugAlive = true;
    
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
            if (debug) {
                HandlePlantLifeState(debugAlive);
            }
            else {
                HandlePlantLifeState(_arCoreScript.Alive);
            }
        }

        void HandlePlantLifeState(bool currentAlive) {
            if (_previousAlive != currentAlive) {
                ActiveRagdollCoreScript.StabilizerMode mode = debugAlive ? ActiveRagdollCoreScript.StabilizerMode.Reviving : ActiveRagdollCoreScript.StabilizerMode.Dead;

                if (debugAlive) {
                    _revivalParams = new RevivalParams {
                        UseClearance = false,
                        Damper = _lockDamper,
                        Duration = _smoothLockDuration,
                        AngularXEnd = ConfigurableJointMotion.Locked,
                        AngularYEnd = ConfigurableJointMotion.Locked,
                        AngularZEnd = ConfigurableJointMotion.Locked,
                        YMotionStart = ConfigurableJointMotion.Locked,
                    };
                
                    _arCoreScript.SetStabilizerMode(mode, _revivalParams);
                }
                else {
                    _arCoreScript.SetStabilizerMode(mode, _deathParams);
                }
                _previousAlive = currentAlive;
            } 
        }
    }
}
