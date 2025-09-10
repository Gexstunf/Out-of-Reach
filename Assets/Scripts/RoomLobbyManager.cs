using Photon.Pun;
using Photon.Realtime;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;

public class RoomLobbyManager : MonoBehaviourPunCallbacks
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
    private AsyncOperation asyncLoad;

    void Start()
    {
        PhotonNetwork.AutomaticallySyncScene = true;

        roomNameText.text = "Room: " + PhotonNetwork.CurrentRoom.Name;
        leaveRoomButton.onClick.AddListener(LeaveRoom);

        loadingPanel.SetActive(false);
        lobbyUI.SetActive(true);

        if (!PhotonNetwork.IsMasterClient)
            startGameButton.gameObject.SetActive(false);
        else
            startGameButton.onClick.AddListener(() => StartCoroutine(LoadGameAsync()));

        UpdatePlayerList();
    }

    void UpdatePlayerList()
    {
        foreach (Transform child in playerListContainer.transform)
            Destroy(child.gameObject);

        playerEntries.Clear();

        foreach (Player player in PhotonNetwork.PlayerList)
        {
            GameObject entry = Instantiate(playerEntryPrefab, playerListContainer.transform);
            entry.GetComponent<TextMeshProUGUI>().text = player.NickName;
            playerEntries[player.ActorNumber] = entry;
        }
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        UpdatePlayerList();
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        UpdatePlayerList();
    }

    public override void OnMasterClientSwitched(Player newMasterClient)
    {
        startGameButton.gameObject.SetActive(PhotonNetwork.IsMasterClient);
    }

    void LeaveRoom()
    {
        PhotonNetwork.LeaveRoom();
    }

    public override void OnLeftRoom()
    {
        SceneManager.LoadScene("MainMenu");
    }

    IEnumerator LoadGameAsync()
    {
        lobbyUI.SetActive(false);

        loadingPanel.SetActive(true);

        loadingText.text = "Preparando datos...";
        yield return new WaitForSeconds(1f);

        loadingText.text = "Cargando escena...";

        if (PhotonNetwork.IsMasterClient)
        {
            PhotonNetwork.LoadLevel("GameScene");
        }
    }
}
