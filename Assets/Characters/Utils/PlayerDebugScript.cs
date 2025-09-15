using System;
using Characters.LifeSupportSystem.PlayerLifeSupport;
using Characters.StateMachine.PlayerStateMachine;
using UnityEngine;

public class PlayerDebugScript : MonoBehaviour
{
    [Header("Debugging references")]
    [SerializeField] private PlayerStateMachineScript playerStateMachine;
    [SerializeField] private PlayerLifeSupportScript playerLifeSupport;


    private void Awake() {
        playerStateMachine = GetComponent<PlayerStateMachineScript>();
        playerLifeSupport = GetComponent<PlayerLifeSupportScript>();
    }

    [ContextMenu("Make Player Unconscious")]
    private void MakePlayerUnconscious()
    {
        playerLifeSupport.Context.SetUnconscious(true);
    }
    [ContextMenu("Make Player Conscious")]
    private void MakePlayerConscious()
    {
        playerLifeSupport.Context.SetUnconscious(false);
    }

    [ContextMenu("Make Player Tired")]
    private void MakePlayerTired() {
        playerLifeSupport.Context.SetTired(true);
    }
    
    [ContextMenu("Make Player NOT Tired")]
    private void MakePlayerNotTired() {
        playerLifeSupport.Context.SetTired(false);
    }
}
