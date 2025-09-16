using Characters.PlayerController.Scripts;
using Characters.Utils;
using Photon.Pun;
using UnityEngine;

namespace Multiplayer {
    public class PlayerSetup : MonoBehaviourPun
    {
        private Camera _playerCamera;
        private AudioListener _audioListener;
        private PlayerControllerScript _controller;
        private CameraControllerScript _cameraController;
        private RotatorScript _rotator;

        void Awake()
        {
            // Buscar componentes
            _playerCamera = GetComponentInChildren<Camera>();
            _audioListener = GetComponentInChildren<AudioListener>();
            _controller = GetComponent<PlayerControllerScript>();
            _rotator = GetComponent<RotatorScript>();
        
            _cameraController = new CameraControllerScript();
        }

        void Start()
        {
            if (photonView.IsMine)
            {
                _playerCamera.enabled = true;
                _audioListener.enabled = true;
                _controller.enabled = true;
                _rotator.enabled = true;
            }
            else
            {
                _playerCamera.enabled = false;
                _audioListener.enabled = false;
                _controller.enabled = false;
                _rotator.enabled = false;
            }
        }
    }
}
