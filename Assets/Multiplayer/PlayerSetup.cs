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
        private PlayerControllerScript _controller;
        private RotatorScript _rotator;

        void Awake()
        {
            _playerCamera = GetComponentInChildren<Camera>(true);
            _audioListener = GetComponentInChildren<AudioListener>(true);
            _controller = GetComponent<PlayerControllerScript>();
            _rotator = GetComponent<RotatorScript>();
        }

        void Start()
        {
            bool isLocal = photonView.IsMine;

            if (_playerCamera != null) _playerCamera.enabled = isLocal;
            if (_audioListener != null) _audioListener.enabled = isLocal;
            if (_controller != null) _controller.enabled = isLocal;
            if (_rotator != null) _rotator.enabled = isLocal;

            if (!isLocal)
            {
                Rigidbody rb = GetComponent<Rigidbody>();
                if (rb != null) rb.isKinematic = true;
            }
        }
    }
}
