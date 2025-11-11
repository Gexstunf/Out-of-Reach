using Photon.Pun;
using Photon.Realtime;
using System;
using System.Collections.Generic;
using UnityEngine;

public class GameNetworkController : MonoBehaviourPunCallbacks
{
    public static GameNetworkController Instance;

    [Header("Prefabs")]
    public GameObject playerPrefab;
    public GameObject doorPrefab;

    [Header("Spawns")]
    public Transform[] spawnPoints;

    public event Action<Player, GameObject> OnPlayerSpawned;

    private Dictionary<int, PlayerStatusReporter> playerTrackers = new();
    private Dictionary<int, bool> playerAliveStates = new();


    #region Awake-Start
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
    private void Start()
    {
        if (!PhotonNetwork.InRoom)
        {
            Debug.LogError("No estás en una sala, no se puede spawnear jugador.");
            return;
        }

    SpawnLocalPlayer();
    }
    #endregion

    #region Pl_handling
    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        Debug.Log($"Jugador entró: {newPlayer.NickName}");
    }

    public void SpawnLocalPlayer()
    {
        int playerIndex = PhotonNetwork.LocalPlayer.ActorNumber - 1;
        Transform spawnPoint = spawnPoints[playerIndex % spawnPoints.Length];

        GameObject player = PhotonNetwork.Instantiate(playerPrefab.name, spawnPoint.position, spawnPoint.rotation);

        RegisterPlayer(PhotonNetwork.LocalPlayer.ActorNumber, player.GetComponent<PlayerStatusReporter>());

        OnPlayerSpawned?.Invoke(PhotonNetwork.LocalPlayer, player);

        Debug.Log($"Jugador local spawneado en punto {playerIndex}: {spawnPoint.position}");
    }

    public void RegisterPlayer(int actorNumber, PlayerStatusReporter reporter)
    {
        if (!playerTrackers.ContainsKey(actorNumber))
        {
            playerTrackers[actorNumber] = reporter;
            playerAliveStates[actorNumber] = true;
        }
    }

    public void UnregisterPlayer(int actorNumber)
    {
        playerTrackers.Remove(actorNumber);
        playerAliveStates.Remove(actorNumber);
    }

    public void UpdatePlayerStatus(int actorNumber, bool isAlive)
    {
        if (playerAliveStates.ContainsKey(actorNumber))
            playerAliveStates[actorNumber] = isAlive;
    }

    public bool IsPlayerAlive(int actorNumber)
    {
        return playerAliveStates.TryGetValue(actorNumber, out bool isAlive) && isAlive;
    }

    public List<int> GetAlivePlayers()
    {
        List<int> alive = new();
        foreach (var kvp in playerAliveStates)
            if (kvp.Value)
                alive.Add(kvp.Key);
        return alive;
    }

    public List<int> GetDeadPlayers()
    {
        List<int> dead = new();
        foreach (var kvp in playerAliveStates)
            if (!kvp.Value)
                dead.Add(kvp.Key);
        return dead;
    }
    #endregion

    #region Doors handling



    #endregion
}