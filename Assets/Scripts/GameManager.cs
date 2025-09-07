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
        if (!PhotonNetwork.IsConnected)
        {
            Debug.LogError("PhotonNetwork no está conectado.");
            return;
        }

        SpawnPlayer();
    }

    void SpawnPlayer()
    {
        int playerIndex = PhotonNetwork.LocalPlayer.ActorNumber - 1;

        // Elegir spawnPoint correspondiente o aleatorio si hay más jugadores que spawnPoints
        Transform spawnPoint = (playerIndex < spawnPoints.Length)
            ? spawnPoints[playerIndex]
            : spawnPoints[Random.Range(0, spawnPoints.Length)];

        // Instanciar prefab de jugador con Photon
        GameObject player = PhotonNetwork.Instantiate("PhotonPrefabs/FirstPersonController NETWORK", spawnPoint.position, spawnPoint.rotation);

        // Activar solo la cámara y UI del jugador local
        if (player.TryGetComponent(out PhotonView pv))
        {
            if (!pv.IsMine)
            {
                // Desactivar cámara y Canvas de jugadores remotos
                Camera cam = player.GetComponentInChildren<Camera>();
                if (cam != null) cam.gameObject.SetActive(false);

                Canvas canvas = player.GetComponentInChildren<Canvas>();
                if (canvas != null) canvas.gameObject.SetActive(false);
            }
        }
    }
}
