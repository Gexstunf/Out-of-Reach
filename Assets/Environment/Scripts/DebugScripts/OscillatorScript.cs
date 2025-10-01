using UnityEngine;

namespace Environment.Scripts.DebugScripts {
    public class OscillateScript : MonoBehaviour, IMovingPlatform {
        public float amplitude = 2f; 
        public float speed = 1f;   
        public Vector3 direction = Vector3.right; 

        private Vector3 startPosition;
        private Vector3 currentVelocity;
        private Rigidbody rb;

        void Start() {
            startPosition = transform.position;
            rb = GetComponent<Rigidbody>();
        }

        void Update() {
            //Vector3 offset = direction.normalized * (Mathf.Sin(Time.time * speed) * amplitude);
            //transform.position = startPosition + offset;

            // Calcula la velocidad
            //currentVelocity = direction.normalized * (Mathf.Cos(Time.time * speed) * amplitude * speed);
            
            Vector3 targetPosition = startPosition + direction.normalized * (Mathf.Sin(Time.time * speed) * amplitude);
            currentVelocity = (targetPosition - rb.position) / Time.fixedDeltaTime;
            rb.MovePosition(targetPosition);
        }
        
        public Vector3 GetVelocity() {
            UnityEngine.Debug.Log("Get velocity, " + currentVelocity.x + ", " + currentVelocity.y + ", " + currentVelocity.z);
            return currentVelocity;
        }
    }
}
