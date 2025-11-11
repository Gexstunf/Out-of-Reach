using Photon.Pun;
using UnityEngine;
using Characters.ActiveRagdollSystem;
using Characters.Enemies.Scripts;
using Items.Scripts;

namespace Characters.Enemies.Scripts.Plant
{
    [RequireComponent(typeof(PhotonView))]
    [RequireComponent(typeof(ActiveRagdollCoreScript))]
    [RequireComponent(typeof(NervousSystemScript))]
    [RequireComponent(typeof(AttackScript))]
    [RequireComponent(typeof(PlantAnimController))]
    public class PlantNetworkController : MonoBehaviourPun, IPunObservable
    {
        [Header("References")]
        [SerializeField] private ActiveRagdollCoreScript _ragdollCore;
        [SerializeField] private NervousSystemScript _nervousSystem;
        [SerializeField] private AttackScript _attackScript;
        [SerializeField] private PlantAnimController _animController;

        [Header("Stats")]
        [SerializeField] private float _maxHealth = 100f;
        private float _currentHealth;
        private bool _isDead;
        private bool _isAttacking;

        private float _attackTimer;
        private readonly float _attackCooldown = 3f;

        private bool IsOwner => PhotonNetwork.IsMasterClient;

        // Variables de red para interpolar
        private bool _netIsDead;
        private bool _netIsAttacking;
        private float _netHealth;

        private void Awake()
        {
            _ragdollCore = GetComponent<ActiveRagdollCoreScript>();
            _nervousSystem = GetComponent<NervousSystemScript>();
            _attackScript = GetComponent<AttackScript>();
            _animController = GetComponent<PlantAnimController>();
        }

        private void Start()
        {
            _currentHealth = _maxHealth;
            _attackTimer = _attackCooldown;

            if (_animController != null)
                _animController.TriggerByName(PlantAnimController.EPlantStates.Idle.ToString());
        }

        private void Update()
        {
            if (IsOwner)
            {
                HandleAI();
            }
            else
            {
                // Actualiza visualmente en los clientes
                if (_isDead != _netIsDead)
                {
                    if (_netIsDead)
                        OnRemoteDeath();
                    else
                        OnRemoteRevive();
                }

                if (_isAttacking != _netIsAttacking)
                {
                    if (_netIsAttacking)
                        OnRemoteAttack();
                }

                _currentHealth = Mathf.Lerp(_currentHealth, _netHealth, Time.deltaTime * 10f);
            }
        }

        private void HandleAI()
        {
            if (_isDead) return;

            _attackTimer -= Time.deltaTime;

            // Si las "nervios" detectan un golpe
            if (_nervousSystem.NervesTriggered)
            {
                var hurting = _nervousSystem.HurtingScript;
                if (hurting != null)
                    TakeDamage(hurting.Damage);

                _nervousSystem.ResetNerves();
            }

            // Si puede atacar
            if (_attackTimer <= 0f)
            {
                DoAttack();
                _attackTimer = _attackCooldown;
            }
        }

        private void DoAttack()
        {
            if (_attackScript == null || _animController == null) return;

            _isAttacking = true;
            photonView.RPC(nameof(RPC_DoAttack), RpcTarget.All);
            Invoke(nameof(ResetAttack), 1f);
        }

        [PunRPC]
        private void RPC_DoAttack()
        {
            if (_animController)
            {
                _animController.TriggerByName(PlantAnimController.EPlantStates.MediumAttack.ToString());
                _animController.attack = true;
            }

            if (_attackScript)
                _attackScript.PerformAttack();
        }

        private void ResetAttack()
        {
            _isAttacking = false;

            if (_animController)
            {
                _animController.attack = false;
                _animController.TriggerByName(PlantAnimController.EPlantStates.Idle.ToString());
            }
        }

        private void TakeDamage(float dmg)
        {
            if (_isDead) return;

            _currentHealth -= dmg;
            _currentHealth = Mathf.Clamp(_currentHealth, 0, _maxHealth);

            if (_currentHealth <= 0f)
                Die();
        }

        private void Die()
        {
            _isDead = true;
            photonView.RPC(nameof(RPC_OnDeath), RpcTarget.All);
        }

        [PunRPC]
        private void RPC_OnDeath()
        {
            _isDead = true;

            if (_animController)
                _animController.TriggerByName(PlantAnimController.EPlantStates.Dead.ToString());

            if (_ragdollCore)
                _ragdollCore.Kill();
        }

        private void OnRemoteDeath() => RPC_OnDeath();
        private void OnRemoteAttack() => RPC_DoAttack();

        private void OnRemoteRevive()
        {
            _isDead = false;
            _currentHealth = _maxHealth;

            if (_animController)
                _animController.TriggerByName(PlantAnimController.EPlantStates.Alive.ToString());

            if (_ragdollCore)
                _ragdollCore.Revive();
        }

        public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
        {
            if (stream.IsWriting)
            {
                stream.SendNext(_currentHealth);
                stream.SendNext(_isDead);
                stream.SendNext(_isAttacking);
            }
            else
            {
                _netHealth = (float)stream.ReceiveNext();
                _netIsDead = (bool)stream.ReceiveNext();
                _netIsAttacking = (bool)stream.ReceiveNext();
            }
        }
    }
}
