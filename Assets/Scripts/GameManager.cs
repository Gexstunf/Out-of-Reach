using Photon.Pun;
using UnityEngine;

public class GameManager : MonoBehaviourPunCallbacks
{
    [Header("Prefab del jugador (poner en Resources/PhotonPrefabs/)")]
    public GameObject playerPrefab;

    [Header("Spawn Points")]
    public Transform[] spawnPoints;

    private void Start()
    {
        if (!PhotonNetwork.IsConnected)
        {
            Debug.LogError("Photon no está conectado.");
            return;
        }

        if (playerPrefab == null)
        {
            Debug.LogError("No hay prefab asignado en el inspector.");
            return;
        }

        if (spawnPoints == null || spawnPoints.Length == 0)
        {
            Debug.LogError("No hay spawn points asignados.");
            return;
        }

        // Solo crear jugador si todavía no existe uno local
        if (PhotonNetwork.LocalPlayer.TagObject == null)
        {
            SpawnLocalPlayer();
        }
    }

    private void SpawnLocalPlayer()
    {
        // Elegimos spawn aleatorio
        Transform spawnPoint = spawnPoints[Random.Range(0, spawnPoints.Length)];

        // Instanciamos el prefab
        GameObject player = PhotonNetwork.Instantiate(
            playerPrefab.name,
            spawnPoint.position,
            spawnPoint.rotation
        );

        // Guardamos referencia en el TagObject del jugador de Photon
        PhotonNetwork.LocalPlayer.TagObject = player;
    }
}
