using System;
using UnityEngine;

namespace Characters.Utils
{
    public class CameraControllerScript : MonoBehaviour
    {
        private Vector2 _cameraRotation = Vector2.zero;
        private float _lookSenseH;
        private float _lookSenseV;
        private float _lookLimitV;

        public void Init(float lookSenseH, float lookSenseV, float lookLimitV)
        {
            _lookSenseH = lookSenseH;
            _lookSenseV = lookSenseV;
            _lookLimitV = lookLimitV;
        }

        public void UpdateCameraRotation(Vector2 lookInput, Camera _camera)
        {
            _cameraRotation.x += lookInput.x * _lookSenseH;
            _cameraRotation.y -= lookInput.y * _lookSenseV;
            // _cameraRotation.y += Mathf.Clamp(_cameraRotation.y, -lookLimitV, lookLimitV);
            _cameraRotation.y = Mathf.Clamp(_cameraRotation.y, -(_lookLimitV), _lookLimitV);
            
            _camera.transform.rotation = Quaternion.Euler(_cameraRotation.y, _cameraRotation.x, 0f);
        }
    }
}
