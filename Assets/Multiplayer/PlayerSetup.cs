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
            if (!photonView.IsMine) {
                // Desactivar cosas en jugadores remotos
                if (_playerCamera != null) _playerCamera.enabled = false;
                if (_audioListener != null) _audioListener.enabled = false;
                if (_controller != null) _controller.enabled = false;
                if (_rotator != null) _rotator.enabled = false;
            } else {
                // Inicializar cámara para el jugador local
                if (_cameraController != null && _playerCamera != null)
                {
                    _cameraController.Init(2.0f, 2.0f, 80f);
                }
            }
        }
    }
}
