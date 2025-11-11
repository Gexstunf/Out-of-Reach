using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class MainMenuManager : MonoBehaviour
{
    [Header("Panels")]
    public GameObject mainPanel;
    public GameObject hostPanel;
    public GameObject joinPanel;
    public GameObject settingsPanel;
    public GameObject loadingPanel;

    [Header("Input Fields")]
    public TMP_InputField playerNameInput;
    public TMP_InputField roomNameInput_Host;
    public TMP_InputField passwordInput_Host;
    public TMP_InputField roomNameInput_Join;
    public TMP_InputField passwordInput_Join;

    private void Start()
    {
        MainMenuNetworkController.Instance.Connect();

        MainMenuNetworkController.Instance.OnConnectedEvent += () =>
        {
            Debug.Log("Conectado a Photon Master Server.");
        };

        MainMenuNetworkController.Instance.OnRoomJoinedEvent += () =>
        {
            Debug.Log("Sala unida correctamente");
            ShowLoadingPanel();
        };

        MainMenuNetworkController.Instance.OnErrorEvent += (msg) =>
        {
            Debug.LogWarning(msg);
        };

        if (PlayerPrefs.HasKey("PlayerName"))
        {
            string savedName = PlayerPrefs.GetString("PlayerName");
            MainMenuNetworkController.Instance.SetPlayerName(savedName);
            playerNameInput.text = savedName;
        }

        ShowMainPanel();
    }

    private void ShowPanel(GameObject panelToShow)
    {
        if (panelToShow == null) return;

        if (mainPanel != null) mainPanel.SetActive(false);
        if (hostPanel != null) hostPanel.SetActive(false);
        if (joinPanel != null) joinPanel.SetActive(false);
        if (settingsPanel != null) settingsPanel.SetActive(false);
        if (loadingPanel != null) loadingPanel.SetActive(false);

        panelToShow.SetActive(true);
    }

    public void ShowMainPanel() => ShowPanel(mainPanel);
    public void ShowHostPanel() => ShowPanel(hostPanel);
    public void ShowJoinPanel() => ShowPanel(joinPanel);
    public void ShowSettingsPanel() => ShowPanel(settingsPanel);
    public void ShowLoadingPanel() => ShowPanel(loadingPanel);

    public void OnClickChangeName()
    {
        string name = playerNameInput.text;
        if (string.IsNullOrEmpty(name))
        {
            Debug.LogWarning("Nombre vacío.");
            return;
        }

        MainMenuNetworkController.Instance.SetPlayerName(name);
        PlayerPrefs.SetString("PlayerName", name);
        Debug.Log("Nombre cambiado a: " + name);
        ShowMainPanel();
    }

    public void OnClickCreateRoom()
    {
        string roomName = roomNameInput_Host.text;
        string password = passwordInput_Host.text;
        MainMenuNetworkController.Instance.CreateRoom(roomName, password);
    }

    public void OnClickJoinRoom()
    {
        string roomName = roomNameInput_Join.text;
        string password = passwordInput_Join.text;
        MainMenuNetworkController.Instance.JoinRoom(roomName, password);
    }

    public void OnClickStartGame()
    {
        MainMenuNetworkController.Instance.StartGame("RoomLobby");
    }

    public void OnClickQuitGame()
    {
        Debug.Log("Saliendo del juego...");
        Application.Quit();
    }

    public void OnClickBackFromHost() => ShowMainPanel();
    public void OnClickBackFromJoin() => ShowMainPanel();
    public void OnClickBackFromSettings() => ShowMainPanel();
}
