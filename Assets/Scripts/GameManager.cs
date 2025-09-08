using Characters.PlayerController.Scripts;
using Photon.Pun;
using UnityEngine;

public class GameManager : MonoBehaviourPunCallbacks
{
    [Header("Player Prefab (Debe estar en Resources/PhotonPrefabs/)")]
    public GameObject playerPrefab;

    [Header("Spawn Points")]
    public Transform[] spawnPoints;

    void Start()
    {
        if (!PhotonNetwork.IsConnected)
        {
            Debug.LogError("PhotonNetwork no está conectado.");
            return;
        }

        if (playerPrefab == null)
        {
            Debug.LogError("playerPrefab no asignado en GameManager.");
            return;
        }

        if (spawnPoints.Length == 0)
        {
            Debug.LogError("No hay spawn points asignados.");
            return;
        }

        SpawnLocalPlayer();
    }

    private void SpawnLocalPlayer()
    {
        int playerIndex = PhotonNetwork.LocalPlayer.ActorNumber - 1;

        // Elegimos spawn point seguro
        Transform spawnPoint = (playerIndex < spawnPoints.Length)
            ? spawnPoints[playerIndex]
            : spawnPoints[Random.Range(0, spawnPoints.Length)];

        // Instanciamos prefab de red (debe estar en Resources)
        GameObject player = PhotonNetwork.Instantiate(
            "PhotonPrefabs/FirstPersonController NETWORK", // ruta relativa a Resources
            spawnPoint.position,
            spawnPoint.rotation
        );

        PhotonView pv = player.GetComponent<PhotonView>();

        if (pv != null && pv.IsMine)
        {
            // Activamos PlayerControllerScript solo para jugador local
            PlayerControllerScript controller = player.GetComponent<PlayerControllerScript>();
            if (controller != null)
                controller.enabled = true;

            // Activamos cámara solo para jugador local
            Camera cam = player.GetComponentInChildren<Camera>();
            if (cam != null)
                cam.enabled = true;
        }
        else
        {
            // Desactivamos scripts y cámara en jugadores remotos
            PlayerControllerScript controller = player.GetComponent<PlayerControllerScript>();
            if (controller != null)
                controller.enabled = false;

            Camera cam = player.GetComponentInChildren<Camera>();
            if (cam != null)
                cam.enabled = false;
        }
    }
}
