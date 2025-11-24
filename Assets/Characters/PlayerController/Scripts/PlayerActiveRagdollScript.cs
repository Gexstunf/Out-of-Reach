using System;
using Characters.ActiveRagdollSystem;
using Characters.PlayerController.Scripts.Input;
using Characters.StateMachine.PlayerStateMachine;
using Characters.SystemAdaptations;
using UnityEngine;
using UnityEngine.Animations.Rigging;
using Photon.Pun;

namespace Characters.PlayerController.Scripts {
    public class PlayerActiveRagdollScript : MonoBehaviour, IPunObservable
    {
    
        [Header("References")] 
        [SerializeField] private RagdollControllerScript _ragdollController;
        [SerializeField] private PlayerInputScript _playerInputScript;
        [SerializeField] private PlayerControllerScript _playerController;
        [SerializeField] private StateVitalsCoordinator _stateVitalsCoordinator;
        [SerializeField] private PlayerStateMachineScript _playerStateMachine;
        [SerializeField] private ActiveRagdollCoreScript _ar;
        
        [Header("Jump Settings")]
        [SerializeField] private float _stabilizerPitch = 45f;
        [SerializeField] private float _rotationSpeed = 5f;
        [SerializeField] private float _jumpRigBlendSpeed = 4.5f;
        
        [Header("Revival settings")] 
        [SerializeField] private float _smoothLockDuration = 6f;
        [SerializeField] private float _lockSpring = 70f;
        [SerializeField] private float _lockDamper = 20f;
        [SerializeField] private float _initialClearance = 1f;
        
        [Header("Rigs")]
        [SerializeField] private Rig _jumpRig;

        private PhotonView _pv;

        private bool _hasJumped = false;
        private bool _hasCrouched = false;

        private bool netJumping = false;
        private bool netCrouching = false;

        private bool syncedTired = false;

        #region === Unity: Awake / Enable / Disable ===

        private void Awake()
        {
            _pv = GetComponent<PhotonView>();
            _playerInputScript = GetComponent<PlayerInputScript>();
            _playerController = GetComponent<PlayerControllerScript>();
            _ar = GetComponent<ActiveRagdollCoreScript>();
            _playerStateMachine = GetComponent<PlayerStateMachineScript>();
            _ragdollController = GetComponent<RagdollControllerScript>();
        }

        private void OnEnable()
        {
            _stateVitalsCoordinator = GetComponent<StateVitalsCoordinator>();
            _stateVitalsCoordinator.OnTiredChanged += HandleTiredChange;
        }

        private void OnDisable()
        {
            _stateVitalsCoordinator.OnTiredChanged -= HandleTiredChange;
        }

        #endregion


        #region === Update Logic (Jump, Fall, Crouch) ===

        private void FixedUpdate()
        {
            if (!_pv.IsMine) return;

            HandleJump();
            HandleCrouch();
        }

        private void HandleJump()
        {
            if (_playerStateMachine.IsJumping || _playerStateMachine.IsFalling)
            {
                _hasJumped = true;

                _ar.ApplyStabilizerPitch(_stabilizerPitch, _rotationSpeed, true);
                _jumpRig.weight = Mathf.Lerp(_jumpRig.weight, 1f, Time.fixedDeltaTime * _jumpRigBlendSpeed);
            }
            else if (_hasJumped)
            {
                _ar.ApplyStabilizerPitch(0f, _rotationSpeed, false);

                _jumpRig.weight = Mathf.Lerp(_jumpRig.weight, 0f, Time.fixedDeltaTime * _jumpRigBlendSpeed);

                if (_jumpRig.weight < 0.05f)
                    _hasJumped = false;
            }
        }

        private void HandleCrouch()
        {
            if (_playerInputScript.CrouchPressed && _playerStateMachine.IsIdle)
            {
                CrouchParams crouch = new CrouchParams
                {
                    Height = _playerController.crouchHeight
                };

                _hasCrouched = true;
                _ar.SetStabilizerMode(ActiveRagdollCoreScript.StabilizerMode.Crouching, crouch);
            }
            else if (_hasCrouched)
            {
                StandParams stand = new StandParams
                {
                    Duration = 0.3f
                };

                _hasCrouched = false;
                _ar.SetStabilizerMode(ActiveRagdollCoreScript.StabilizerMode.Standing, stand);
            }
        }

        #endregion


        #region === Tired / Revive / Dead Logic ===

        private void HandleTiredChange(bool tired)
        {
            if (_pv.IsMine)
                syncedTired = tired;

            ApplyTiredState(tired);
        }

        private void ApplyTiredState(bool tired)
        {
            _ragdollController.IgnoreInternalCollisions(!tired);

            RevivalParams revive = new RevivalParams
            {
                StartClearance = _initialClearance,
                EndClearance = 0f,
                UseClearance = true,
                Damper = _lockDamper,
                Duration = _smoothLockDuration,
                EndSpring = _lockSpring,
            };

            DeathParams death = new DeathParams
            {
                AllowLimitedMovement = true
            };

            if (tired)
                _ar.SetStabilizerMode(ActiveRagdollCoreScript.StabilizerMode.Dead, death);
            else
                _ar.SetStabilizerMode(ActiveRagdollCoreScript.StabilizerMode.Reviving, revive);
        }

        #endregion


        #region === Photon Sync ===

        public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
        {
            if (stream.IsWriting)
            {
                stream.SendNext(_hasJumped);
                stream.SendNext(_hasCrouched);
                stream.SendNext(syncedTired);
            }
            else
            {
                _hasJumped = (bool)stream.ReceiveNext();
                _hasCrouched = (bool)stream.ReceiveNext();

                bool networkTired = (bool)stream.ReceiveNext();

                if (networkTired != syncedTired)
                {
                    syncedTired = networkTired;
                    ApplyTiredState(syncedTired);
                }
            }
        }

        #endregion
    }
}
