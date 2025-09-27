using UnityEngine;
using Photon.Pun;

public class GameManager : MonoBehaviour
{
    [Header("Prefab del jugador")]
    public GameObject playerPrefab;

    [Header("Puntos de spawn")]
    public Transform[] spawnPoints;

    private void Start()
    {
        if (!PhotonNetwork.InRoom)
        {
            Debug.LogError("No estás en ninguna sala de juego, no se puede spawnear jugador.");
            return;
        }

        // Suscribirse al evento de spawn
        GameNetworkController.Instance.OnPlayerSpawned += HandlePlayerSpawned;

        // Spawnear el jugador local
        SpawnLocalPlayer();
    }

    void SpawnLocalPlayer()
    {
        int playerIndex = PhotonNetwork.LocalPlayer.ActorNumber - 1;
        Transform spawnPoint = spawnPoints[playerIndex % spawnPoints.Length];

        GameNetworkController.Instance.SpawnLocalPlayer(playerPrefab, spawnPoint);
    }

    void HandlePlayerSpawned(Photon.Realtime.Player player)
    {
        Debug.Log($"Jugador listo en escena: {player.NickName}");
        // Aquí podrías actualizar UI, inventario, cámaras, etc.
    }
}
