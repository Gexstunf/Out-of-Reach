using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using TMPro;

public class RoomLobbyUI : MonoBehaviourPunCallbacks
{
    public GameObject playerEntryPrefab;
    public Transform contentPanel;

    private Dictionary<int, GameObject> playerEntries = new Dictionary<int, GameObject>();

    void Start()
    {
        UpdatePlayerList();
    }

    void UpdatePlayerList()
    {
        foreach (var entry in playerEntries.Values)
        {
            Destroy(entry);
        }
        playerEntries.Clear();

        foreach (Player player in PhotonNetwork.PlayerList)
        {
            GameObject entry = Instantiate(playerEntryPrefab, contentPanel);
            entry.transform.Find("PlayerNameText").GetComponent<TextMeshProUGUI>().text = player.NickName;
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
}
