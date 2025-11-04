using UnityEngine;

namespace UI.Scripts {
    public class UIFaceCameraScript : MonoBehaviour
    {
        private Camera _mainCamera;

        private void Start()
        {
            _mainCamera = Camera.main;

            if (!_mainCamera) {
                Debug.LogError("No MainCamera for UI Worldspace");
            }
        }

        private void LateUpdate()
        {
            if (!_mainCamera) return;

            // face the camera
            transform.LookAt(transform.position + _mainCamera.transform.rotation * Vector3.forward,
                _mainCamera.transform.rotation * Vector3.up);
        }
    }
}
