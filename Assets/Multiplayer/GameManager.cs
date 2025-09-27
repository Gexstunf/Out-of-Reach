using Photon.Pun;
using UnityEngine;

public class GameManager : MonoBehaviourPunCallbacks
{
    [Header("Prefab del jugador")]
    public GameObject playerPrefab;

    [Header("Puntos de spawn")]
    public Transform[] spawnPoints;

    void Start()
    {
        if (PhotonNetwork.InRoom)
        {
            Debug.Log(" Ya estoy en una sala, spawneo jugador...");

            int playerIndex = PhotonNetwork.LocalPlayer.ActorNumber - 1;
            Transform spawnPoint = spawnPoints[playerIndex % spawnPoints.Length];

            PhotonNetwork.Instantiate(playerPrefab.name, spawnPoint.position, spawnPoint.rotation);
        }
        else
        {
            Debug.LogError(" No estás en ninguna sala, no se puede spawnear jugador.");
        }
    }
}
