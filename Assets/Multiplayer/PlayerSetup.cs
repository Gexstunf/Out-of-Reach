using Characters.LifeSupportSystem.PlayerLifeSupport;
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

        public bool IsLocalPlayer => photonView.IsMine;

        void Awake()
        {
            _playerCamera = GetComponentInChildren<Camera>(true);
            _audioListener = GetComponentInChildren<AudioListener>(true);
            _controller = GetComponent<PlayerControllerScript>();
            _rotator = GetComponent<RotatorScript>();
            _lifeSupport = GetComponent<PlayerLifeSupportScript>();
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
        
    }
}
