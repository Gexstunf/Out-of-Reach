using Photon.Pun;
using System;
using System.Collections;
using Environment.Scripts.Doors;
using UnityEngine;
using UnityEngine.Serialization;

namespace Environment.Scripts {
    public class SlidingDoorScript : MonoBehaviourPun, IPunObservable
    {
        
        #region Variables
        [Header("References")] 
        public Transform femaleDoor;
        public Transform maleDoor;
        [SerializeField] private AudioSource _doorAudio;
        [SerializeField] private SlidingDoorsManagerScript _doorsManager;

        [Header("Settings")] 
        [SerializeField] private MovementAxis movementAxis = MovementAxis.Z;
        public bool useDetection = true;
        public float detectionRadius = 4f;
        public LayerMask detectionLayerMask;
        public float doorSpeed = 6f;
        public float doorCrackOffset = 0.05f;
        public bool shouldOpenForEnemies = true;
        
        [Header("Audio")]
        public float timeSkip = 0.5f;
        
        [Header("Settings Failure")] 
        public float doorFailureChance = 0.4f;
        public DoorFailureMode doorMode = DoorFailureMode.None;
        
        [SerializeField] private float slamDistanceOpen = 2f; 
        [SerializeField] private float slamDistanceClosed = -1f;
        [SerializeField] private float doorSpeedModifier = -2f;

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
        
        #endregion
        
        #region Public API
        public bool IsOpen => _isOpen;
        
        public void SwitchDoorState()
        {
            _isOpen = !_isOpen;
        }
        
        #endregion

        #region Unity Methods
        private void Awake() {
            _doorAudio = GetComponent<AudioSource>();
        }

        private void Start()
        {
            _doorsManager = SlidingDoorsManagerScript.Instance;
            _initialFemalePosition = femaleDoor.localPosition;
            _initialMalePosition = maleDoor.localPosition;

            if (UnityEngine.Random.value < doorFailureChance)
                doorMode = (DoorFailureMode)UnityEngine.Random.Range(1, Enum.GetValues(typeof(DoorFailureMode)).Length);
            else
                doorMode = DoorFailureMode.None;
        }

        private void Update()
        {
            if (doorMode == DoorFailureMode.None) {
                HandleDoor(doorSpeed);
            }
            
            doorIsNowOpen = _isOpen;

            if ((!photonView.IsMine || !PhotonNetwork.IsConnected) && _doorsManager.usePhoton) return; // Solo el Master actualiza la logica
            
            if (useDetection && doorMode == DoorFailureMode.None) {
                Collider[] hits = Physics.OverlapSphere(transform.position, detectionRadius, detectionLayerMask);
                bool detected = HandleDetection(hits);
                _isOpen = detected;
            }
            
            if (doorMode != DoorFailureMode.None) {
                HandleDoorFailure();
            }
        }
        #endregion
        
        #region Door Logic

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
                    HandleDoor(doorSpeed - Math.Abs(doorSpeedModifier));
                    break;
            }
        }

        private void HandleDoor(float speed)
        {
            if (_isOpen) {
                if (_previouslyClosed) PlaySound();
                _previouslyClosed = false;
                femaleDoor.localPosition = Vector3.Lerp(femaleDoor.localPosition, _initialFemalePosition, speed * Time.deltaTime);
                maleDoor.localPosition = Vector3.Lerp(maleDoor.localPosition, _initialMalePosition, speed * Time.deltaTime);
            }
            else {
                if (!_previouslyClosed) PlaySound();
                
                _previouslyClosed = true;
                SetClosedOffsetPositionAlongAxis(ref _closedPosOffset, doorCrackOffset);
                femaleDoor.localPosition = Vector3.Lerp(femaleDoor.localPosition, Vector3.zero + _closedPosOffset, speed * Time.deltaTime);
                maleDoor.localPosition = Vector3.Lerp(maleDoor.localPosition, Vector3.zero - _closedPosOffset, speed * Time.deltaTime);
            }
        }

        private bool HandleDetection(Collider[] hits) {
            foreach (Collider c in hits)
            {
                if (c.CompareTag("Player") || (c.CompareTag("Enemy") && shouldOpenForEnemies))
                {
                    return true;
                }
            }
            return false;
        }
        
        #endregion

        #region Slam door logic
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
        #endregion
        
        #region  Utils
        
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
        
        private void SetClosedOffsetPositionAlongAxis(ref Vector3 position, float offset)
        {
            switch (movementAxis)
            {
                case MovementAxis.X: position = new Vector3(offset, 0f, 0f); break;
                case MovementAxis.Y: position = new Vector3(0f, offset, 0f); break;
                case MovementAxis.Z: position = new Vector3(0f, 0f, offset); break;
            }
        }

        private void PlaySound() {
            _doorAudio.time = timeSkip;
            _doorAudio.Play();
        }
        
        #endregion

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
    }
}