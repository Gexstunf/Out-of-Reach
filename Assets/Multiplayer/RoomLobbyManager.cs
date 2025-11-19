using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Collections;
using Photon.Pun;
using Photon.Realtime;

public class RoomLobbyManager : MonoBehaviour
{
    [Header("UI Sala")]
    public GameObject lobbyUI;
    public TextMeshProUGUI roomNameText;
    public GameObject playerListContainer;
    public GameObject playerEntryPrefab;
    public Button startGameButton;
    public Button leaveRoomButton;

    [Header("UI Carga")]
    public GameObject loadingPanel;
    public Slider loadingBar;
    public TextMeshProUGUI loadingText;

    private Dictionary<int, GameObject> playerEntries = new Dictionary<int, GameObject>();

    void Start()
    {
        if (PhotonNetwork.CurrentRoom == null)
        {
            Debug.LogError("No est�s en ninguna sala");
            return;
        }

        roomNameText.text = "Room: " + PhotonNetwork.CurrentRoom.Name;

        // Botones
        leaveRoomButton.onClick.AddListener(() => RoomLobbyNetworkController.Instance.LeaveRoom());
        startGameButton.onClick.AddListener(() => StartCoroutine(LoadGameAsync()));

        // Inicialmente ocultar loading
        loadingPanel.SetActive(false);
        lobbyUI.SetActive(true);

        // Actualizar MasterClient
        startGameButton.gameObject.SetActive(PhotonNetwork.IsMasterClient);

        // Suscribirse a eventos del network controller
        RoomLobbyNetworkController.Instance.OnPlayerJoined += OnPlayerJoined;
        RoomLobbyNetworkController.Instance.OnPlayerLeft += OnPlayerLeft;
        RoomLobbyNetworkController.Instance.OnMasterClientChanged += OnMasterClientChanged;

        // Cargar jugadores actuales
        UpdatePlayerList();
    }

    void UpdatePlayerList()
    {
        foreach (Transform child in playerListContainer.transform)
            Destroy(child.gameObject);

        playerEntries.Clear();

        foreach (var player in PhotonNetwork.PlayerList)
        {
            AddPlayerToUI(player);
        }
    }

    void AddPlayerToUI(Player player)
    {
        GameObject entry = Instantiate(playerEntryPrefab, playerListContainer.transform);
        entry.GetComponent<TextMeshProUGUI>().text = player.NickName;
        playerEntries[player.ActorNumber] = entry;
    }

    // Eventos
    void OnPlayerJoined(Player player) => AddPlayerToUI(player);
    void OnPlayerLeft(Player player)
    {
        if (playerEntries.ContainsKey(player.ActorNumber))
        {
            Destroy(playerEntries[player.ActorNumber]);
            playerEntries.Remove(player.ActorNumber);
        }
    }

    void OnMasterClientChanged(Player newMaster)
    {
        startGameButton.gameObject.SetActive(PhotonNetwork.IsMasterClient);
    }

    IEnumerator LoadGameAsync()
    {
        lobbyUI.SetActive(false);
        loadingPanel.SetActive(true);
        loadingText.text = "Preparando datos...";
        yield return new WaitForSeconds(1f);

        loadingText.text = "Cargando escena...";

        // Solo MasterClient inicia la carga
        if (PhotonNetwork.IsMasterClient)
        {
            RoomLobbyNetworkController.Instance.StartGame("Gameplay-Testing");
        }
    }
}
