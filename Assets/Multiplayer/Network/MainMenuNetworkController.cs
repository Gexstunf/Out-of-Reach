using Photon.Pun;
using Photon.Realtime;
using System;
using System.Collections.Generic;
using UnityEngine;
using ExitGames.Client.Photon;

public class MainMenuNetworkController : MonoBehaviourPunCallbacks
{
    public static MainMenuNetworkController Instance;

    public event Action OnConnectedEvent;
    public event Action OnRoomJoinedEvent;
    public event Action<string> OnErrorEvent;

    // Diccionario para almacenar contraseñas de salas
    private readonly System.Collections.Generic.Dictionary<string, string> roomPasswords = new();

    private void Awake()
    {
        // Singleton + DontDestroyOnLoad
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            PhotonNetwork.AutomaticallySyncScene = true; // sincronización automática de escenas
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // ===== Conexión =====
    public void Connect()
    {
        if (!PhotonNetwork.IsConnected)
        {
            PhotonNetwork.ConnectUsingSettings();
        }
    }

    public void SetPlayerName(string name)
    {
        PhotonNetwork.NickName = name;
    }

    // ===== Crear sala =====
    public void CreateRoom(string roomName, string password)
    {
        if (string.IsNullOrEmpty(roomName))
        {
            OnErrorEvent?.Invoke("Nombre de sala vacío.");
            return;
        }

        RoomOptions options = new RoomOptions { MaxPlayers = 4 };
        var customProps = new Hashtable { { "pwd", password } };
        options.CustomRoomProperties = customProps;
        options.CustomRoomPropertiesForLobby = new string[] { }; // oculto
        roomPasswords[roomName] = password;

        PhotonNetwork.CreateRoom(roomName, options);
    }

    // ===== Unirse a sala =====
    public void JoinRoom(string roomName, string password)
    {
        if (string.IsNullOrEmpty(roomName))
        {
            OnErrorEvent?.Invoke("Nombre de sala vacío.");
            return;
        }

        // Comprobamos contraseña si la tenemos guardada
        if (roomPasswords.ContainsKey(roomName) && roomPasswords[roomName] != password)
        {
            OnErrorEvent?.Invoke("Contraseña incorrecta.");
            return;
        }

        PhotonNetwork.JoinRoom(roomName);
    }

    // ===== Iniciar juego =====
    public void StartGame(string sceneName)
    {
        if (PhotonNetwork.IsMasterClient)
        {
            PhotonNetwork.LoadLevel(sceneName);
        }
    }

    // ===== Callbacks de Photon =====
    public override void OnConnectedToMaster()
    {
        Debug.Log("Conectado a Photon Master Server.");
        OnConnectedEvent?.Invoke();
    }

    public override void OnJoinedRoom()
    {
        OnRoomJoinedEvent?.Invoke();

        if (PhotonNetwork.IsMasterClient)
        {
            PhotonNetwork.LoadLevel("RoomLobby");
        }
    }


    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        Debug.LogError($"Fallo al crear la sala: {message} (Código: {returnCode})");
        OnErrorEvent?.Invoke(message);
    }

    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        Debug.LogError($"Fallo al unirse a la sala: {message} (Código: {returnCode})");
        OnErrorEvent?.Invoke(message);
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        Debug.LogWarning("Desconectado de Photon: " + cause);
        OnErrorEvent?.Invoke("Desconectado de Photon: " + cause);
    }
}
