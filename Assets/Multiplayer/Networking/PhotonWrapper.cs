using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.SceneManagement;
using System;
using System.Collections;

public class PhotonWrapper : MonoBehaviourPunCallbacks
{
    public static PhotonWrapper Instance;

    [Header("Default Player Settings")]
    public GameObject defaultPlayerPrefab;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    #region CONNECTION
    public void Connect(string playerName)
    {
        PhotonNetwork.NickName = playerName;
        PhotonNetwork.AutomaticallySyncScene = true;
        PhotonNetwork.ConnectUsingSettings();
        Debug.Log("[PhotonWrapper] Connecting to Photon...");
    }

    public override void OnConnectedToMaster()
    {
        Debug.Log("[PhotonWrapper] Connected to Master Server");
        PhotonNetwork.JoinLobby();
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        Debug.LogWarning("[PhotonWrapper] Disconnected: " + cause);
    }
    #endregion

    #region ROOM MANAGEMENT
    public void CreateRoom(string roomName, byte maxPlayers = 4)
    {
        RoomOptions options = new RoomOptions { MaxPlayers = maxPlayers };
        PhotonNetwork.CreateRoom(roomName, options);
    }

    public void JoinRoom(string roomName)
    {
        PhotonNetwork.JoinRoom(roomName);
    }

    public override void OnJoinedRoom()
    {
        Debug.Log("[PhotonWrapper] Joined room: " + PhotonNetwork.CurrentRoom.Name);
        if (PhotonNetwork.IsMasterClient)
        {
            Debug.Log("[PhotonWrapper] MasterClient: Load scene if needed");
        }
    }

    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        Debug.LogError("[PhotonWrapper] Failed to create room: " + message);
    }

    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        Debug.LogError("[PhotonWrapper] Failed to join room: " + message);
    }
    #endregion

    #region SCENE MANAGEMENT
    public void LoadScene(string sceneName)
    {
        if (PhotonNetwork.IsMasterClient)
        {
            PhotonNetwork.LoadLevel(sceneName);
        }
    }

    public override void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "GameScene") // Optional: parameterize or pass callback
        {
            Debug.Log("[PhotonWrapper] GameScene loaded, spawning player...");
            SpawnPlayer(defaultPlayerPrefab, null);
        }
    }
    #endregion

    #region PLAYER SPAWN
    public GameObject SpawnPlayer(GameObject prefab, Transform spawnPoint = null)
    {
        if (!PhotonNetwork.InRoom)
        {
            Debug.LogError("[PhotonWrapper] Not in a room, cannot spawn player.");
            return null;
        }

        Vector3 position = spawnPoint != null ? spawnPoint.position : Vector3.zero;
        Quaternion rotation = spawnPoint != null ? spawnPoint.rotation : Quaternion.identity;

        GameObject player = PhotonNetwork.Instantiate(prefab.name, position, rotation);
        SetupLocalPlayer(player);

        OnPlayerSpawned?.Invoke(player);

        Debug.Log("[PhotonWrapper] Player spawned: " + prefab.name);
        return player;
    }

    private void SetupLocalPlayer(GameObject player)
    {
        var photonView = player.GetComponent<PhotonView>();
        bool isLocal = photonView != null && photonView.IsMine;

        // Camera / audio
        var cam = player.GetComponentInChildren<Camera>();
        var audio = player.GetComponentInChildren<AudioListener>();

        if (cam != null) cam.enabled = isLocal;
        if (audio != null) audio.enabled = isLocal;

        // Controllers
        var controller = player.GetComponent<PlayerControllerScript>();
        var rotator = player.GetComponent<RotatorScript>();

        if (controller != null) controller.enabled = isLocal;
        if (rotator != null) rotator.enabled = isLocal;
    }
    #endregion

    #region EVENTS
    // Optional callbacks for UI or other systems
    public event Action<GameObject> OnPlayerSpawned;
    #endregion
}
