using UnityEngine;
using System.Linq;
using System.Collections.Generic;
using Characters.StateMachine.PlayerStateMachine;
using Multiplayer.Network;
using Unity.Cinemachine;

public class SpectatorController : MonoBehaviour
{
    [Header("C�mara de espectador")]
    [SerializeField] private CinemachineCamera spectatorCam;

    [Header("Delay antes de espectear (killcam)")]
    [SerializeField] private float spectateDelay = 3f;

    private List<PlayerStateMachineScript> alivePlayers = new();
    private int index = 0;
    private Transform target;
    private bool isSpectating = false;

    public static SpectatorController Instance;
    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        spectatorCam.gameObject.SetActive(false);
    }

    private void Update()
    {
        if (!isSpectating) return;

        if (Input.GetKeyDown(KeyCode.Alpha1))
            PreviousTarget();
        if (Input.GetKeyDown(KeyCode.Alpha2))
            NextTarget();
    }

    // Refresca la lista de jugadores vivos
    void RefreshAlivePlayers()
    {
        alivePlayers.Clear();

        if (GameNetworkController.Instance == null) return;

        var aliveActorNumbers = GameNetworkController.Instance.GetAlivePlayers();

        foreach (var reporter in FindObjectsByType<PlayerStatusReporter>(FindObjectsSortMode.None))
        {
            if (reporter == null || reporter.photonView == null) continue;

            int actor = reporter.photonView.OwnerActorNr;
            if (aliveActorNumbers.Contains(actor))
            {
                var stateMachine = reporter.GetComponent<PlayerStateMachineScript>();
                if (stateMachine != null)
                    alivePlayers.Add(stateMachine);
            }
        }

        if (index >= alivePlayers.Count)
            index = 0;
    }

    bool IsPlayerUnconscious(PlayerStateMachineScript player)
    {
        if (player == null) return true;
        return player.isUnconscious;
    }

    void SetTarget(PlayerStateMachineScript player)
    {
        if (player == null || player.gameObject == null) return;

        target = player.transform;
        spectatorCam.Follow = target;
        spectatorCam.LookAt = target;
    }

    public void NextTarget()
    {
        RefreshAlivePlayers();
        if (alivePlayers.Count == 0) return;

        index = (index + 1) % alivePlayers.Count;
        SetTarget(alivePlayers[index]);
    }

    public void PreviousTarget()
    {
        RefreshAlivePlayers();
        if (alivePlayers.Count == 0) return;

        index = (index - 1 + alivePlayers.Count) % alivePlayers.Count;
        SetTarget(alivePlayers[index]);
    }

    public void EnableSpectatorMode()
    {
        RefreshAlivePlayers();
        if (alivePlayers.Count == 0) return;

        index = 0;
        SetTarget(alivePlayers[index]);
        spectatorCam.gameObject.SetActive(true);
        spectatorCam.ForceCameraPosition(target.position, target.rotation);
        isSpectating = true;
    }

    public void DisableSpectatorMode()
    {
        spectatorCam.gameObject.SetActive(false);
        isSpectating = false;
    }

    public void StartSpectatingWithDelay()
    {
        Invoke(nameof(EnableSpectatorMode), spectateDelay);
    }
}
