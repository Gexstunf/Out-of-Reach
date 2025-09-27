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
        // Conectar con Photon
        PhotonManager.Instance.Connect();

        // Suscribirse a eventos de PhotonManager
        PhotonManager.Instance.OnConnectedEvent += () =>
        {
            Debug.Log("Conectado a Photon Master Server.");
        };

        PhotonManager.Instance.OnRoomJoinedEvent += () =>
        {
            Debug.Log("Sala unida correctamente");
            ShowLoadingPanel(); // feedback visual mientras carga RoomLobby
        };

        PhotonManager.Instance.OnErrorEvent += (msg) =>
        {
            Debug.LogWarning(msg);
            // Aquí podrías mostrar un mensaje en UI
        };

        // Cargar nombre guardado
        if (PlayerPrefs.HasKey("PlayerName"))
        {
            string savedName = PlayerPrefs.GetString("PlayerName");
            PhotonManager.Instance.SetPlayerName(savedName);
            playerNameInput.text = savedName;
        }

        ShowMainPanel();
    }

    // ===== Gestión de Paneles =====
    private void ShowPanel(GameObject panelToShow)
    {
        mainPanel.SetActive(false);
        hostPanel.SetActive(false);
        joinPanel.SetActive(false);
        settingsPanel.SetActive(false);
        if (loadingPanel != null) loadingPanel.SetActive(false);

        panelToShow.SetActive(true);
    }

    public void ShowMainPanel() => ShowPanel(mainPanel);
    public void ShowHostPanel() => ShowPanel(hostPanel);
    public void ShowJoinPanel() => ShowPanel(joinPanel);
    public void ShowSettingsPanel() => ShowPanel(settingsPanel);
    public void ShowLoadingPanel() => ShowPanel(loadingPanel);

    // ===== Botones =====
    public void OnClickChangeName()
    {
        string name = playerNameInput.text;
        if (string.IsNullOrEmpty(name))
        {
            Debug.LogWarning("Nombre vacío.");
            return;
        }

        PhotonManager.Instance.SetPlayerName(name);
        PlayerPrefs.SetString("PlayerName", name);
        Debug.Log("Nombre cambiado a: " + name);
        ShowMainPanel();
    }

    public void OnClickCreateRoom()
    {
        string roomName = roomNameInput_Host.text;
        string password = passwordInput_Host.text;
        PhotonManager.Instance.CreateRoom(roomName, password);
    }

    public void OnClickJoinRoom()
    {
        string roomName = roomNameInput_Join.text;
        string password = passwordInput_Join.text;
        PhotonManager.Instance.JoinRoom(roomName, password);
    }

    public void OnClickStartGame()
    {
        PhotonManager.Instance.StartGame("RoomLobby");
    }

    public void OnClickQuitGame()
    {
        Debug.Log("Saliendo del juego...");
        Application.Quit();
    }

    // ===== Botones de volver al menú principal =====
    public void OnClickBackFromHost() => ShowMainPanel();
    public void OnClickBackFromJoin() => ShowMainPanel();
    public void OnClickBackFromSettings() => ShowMainPanel();
}
