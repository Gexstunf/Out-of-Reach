using UnityEngine;

namespace Environment.Scripts.Debug {
    public class SpinScript : MonoBehaviour, IMovingPlatform {
        public float rotationSpeed = 100f; 
        public Vector3 rotationAxis = Vector3.up;

        void Update() {
            transform.Rotate(rotationAxis.normalized * (rotationSpeed * Time.deltaTime));
        }

        public Vector3 GetVelocity() {
            return Vector3.zero;
        }
    }
}