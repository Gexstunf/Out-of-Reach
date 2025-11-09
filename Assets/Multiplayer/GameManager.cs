using Photon.Pun;
using UnityEngine;

public class GameManager : MonoBehaviour
{

    private void Start()
    {
        // El GameManager ya no spawnea al jugador directamente.
        // Se asegura de que exista GameNetworkController.
        if (GameNetworkController.Instance == null)
        {
            Debug.LogError("No existe GameNetworkController en la escena.");
            return;
        }

        // Aquí podrías iniciar la lógica del juego,
        // por ejemplo: temporizador, enemigos, objetivos, etc.
        Debug.Log("GameManager iniciado. Esperando jugadores...");
    }
}
