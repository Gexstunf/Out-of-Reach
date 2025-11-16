using System;
using UnityEngine;
using UnityEngine.AI;
using Photon.Pun;

namespace Characters.Enemies.Scripts
{
    [RequireComponent(typeof(PhotonView))]
    public class EnemyNetworkController : MonoBehaviourPun, IPunObservable
    {

        [Header("References")]
        [SerializeField] private TargetingScript _targetingScript;
        [SerializeField] private NavMeshAgent _agent;
        [SerializeField] private EnemiesManagerScript _manager;

        public NavMeshAgent Agent => _agent;
        public bool IsChasing => _chasing;

        private bool _chasing;

        // variables para interpolar movimiento remoto
        private Vector3 _networkPosition;
        private Quaternion _networkRotation;

        void Awake()
        {
            _targetingScript = GetComponent<TargetingScript>();
            _agent = GetComponent<NavMeshAgent>();

            if (_agent == null) Debug.LogWarning("No NavMeshAgent attached to " + gameObject.name);
            if (_targetingScript == null) Debug.LogWarning("No TargetingScript attached to " + gameObject.name);
        }

        private void Start() {
            _manager = EnemiesManagerScript.Instance;
        }

        void Update()
        {
            // SOLO el due�o controla el movimiento
            if (photonView.IsMine && _manager.AgentsCanInteract)
            {
                if (_targetingScript.CurrentTargetTransform && _agent.isActiveAndEnabled)
                {
                    _agent.destination = _targetingScript.CurrentTargetTransform.position;
                    _chasing = true;
                }
                else
                {
                    _chasing = false;
                }
            }
            else
            {
                // Para clientes remotos, interpolamos hacia la posici�n sincronizada
                transform.position = Vector3.Lerp(transform.position, _networkPosition, Time.deltaTime * 10f);
                transform.rotation = Quaternion.Lerp(transform.rotation, _networkRotation, Time.deltaTime * 10f);
            }
        }

        // M�todo obligatorio para sincronizar datos con Photon
        public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
        {
            if (stream.IsWriting)
            {
                // Enviar posici�n y rotaci�n actuales (solo el due�o lo hace)
                stream.SendNext(transform.position);
                stream.SendNext(transform.rotation);
            }
            else
            {
                // Recibir posici�n y rotaci�n desde la red
                _networkPosition = (Vector3)stream.ReceiveNext();
                _networkRotation = (Quaternion)stream.ReceiveNext();
            }
        }
    }
}
