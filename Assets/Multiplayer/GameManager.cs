using Photon.Pun;
using UnityEngine;

public class GameManager : MonoBehaviourPunCallbacks
{
    public GameObject playerPrefab;
    public Transform[] spawnPoints;

    void Start()
    {
        if (PhotonNetwork.IsConnected)
        {
            int playerIndex = PhotonNetwork.LocalPlayer.ActorNumber - 1;

            // Si hay menos jugadores que puntos, usar el punto correspondiente
            // Si hay mas, tomar aleatorio pero evitando superposicion
            Transform spawnPoint;
            if (playerIndex < spawnPoints.Length)
            {
                spawnPoint = spawnPoints[playerIndex];
            }
            else
            {
                // Backup: tomar uno aleatorio no ocupado (mejorable)
                spawnPoint = spawnPoints[Random.Range(0, spawnPoints.Length)];
            }

            PhotonNetwork.Instantiate(playerPrefab.name, spawnPoint.position, spawnPoint.rotation);
        }
    }
}
