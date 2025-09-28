using Photon.Pun;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [Header("Prefab del jugador")]
    public GameObject playerPrefab;

    [Header("Puntos de spawn")]
    public Transform[] spawnPoints;

    void Start()
    {
        // Verificamos que estemos en sala
        if (!PhotonNetwork.InRoom)
        {
            Debug.LogError("No estás en ninguna sala, no se puede spawnear jugador.");
            return;
        }

        SpawnPlayer();
    }

    void SpawnPlayer()
    {
        int playerIndex = PhotonNetwork.LocalPlayer.ActorNumber - 1;
        Transform spawnPoint = spawnPoints[playerIndex % spawnPoints.Length];

        // Instancia sincronizada de Photon
        PhotonNetwork.Instantiate(playerPrefab.name, spawnPoint.position, spawnPoint.rotation);
        Debug.Log($"Jugador spawneado en punto {playerIndex}: {spawnPoint.position}");
    }
}
