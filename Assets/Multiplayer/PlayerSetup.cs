using Characters.LifeSupportSystem.PlayerLifeSupport;
using Characters.PlayerController.Scripts;
using Characters.Utils;
using Photon.Pun;
using UnityEngine;

namespace Multiplayer
{
    public class PlayerSetup : MonoBehaviourPun
    {
        private Camera _playerCamera;
        private AudioListener _audioListener;
        private PhotonAnimatorView _animView;

        [Header("Player Components")]
        private PlayerControllerScript _controller;
        private RotatorScript _rotator;
        private PlayerLifeSupportScript _lifeSupport;
        private ActiveRagdollControllerScript _ragdoll;

        public bool IsLocalPlayer => photonView.IsMine;

        private bool _wasCrouching;

        void Awake()
        {
            _playerCamera = GetComponentInChildren<Camera>(true);
            _audioListener = GetComponentInChildren<AudioListener>(true);
            _controller = GetComponent<PlayerControllerScript>();
            _rotator = GetComponent<RotatorScript>();
            _lifeSupport = GetComponent<PlayerLifeSupportScript>();
            _ragdoll = GetComponent<ActiveRagdollControllerScript>();
            //_animView = GetComponentInChildren<PhotonAnimatorView>(true);
        }


        void Start()
        {
            bool isLocal = IsLocalPlayer;

            if (_playerCamera != null) _playerCamera.enabled = isLocal;
            if (_audioListener != null) _audioListener.enabled = isLocal;
            if (_rotator != null) _rotator.enabled = isLocal;

            if (_controller != null)
            {
                _controller.SetAsLocalPlayer(isLocal);
                _controller.enabled = true;
            }

            if (_lifeSupport != null)
                _lifeSupport.Initialize(isLocal);
            /*
            if (_animView != null)
                _animView.enabled = !isLocal;
            */
            if (!isLocal)
            {
                var rb = GetComponent<Rigidbody>();
                if (rb != null) rb.isKinematic = true;
            }
        }
        void Update()
        {
            if (!IsLocalPlayer) return;

            // Use your own input system here — this is an example:
            bool crouchPressed = Input.GetKey(KeyCode.LeftControl);

            if (crouchPressed && !_wasCrouching)
            {
                photonView.RPC(nameof(RPC_SetCrouch), RpcTarget.All, true);
                _wasCrouching = true;
            }
            else if (!crouchPressed && _wasCrouching)
            {
                photonView.RPC(nameof(RPC_SetCrouch), RpcTarget.All, false);
                _wasCrouching = false;
            }
        }

        [PunRPC]
        private void RPC_SetCrouch(bool isCrouching)
        {
            if (_ragdoll != null)
                _ragdoll.SetCrouch(isCrouching);
        }
    }
}
