using Photon.Pun;
using Photon.Realtime;
using System;
using UnityEngine;

public class GameNetworkController : MonoBehaviourPunCallbacks
{
    public static GameNetworkController Instance;

    public event Action<Player> OnPlayerSpawned;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // Llamar cuando un jugador entra a la sala de juego
    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        Debug.Log($"Jugador entró: {newPlayer.NickName}");
        OnPlayerSpawned?.Invoke(newPlayer);
    }

    // Spawnear jugador local
    public void SpawnLocalPlayer(GameObject prefab, Transform spawnPoint)
    {
        GameObject player = PhotonNetwork.Instantiate(prefab.name, spawnPoint.position, spawnPoint.rotation);
        OnPlayerSpawned?.Invoke(PhotonNetwork.LocalPlayer);
        Debug.Log($"Jugador spawneado: {PhotonNetwork.LocalPlayer.NickName}");
    }
}
