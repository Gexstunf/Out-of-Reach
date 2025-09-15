using System;
using UnityEngine;

namespace Characters.Utils
{
    public class RotatorScript : MonoBehaviour
    {
        private float _lookSenseH;
        private float _lookSenseV;
        private float _lookLimitV;
        
        private Vector2 _targetRotation;
        private Transform _transform;

        public void Init(float lookSenseH, float lookSenseV, float lookLimitV)
        {
            _lookSenseH = lookSenseH;
            _lookSenseV = lookSenseV;
            _lookLimitV = lookLimitV;
        }

        private void Awake()
        {
            _transform = GetComponent<Transform>();
        }

        public void RotateTransform(Vector2 lookInput)
        {
            _targetRotation.x += _lookSenseH * lookInput.x;
            _transform.rotation = Quaternion.Euler(0f, _targetRotation.x, 0f);
        }
        
        public float GetYaw() => _targetRotation.x;

    }
}
