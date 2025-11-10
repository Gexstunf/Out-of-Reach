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

        void Update()
        {
            // SOLO el dueño controla el movimiento
            if (photonView.IsMine)
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
                // Para clientes remotos, interpolamos hacia la posición sincronizada
                transform.position = Vector3.Lerp(transform.position, _networkPosition, Time.deltaTime * 10f);
                transform.rotation = Quaternion.Lerp(transform.rotation, _networkRotation, Time.deltaTime * 10f);
            }
        }

        // Método obligatorio para sincronizar datos con Photon
        public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
        {
            if (stream.IsWriting)
            {
                // Enviar posición y rotación actuales (solo el dueño lo hace)
                stream.SendNext(transform.position);
                stream.SendNext(transform.rotation);
            }
            else
            {
                // Recibir posición y rotación desde la red
                _networkPosition = (Vector3)stream.ReceiveNext();
                _networkRotation = (Quaternion)stream.ReceiveNext();
            }
        }
    }
}
