using Photon.Pun;
using Photon.Realtime;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class RoomLobbyManager : MonoBehaviourPunCallbacks
{
    public TextMeshProUGUI roomNameText;
    public GameObject playerListContainer;
    public GameObject playerEntryPrefab;
    public Button startGameButton;
    public Button leaveRoomButton;

    private Dictionary<int, GameObject> playerEntries = new Dictionary<int, GameObject>();

    void Start()
    {
        roomNameText.text = "Room: " + PhotonNetwork.CurrentRoom.Name;
        leaveRoomButton.onClick.AddListener(LeaveRoom);

        if (!PhotonNetwork.IsMasterClient)
            startGameButton.gameObject.SetActive(false);
        else
            startGameButton.onClick.AddListener(StartGame);

        UpdatePlayerList();
    }

    void UpdatePlayerList()
    {
        foreach (Transform child in playerListContainer.transform)
        {
            Destroy(child.gameObject);
        }

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

    void StartGame()
    {
        PhotonNetwork.LoadLevel("GameScene"); 
    }
}
