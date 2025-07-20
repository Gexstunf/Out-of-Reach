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
        PhotonNetwork.AutomaticallySyncScene = true;
        PhotonNetwork.ConnectUsingSettings();
        ShowPanel(mainPanel);

        if (PlayerPrefs.HasKey("PlayerName"))
        {
            PhotonNetwork.NickName = PlayerPrefs.GetString("PlayerName");
            playerNameInput.text = PhotonNetwork.NickName;
        }
    }

    // Muestra el panel deseado y oculta los otros
    private void ShowPanel(GameObject panelToShow)
    {
        mainPanel.SetActive(false);
        hostPanel.SetActive(false);
        joinPanel.SetActive(false);
        settingsPanel.SetActive(false);

        panelToShow.SetActive(true);
    }

    // ===== Botones de navegación =====
    public void ShowHostPanel() => ShowPanel(hostPanel);
    public void ShowJoinPanel() => ShowPanel(joinPanel);
    public void ShowSettingsPanel() => ShowPanel(settingsPanel);
    public void ShowMainPanel() => ShowPanel(mainPanel);

    // ===== Crear sala =====
    public void OnClickCreateRoom()
    {
        string roomName = roomNameInput_Host.text;
        string password = passwordInput_Host.text;

        if (string.IsNullOrEmpty(roomName)) return;

        RoomOptions options = new RoomOptions { MaxPlayers = 4 };
        roomPasswords[roomName] = password;

        PhotonNetwork.CreateRoom(roomName, options);
    }

    // ===== Unirse a sala =====
    public void OnClickJoinRoom()
    {
        string roomName = roomNameInput_Join.text;
        string password = passwordInput_Join.text;

        if (string.IsNullOrEmpty(roomName)) return;

        if (roomPasswords.ContainsKey(roomName) && roomPasswords[roomName] != password)
        {
            Debug.Log("Contraseña incorrecta");
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
        }

        ShowMainPanel();
    }

    // ===== Salir del juego =====
    public void OnClickQuitGame()
    {
        Application.Quit();
    }

    // ===== CALLBACKS DE PHOTON =====
    public override void OnJoinedRoom()
    {
        Debug.Log("Jugador unido a la sala.");

        if (PhotonNetwork.IsMasterClient)
        {
            PhotonNetwork.LoadLevel("RoomLobby");
        }
    }

    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        Debug.Log($"Fallo al crear la sala: {message}");
    }

    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        Debug.Log($"Fallo al unirse a la sala: {message}");
    }
}
