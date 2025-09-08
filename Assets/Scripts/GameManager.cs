using Characters.PlayerController.Scripts;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Windows.Speech;

public class GameManager : MonoBehaviourPunCallbacks
{
    [Header("Player Prefab (Debe estar en Resources/PhotonPrefabs/)")]
    public GameObject playerPrefab;

    [Header("Spawn Points")]
    public Transform[] spawnPoints;

    [SerializeField] Dictionary<int, PlayerData> players = new Dictionary<int, PlayerData> ();

    private void Start()
    {
        if (!PhotonNetwork.IsConnected)
        {
            Debug.LogError("photon network no conecta");
            return;
        }

        if(playerPrefab == null)
        {
            Debug.LogError("no hay playerprefab");
            return;
        }

        if(spawnPoints.Length == 0)
        {
            Debug.LogError("no hay spaws");
            return;
        }
        
        if(PhotonNetwork.IsConnected)
            InitializePlayers();

        SpawnLocalPlayer();
    }

    private void InitializePlayers()
    {
        foreach(var p in PhotonNetwork.PlayerList)
        {
            if (!players.ContainsKey(p.ActorNumber))
            {
                players.Add(p.ActorNumber, new PlayerData());
            }
        }
    }

    private void SpawnLocalPlayer()
    {
        int playerIndex = PhotonNetwork.LocalPlayer.ActorNumber - 1;

        Transform spawnPoint = (playerIndex < spawnPoints.Length)
            ? spawnPoints[playerIndex]
            : spawnPoints[Random.Range(0, spawnPoints.Length)];

        GameObject player = PhotonNetwork.Instantiate(
            "PhotonPrefabs/FirstPersonController NETWORK",
            spawnPoint.position,
            spawnPoint.rotation
        );

        PhotonView pv = player.GetComponent<PhotonView>();

        PlayerData pdata = new PlayerData
        {
            character = player,
            camera = player.GetComponentInChildren<Camera>()
        };

        if (pv.IsMine)
        {
            player.GetComponent<PlayerControllerScript>().enabled = true;
            player.GetComponentInChildren<Camera>().enabled = true;
        }
        else
        {
            player.GetComponent<PlayerControllerScript>().enabled = false;
            player.GetComponentInChildren<Camera>().enabled = false;
        }

        if (PhotonNetwork.IsMasterClient)
        {
            players[PhotonNetwork.LocalPlayer.ActorNumber] = new PlayerData
            {
                character = player,
                camera = player.GetComponentInChildren<Camera>()
            };
        }
    }

    [PunRPC]
    public void ReceivePlayerAction(int actorNumber, string action)
    {
        if (!PhotonNetwork.IsMasterClient) return;

        if (players.ContainsKey(actorNumber))
        {
            Debug.Log($"Acción recibida de {actorNumber}: {action}");
        }
    }
}

[System.Serializable]
public class PlayerData
{
    public GameObject character;
    public Camera camera;
}

