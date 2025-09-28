using Photon.Pun;
using Photon.Realtime;
using System;
using UnityEngine;

public class RoomLobbyNetworkController : MonoBehaviourPunCallbacks
{
    public static RoomLobbyNetworkController Instance;

    // Eventos para que la UI se actualice
    public event Action<Player> OnPlayerJoined;
    public event Action<Player> OnPlayerLeft;
    public event Action<Player> OnMasterClientChanged;

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

    // ===== Callbacks de Photon =====
    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        Debug.Log($"Jugador entró: {newPlayer.NickName}");
        OnPlayerJoined?.Invoke(newPlayer);
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        Debug.Log($"Jugador salió: {otherPlayer.NickName}");
        OnPlayerLeft?.Invoke(otherPlayer);
    }

    public override void OnMasterClientSwitched(Player newMasterClient)
    {
        Debug.Log($"Nuevo MasterClient: {newMasterClient.NickName}");
        OnMasterClientChanged?.Invoke(newMasterClient);
    }

    public void StartGame(string sceneName)
    {
        if (PhotonNetwork.IsMasterClient)
        {
            PhotonNetwork.LoadLevel(sceneName);
        }
    }

    public void LeaveRoom()
    {
        PhotonNetwork.LeaveRoom();
    }
}
