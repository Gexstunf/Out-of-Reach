using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Photon.Pun;
using Photon.Realtime;
using System.Collections.Generic;
using TMPro;

public class MainMenuManager : MonoBehaviourPunCallbacks
{
    [Header("Panels")]
    public GameObject mainPanel;
    public GameObject hostPanel;
    public GameObject joinPanel;
    public GameObject settingsPanel;

    [Header("Input Fields")]
    public TMP_InputField roomNameInput_Host;
    public TMP_InputField passwordInput_Host;
    public TMP_InputField roomNameInput_Join;
    public TMP_InputField passwordInput_Join;
    public TMP_InputField playerNameInput;

    private Dictionary<string, string> roomPasswords = new Dictionary<string, string>();

    private void Start()
    {
        Debug.Log("Iniciando conexión con Photon...");
        PhotonNetwork.AutomaticallySyncScene = true;
        PhotonNetwork.ConnectUsingSettings();
        ShowPanel(mainPanel);

        if (PlayerPrefs.HasKey("PlayerName"))
        {
            PhotonNetwork.NickName = PlayerPrefs.GetString("PlayerName");
            playerNameInput.text = PhotonNetwork.NickName;
            Debug.Log("Nombre cargado: " + PhotonNetwork.NickName);
        }
    }

    private void ShowPanel(GameObject panelToShow)
    {
        mainPanel.SetActive(false);
        hostPanel.SetActive(false);
        joinPanel.SetActive(false);
        settingsPanel.SetActive(false);

        panelToShow.SetActive(true);
        Debug.Log("Mostrando panel: " + panelToShow.name);
    }

    public void ShowHostPanel() => ShowPanel(hostPanel);
    public void ShowJoinPanel() => ShowPanel(joinPanel);
    public void ShowSettingsPanel() => ShowPanel(settingsPanel);
    public void ShowMainPanel() => ShowPanel(mainPanel);

    // ===== Crear sala =====
    public void OnClickCreateRoom()
    {
        string roomName = roomNameInput_Host.text;
        string password = passwordInput_Host.text;

        Debug.Log($"Intentando crear sala: {roomName}");

        if (string.IsNullOrEmpty(roomName))
        {
            Debug.LogWarning("Nombre de sala vacío.");
            return;
        }

        RoomOptions options = new RoomOptions { MaxPlayers = 4 };
        roomPasswords[roomName] = password;

        PhotonNetwork.CreateRoom(roomName, options);
    }

    // ===== Unirse a sala =====
    public void OnClickJoinRoom()
    {
        string roomName = roomNameInput_Join.text;
        string password = passwordInput_Join.text;

        Debug.Log($"Intentando unirse a la sala: {roomName}");

        if (string.IsNullOrEmpty(roomName))
        {
            Debug.LogWarning("Nombre de sala vacío.");
            return;
        }

        if (roomPasswords.ContainsKey(roomName) && roomPasswords[roomName] != password)
        {
            Debug.LogWarning("Contraseña incorrecta.");
            return;
        }

        PhotonNetwork.JoinRoom(roomName);
    }

    // ===== Guardar nombre =====
    public void OnClickChangeName()
    {
        string playerName = playerNameInput.text;

        if (!string.IsNullOrEmpty(playerName))
        {
            PhotonNetwork.NickName = playerName;
            PlayerPrefs.SetString("PlayerName", playerName);
            Debug.Log("Nombre cambiado a: " + playerName);
        }

        ShowMainPanel();
    }

    // ===== Salir del juego =====
    public void OnClickQuitGame()
    {
        Debug.Log("Saliendo del juego...");
        Application.Quit();
    }

    // ===== CALLBACKS DE PHOTON =====
    public override void OnJoinedRoom()
    {
        Debug.Log("Jugador unido a la sala correctamente.");

        if (PhotonNetwork.IsMasterClient)
        {
            Debug.Log("Es el Master Client. Cargando RoomLobby...");
            PhotonNetwork.LoadLevel("RoomLobby");
        }
        else
        {
            Debug.Log("No es el Master Client.");
        }
    }

    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        Debug.LogError($"Fallo al crear la sala: {message} (Código: {returnCode})");
    }

    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        Debug.LogError($"Fallo al unirse a la sala: {message} (Código: {returnCode})");
    }

    public override void OnConnectedToMaster()
    {
        Debug.Log("Conectado a Photon Master Server.");
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        Debug.LogWarning("Desconectado de Photon: " + cause);
    }
}
