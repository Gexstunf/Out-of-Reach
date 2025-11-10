using Photon.Pun;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Serialization;

namespace Environment.Scripts {
    public class SlidingDoorScript : MonoBehaviourPun, IPunObservable
    {

        [Header("References")] 
        public Transform femaleDoor;
        public Transform maleDoor;
        [SerializeField] private AudioSource _audioSource;

        [Header("Settings")] 
        [SerializeField] private MovementAxis movementAxis = MovementAxis.Z;
        public bool useDetection = true;
        public float detectionRadius = 4f;
        public LayerMask detectionLayerMask;
        public float doorSpeed = 0.2f;
        public float doorCrackOffset = 0.05f;
        public bool shouldOpenForEnemies = true;
        
        [Header("Debug")]
        [SerializeField] private bool open;
        public bool debug;
        
        [Header("Settings Failure")] 
        public float doorFailureChance = 0.4f;
        public DoorFailureMode doorMode;
        
        [SerializeField] private float slamDistanceOpen = 2f; 
        [SerializeField] private float slamDistanceClosed = -1f;
        [SerializeField] private float slowDoorSpeed = 2f;

        [SerializeField] private float slamSpeed = 5f;       
        [SerializeField] private float minDelay = 0.3f;           // shortest pause
        [SerializeField] private float maxDelay = 1.5f;   
        
        [Header("Visualize")]
        [SerializeField] private bool doorIsNowOpen;

        private bool _isSlamming;
        private Vector3 _femaleTarget;
        private Vector3 _maleTarget;
        
        private enum MovementAxis {
            X,
            Y,
            Z,
        }

        private bool _previouslyClosed;
        private bool _isOpen;
        private Vector3 _closedPosOffset;
        
        //these initial pos have to be in the open position.
        private Vector3 _initialFemalePosition;
        private Vector3 _initialMalePosition;
        
        public enum DoorFailureMode {
            None,       
            FailsToClose,
            FailsToOpen,  
            Slowed,
            JammedOpen,
        }
        
        public bool IsOpen => _isOpen;

        private void Awake() {
            _audioSource = GetComponent<AudioSource>();
        }

        void Start()
        {
            _initialFemalePosition = femaleDoor.localPosition;
            _initialMalePosition = maleDoor.localPosition;

            if (UnityEngine.Random.value < doorFailureChance)
                doorMode = (DoorFailureMode)UnityEngine.Random.Range(1, Enum.GetValues(typeof(DoorFailureMode)).Length);
            else
                doorMode = DoorFailureMode.None;
        }

        private void Update()
        {
            // Solo el Master actualiza la lógica
            if (photonView.IsMine || !PhotonNetwork.IsConnected)
            {
                if (!debug && useDetection && doorMode == DoorFailureMode.None)
                {
                    Collider[] hits = Physics.OverlapSphere(transform.position, detectionRadius, detectionLayerMask);
                    bool detected = false;

                    foreach (Collider c in hits)
                    {
                        if (c.CompareTag("Player") || (c.CompareTag("Enemy") && shouldOpenForEnemies))
                        {
                            detected = true;
                            break;
                        }
                    }
                    _isOpen = detected;
                }
                else
                    _isOpen = open;

                doorIsNowOpen = _isOpen;

                if (doorMode != DoorFailureMode.None)
                {
                    HandleDoorFailure();
                    return;
                }

                HandleDoor(doorSpeed);
            }
            else
            {
                // Los otros clientes simplemente interpolan lo que reciben
                HandleDoor(doorSpeed);
            }
        }

        private void HandleDoorFailure()
        {
            switch (doorMode)
            {
                case DoorFailureMode.JammedOpen:
                    _isOpen = true;
                    return;
                case DoorFailureMode.FailsToOpen:
                    SlamDoor(Vector3.zero + _closedPosOffset, Vector3.zero - _closedPosOffset, true);
                    break;
                case DoorFailureMode.FailsToClose:
                    _isOpen = true;
                    SlamDoor(_initialFemalePosition, _initialMalePosition, false);
                    break;
                case DoorFailureMode.Slowed:
                    HandleDoor(slowDoorSpeed);
                    break;
            }
        }

        private void SetClosedOffsetPositionAlongAxis(ref Vector3 position, float offset)
        {
            switch (movementAxis)
            {
                case MovementAxis.X: position = new Vector3(offset, 0f, 0f); break;
                case MovementAxis.Y: position = new Vector3(0f, offset, 0f); break;
                case MovementAxis.Z: position = new Vector3(0f, 0f, offset); break;
            }
        }

        private void HandleDoor(float speed)
        {
            if (_isOpen)
            {
                femaleDoor.localPosition = Vector3.Lerp(femaleDoor.localPosition, _initialFemalePosition, speed * Time.deltaTime);
                maleDoor.localPosition = Vector3.Lerp(maleDoor.localPosition, _initialMalePosition, speed * Time.deltaTime);
            }
            else
            {
                SetClosedOffsetPositionAlongAxis(ref _closedPosOffset, doorCrackOffset);
                femaleDoor.localPosition = Vector3.Lerp(femaleDoor.localPosition, Vector3.zero + _closedPosOffset, speed * Time.deltaTime);
                maleDoor.localPosition = Vector3.Lerp(maleDoor.localPosition, Vector3.zero - _closedPosOffset, speed * Time.deltaTime);
            }
        }

        private void SlamDoor(Vector3 femaleTarget, Vector3 maleTarget, bool openSlam)
        {
            if (_isSlamming) return;
            StartCoroutine(SlamCycle(femaleTarget, maleTarget, openSlam));
        }

        private IEnumerator SlamCycle(Vector3 femaleTarget, Vector3 maleTarget, bool openSlam)
        {
            _isSlamming = true;
            float slamDistance = openSlam ? slamDistanceOpen : slamDistanceClosed;

            Vector3 femaleOvershoot = AddOffsetPositionAlongAxis(femaleTarget, slamDistance);
            Vector3 maleOvershoot = AddOffsetPositionAlongAxis(maleTarget, slamDistance, false);

            float t = 0f;
            while (t < 1f)
            {
                t += Time.deltaTime * slamSpeed;
                femaleDoor.localPosition = Vector3.Lerp(femaleDoor.localPosition, femaleOvershoot, t);
                maleDoor.localPosition = Vector3.Lerp(maleDoor.localPosition, maleOvershoot, t);
                yield return null;
            }

            t = 0f;
            while (t < 1f)
            {
                t += Time.deltaTime * slamSpeed;
                femaleDoor.localPosition = Vector3.Lerp(femaleDoor.localPosition, femaleTarget, t);
                maleDoor.localPosition = Vector3.Lerp(maleDoor.localPosition, maleTarget, t);
                yield return null;
            }

            yield return new WaitForSeconds(UnityEngine.Random.Range(minDelay, maxDelay));
            _isSlamming = false;
        }

        private Vector3 AddOffsetPositionAlongAxis(Vector3 position, float offset, bool positive = true)
        {
            float offsetWithSign = positive ? offset : -offset;
            switch (movementAxis)
            {
                case MovementAxis.X: return position + new Vector3(offsetWithSign, 0, 0);
                case MovementAxis.Y: return position + new Vector3(0, offsetWithSign, 0);
                case MovementAxis.Z: return position + new Vector3(0, 0, offsetWithSign);
                default: return position;
            }
        }

        public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
        {
            if (stream.IsWriting)
            {
                stream.SendNext(_isOpen);
            }
            else
            {
                _isOpen = (bool)stream.ReceiveNext();
            }
        }

        public void SwitchDoorState()
        {
            _isOpen = !_isOpen;
        }
    }
}