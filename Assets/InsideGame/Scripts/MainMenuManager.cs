using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviourPunCallbacks
{
    [Header("Panels")]
    public GameObject panelMain;
    public GameObject panelHost;
    public GameObject panelJoin;
    public GameObject panelSettings;

    [Header("Inputs")]
    public InputField hostRoomNameInput;
    public InputField hostPasswordInput;

    public InputField joinRoomNameInput;
    public InputField joinPasswordInput;

    public InputField playerNameInput;

    private void Start()
    {
        PhotonNetwork.ConnectUsingSettings(); // Conecta a Photon automáticamente
        if (PlayerPrefs.HasKey("PlayerName"))
        {
            PhotonNetwork.NickName = PlayerPrefs.GetString("PlayerName");
            playerNameInput.text = PhotonNetwork.NickName;
        }
    }

    #region Panel Navigation
    public void OpenPanel(string panelName)
    {
        panelMain.SetActive(false);
        panelHost.SetActive(false);
        panelJoin.SetActive(false);
        panelSettings.SetActive(false);

        switch (panelName)
        {
            case "Main": panelMain.SetActive(true); break;
            case "Host": panelHost.SetActive(true); break;
            case "Join": panelJoin.SetActive(true); break;
            case "Settings": panelSettings.SetActive(true); break;
        }
    }

    public void QuitGame()
    {
        Application.Quit();
    }
    #endregion

    #region Hosting & Joining
    public void CreateRoom()
    {
        string roomName = hostRoomNameInput.text;
        string password = hostPasswordInput.text;

        if (string.IsNullOrEmpty(roomName)) return;

        RoomOptions options = new RoomOptions();
        options.MaxPlayers = 4;
        options.CustomRoomProperties = new ExitGames.Client.Photon.Hashtable()
        {
            { "password", password }
        };
        options.CustomRoomPropertiesForLobby = new string[] { "password" };

        PhotonNetwork.CreateRoom(roomName, options);
    }

    public void JoinRoom()
    {
        string roomName = joinRoomNameInput.text;
        string inputPassword = joinPasswordInput.text;

        PhotonNetwork.JoinRoom(roomName);
    }

    public override void OnJoinedRoom()
    {
        // Vas a la escena de espera donde todos están listos
        SceneManager.LoadScene("LobbyScene");
    }

    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        Debug.Log("No se pudo unir a la sala: " + message);
    }

    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        Debug.Log("No se pudo crear la sala: " + message);
    }

    public override void OnRoomListUpdate(System.Collections.Generic.List<RoomInfo> roomList)
    {
        // Si querés mostrar una lista de salas visibles, lo hacés acá
    }

    #endregion

    #region Player Name
    public void SetPlayerName()
    {
        PhotonNetwork.NickName = playerNameInput.text;
        PlayerPrefs.SetString("PlayerName", PhotonNetwork.NickName);
    }
    #endregion
}
