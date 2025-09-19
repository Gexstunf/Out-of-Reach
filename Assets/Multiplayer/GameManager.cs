using UnityEngine;
using Photon.Pun;
using System.Collections;

public class GameManager : MonoBehaviour
{
    [Header("Prefab del jugador")]
    public GameObject playerPrefab;

    [Header("Puntos de spawn")]
    public Transform[] spawnPoints;

    void Start()
    {
        StartCoroutine(SpawnPlayerWhenReady());
    }

    IEnumerator SpawnPlayerWhenReady()
    {
        while (!PhotonNetwork.InRoom || PhotonWrapperInstantiatePlayer.Instance == null)
            yield return null;

        int index = PhotonNetwork.LocalPlayer.ActorNumber - 1;
        Transform spawnPoint = spawnPoints[index % spawnPoints.Length];

        var player = PhotonWrapperInstantiatePlayer.Instance.SpawnPlayer(playerPrefab, spawnPoint);

        if (player != null)
            Debug.Log("[GameManager] Jugador instanciado correctamente: " + player.name);
    }
}
