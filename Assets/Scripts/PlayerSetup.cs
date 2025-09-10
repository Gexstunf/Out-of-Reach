using Photon.Pun;
using UnityEngine;
using Characters.PlayerController.Scripts;

public class PlayerSetup : MonoBehaviourPun
{
    public PlayerControllerScript _controller;
    public GameObject _camera; // Cámara con AudioListener

    void Start()
    {
        if (photonView.IsMine)
        {
            // Este es mi jugador → activo controles y cámara
            if (_controller != null) _controller.enabled = true;
            if (_camera != null) _camera.SetActive(true);
        }
        else
        {
            // Jugador remoto → desactivo controles y cámara
            if (_controller != null) _controller.enabled = false;
            if (_camera != null) _camera.SetActive(false);
        }
    }
}
