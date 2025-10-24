using UnityEngine;

namespace Environment.Scripts.Interactable {
    [CreateAssetMenu(fileName = "Terminal", menuName = "Interactable/Environment/Terminal")]
    public class InteractableTerminalSO : InteractableObjectSO
    {
        [Header("References")]
        public Camera mainCamera;
        public Transform screenTransform;
        
        [Header("Settings")]
        public float transitionSpeed = 2f;
        public float offset = 5f;
        
        private Transform _originalTransform;
        private float _time;
        
        public override void Interact(Transform actor) { 
            Debug.Log("Interacting with terminal!");
            Vector3 targetPosition = actor.position + new Vector3(0f, offset, 0f);
            mainCamera = Camera.main;
            if (mainCamera) {
                _originalTransform = mainCamera.transform;
                _time = 0f; 
                while (mainCamera.transform.position != targetPosition) {
                    MoveCameraToPos(targetPosition);
                }
                _time = 0f; 
            }
        }

        private void MoveCameraToPos(Vector3 pos) {
            _time += Time.deltaTime;
            mainCamera.transform.position = Vector3.Lerp(_originalTransform.position, pos, _time * transitionSpeed);
        }
    }
}
