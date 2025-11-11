using Photon.Pun;
using UnityEngine;
using Characters.PlayerController.Scripts.Input;

public class PlayerAnimationController : MonoBehaviourPun
{
    private Animator _animator;
    private PlayerInputScript _input;

    void Awake()
    {
        _animator = GetComponentInChildren<Animator>();
        _input = GetComponent<PlayerInputScript>();
    }

    void Update()
    {
        if (!photonView.IsMine) return;

        _animator.SetBool("Crouch", _input.CrouchPressed);
        _animator.SetBool("Jumping", _input.JumpPressed);
        _animator.SetBool("Falling", false);

        /*
        if (_input.Emote1Triggered) {
            _animator.SetTrigger("Emote1");
            _input.Emote1Triggered = false;
        }
        */
    }
}
