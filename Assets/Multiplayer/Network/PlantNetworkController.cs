using Photon.Pun;
using UnityEngine;
using Characters.Enemies.Scripts.Plant;
using Characters.LifeSupportSystem.EnemyLifeSupport;

namespace Characters.Enemies.Scripts.Network
{
    [RequireComponent(typeof(PhotonView))]
    public class PlantNetworkController : MonoBehaviourPun, IPunObservable
    {
        [Header("References")]
        [SerializeField] private PlantAnimController _animController;
        [SerializeField] private EnemyLifeSupportScript _lifeSupport;
        [SerializeField] private Animator _animator;

        private float _currentHealth;
        private bool _isDead;

        private void Awake()
        {
            if (!_animController) _animController = GetComponent<PlantAnimController>();
            if (!_lifeSupport) _lifeSupport = GetComponent<EnemyLifeSupportScript>();
            if (!_animator) _animator = GetComponent<Animator>();
        }

        private void Start()
        {
            _currentHealth = GetHealthValue();
        }

        private void Update()
        {
            if (PhotonNetwork.IsMasterClient)
            {
                float newHealth = GetHealthValue();

                if (Mathf.Abs(newHealth - _currentHealth) > 0.01f)
                {
                    _currentHealth = newHealth;
                    photonView.RPC(nameof(RPC_UpdateHealth), RpcTarget.Others, _currentHealth);
                }

                if (!_isDead && _currentHealth <= 0f)
                {
                    _isDead = true;
                    photonView.RPC(nameof(RPC_PlayDeath), RpcTarget.All);
                }
            }
        }

        // --- Método auxiliar ---
        private float GetHealthValue()
        {
            var healthVital = _lifeSupport.Vitals[EnemyLifeSupportScript.EVitals.Health];

            // Intentamos obtener el valor real según la estructura
            var property = healthVital.GetType().GetProperty("CurrentValue") ??
                           healthVital.GetType().GetProperty("Current") ??
                           healthVital.GetType().GetProperty("Value");

            if (property != null)
            {
                object result = property.GetValue(healthVital);
                if (result is float f) return f;
            }

            // Si no hay propiedad conocida, devolvemos 0 para no romper
            Debug.LogWarning("[PlantNetworkController] No se pudo obtener el valor de salud real del vital.");
            return 0f;
        }

        [PunRPC]
        private void RPC_UpdateHealth(float newHealth)
        {
            _currentHealth = newHealth;
        }

        [PunRPC]
        private void RPC_PlayDeath()
        {
            _isDead = true;
            _animController.TriggerByName(PlantAnimController.EPlantStates.Dead.ToString());
        }

        [PunRPC]
        public void RPC_PlayAttack(string attackType)
        {
            _animController.TriggerByName(attackType);
        }

        public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
        {
            if (stream.IsWriting)
            {
                stream.SendNext(_currentHealth);
                stream.SendNext(_isDead);
            }
            else
            {
                _currentHealth = (float)stream.ReceiveNext();
                _isDead = (bool)stream.ReceiveNext();
            }
        }

        public void RequestAttack(PlantAnimController.EPlantStates attackType)
        {
            if (PhotonNetwork.IsMasterClient)
            {
                photonView.RPC(nameof(RPC_PlayAttack), RpcTarget.All, attackType.ToString());
            }
        }
    }
}
