using Photon.Pun;
using Photon.Realtime;
using System;
using System.Collections.Generic;
using UnityEngine;

public class GameNetworkController : MonoBehaviourPunCallbacks
{
    public static GameNetworkController Instance;

    public event Action<Player> OnPlayerSpawned;

    // Mapeo de jugadores (actorNumber -> PlayerStatusReporter)
    private Dictionary<int, PlayerStatusReporter> playerTrackers = new();
    private Dictionary<int, bool> playerAliveStates = new();

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

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        Debug.Log($"Jugador entró: {newPlayer.NickName}");
        OnPlayerSpawned?.Invoke(newPlayer);
    }

    public void SpawnLocalPlayer(GameObject prefab, Transform spawnPoint)
    {
        GameObject player = PhotonNetwork.Instantiate(prefab.name, spawnPoint.position, spawnPoint.rotation);
        OnPlayerSpawned?.Invoke(PhotonNetwork.LocalPlayer);
        Debug.Log($"Jugador spawneado: {PhotonNetwork.LocalPlayer.NickName}");
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
}
