using System;
using UnityEngine;

namespace Characters.Utils
{
    public class CameraControllerScript
    {
        private Vector2 _cameraRotation = Vector2.zero;
        private float _cameraPitch = 0f;
        private float _lookSenseH;
        private float _lookSenseV;
        private float _lookLimitV;
        private Transform _cameraTieTransform;
        private Vector3 _camOffset;


        public void Init(float lookSenseH, float lookSenseV, float lookLimitV)
        {
            _lookSenseH = lookSenseH;
            _lookSenseV = lookSenseV;
            _lookLimitV = lookLimitV;
        }
        
        public void TieToTransform(Transform tieTransform, Vector3 offset)
        {
            _cameraTieTransform = tieTransform;
            _camOffset = offset;
        }
        
        public void UpdateCameraRotation(Vector2 lookInput, Camera camera, float characterYaw)
        {
            if (!_cameraTieTransform) return;

            _cameraPitch -= lookInput.y * _lookSenseV;
            _cameraPitch = Mathf.Clamp(_cameraPitch, -_lookLimitV, _lookLimitV);

            // Set camera position with offset relative to tie transform
            camera.transform.position = _cameraTieTransform.position 
                                        + _cameraTieTransform.TransformDirection(_camOffset);

            // Build rotation: take yaw from tie transform, apply pitch around local X
            Quaternion tieRotation = _cameraTieTransform.rotation;  // full rotation
            Vector3 tieForward = tieRotation * Vector3.forward;      // tie’s forward

            // Apply pitch relative to tie’s right vector
            Quaternion pitch = Quaternion.AngleAxis(_cameraPitch, _cameraTieTransform.right);

            camera.transform.rotation = pitch * Quaternion.LookRotation(tieForward, Vector3.up);
        }
    }
}
