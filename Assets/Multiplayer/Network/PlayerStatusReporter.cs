using Characters.StateMachine.PlayerStateMachine;
using Characters.SystemAdaptations;
using Multiplayer.Network;
using Photon.Pun;
using UnityEngine;

[RequireComponent(typeof(PlayerStateMachineScript))]
[RequireComponent(typeof(StateVitalsCoordinator))]
public class PlayerStatusReporter : MonoBehaviourPun
{
    private PlayerStateMachineScript _playerStateMachine;
    private bool _lastAliveState;

    private void Awake()
    {
        _playerStateMachine = GetComponent<PlayerStateMachineScript>();
    }

    private void Start()
    {
        // Registramos este jugador en el GameNetworkController
        if (PhotonNetwork.IsConnected && GameNetworkController.Instance != null)
        {
            GameNetworkController.Instance.RegisterPlayer(photonView.OwnerActorNr, this);
        }
    }

    private void OnDestroy()
    {
        if (PhotonNetwork.IsConnected && GameNetworkController.Instance != null)
        {
            GameNetworkController.Instance.UnregisterPlayer(photonView.OwnerActorNr);
        }
    }

    private void Update()
    {
        bool isAlive = !_playerStateMachine.isUnconscious;

        if (isAlive != _lastAliveState)
        {
            _lastAliveState = isAlive;
            GameNetworkController.Instance?.UpdatePlayerStatus(photonView.OwnerActorNr, isAlive);

            // Si es el jugador local y muere, iniciar modo espectador
            if (!isAlive && photonView.IsMine)
            {
                SpectatorController.Instance?.StartSpectatingWithDelay();
            }
            // Si revive, salimos del modo espectador
            else if (isAlive && photonView.IsMine)
            {
                SpectatorController.Instance?.DisableSpectatorMode();
            }
        }
    }

    public bool IsAlive => !_playerStateMachine.isUnconscious;
}
