using Characters.PlayerController.Scripts;
using Characters.Utils;
using Photon.Pun;
using UnityEngine;

public class PlayerSetup : MonoBehaviourPun
{
    private Camera playerCamera;
    private AudioListener audioListener;
    private PlayerControllerScript controller;
    private CameraControllerScript cameraController;
    private RotatorScript rotator;

    void Awake()
    {
        // Buscar componentes
        playerCamera = GetComponentInChildren<Camera>();
        audioListener = GetComponentInChildren<AudioListener>();
        controller = GetComponent<PlayerControllerScript>();
        cameraController = GetComponentInChildren<CameraControllerScript>();
        rotator = GetComponent<RotatorScript>();
    }

    void Start()
    {
        if (!photonView.IsMine)
        {
            // Desactivar cosas en jugadores remotos
            if (playerCamera != null) playerCamera.enabled = false;
            if (audioListener != null) audioListener.enabled = false;
            if (controller != null) controller.enabled = false;
            if (cameraController != null) cameraController.enabled = false;
            if (rotator != null) rotator.enabled = false;
        }
        else
        {
            // Inicializar cámara para el jugador local
            if (cameraController != null && playerCamera != null)
            {
                cameraController.Init(2.0f, 2.0f, 80f);
            }
        }
    }
}
