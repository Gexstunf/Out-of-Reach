using System;
using UnityEngine;

namespace Environment.Scripts.DungeonGeneration.Data {
    public class StructureSocketScript : MonoBehaviour {
        [Header("Settings")]
        [SerializeField] private StructureSocketType _socketType = StructureSocketType.Entry;
        
        [Header("Debug")]
        //[SerializeField] private bool debug = true;
        [SerializeField] private Color debugColorEntry = Color.green;
        [SerializeField] private Color debugColorExit = Color.red;
        [SerializeField] private float debugLength = 5f;
        [SerializeField] private float sphereRad = 1f;
        
        //public Axis outwardFacingAxis = Axis.X;

        public enum StructureSocketType { Entry, Exit}
        public enum Axis { X, Y, Z}
        public bool IsConnected { get; private set; } = false;
        public StructureSocketType SocketType => _socketType;

        public void SetConnected(bool connected)
        {
            IsConnected = connected;
        }

        private void OnDrawGizmos()
        {
            //if (!debug) return;
            if (_socketType == StructureSocketType.Entry)
                Gizmos.color = debugColorEntry;
            else 
                Gizmos.color = debugColorExit;
            
            // Main line
            Vector3 start = transform.position;
            Vector3 end = start + transform.forward * debugLength;
            Gizmos.DrawLine(start, end);

            // Arrow head
            Vector3 right = Quaternion.LookRotation(transform.forward) * Quaternion.Euler(0, 150, 0) * Vector3.forward;
            Vector3 left  = Quaternion.LookRotation(transform.forward) * Quaternion.Euler(0, -150, 0) * Vector3.forward;

            Gizmos.DrawLine(end, end + right * 0.5f);
            Gizmos.DrawLine(end, end + left * 0.5f);
        }

    }
}
